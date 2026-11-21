using UnityEngine;

namespace BlockMergePuzzle.Utils
{
    public static class SpriteGenerator
    {
        private static Sprite _cachedBlockSprite;
        private static Sprite _cachedSlotSprite;
        private static Sprite _cachedCircleSprite;
        private static Sprite _cachedBannerSprite;
        private static Sprite _cachedRadialGlowSprite;
        private static Sprite _cachedStarFlareSprite;
        private static Sprite _cachedShardSprite;
        private static Sprite _cachedSettingRowSprite;
        private static Sprite _cachedLevelBannerSprite;
        private static Sprite _cachedIceOverlaySprite;
        private static Sprite _cachedVictoryCardSprite;
        private static Sprite _cachedVictoryRibbonSprite;
        private static Sprite _cachedVictoryNextBtnSprite;
        private static Sprite _cachedVictoryReplayBtnSprite;
        private static Sprite _cachedGoldenStarSprite;

        public static Sprite GetVictoryCardSprite()
        {
            if (_cachedVictoryCardSprite != null) return _cachedVictoryCardSprite;
            _cachedVictoryCardSprite = CreateVictoryCardSprite(640, 520, 42);
            return _cachedVictoryCardSprite;
        }

        public static Sprite GetVictoryRibbonSprite()
        {
            if (_cachedVictoryRibbonSprite != null) return _cachedVictoryRibbonSprite;
            _cachedVictoryRibbonSprite = CreateVictoryRibbonSprite(520, 112);
            return _cachedVictoryRibbonSprite;
        }

        public static Sprite GetVictoryNextButtonSprite()
        {
            if (_cachedVictoryNextBtnSprite != null) return _cachedVictoryNextBtnSprite;
            _cachedVictoryNextBtnSprite = CreateGlossyButtonSprite(
                520, 124, 34,
                new Color(0.27f, 0.94f, 0.52f, 1f),
                new Color(0.10f, 0.72f, 0.34f, 1f),
                new Color(0.05f, 0.45f, 0.21f, 1f),
                new Color(0.62f, 1.00f, 0.75f, 1f));
            return _cachedVictoryNextBtnSprite;
        }

        public static Sprite GetVictoryReplayButtonSprite()
        {
            if (_cachedVictoryReplayBtnSprite != null) return _cachedVictoryReplayBtnSprite;
            _cachedVictoryReplayBtnSprite = CreateGlossyButtonSprite(
                440, 104, 30,
                new Color(0.35f, 0.28f, 0.64f, 1f),
                new Color(0.22f, 0.16f, 0.44f, 1f),
                new Color(0.13f, 0.09f, 0.28f, 1f),
                new Color(0.58f, 0.50f, 0.92f, 1f));
            return _cachedVictoryReplayBtnSprite;
        }

        public static Sprite GetGoldenStarSprite()
        {
            if (_cachedGoldenStarSprite != null) return _cachedGoldenStarSprite;
            _cachedGoldenStarSprite = CreateGoldenStarSprite(128);
            return _cachedGoldenStarSprite;
        }

        public static Sprite GetIceOverlaySprite()
        {
            if (_cachedIceOverlaySprite != null) return _cachedIceOverlaySprite;
            _cachedIceOverlaySprite = CreateIceOverlaySprite(256, 256, 36);
            return _cachedIceOverlaySprite;
        }

        public static Sprite GetLevelBannerSprite()
        {
            if (_cachedLevelBannerSprite != null) return _cachedLevelBannerSprite;
            _cachedLevelBannerSprite = CreateLevelBannerSprite(320, 72);
            return _cachedLevelBannerSprite;
        }

        public static Sprite GetSettingRowSprite()
        {
            if (_cachedSettingRowSprite != null) return _cachedSettingRowSprite;
            _cachedSettingRowSprite = CreateSettingRowSprite(640, 128, 28);
            return _cachedSettingRowSprite;
        }

        public static Sprite GetShardSprite()
        {
            if (_cachedShardSprite != null) return _cachedShardSprite;
            _cachedShardSprite = CreateSquareShardSprite(16);
            return _cachedShardSprite;
        }

        public static Sprite GetRadialGlowSprite()
        {
            if (_cachedRadialGlowSprite != null) return _cachedRadialGlowSprite;
            _cachedRadialGlowSprite = CreateRadialGlowSprite(128);
            return _cachedRadialGlowSprite;
        }

        public static Sprite GetStarFlareSprite()
        {
            if (_cachedStarFlareSprite != null) return _cachedStarFlareSprite;
            _cachedStarFlareSprite = CreateStarFlareSprite(128);
            return _cachedStarFlareSprite;
        }

        public static Sprite GetBannerSprite()
        {
            if (_cachedBannerSprite != null) return _cachedBannerSprite;
            _cachedBannerSprite = CreateRoundedRectSprite(256, 64, 28, Color.white);
            return _cachedBannerSprite;
        }

        public static Sprite GetBlockSprite()
        {
            if (_cachedBlockSprite != null) return _cachedBlockSprite;
            _cachedBlockSprite = CreateRoundedRectSprite(128, 128, 24, Color.white);
            return _cachedBlockSprite;
        }

        public static Sprite GetSlotSprite()
        {
            if (_cachedSlotSprite != null) return _cachedSlotSprite;
            _cachedSlotSprite = CreateRoundedSlotSocketSprite(128, 128, 24);
            return _cachedSlotSprite;
        }

        public static Sprite GetCircleSprite()
        {
            if (_cachedCircleSprite != null) return _cachedCircleSprite;
            _cachedCircleSprite = CreateCircleSprite(64, Color.white);
            return _cachedCircleSprite;
        }

        private static Sprite CreateRoundedSlotSocketSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;
            int borderWidth = 4;

            Color socketColor = new Color(0.12f, 0.14f, 0.19f, 0.85f);
            Color rimColor = new Color(0.24f, 0.28f, 0.38f, 0.7f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distSq = dx * dx + dy * dy;
                    if (distSq <= r * r)
                    {
                        float dist = Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);

                        // Check if near border edge
                        bool isBorder = (x < borderWidth || x >= width - borderWidth ||
                                         y < borderWidth || y >= height - borderWidth ||
                                         dist >= r - borderWidth);

                        Color fill = isBorder ? rimColor : socketColor;
                        pixels[y * width + x] = new Color(fill.r, fill.g, fill.b, fill.a * alpha);
                    }
                    else if (dx > 0 && dy > 0)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        bool isBorder = (x < borderWidth || x >= width - borderWidth ||
                                         y < borderWidth || y >= height - borderWidth);
                        Color fill = isBorder ? rimColor : socketColor;
                        pixels[y * width + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRoundedRectSprite(int width, int height, int radius, Color fillColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distSq = dx * dx + dy * dy;
                    if (distSq <= r * r)
                    {
                        float dist = Mathf.Sqrt(distSq);
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        pixels[y * width + x] = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha);
                    }
                    else if (dx > 0 && dy > 0)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        pixels[y * width + x] = fillColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(r + 0.5f - dist);
                        pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRadialGlowSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / r);
                    float alpha = Mathf.Pow(1f - t, 2.2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateStarFlareSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            float r = size * 0.5f;
            Vector2 center = new Vector2(r - 0.5f, r - 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - center.x) / r;
                    float ny = Mathf.Abs(y - center.y) / r;
                    float distN = Mathf.Sqrt(nx * nx + ny * ny);
                    float core = Mathf.Pow(Mathf.Clamp01(1f - distN * 2.2f), 2f);
                    float rayH = Mathf.Clamp01(1f - nx) * Mathf.Pow(Mathf.Clamp01(1f - ny * 6.5f), 2f);
                    float rayV = Mathf.Clamp01(1f - ny) * Mathf.Pow(Mathf.Clamp01(1f - nx * 6.5f), 2f);
                    float alpha = Mathf.Clamp01(core + (rayH + rayV) * 0.85f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSquareShardSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x == 0 || x == size - 1 || y == 0 || y == size - 1);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, isBorder ? 0.65f : 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateSettingRowSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;
            float borderThickness = 3.2f;

            Color fillTop = new Color(0.32f, 0.24f, 0.56f, 0.94f);
            Color fillBot = new Color(0.27f, 0.20f, 0.49f, 0.96f);
            Color borderColor = new Color(0.49f, 0.38f, 0.78f, 0.92f);

            for (int y = 0; y < height; y++)
            {
                float vt = (float)y / (height - 1);
                Color fill = Color.Lerp(fillBot, fillTop, vt);

                for (int x = 0; x < width; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distToEdge;
                    if (dx > 0f && dy > 0f)
                    {
                        distToEdge = r - Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    else
                    {
                        float dLeft = x;
                        float dRight = (width - 1) - x;
                        float dBot = y;
                        float dTop = (height - 1) - y;
                        distToEdge = Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBot, dTop));
                    }

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        float aa = Mathf.Clamp01(distToEdge + 0.5f);
                        float borderBlend = 1f - Mathf.Clamp01((distToEdge - (borderThickness - 1f)) / 1.5f);
                        Color c = Color.Lerp(fill, borderColor, borderBlend);
                        pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Sprite CreateLevelBannerSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float chamfer = 15f;
            float borderOut = 2.0f;
            float bevel = 5.5f;

            // Matching UI_Bn Time.png stone tablet palette
            Color outerDark = new Color(0.09f, 0.09f, 0.22f, 0.96f);
            Color rimTop = new Color(0.38f, 0.43f, 0.72f, 1.0f);
            Color rimBot = new Color(0.22f, 0.20f, 0.44f, 1.0f);
            Color bodyTop = new Color(0.19f, 0.42f, 0.52f, 0.98f); // Teal slate
            Color bodyBot = new Color(0.17f, 0.20f, 0.42f, 0.98f); // Deep indigo slate

            for (int y = 0; y < height; y++)
            {
                float vt = (float)y / (height - 1);
                Color bodyCol = Color.Lerp(bodyBot, bodyTop, vt);
                Color rimCol = Color.Lerp(rimBot, rimTop, vt);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Min(x, (width - 1) - x);
                    float dy = Mathf.Min(y, (height - 1) - y);
                    // Chamfered octagonal stone silhouette
                    float diag = (dx + dy - chamfer) * 0.7071f;
                    float distToEdge = Mathf.Min(Mathf.Min(dx, dy), diag);

                    // Subtle carved stone notch on top-left & bottom-right matching UI_Bn Time
                    float notch1 = Mathf.Abs((x - width * 0.14f) + (height - 1 - y) * 0.6f);
                    float notch2 = Mathf.Abs((x - width * 0.86f) - y * 0.6f);
                    bool isNotch = (y > height - 9 && notch1 < 1.3f) || (y < 9 && notch2 < 1.3f);

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else
                    {
                        float aa = Mathf.Clamp01(distToEdge + 0.5f);
                        Color c;
                        if (distToEdge < borderOut)
                        {
                            c = outerDark;
                        }
                        else if (distToEdge < borderOut + bevel)
                        {
                            float tRim = Mathf.Clamp01((distToEdge - borderOut) / bevel);
                            c = Color.Lerp(outerDark, rimCol, 0.45f + 0.55f * tRim);
                        }
                        else
                        {
                            float innerT = Mathf.Clamp01((distToEdge - (borderOut + bevel)) / 2.0f);
                            c = Color.Lerp(rimCol, bodyCol, innerT);
                        }

                        if (isNotch && distToEdge >= borderOut)
                        {
                            c = Color.Lerp(c, outerDark, 0.55f);
                        }

                        pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            Vector2 proj = a + t * ab;
            return Vector2.Distance(p, proj);
        }

        private static Sprite CreateIceOverlaySprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float r = radius;

            // Frosty periwinkle-blue ice palette matching reference image
            Color iceTop = new Color(0.51f, 0.66f, 0.96f, 0.96f);
            Color iceMid = new Color(0.42f, 0.56f, 0.90f, 0.96f);
            Color iceBot = new Color(0.33f, 0.47f, 0.83f, 0.97f);
            Color bevelLight = new Color(0.70f, 0.85f, 1.00f, 0.98f);
            Color bevelDark = new Color(0.24f, 0.37f, 0.74f, 0.98f);
            Color topRimWhite = new Color(0.88f, 0.98f, 1.00f, 0.99f);
            Color crackGlowCol = new Color(0.38f, 0.93f, 1.00f, 1.00f);
            Color crackCoreCol = new Color(0.90f, 0.99f, 1.00f, 1.00f);

            // Crack line segments in normalized UV (0..1)
            Vector2[,] segments = new Vector2[,]
            {
                // Main diagonal bottom-left to top-right
                { new Vector2(0.06f, 0.14f), new Vector2(0.27f, 0.20f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.54f, 0.49f) },
                { new Vector2(0.54f, 0.49f), new Vector2(0.93f, 0.88f) },
                // Top-left to bottom-right branch
                { new Vector2(0.09f, 0.91f), new Vector2(0.36f, 0.67f) },
                { new Vector2(0.36f, 0.67f), new Vector2(0.66f, 0.43f) },
                { new Vector2(0.66f, 0.43f), new Vector2(0.92f, 0.23f) },
                // Steep right fracture
                { new Vector2(0.91f, 0.86f), new Vector2(0.77f, 0.46f) },
                { new Vector2(0.77f, 0.46f), new Vector2(0.67f, 0.07f) },
                // Bottom-left star fracture branches
                { new Vector2(0.27f, 0.20f), new Vector2(0.18f, 0.05f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.45f, 0.14f) },
                { new Vector2(0.27f, 0.20f), new Vector2(0.16f, 0.42f) },
                // Upper-left frost sub-branch
                { new Vector2(0.36f, 0.67f), new Vector2(0.54f, 0.88f) },
                { new Vector2(0.16f, 0.42f), new Vector2(0.06f, 0.56f) }
            };
            int segCount = segments.GetLength(0);

            // Frost intersection nodes (bright icy hotspots)
            Vector2[] nodes = new Vector2[]
            {
                new Vector2(0.27f, 0.20f),
                new Vector2(0.10f, 0.89f),
                new Vector2(0.77f, 0.46f),
                new Vector2(0.90f, 0.85f)
            };

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);

                    float dx = 0f;
                    float dy = 0f;
                    if (x < r) dx = r - x;
                    else if (x > width - r - 1) dx = x - (width - r - 1);
                    if (y < r) dy = r - y;
                    else if (y > height - r - 1) dy = y - (height - r - 1);

                    float distToEdge;
                    if (dx > 0f && dy > 0f)
                    {
                        distToEdge = r - Mathf.Sqrt(dx * dx + dy * dy);
                    }
                    else
                    {
                        float dL = x;
                        float dR = (width - 1) - x;
                        float dB = y;
                        float dT = (height - 1) - y;
                        distToEdge = Mathf.Min(Mathf.Min(dL, dR), Mathf.Min(dB, dT));
                    }

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);

                    // Base vertical gradient
                    Color col = ny > 0.5f
                        ? Color.Lerp(iceMid, iceTop, (ny - 0.5f) * 2f)
                        : Color.Lerp(iceBot, iceMid, ny * 2f);

                    // Gem-like facet bevels (top/left lighter, bottom/right darker)
                    float bevelW = 24f;
                    if (distToEdge < bevelW)
                    {
                        float bt = 1f - (distToEdge / bevelW);
                        float lightWeight = Mathf.Clamp01((ny - nx + 0.35f) * 0.85f);
                        Color facetCol = Color.Lerp(bevelDark, bevelLight, lightWeight);
                        col = Color.Lerp(col, facetCol, bt * 0.52f);
                    }

                    // Upper-left crystal corner facet sheen
                    float cornerDiag = (1f - nx) + ny; // max 2 at top-left
                    if (cornerDiag > 1.52f && distToEdge >= 10f && distToEdge <= 44f)
                    {
                        float sheen = Mathf.Clamp01((cornerDiag - 1.52f) / 0.25f) * 0.45f;
                        col = Color.Lerp(col, topRimWhite, sheen);
                    }

                    // Top specular ice rim bar
                    if (ny > 0.93f && nx > 0.14f && nx < 0.86f)
                    {
                        float topT = Mathf.Clamp01((ny - 0.93f) / 0.055f);
                        col = Color.Lerp(col, topRimWhite, topT * 0.78f);
                    }

                    // Frost loop subtle ring on middle-left (matching image detail)
                    Vector2 uv = new Vector2(nx, ny);
                    float loopDist = Mathf.Abs(Vector2.Distance(uv, new Vector2(0.16f, 0.56f)) - 0.14f);
                    if (loopDist < 0.035f)
                    {
                        float lt = 1f - (loopDist / 0.035f);
                        col = Color.Lerp(col, crackGlowCol, lt * lt * 0.38f);
                    }

                    // Glowing ice crack veins with organic curvature
                    Vector2 warpedUv = new Vector2(
                        uv.x + Mathf.Sin(uv.y * 11.5f + uv.x * 6.0f) * 0.018f,
                        uv.y + Mathf.Cos(uv.x * 10.5f - uv.y * 7.0f) * 0.018f);
                    float minSegDist = 10f;
                    for (int s = 0; s < segCount; s++)
                    {
                        float d = DistanceToSegment(warpedUv, segments[s, 0], segments[s, 1]);
                        if (d < minSegDist) minSegDist = d;
                    }

                    float crackD = Mathf.Max(0f, minSegDist);

                    if (crackD < 0.058f)
                    {
                        float glowT = 1f - (crackD / 0.058f);
                        col = Color.Lerp(col, crackGlowCol, glowT * glowT * 0.62f);
                    }
                    if (crackD < 0.016f)
                    {
                        float coreT = 1f - (crackD / 0.016f);
                        col = Color.Lerp(col, crackCoreCol, coreT * coreT * 0.88f);
                    }

                    // Frost hotspots at vein intersections
                    for (int n = 0; n < nodes.Length; n++)
                    {
                        float nd = Vector2.Distance(uv, nodes[n]);
                        float rad = (n == 0) ? 0.11f : 0.075f;
                        if (nd < rad)
                        {
                            float nt = 1f - (nd / rad);
                            col = Color.Lerp(col, crackCoreCol, nt * nt * 0.72f);
                        }
                    }

                    // Crisp outer ice rim
                    if (distToEdge < 3.0f)
                    {
                        float rimT = 1f - (distToEdge / 3.0f);
                        Color outerRim = Color.Lerp(bevelDark, bevelLight, ny);
                        col = Color.Lerp(col, outerRim, rimT * 0.65f);
                    }

                    pixels[y * width + x] = new Color(col.r, col.g, col.b, col.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float ComputeRoundedRectEdgeDist(int x, int y, int width, int height, float radius)
        {
            float dx = 0f;
            float dy = 0f;
            if (x < radius) dx = radius - x;
            else if (x > width - radius - 1) dx = x - (width - radius - 1);

            if (y < radius) dy = radius - y;
            else if (y > height - radius - 1) dy = y - (height - radius - 1);

            if (dx > 0f && dy > 0f)
            {
                return radius - Mathf.Sqrt(dx * dx + dy * dy);
            }

            float dLeft = x;
            float dRight = (width - 1) - x;
            float dBot = y;
            float dTop = (height - 1) - y;
            return Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBot, dTop));
        }

        private static Sprite CreateVictoryCardSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color outlineCol = new Color(0.063f, 0.035f, 0.149f, 0.99f); // #100926
            Color rimTop = new Color(0.957f, 0.784f, 0.314f, 1.0f);     // #F4C850
            Color rimMid = new Color(0.478f, 0.361f, 0.878f, 1.0f);     // #7A5CE0
            Color rimBot = new Color(0.231f, 0.714f, 1.000f, 1.0f);     // #3BB6FF
            Color grooveCol = new Color(0.078f, 0.051f, 0.200f, 0.98f); // #140D33
            Color fillBot = new Color(0.118f, 0.086f, 0.271f, 0.98f);   // #1E1645
            Color fillTop = new Color(0.200f, 0.145f, 0.439f, 0.98f);   // #332570
            Color sheenCol = new Color(0.46f, 0.38f, 0.85f, 1.0f);
            Color warmSpotCol = new Color(0.50f, 0.33f, 0.65f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color outerBevel = ny >= 0.5f
                    ? Color.Lerp(rimMid, rimTop, (ny - 0.5f) * 2f)
                    : Color.Lerp(rimBot, rimMid, ny * 2f);
                Color bodyBase = Color.Lerp(fillBot, fillTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 3.5f)
                    {
                        c = outlineCol;
                    }
                    else if (distToEdge < 11.5f)
                    {
                        float rimT = (distToEdge - 3.5f) / 8.0f;
                        c = Color.Lerp(outerBevel * 0.85f, outerBevel, Mathf.Sin(rimT * Mathf.PI));
                    }
                    else if (distToEdge < 15.0f)
                    {
                        float grooveT = (distToEdge - 11.5f) / 3.5f;
                        c = Color.Lerp(grooveCol, bodyBase, grooveT);
                    }
                    else
                    {
                        float nx = (float)x / (width - 1);
                        c = bodyBase;

                        // Subtle diagonal crystal sheen
                        float diag = Mathf.Abs((nx * 0.75f + (1f - ny) * 0.65f) - 0.62f);
                        if (diag < 0.16f)
                        {
                            float sT = 1f - (diag / 0.16f);
                            c = Color.Lerp(c, sheenCol, sT * sT * 0.16f);
                        }

                        // Soft warm glow behind stars upper area
                        float spotDist = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.72f));
                        if (spotDist < 0.42f)
                        {
                            float wT = 1f - (spotDist / 0.42f);
                            c = Color.Lerp(c, warmSpotCol, wT * wT * 0.28f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(48f, 48f, 48f, 48f));
        }

        private static Sprite CreateVictoryRibbonSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float chamfer = 22f;
            Color borderCol = new Color(0.369f, 0.169f, 0.024f, 1.0f); // #5E2B06
            Color specCol = new Color(1.000f, 0.975f, 0.760f, 1.0f);
            Color goldTop = new Color(1.000f, 0.902f, 0.427f, 1.0f);   // #FFE66D
            Color goldBot = new Color(0.941f, 0.608f, 0.114f, 1.0f);   // #F09B1D

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyCol = Color.Lerp(goldBot, goldTop, ny);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Min(x, (width - 1) - x);
                    float dy = Mathf.Min(y, (height - 1) - y);
                    float diag = (dx + dy - chamfer) * 0.7071f;
                    float distToEdge = Mathf.Min(Mathf.Min(dx, dy), diag);

                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 4.5f)
                    {
                        c = borderCol;
                    }
                    else if (distToEdge < 9.0f)
                    {
                        float rT = (distToEdge - 4.5f) / 4.5f;
                        Color rimCol = ny > 0.45f ? specCol : Color.Lerp(borderCol, goldBot, 0.65f);
                        c = Color.Lerp(rimCol, bodyCol, rT * 0.6f);
                    }
                    else
                    {
                        c = bodyCol;
                        if (ny > 0.74f)
                        {
                            float hiT = (ny - 0.74f) / 0.26f;
                            c = Color.Lerp(c, specCol, hiT * 0.35f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(36f, 26f, 36f, 26f));
        }

        private static Sprite CreateGlossyButtonSprite(int width, int height, int radius, Color topCol, Color midCol, Color lipCol, Color rimCol)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color darkOutline = new Color(0.05f, 0.04f, 0.13f, 0.96f);
            float lipHeight = 14f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                Color bodyFill = Color.Lerp(midCol, topCol, Mathf.Clamp01((ny - 0.10f) / 0.85f));

                for (int x = 0; x < width; x++)
                {
                    float distToEdge = ComputeRoundedRectEdgeDist(x, y, width, height, radius);
                    if (distToEdge < -0.5f)
                    {
                        pixels[y * width + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(distToEdge + 0.5f);
                    Color c;
                    if (distToEdge < 2.8f)
                    {
                        c = darkOutline;
                    }
                    else if (y < lipHeight)
                    {
                        float lipBlend = Mathf.Clamp01((y - (lipHeight - 3.5f)) / 3.5f);
                        c = Color.Lerp(lipCol, bodyFill, lipBlend);
                    }
                    else
                    {
                        c = bodyFill;
                        if (distToEdge < 6.0f && ny > 0.35f)
                        {
                            float edgeT = 1f - ((distToEdge - 2.8f) / 3.2f);
                            c = Color.Lerp(c, rimCol, edgeT * 0.75f);
                        }
                        if (ny > 0.72f)
                        {
                            float specT = (ny - 0.72f) / 0.28f;
                            c = Color.Lerp(c, rimCol, specT * 0.38f);
                        }
                    }

                    pixels[y * width + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(38f, 38f, 38f, 38f));
        }

        private static Sprite CreateGoldenStarSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float outerR = size * 0.422f; // ~54 at 128px
            float innerR = size * 0.180f; // ~23 at 128px
            float sectorAngle = Mathf.PI / 5f;
            float sinSector = Mathf.Sin(sectorAngle);
            float cosSector = Mathf.Cos(sectorAngle);

            Vector2[] verts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI * 0.5f + i * sectorAngle;
                float r = (i % 2 == 0) ? outerR : innerR;
                verts[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }

            Color centerGold = new Color(1.000f, 0.969f, 0.698f, 1.0f); // #FFF7B2
            Color midGold = new Color(1.000f, 0.788f, 0.161f, 1.0f);    // #FFC929
            Color deepGold = new Color(0.871f, 0.478f, 0.039f, 1.0f);   // #DE7A0A
            Color rimOutline = new Color(0.49f, 0.23f, 0.02f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center, y - center);
                    float dist = p.magnitude;

                    float rawAngle = Mathf.Atan2(p.y, p.x) - Mathf.PI * 0.5f;
                    if (rawAngle < 0f) rawAngle += Mathf.PI * 2f;
                    int sectorIdx = Mathf.FloorToInt(rawAngle / sectorAngle) % 10;
                    float localAngle = rawAngle - sectorIdx * sectorAngle;
                    float phi = (sectorIdx % 2 == 0) ? localAngle : (sectorAngle - localAngle);

                    float denom = innerR * sinSector * Mathf.Cos(phi) + (outerR - innerR * cosSector) * Mathf.Sin(phi);
                    float boundR = denom > 0.0001f ? (outerR * innerR * sinSector) / denom : outerR;
                    bool inside = dist <= boundR;

                    float minEdgeDist = 1000f;
                    for (int i = 0; i < 10; i++)
                    {
                        float d = DistanceToSegment(p, verts[i], verts[(i + 1) % 10]);
                        if (d < minEdgeDist) minEdgeDist = d;
                    }

                    float signedDist = inside ? minEdgeDist : -minEdgeDist;
                    if (signedDist < -0.6f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float aa = Mathf.Clamp01(signedDist + 0.6f);
                    float rNorm = Mathf.Clamp01(dist / outerR);
                    Color c = Color.Lerp(centerGold, midGold, Mathf.Clamp01(rNorm * 1.15f));

                    // Subtle 3D facet shading per alternating half-ray
                    if (sectorIdx % 2 == 0)
                    {
                        c = Color.Lerp(c, Color.white, 0.16f);
                    }
                    else
                    {
                        c = Color.Lerp(c, deepGold, 0.24f);
                    }

                    if (signedDist < 2.2f)
                    {
                        float outT = 1f - Mathf.Clamp01(signedDist / 2.2f);
                        c = Color.Lerp(c, rimOutline, outT * 0.85f);
                    }
                    else if (signedDist < 5.5f)
                    {
                        float bevT = 1f - ((signedDist - 2.2f) / 3.3f);
                        c = Color.Lerp(c, deepGold, bevT * 0.55f);
                    }

                    pixels[y * size + x] = new Color(c.r, c.g, c.b, c.a * aa);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
