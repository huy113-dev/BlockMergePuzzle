using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BlockMergePuzzle.Core;

namespace BlockMergePuzzle.Utils
{
    /// <summary>
    /// Generates sculpted cosmic silver-lavender carved-stone dungeon boards with
    /// C1 concave-fillet stepped walls, Voronoi cracks with directional specular lips,
    /// plush 3D pillow-beveled indigo floor tiles, and glossy gate pads.
    /// </summary>
    public static class ProceduralBoardRenderer
    {
        private const float WallMarginCells = 0.50f;
        private const float WallThicknessCells = 0.355f;
        private const float ConcaveFilletRadius = 0.22f;

        private struct GateBox
        {
            public float MinX, MaxX, MinY, MaxY;
            public Color32 BaseCol, HighlightCol, MidLitCol, ShadowCol, RimCol;
        }

        public static Texture2D GenerateBoardTexture(CustomLevelData data, int cellPx = 64)
        {
            return BuildBoardTexture(data, out _, out _, cellPx);
        }

        public static Sprite GenerateBoardSprite(CustomLevelData data, out float frameWidthWorld, out float frameHeightWorld, int cellPx = 128)
        {
            Texture2D tex = BuildBoardTexture(data, out frameWidthWorld, out frameHeightWorld, cellPx, out int usedCellPx);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), usedCellPx);
        }

#if UNITY_EDITOR
        public static Sprite SaveBoardSpriteAsset(CustomLevelData data, string assetPath, int cellPx = 128)
        {
            Texture2D tex = BuildBoardTexture(data, out _, out _, cellPx, out int usedCellPx);
            if (tex == null) return null;

            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(assetPath, png);
            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.ForceUpdate);

            var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
            if (importer != null)
            {
                importer.textureType = UnityEditor.TextureImporterType.Sprite;
                importer.spriteImportMode = UnityEditor.SpriteImportMode.Single;
                importer.spritePixelsPerUnit = usedCellPx;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
#endif

        private static Texture2D BuildBoardTexture(CustomLevelData data, out float frameWidthWorld, out float frameHeightWorld, int requestedCellPx)
        {
            return BuildBoardTexture(data, out frameWidthWorld, out frameHeightWorld, requestedCellPx, out _);
        }

        private static Texture2D BuildBoardTexture(
            CustomLevelData data,
            out float frameWidthWorld,
            out float frameHeightWorld,
            int requestedCellPx,
            out int effCellPx)
        {
            data.Normalize();
            data.GetActiveBounds(out int minC, out int maxC, out int minR, out int maxR);

            int activeCols = Mathf.Max(1, maxC - minC + 1);
            int activeRows = Mathf.Max(1, maxR - minR + 1);
            float worldCell = data.ComputeWorldCellSize();

            float totalCellsW = activeCols + WallMarginCells * 2f;
            float totalCellsH = activeRows + WallMarginCells * 2f;
            frameWidthWorld = totalCellsW * worldCell;
            frameHeightWorld = totalCellsH * worldCell;

            int maxDimFit = Mathf.FloorToInt(2048f / Mathf.Max(1f, Mathf.Max(totalCellsW, totalCellsH)));
            effCellPx = Mathf.Clamp(Mathf.Min(requestedCellPx, maxDimFit), 32, 256);

            int texW = Mathf.Clamp(Mathf.RoundToInt(totalCellsW * effCellPx), 64, 2048);
            int texH = Mathf.Clamp(Mathf.RoundToInt(totalCellsH * effCellPx), 64, 2048);

            // Padded mask with 1 empty cell ring around active bounds
            int maskW = activeCols + 2;
            int maskH = activeRows + 2;
            bool[] activeMask = new bool[maskW * maskH];
            bool[] playableMask = new bool[maskW * maskH];

            for (int r = minR; r <= maxR; r++)
            {
                for (int c = minC; c <= maxC; c++)
                {
                    int mx = (c - minC) + 1;
                    int my = (r - minR) + 1;
                    bool play = data.IsPlayableCell(c, r);
                    bool valid = play || data.IsGateCell(c, r);
                    playableMask[my * maskW + mx] = play;
                    activeMask[my * maskW + mx] = valid;
                }
            }

            List<GateBox> gateBoxes = new List<GateBox>();
            if (data.gates != null)
            {
                for (int i = 0; i < data.gates.Count; i++)
                {
                    var g = data.gates[i];
                    if (g == null) continue;
                    var coords = g.GetGateCoords();
                    if (coords.Count == 0) continue;

                    int gMinC = int.MaxValue, gMaxC = int.MinValue;
                    int gMinR = int.MaxValue, gMaxR = int.MinValue;
                    for (int j = 0; j < coords.Count; j++)
                    {
                        if (coords[j].x < gMinC) gMinC = coords[j].x;
                        if (coords[j].x > gMaxC) gMaxC = coords[j].x;
                        if (coords[j].y < gMinR) gMinR = coords[j].y;
                        if (coords[j].y > gMaxR) gMaxR = coords[j].y;
                    }

                    Color baseCol = GameConfig.GetColorRGB(g.GetPieceColor());
                    Color saturatedCol = SaturateNeon(baseCol);
                    Color hiCol = Color.Lerp(saturatedCol, Color.white, 0.44f);
                    Color midLitCol = Color.Lerp(saturatedCol, Color.white, 0.16f);
                    Color shCol = Color.Lerp(saturatedCol, new Color(0.06f, 0.04f, 0.18f, 1f), 0.42f);
                    Color rimCol = Color.Lerp(saturatedCol, Color.white, 0.58f);

                    gateBoxes.Add(new GateBox
                    {
                        MinX = (gMinC - minC) + 0.055f,
                        MaxX = (gMaxC - minC + 1) - 0.055f,
                        MinY = (gMinR - minR) + 0.055f,
                        MaxY = (gMaxR - minR + 1) - 0.055f,
                        BaseCol = (Color32)saturatedCol,
                        HighlightCol = (Color32)hiCol,
                        MidLitCol = (Color32)midLitCol,
                        ShadowCol = (Color32)shCol,
                        RimCol = (Color32)rimCol
                    });
                }
            }

            Color32[] pixels = new Color32[texW * texH];
            float invCellPx = 1f / effCellPx;
            float aaStep = 1.35f * invCellPx;

            // Reference cosmic royal-indigo pit + pillow tile palette
            Color32 colPitDeep = new Color32(14, 13, 50, 255);
            Color32 colPitShadow = new Color32(8, 7, 30, 255);
            Color32 colTileTop = new Color32(58, 59, 136, 255);
            Color32 colTileMid = new Color32(45, 46, 112, 255);
            Color32 colTileBot = new Color32(36, 37, 94, 255);
            Color32 colTileRimLight = new Color32(102, 106, 194, 255);
            Color32 colTileSpecTop = new Color32(126, 132, 218, 255);
            Color32 colTileRimDark = new Color32(22, 22, 64, 255);

            // Reference sculpted silver-lavender rock frame palette
            Color32 colInnerLipDark = new Color32(16, 13, 52, 255);
            Color32 colStoneShadow = new Color32(66, 68, 134, 255);
            Color32 colStoneMid = new Color32(118, 124, 198, 255);
            Color32 colStoneLit = new Color32(178, 189, 244, 255);
            Color32 colStoneSpec = new Color32(236, 242, 255, 255);
            Color32 colCrackDark = new Color32(34, 30, 82, 255);
            Color32 colCrackLip = new Color32(238, 245, 255, 255);
            Color32 colOuterInk = new Color32(32, 28, 78, 255);

            // Light direction in (gridX right, gridY down, Z out of screen)
            Vector3 lightDir = new Vector3(-0.56f, -0.72f, 0.41f).normalized;
            float seedOffset = (data.levelNumber * 1.37f) % 17.0f;

            for (int py = 0; py < texH; py++)
            {
                float gy = ((texH - 1 - py) + 0.5f) * invCellPx - WallMarginCells;
                int ir = Mathf.FloorToInt(gy);
                int my = ir + 1;
                float fy = gy - ir;

                for (int px = 0; px < texW; px++)
                {
                    float gx = (px + 0.5f) * invCellPx - WallMarginCells;
                    int ic = Mathf.FloorToInt(gx);
                    int mx = ic + 1;
                    float fx = gx - ic;

                    bool inMask = mx >= 0 && mx < maskW && my >= 0 && my < maskH;
                    bool isInsideActive = inMask && activeMask[my * maskW + mx];

                    if (isInsideActive)
                    {
                        // Inner overhang contact shadow from adjacent stone walls
                        bool hasTopWall = !activeMask[(my - 1) * maskW + mx];
                        bool hasLeftWall = !activeMask[my * maskW + (mx - 1)];
                        bool hasRightWall = !activeMask[my * maskW + (mx + 1)];
                        bool hasBotWall = !activeMask[(my + 1) * maskW + mx];
                        bool hasTopLeftWall = !activeMask[(my - 1) * maskW + (mx - 1)];

                        float ao = 0f;
                        if (hasTopWall && fy < 0.24f) ao = Mathf.Max(ao, (1f - fy / 0.24f) * 0.62f);
                        if (hasLeftWall && fx < 0.20f) ao = Mathf.Max(ao, (1f - fx / 0.20f) * 0.50f);
                        if (hasRightWall && fx > 0.86f) ao = Mathf.Max(ao, ((fx - 0.86f) / 0.14f) * 0.28f);
                        if (hasBotWall && fy > 0.88f) ao = Mathf.Max(ao, ((fy - 0.88f) / 0.12f) * 0.28f);
                        if (hasTopLeftWall && fx < 0.20f && fy < 0.20f)
                        {
                            float dCorner = Mathf.Sqrt(fx * fx + fy * fy);
                            if (dCorner < 0.22f) ao = Mathf.Max(ao, (1f - dCorner / 0.22f) * 0.45f);
                        }

                        Color32 interiorColor;
                        if (playableMask[my * maskW + mx])
                        {
                            // Plump rounded 3D pillow-beveled royal-indigo floor tile
                            const float halfTile = 0.448f;
                            const float tileRadius = 0.132f;
                            float dx = Mathf.Abs(fx - 0.5f);
                            float dy = Mathf.Abs(fy - 0.5f);
                            float sdx = Mathf.Max(0f, dx - (halfTile - tileRadius));
                            float sdy = Mathf.Max(0f, dy - (halfTile - tileRadius));
                            float tileDist = Mathf.Sqrt(sdx * sdx + sdy * sdy) - tileRadius;

                            // Shadow cast by tile into pit grout
                            float sdxSh = Mathf.Max(0f, Mathf.Abs((fx - 0.022f) - 0.5f) - (halfTile - tileRadius));
                            float sdySh = Mathf.Max(0f, Mathf.Abs((fy - 0.032f) - 0.5f) - (halfTile - tileRadius));
                            float tileShDist = Mathf.Sqrt(sdxSh * sdxSh + sdySh * sdySh) - tileRadius;

                            if (tileDist > aaStep)
                            {
                                interiorColor = tileShDist < 0.02f ? colPitShadow : colPitDeep;
                            }
                            else
                            {
                                // Tile interior shading
                                Color32 baseBody = fy < 0.46f
                                    ? Color32.Lerp(colTileTop, colTileMid, fy / 0.46f)
                                    : Color32.Lerp(colTileMid, colTileBot, (fy - 0.46f) / 0.54f);

                                // Gentle center pillow dome sheen
                                float dome = Mathf.Clamp01(1f - ((fx - 0.46f) * (fx - 0.46f) + (fy - 0.40f) * (fy - 0.40f)) * 2.4f);
                                if (dome > 0f)
                                {
                                    baseBody = Color32.Lerp(baseBody, colTileRimLight, dome * 0.08f);
                                }

                                float depth = Mathf.Max(0f, -tileDist);
                                if (depth < 0.105f)
                                {
                                    float bevelT = 1f - depth / 0.105f;
                                    // Directional bevel weight from tile center -> top-left lit, bottom-right shaded
                                    float ux = fx - 0.5f;
                                    float uy = fy - 0.5f;
                                    float topLeftBias = (-ux * 0.58f - uy * 0.82f) * 2.2f;
                                    if (topLeftBias >= 0f)
                                    {
                                        float hiStrength = Mathf.Clamp01(topLeftBias) * bevelT;
                                        Color32 targetHi = (depth > 0.008f && depth < 0.038f && uy < -0.15f) ? colTileSpecTop : colTileRimLight;
                                        baseBody = Color32.Lerp(baseBody, targetHi, hiStrength * 0.62f);
                                    }
                                    else
                                    {
                                        float shStrength = Mathf.Clamp01(-topLeftBias) * bevelT;
                                        baseBody = Color32.Lerp(baseBody, colTileRimDark, shStrength * 0.72f);
                                    }
                                }

                                if (tileDist > -aaStep)
                                {
                                    float tEdge = Mathf.Clamp01((tileDist + aaStep) / (2f * aaStep));
                                    Color32 outsideCol = tileShDist < 0.02f ? colPitShadow : colPitDeep;
                                    interiorColor = Color32.Lerp(baseBody, outsideCol, tEdge);
                                }
                                else
                                {
                                    interiorColor = baseBody;
                                }
                            }
                        }
                        else
                        {
                            // Gate pad cell
                            interiorColor = colPitDeep;
                            for (int gi = 0; gi < gateBoxes.Count; gi++)
                            {
                                GateBox gb = gateBoxes[gi];
                                if (gx >= gb.MinX - 0.04f && gx <= gb.MaxX + 0.04f && gy >= gb.MinY - 0.04f && gy <= gb.MaxY + 0.04f)
                                {
                                    float halfW = (gb.MaxX - gb.MinX) * 0.5f;
                                    float halfH = (gb.MaxY - gb.MinY) * 0.5f;
                                    float cxBox = (gb.MinX + gb.MaxX) * 0.5f;
                                    float cyBox = (gb.MinY + gb.MaxY) * 0.5f;
                                    float padR = Mathf.Min(0.135f, Mathf.Min(halfW, halfH) * 0.48f);
                                    float bdx = Mathf.Max(0f, Mathf.Abs(gx - cxBox) - (halfW - padR));
                                    float bdy = Mathf.Max(0f, Mathf.Abs(gy - cyBox) - (halfH - padR));
                                    float bDist = Mathf.Sqrt(bdx * bdx + bdy * bdy) - padR;

                                    if (bDist <= aaStep)
                                    {
                                        float localY = Mathf.Clamp01((gy - gb.MinY) / Mathf.Max(0.01f, gb.MaxY - gb.MinY));
                                        Color32 gCol;
                                        if (bDist > -0.045f)
                                        {
                                            gCol = Color32.Lerp(gb.ShadowCol, colPitShadow, localY * 0.45f);
                                        }
                                        else if (localY < 0.24f)
                                        {
                                            gCol = Color32.Lerp(gb.HighlightCol, gb.MidLitCol, localY / 0.24f);
                                        }
                                        else if (localY > 0.78f)
                                        {
                                            gCol = Color32.Lerp(gb.BaseCol, gb.ShadowCol, (localY - 0.78f) / 0.22f);
                                        }
                                        else
                                        {
                                            gCol = Color32.Lerp(gb.MidLitCol, gb.BaseCol, (localY - 0.24f) / 0.54f);
                                        }

                                        // Top specular bevel rim inside gate pad
                                        float padDepth = Mathf.Max(0f, -bDist);
                                        if (padDepth > 0.012f && padDepth < 0.048f && localY < 0.35f)
                                        {
                                            gCol = Color32.Lerp(gCol, gb.RimCol, 0.55f);
                                        }

                                        if (bDist > -aaStep)
                                        {
                                            float tAA = Mathf.Clamp01((bDist + aaStep) / (2f * aaStep));
                                            interiorColor = Color32.Lerp(gCol, colPitDeep, tAA);
                                        }
                                        else
                                        {
                                            interiorColor = gCol;
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        if (ao > 0f)
                        {
                            interiorColor = Color32.Lerp(interiorColor, colPitShadow, ao);
                        }

                        pixels[py * texW + px] = interiorColor;
                        continue;
                    }

                    // Outside active cells: carved silver-lavender stone rim + concave fillets + Voronoi cracks
                    float dist = EvalWallSdf(gx, gy, activeMask, maskW, maskH, activeCols, activeRows);
                    if (dist > WallMarginCells)
                    {
                        pixels[py * texW + px] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    // Subtle organic rock-chipped outer edge variation
                    float nChip = Fbm2(gx * 2.6f + seedOffset, gy * 2.6f - seedOffset);
                    float notchN = Hash2D(Mathf.FloorToInt(gx * 1.4f + 13f), Mathf.FloorToInt(gy * 1.4f + 29f));
                    float chipAmt = Mathf.Clamp01(nChip - 0.55f) * 0.032f + (notchN > 0.88f ? 0.014f : 0f);
                    float effWall = WallThicknessCells - chipAmt;

                    // Compute outer drop shadow + lavender aura sample (offset down-right)
                    float distShadowSample = EvalWallSdf(gx - 0.032f, gy - 0.062f, activeMask, maskW, maskH, activeCols, activeRows);
                    Color32 outerBg = new Color32(0, 0, 0, 0);
                    float shBeyond = distShadowSample - effWall;
                    if (shBeyond > -0.02f && shBeyond < 0.145f)
                    {
                        float shAlpha = Mathf.Clamp01(1f - Mathf.Max(0f, shBeyond) / 0.145f);
                        shAlpha = shAlpha * shAlpha * 0.58f;
                        outerBg = new Color32(10, 6, 36, (byte)Mathf.RoundToInt(shAlpha * 255f));
                    }

                    float haloBeyond = dist - effWall;
                    if (haloBeyond > 0f && haloBeyond < 0.065f)
                    {
                        float hAlpha = Mathf.Clamp01(1f - haloBeyond / 0.065f);
                        hAlpha = hAlpha * hAlpha * 0.24f;
                        Color32 haloCol = new Color32(142, 160, 255, (byte)Mathf.RoundToInt(hAlpha * 255f));
                        outerBg = AlphaBlendOver(outerBg, haloCol);
                    }

                    if (dist > effWall + aaStep)
                    {
                        pixels[py * texW + px] = outerBg;
                        continue;
                    }

                    // Smooth finite-difference normal from SDF (points outward from pit)
                    const float gradEps = 0.012f;
                    float dxPos = EvalWallSdf(gx + gradEps, gy, activeMask, maskW, maskH, activeCols, activeRows);
                    float dxNeg = EvalWallSdf(gx - gradEps, gy, activeMask, maskW, maskH, activeCols, activeRows);
                    float dyPos = EvalWallSdf(gx, gy + gradEps, activeMask, maskW, maskH, activeCols, activeRows);
                    float dyNeg = EvalWallSdf(gx, gy - gradEps, activeMask, maskW, maskH, activeCols, activeRows);
                    float nx = dxPos - dxNeg;
                    float ny = dyPos - dyNeg;
                    float nLen = Mathf.Sqrt(nx * nx + ny * ny);
                    if (nLen > 1e-5f)
                    {
                        nx /= nLen;
                        ny /= nLen;
                    }
                    else
                    {
                        nx = 0f;
                        ny = -1f;
                    }

                    float uWall = Mathf.Clamp01(dist / effWall);

                    // 3D cross-section surface slope: inner lip -> rising inner bevel -> chiseled crown -> falling outer bevel -> outer ink rim
                    float radialTilt;
                    if (uWall < 0.065f)
                    {
                        radialTilt = -0.35f;
                    }
                    else if (uWall < 0.25f)
                    {
                        float tB = (uWall - 0.065f) / 0.185f;
                        radialTilt = -Mathf.Lerp(0.88f, 0.28f, tB);
                    }
                    else if (uWall < 0.72f)
                    {
                        radialTilt = 0.0f;
                    }
                    else
                    {
                        float tO = Mathf.Clamp01((uWall - 0.72f) / 0.24f);
                        radialTilt = Mathf.Lerp(0.24f, 0.92f, tO);
                    }

                    // Voronoi chiseled stone slab cracks + subtle rock micro-facet perturbation
                    float warpX = gx + (Fbm2(gx * 1.45f + seedOffset, gy * 1.45f) - 0.5f) * 0.22f;
                    float warpY = gy + (Fbm2(gx * 1.45f + 7.1f, gy * 1.45f + seedOffset) - 0.5f) * 0.22f;
                    VoronoiStone(warpX * 1.18f, warpY * 1.18f, out float slabEdgeDist, out float cellHash);

                    // Secondary finer rock crack branching near slab borders
                    float fineWarpX = gx + (Fbm2(gx * 2.8f + 3.2f, gy * 2.8f) - 0.5f) * 0.14f;
                    float fineWarpY = gy + (Fbm2(gx * 2.8f, gy * 2.8f + 9.4f) - 0.5f) * 0.14f;
                    VoronoiStone(fineWarpX * 2.35f + 4.3f, fineWarpY * 2.35f - 2.1f, out float fineEdgeDist, out float fineHash);

                    bool hasSubCrack = (cellHash > 0.42f && fineHash > 0.35f && slabEdgeDist > 0.04f && slabEdgeDist < 0.36f);
                    float combCrackDist = hasSubCrack ? Mathf.Min(slabEdgeDist, fineEdgeDist * 0.92f) : slabEdgeDist;

                    // Directional light-shifted crack sample to paint crisp top-left / lower-right rock lip catchlights
                    float shift = 0.030f;
                    VoronoiStone((warpX - shift * 0.65f) * 1.18f, (warpY - shift * 0.80f) * 1.18f, out float slabShift, out _);
                    VoronoiStone((fineWarpX - shift * 0.65f) * 2.35f + 4.3f, (fineWarpY - shift * 0.80f) * 2.35f - 2.1f, out float fineShift, out _);
                    float combShift = hasSubCrack ? Mathf.Min(slabShift, fineShift * 0.92f) : slabShift;

                    // Perturb surface normal subtly per slab & rock grain
                    float rockGrain = (Fbm2(gx * 4.2f + seedOffset, gy * 4.2f) - 0.5f) * 0.22f;
                    float facetBias = (cellHash - 0.5f) * 0.26f;
                    Vector3 surfN = new Vector3(
                        nx * radialTilt + rockGrain * 0.4f + facetBias * 0.5f,
                        ny * radialTilt + rockGrain * 0.4f - facetBias * 0.4f,
                        1.0f - Mathf.Abs(radialTilt) * 0.42f
                    ).normalized;

                    float ndl = Mathf.Clamp01(Vector3.Dot(surfN, lightDir) * 0.72f + 0.38f + facetBias * 0.22f);

                    Color32 stoneCol;
                    if (ndl < 0.42f)
                        stoneCol = Color32.Lerp(colStoneShadow, colStoneMid, ndl / 0.42f);
                    else if (ndl < 0.78f)
                        stoneCol = Color32.Lerp(colStoneMid, colStoneLit, (ndl - 0.42f) / 0.36f);
                    else
                        stoneCol = Color32.Lerp(colStoneLit, colStoneSpec, (ndl - 0.78f) / 0.22f);

                    // Crisp silver specular catchlights on outer top-left bevel and inner bottom-right rising lip
                    if (uWall >= 0.78f && uWall <= 0.94f)
                    {
                        float rimFace = Mathf.Clamp01((-nx * 0.56f - ny * 0.72f) * 0.85f + 0.25f);
                        if (rimFace > 0.35f)
                        {
                            stoneCol = Color32.Lerp(stoneCol, colStoneSpec, (rimFace - 0.35f) * 1.15f);
                        }
                    }
                    else if (uWall >= 0.08f && uWall <= 0.24f)
                    {
                        float innerFace = Mathf.Clamp01((nx * 0.56f + ny * 0.72f) * 0.85f + 0.20f);
                        if (innerFace > 0.32f)
                        {
                            stoneCol = Color32.Lerp(stoneCol, colStoneSpec, (innerFace - 0.32f) * 0.95f);
                        }
                    }

                    // Inner pit cavity dark lip & outer crisp dark-violet outline
                    if (uWall < 0.075f)
                    {
                        float tPit = 1f - uWall / 0.075f;
                        stoneCol = Color32.Lerp(stoneCol, colInnerLipDark, tPit * 0.92f);
                    }
                    else if (uWall > 0.935f)
                    {
                        float tInk = (uWall - 0.935f) / 0.065f;
                        stoneCol = Color32.Lerp(stoneCol, colOuterInk, tInk * 0.82f);
                    }

                    // Overlay carved Voronoi slab cracks + bright highlight lips
                    if (uWall > 0.07f && uWall < 0.95f)
                    {
                        if (combCrackDist < 0.030f)
                        {
                            float tCrack = 1f - combCrackDist / 0.030f;
                            stoneCol = Color32.Lerp(stoneCol, colCrackDark, Mathf.Clamp01(tCrack * 1.15f) * 0.88f);
                        }
                        else if (combCrackDist < 0.082f && combCrackDist > combShift + 0.004f)
                        {
                            float lip = 1f - Mathf.Abs(combCrackDist - 0.048f) / 0.034f;
                            float dirWeight = Mathf.Clamp01((combCrackDist - combShift) * 32f);
                            stoneCol = Color32.Lerp(stoneCol, colCrackLip, Mathf.Clamp01(lip) * dirWeight * 0.72f);
                        }
                    }

                    // Anti-aliased outer blend over drop shadow / halo
                    if (dist > effWall - aaStep)
                    {
                        float coverage = Mathf.Clamp01(1f - (dist - (effWall - aaStep)) / (2f * aaStep));
                        stoneCol.a = (byte)Mathf.RoundToInt(coverage * 255f);
                        pixels[py * texW + px] = AlphaBlendOver(outerBg, stoneCol);
                    }
                    else
                    {
                        stoneCol.a = 255;
                        pixels[py * texW + px] = stoneCol;
                    }
                }
            }

            Texture2D tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static float EvalWallSdf(float gx, float gy, bool[] activeMask, int maskW, int maskH, int activeCols, int activeRows)
        {
            int ic = Mathf.FloorToInt(gx);
            int ir = Mathf.FloorToInt(gy);
            int mx = ic + 1;
            int my = ir + 1;

            float minDistSq = 99f;
            for (int dr = -1; dr <= 1; dr++)
            {
                int nmy = my + dr;
                if (nmy < 0 || nmy >= maskH) continue;
                int nr = ir + dr;
                float clampY = Mathf.Clamp(gy, nr, nr + 1f);
                float dy = gy - clampY;

                for (int dc = -1; dc <= 1; dc++)
                {
                    int nmx = mx + dc;
                    if (nmx < 0 || nmx >= maskW) continue;
                    if (!activeMask[nmy * maskW + nmx]) continue;

                    int nc = ic + dc;
                    float clampX = Mathf.Clamp(gx, nc, nc + 1f);
                    float dx = gx - clampX;
                    float d2 = dx * dx + dy * dy;
                    if (d2 < minDistSq) minDistSq = d2;
                }
            }

            float dEuclid = Mathf.Sqrt(minDistSq);
            if (dEuclid <= 0.045f || dEuclid > WallMarginCells + 0.10f)
            {
                return dEuclid;
            }

            // C1 circular concave fillets in empty cells where two orthogonal adjacent neighbors are active
            if (mx >= 1 && mx < maskW - 1 && my >= 1 && my < maskH - 1)
            {
                bool aLeft = activeMask[my * maskW + (mx - 1)];
                bool aRight = activeMask[my * maskW + (mx + 1)];
                bool aTop = activeMask[(my - 1) * maskW + mx];
                bool aBot = activeMask[(my + 1) * maskW + mx];

                if ((aLeft || aRight) && (aTop || aBot))
                {
                    float fx = gx - ic;
                    float fy = gy - ir;
                    float filletCenter = WallThicknessCells + ConcaveFilletRadius;
                    float bestFillet = dEuclid;

                    if (aLeft && aTop)
                    {
                        float dF = EvalCornerFillet(fx, fy, filletCenter);
                        if (dF < bestFillet) bestFillet = dF;
                    }
                    if (aRight && aTop)
                    {
                        float dF = EvalCornerFillet(1f - fx, fy, filletCenter);
                        if (dF < bestFillet) bestFillet = dF;
                    }
                    if (aLeft && aBot)
                    {
                        float dF = EvalCornerFillet(fx, 1f - fy, filletCenter);
                        if (dF < bestFillet) bestFillet = dF;
                    }
                    if (aRight && aBot)
                    {
                        float dF = EvalCornerFillet(1f - fx, 1f - fy, filletCenter);
                        if (dF < bestFillet) bestFillet = dF;
                    }

                    if (bestFillet < dEuclid)
                    {
                        float blend = SmoothStep(0.05f, 0.20f, dEuclid);
                        return Mathf.Lerp(dEuclid, Mathf.Max(0.05f, bestFillet), blend);
                    }
                }
            }

            return dEuclid;
        }

        private static float EvalCornerFillet(float u, float v, float c)
        {
            if (u <= c && v <= c)
            {
                float du = c - u;
                float dv = c - v;
                return c - Mathf.Sqrt(du * du + dv * dv);
            }
            return 99f;
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static float Hash2D(int ix, int iy)
        {
            unchecked
            {
                uint h = (uint)(ix * 374761393 + iy * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= (h >> 16);
                return (h & 0xFFFFu) / 65535f;
            }
        }

        private static float ValueNoise2D(float x, float y)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;
            float ux = fx * fx * (3f - 2f * fx);
            float uy = fy * fy * (3f - 2f * fy);

            float v00 = Hash2D(ix, iy);
            float v10 = Hash2D(ix + 1, iy);
            float v01 = Hash2D(ix, iy + 1);
            float v11 = Hash2D(ix + 1, iy + 1);

            float a = v00 + (v10 - v00) * ux;
            float b = v01 + (v11 - v01) * ux;
            return a + (b - a) * uy;
        }

        private static float Fbm2(float x, float y)
        {
            float v = ValueNoise2D(x, y) * 0.65f;
            v += ValueNoise2D(x * 2.13f + 5.7f, y * 2.13f - 3.1f) * 0.35f;
            return v;
        }

        private static void VoronoiStone(float x, float y, out float edgeDist, out float cellHash)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;

            float d1 = 99f;
            float d2 = 99f;
            float bestHash = 0.5f;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int cx = ix + dx;
                    int cy = iy + dy;
                    float jx = 0.14f + 0.72f * Hash2D(cx * 3 + 17, cy * 5 + 31);
                    float jy = 0.14f + 0.72f * Hash2D(cx * 7 + 53, cy * 11 + 19);
                    float px = dx + jx - fx;
                    float py = dy + jy - fy;
                    float dist = Mathf.Sqrt(px * px + py * py);
                    if (dist < d1)
                    {
                        d2 = d1;
                        d1 = dist;
                        bestHash = Hash2D(cx + 97, cy + 131);
                    }
                    else if (dist < d2)
                    {
                        d2 = dist;
                    }
                }
            }

            edgeDist = d2 - d1;
            cellHash = bestHash;
        }

        private static Color SaturateNeon(Color col)
        {
            Color.RGBToHSV(col, out float h, out float s, out float v);
            s = Mathf.Clamp01(Mathf.Max(s, 0.78f));
            v = Mathf.Clamp01(Mathf.Max(v, 0.90f));
            return Color.HSVToRGB(h, s, v);
        }

        private static Color32 AlphaBlendOver(Color32 dst, Color32 src)
        {
            float sa = src.a / 255f;
            if (sa <= 0f) return dst;
            if (sa >= 1f) return src;
            float da = dst.a / 255f;
            float outA = sa + da * (1f - sa);
            if (outA <= 1e-4f) return new Color32(0, 0, 0, 0);

            byte r = (byte)Mathf.Clamp(Mathf.RoundToInt((src.r * sa + dst.r * da * (1f - sa)) / outA), 0, 255);
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt((src.g * sa + dst.g * da * (1f - sa)) / outA), 0, 255);
            byte b = (byte)Mathf.Clamp(Mathf.RoundToInt((src.b * sa + dst.b * da * (1f - sa)) / outA), 0, 255);
            byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(outA * 255f), 0, 255);
            return new Color32(r, g, b, a);
        }
    }
}
