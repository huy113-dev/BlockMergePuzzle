using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BlockMergePuzzle.Core;

namespace BlockMergePuzzle.Utils
{
    public static class GemSpriteManager
    {
        private static readonly Dictionary<string, Sprite> _cachedSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> _derivedPieceSprites = new Dictionary<string, Sprite>();

        public static void ClearCache()
        {
            _cachedSprites.Clear();
            _derivedPieceSprites.Clear();
        }

        public static Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            if (_cachedSprites.TryGetValue(spriteName, out Sprite cached) && cached != null)
                return cached;

            // 1. Try Resources.Load
            Sprite sprite = Resources.Load<Sprite>($"Sprites/LevelAssets/{spriteName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Sprites/Blocks/{spriteName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Sprites/UI B1 (1)/{spriteName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Sprites/Background/{spriteName}");

            if (sprite != null)
            {
                _cachedSprites[spriteName] = sprite;
                return sprite;
            }

            // 2. Try loading raw PNG bytes from disk across all directories
            string[] searchDirs = new string[]
            {
                Path.Combine(Application.dataPath, "Sprites", "Main Scene", "UI"),
                "D:/BlockMergePuzzle/Assets/Sprites/Main Scene/UI",
                Path.Combine(Application.dataPath, "Sprites", "Main Scene", "UI", "setting"),
                "D:/BlockMergePuzzle/Assets/Sprites/Main Scene/UI/setting",
                Path.Combine(Application.dataPath, "Sprites", "Main Scene", "Block", "Block"),
                "D:/BlockMergePuzzle/Assets/Sprites/Main Scene/Block/Block",
                Path.Combine(Application.dataPath, "Sprites", "Main Scene", "Map"),
                "D:/BlockMergePuzzle/Assets/Sprites/Main Scene/Map",
                Path.Combine(Application.dataPath, "Sprites", "Main Scene", "Pause game"),
                "D:/BlockMergePuzzle/Assets/Sprites/Main Scene/Pause game",
                Path.Combine(Application.dataPath, "Sprites", "Background"),
                Path.Combine(Application.dataPath, "Resources", "Sprites", "Background"),
                "D:/BlockMergePuzzle/Assets/Sprites/Background",
                Path.Combine(Application.dataPath, "Sprites", "UI B1 (1)"),
                Path.Combine(Application.dataPath, "Resources", "Sprites", "LevelAssets"),
                Path.Combine(Application.dataPath, "Sprites", "LevelAssets"),
                Path.Combine(Application.dataPath, "Resources", "Sprites", "Blocks"),
                Path.Combine(Application.dataPath, "Sprites", "Blocks"),
                "D:/BlockMergePuzzle/Assets/Sprites/UI B1 (1)",
                "D:/BlockMergePuzzle/Assets/Resources/Sprites/LevelAssets",
                "D:/BlockMergePuzzle/Assets/Sprites/LevelAssets",
                "D:/BlockMergePuzzle/Assets/Resources/Sprites/Blocks",
                "D:/BlockMergePuzzle/Assets/Sprites/Blocks"
            };

            foreach (string dir in searchDirs)
            {
                string path = Path.Combine(dir, spriteName + ".png");
                if (File.Exists(path))
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(path);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        if (ImageConversion.LoadImage(tex, fileData))
                        {
                            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                            _cachedSprites[spriteName] = sprite;
                            return sprite;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[GemSpriteManager] Could not load image from {path}: {ex.Message}");
                    }
                }
            }

            // Fallback: procedural block
            return SpriteGenerator.GetBlockSprite();
        }

        public static Sprite GetSpriteExplicit(string subDir, string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            string cacheKey = $"{subDir}/{spriteName}";
            if (_cachedSprites.TryGetValue(cacheKey, out Sprite cached) && cached != null)
                return cached;

            Sprite sprite = Resources.Load<Sprite>($"{subDir}/{spriteName}");
            if (sprite != null)
            {
                _cachedSprites[cacheKey] = sprite;
                return sprite;
            }

            string[] baseDirs = new string[]
            {
                Path.Combine(Application.dataPath, subDir),
                Path.Combine(Application.dataPath, "Resources", subDir),
                Path.Combine("D:/BlockMergePuzzle/Assets", subDir)
            };

            foreach (string dir in baseDirs)
            {
                string path = Path.Combine(dir, spriteName + ".png");
                if (File.Exists(path))
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(path);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        tex.filterMode = FilterMode.Bilinear;
                        tex.wrapMode = TextureWrapMode.Clamp;
                        if (ImageConversion.LoadImage(tex, fileData))
                        {
                            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                            _cachedSprites[cacheKey] = sprite;
                            return sprite;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[GemSpriteManager] Could not load image from {path}: {ex.Message}");
                    }
                }
            }

            return null;
        }

        public static Sprite GetPauseSprite(string spriteName)
        {
            return GetSpriteExplicit("Sprites/Main Scene/Pause game", spriteName) ?? GetSprite(spriteName);
        }

        public static Sprite GetMainUISprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Main Scene/UI/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Main Scene/UI", spriteName) ?? GetSprite(spriteName);
        }

        public static Sprite GetBoosterSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Main Scene/UI/booster/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Main Scene/UI/booster", spriteName)
                ?? Resources.Load<Sprite>($"Sprites/BoosterUI/{spriteName}");
        }

        public static Sprite GetSettingSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Main Scene/UI/setting/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Main Scene/UI/setting", spriteName);
        }

        public static Sprite GetWinSceneSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Win&Lose scene/win scene/UI B1 (15)", spriteName)
                ?? Resources.Load<Sprite>($"Sprites/WinUI/{spriteName}");
        }

        public static Sprite GetWinHelperSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Resources/Sprites/WinUI/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return Resources.Load<Sprite>($"Sprites/WinUI/{spriteName}");
        }

        public static Sprite GetLoseHelperSprite(string spriteName)
        {
#if UNITY_EDITOR
            string loseOverridePath = $"Assets/Sprites/Win&Lose scene/lose scene/{spriteName}.png";
            Sprite overrideSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(loseOverridePath);
            if (overrideSprite != null) return overrideSprite;

            string editorPath = $"Assets/Resources/Sprites/LoseUI/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Win&Lose scene/lose scene", spriteName)
                ?? Resources.Load<Sprite>($"Sprites/LoseUI/{spriteName}");
        }

        public static Sprite GetHomeSceneSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Home scene/UI B1 (16)/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Home scene/UI B1 (16)", spriteName)
                ?? Resources.Load<Sprite>($"Sprites/HomeUI/{spriteName}");
        }

        public static Sprite GetHomeHelperSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Resources/Sprites/HomeUI/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return Resources.Load<Sprite>($"Sprites/HomeUI/{spriteName}");
        }

        public static Sprite GetRemoveAdsSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Resources/Sprites/RemoveAdsUI/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return Resources.Load<Sprite>($"Sprites/RemoveAdsUI/{spriteName}");
        }

        public static Sprite GetHeaderReturnSprite()
        {
            return GetSpriteExplicit("Resources/Sprites/LevelAssets", "Return")
                ?? GetSpriteExplicit("Sprites/LevelAssets", "Return")
                ?? GetSprite("Return");
        }

        public static Sprite GetMapSprite(int levelIndex)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Main Scene/Map/map_lv{levelIndex}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath)
                ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Main Scene/Map/Map_lv{levelIndex}.png");
            if (es != null) return es;

            string mapDir = "Assets/Sprites/Main Scene/Map";
            if (System.IO.Directory.Exists(mapDir))
            {
                string[] files = System.IO.Directory.GetFiles(mapDir, $"*map_lv{levelIndex}*.png", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string f in files)
                {
                    string p = f.Replace("\\", "/");
                    Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (s != null) return s;
                }
            }
#endif
            return GetSpriteExplicit("Sprites/Main Scene/Map", $"Map_lv{levelIndex}")
                ?? GetSpriteExplicit("Sprites/Main Scene/Map", $"map_lv{levelIndex}")
                ?? GetSpriteExplicit("Sprites/Main Scene/Map", $"map_lv{levelIndex} (1)")
                ?? GetSpriteExplicit("Sprites/Main Scene/Map", $"map_lv{levelIndex} (2)")
                ?? GetSpriteExplicit("Sprites/Main Scene/Map", $"Map_lv{levelIndex}.png")
                ?? GetSpriteExplicit("Sprites/Main Scene/Map", $"map_lv{levelIndex}.png")
                ?? GetSprite($"map_lv{levelIndex}");
        }

        public static Sprite GetFrameSprite()
        {
            // First check Group 23 (clean frame from Figma)
            Sprite frame = GetSprite("Group 23");
            if (frame == null) frame = GetSprite("wall_frame_clean");
            return frame;
        }

        public static Sprite GetTechGateHoloPadSprite()
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Main Scene/tech_gate_holo_pad.png");
            if (s != null) return s;
#endif
            return GetSpriteExplicit("Sprites/Main Scene", "tech_gate_holo_pad");
        }

        public static Sprite GetTechGateRingSprite()
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Main Scene/tech_gate_ring.png");
            if (s != null) return s;
#endif
            return GetSpriteExplicit("Sprites/Main Scene", "tech_gate_ring");
        }

        public static Sprite GetTechGateInnerRingSprite()
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Main Scene/tech_gate_inner_ring.png");
            if (s != null) return s;
#endif
            return GetSpriteExplicit("Sprites/Main Scene", "tech_gate_inner_ring");
        }

        public static Sprite GetBackgroundSprite()
        {
#if UNITY_EDITOR
            // 1. Try loading directly from AssetDatabase in Editor
            string[] searchEditorPaths = new string[]
            {
                "Assets/Sprites/Background/background_main scene .png",
                "Assets/Sprites/Background/background_main scene.png",
                "Assets/Sprites/Background/Background main play.png"
            };

            foreach (string p in searchEditorPaths)
            {
                Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) return s;
            }

            // Check any png in Background folder in Editor
            string editorBgDir = "Assets/Sprites/Background";
            if (System.IO.Directory.Exists(editorBgDir))
            {
                string[] pngs = System.IO.Directory.GetFiles(editorBgDir, "*.png");
                if (pngs.Length > 0)
                {
                    string p = pngs[0].Replace("\\", "/");
                    Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (s != null) return s;
                }
            }
#endif

            // 2. Try explicit named sprites (GetSpriteExplicit returns null if not found, avoiding block fallback)
            Sprite bg = GetSpriteExplicit("Sprites/Background", "background_main scene ")
                ?? GetSpriteExplicit("Sprites/Background", "background_main scene")
                ?? GetSpriteExplicit("Sprites/Background", "Background main play");

            if (bg != null) return bg;

            // 3. Fallback: search Background directory on disk for any png
            string[] bgDirs = new string[]
            {
                Path.Combine(Application.dataPath, "Sprites", "Background"),
                "D:/BlockMergePuzzle/Assets/Sprites/Background"
            };

            foreach (string dir in bgDirs)
            {
                if (Directory.Exists(dir))
                {
                    string[] files = Directory.GetFiles(dir, "*.png");
                    if (files.Length > 0)
                    {
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(files[0]);
                        bg = GetSpriteExplicit("Sprites/Background", fileNameWithoutExt);
                        if (bg != null) return bg;
                    }
                }
            }

            return null;
        }

        public static Sprite GetNewBlockSprite(string spriteName)
        {
#if UNITY_EDITOR
            string editorPath = $"Assets/Sprites/Main Scene/Block/Block/{spriteName}.png";
            Sprite es = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            if (es != null) return es;
#endif
            return GetSpriteExplicit("Sprites/Main Scene/Block/Block", spriteName);
        }

        public static Sprite GetIceOverlaySprite()
        {
            return GetNewBlockSprite("ice_block_overlay") ?? SpriteGenerator.GetIceOverlaySprite();
        }

        public static Sprite GetPieceSprite(PieceColor color, PieceShape shape)
        {
            switch (shape)
            {
                case PieceShape.Single_1x1:
                case PieceShape.Square_2x2:
                    return Get1x1Or2x2Sprite(color);

                case PieceShape.Vertical_1x2:
                    return GetVertical1x2Sprite(color);

                case PieceShape.Horizontal_2x1:
                    return GetHorizontal2x1Sprite(color);

                case PieceShape.ShapeL_TopRight:
                {
                    if (color == PieceColor.Orange)
                    {
                        Sprite orange = GetNewBlockSprite("block_orange_L_topright");
                        if (orange != null) return orange;
                    }
                    Sprite green = GetNewBlockSprite("block_green_L_topright") ?? GetNewBlockSprite("Group 356 (2)") ?? GetSprite("Vector 114") ?? GetSprite("gem_green_L");
                    if (color == PieceColor.Green) return green;
                    return GetOrCreateDerivedSprite(green, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"L_TR_{color}") ?? green;
                }

                case PieceShape.ShapeL_TopLeft:
                {
                    if (color == PieceColor.Orange)
                    {
                        Sprite orange = GetNewBlockSprite("block_orange_L_topleft");
                        if (orange != null) return orange;
                    }
                    Sprite green = GetNewBlockSprite("block_green_L_topleft") ?? GetNewBlockSprite("Group 356 (2)") ?? GetSprite("gem_green_L");
                    if (color == PieceColor.Green) return green;
                    return GetOrCreateDerivedSprite(green, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"L_TL_{color}") ?? green;
                }

                case PieceShape.ShapeL_BottomLeft:
                    return GetLBottomLeftSprite(color);

                case PieceShape.ShapeL_BottomRight:
                {
                    if (color == PieceColor.Orange)
                    {
                        Sprite orange = GetNewBlockSprite("block_orange_L_bottomright");
                        if (orange != null) return orange;
                    }
                    Sprite baseLeft = GetLBottomLeftSprite(color);
                    return GetOrCreateDerivedSprite(baseLeft, color, sourceIsColored: false, rotateCW90: 0, flipX: true, $"L_BR_{color}") ?? baseLeft;
                }

                case PieceShape.Vertical_1x3:
                    return GetVertical1x3Sprite(color);

                case PieceShape.Horizontal_3x1:
                {
                    if (color == PieceColor.Orange)
                    {
                        Sprite orange3x1 = GetNewBlockSprite("block_orange_3x1");
                        if (orange3x1 != null) return orange3x1;
                    }
                    Sprite v1x3 = GetVertical1x3Sprite(color);
                    return GetOrCreateDerivedSprite(v1x3, color, sourceIsColored: false, rotateCW90: 1, flipX: false, $"H3x1_{color}") ?? v1x3;
                }

                case PieceShape.ShapeT_Up:
                    return GetTUpSprite(color);

                case PieceShape.ShapeT_Down:
                {
                    Sprite tUp = GetTUpSprite(color);
                    return GetOrCreateDerivedSprite(tUp, color, sourceIsColored: false, rotateCW90: 2, flipX: false, $"T_Down_{color}") ?? tUp;
                }

                case PieceShape.ShapeT_Left:
                {
                    Sprite tUp = GetTUpSprite(color);
                    return GetOrCreateDerivedSprite(tUp, color, sourceIsColored: false, rotateCW90: 3, flipX: false, $"T_Left_{color}") ?? tUp;
                }

                case PieceShape.ShapeT_Right:
                {
                    Sprite tUp = GetTUpSprite(color);
                    return GetOrCreateDerivedSprite(tUp, color, sourceIsColored: false, rotateCW90: 1, flipX: false, $"T_Right_{color}") ?? tUp;
                }

                case PieceShape.Cross_3x3:
                {
                    if (color == PieceColor.Red) return GetNewBlockSprite("Group 478");
                    Sprite green = GetNewBlockSprite("Group 521");
                    if (color == PieceColor.Green) return green;
                    return GetOrCreateDerivedSprite(green, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"Cross_{color}") ?? green ?? GetNewBlockSprite("Group 478");
                }

                default:
                    return Get1x1Or2x2Sprite(color);
            }
        }

        private static Sprite Get1x1Or2x2Sprite(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Green: return GetNewBlockSprite("Group 346") ?? GetSprite("Vector 109") ?? GetSprite("gem_green");
                case PieceColor.Pink: return GetNewBlockSprite("Group 344") ?? GetSprite("Vector 112") ?? GetSprite("gem_magenta");
                case PieceColor.Red: return GetNewBlockSprite("Group 345") ?? GetSprite("Vector 113") ?? GetSprite("gem_red");
                case PieceColor.Yellow: return GetNewBlockSprite("Group 347") ?? GetSprite("gem_yellow");
                case PieceColor.Purple: return GetNewBlockSprite("Group 343") ?? GetSprite("gem_purple");
                case PieceColor.Orange: return GetNewBlockSprite("Group 348") ?? GetSprite("gem_orange_1x1");
                case PieceColor.Blue: return GetNewBlockSprite("Group 669") ?? GetSprite("gem_blue_1x1");
                default: return GetNewBlockSprite("Group 346") ?? GetSprite("Vector 109") ?? GetSprite("gem_green");
            }
        }

        private static Sprite GetVertical1x2Sprite(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red: return GetNewBlockSprite("Group 352") ?? GetSprite("Vector 110") ?? GetSprite("gem_red_1x2");
                case PieceColor.Yellow: return GetNewBlockSprite("Group 354") ?? GetSprite("Vector 111") ?? GetSprite("gem_yellow_1x2");
                case PieceColor.Blue: return GetNewBlockSprite("Group 349") ?? GetSprite("Vector 116") ?? GetSprite("gem_blue_1x2");
                case PieceColor.Purple: return GetNewBlockSprite("Group 350") ?? GetSprite("Vector 117") ?? GetSprite("gem_purple_1x2");
                case PieceColor.Green: return GetNewBlockSprite("Group 353") ?? GetSprite("gem_green_1x2");
                case PieceColor.Orange: return GetNewBlockSprite("Group 355") ?? GetSprite("gem_orange_1x2");
                case PieceColor.Pink: return GetNewBlockSprite("Group 351") ?? GetSprite("gem_magenta");
                default: return GetNewBlockSprite("Group 349") ?? GetSprite("Vector 116") ?? GetSprite("gem_blue_1x2");
            }
        }

        private static Sprite GetHorizontal2x1Sprite(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Blue: return GetNewBlockSprite("block_blue_2x1") ?? GetSprite("gem_blue_2x1");
                case PieceColor.Yellow: return GetNewBlockSprite("block_yellow_2x1") ?? GetSprite("gem_yellow_2x1");
                case PieceColor.Red: return GetNewBlockSprite("block_red_2x1") ?? GetSprite("gem_red_2x1");
                case PieceColor.Green: return GetNewBlockSprite("block_green_2x1") ?? GetSprite("gem_green_2x1");
                case PieceColor.Orange: return GetNewBlockSprite("block_orange_2x1") ?? GetSprite("gem_orange_2x1");
                case PieceColor.Purple: return GetNewBlockSprite("block_purple_2x1") ?? GetSprite("gem_purple_2x1");
                case PieceColor.Pink:
                {
                    Sprite pink = GetNewBlockSprite("block_pink_2x1");
                    if (pink != null) return pink;
                    Sprite green = GetNewBlockSprite("block_green_2x1") ?? GetSprite("gem_green_2x1");
                    return GetOrCreateDerivedSprite(green, PieceColor.Pink, sourceIsColored: true, rotateCW90: 0, flipX: false, "H2x1_Pink") ?? green;
                }
                default: return GetNewBlockSprite("block_blue_2x1") ?? GetSprite("gem_blue_2x1");
            }
        }

        private static Sprite GetLBottomLeftSprite(PieceColor color)
        {
            if (color == PieceColor.Green)
                return GetNewBlockSprite("Group 356 (2)") ?? GetSprite("gem_green_L");
            if (color == PieceColor.Orange)
                return GetNewBlockSprite("Group 358") ?? GetSprite("Vector 115") ?? GetSprite("gem_orange_L");
            if (color == PieceColor.Pink)
                return GetNewBlockSprite("Group 433") ?? GetNewBlockSprite("Group 358");

            Sprite greenBase = GetNewBlockSprite("Group 356 (2)") ?? GetSprite("gem_green_L");
            return GetOrCreateDerivedSprite(greenBase, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"L_BL_{color}") ?? greenBase;
        }

        private static Sprite GetVertical1x3Sprite(PieceColor color)
        {
            if (color == PieceColor.Yellow) return GetNewBlockSprite("Group 423");
            if (color == PieceColor.Green) return GetNewBlockSprite("Group 424");
            if (color == PieceColor.Red) return GetNewBlockSprite("Group 425");
            if (color == PieceColor.Orange)
            {
                Sprite orange = GetNewBlockSprite("block_orange_1x3");
                if (orange != null) return orange;
            }

            Sprite greenBase = GetNewBlockSprite("Group 424") ?? GetNewBlockSprite("Group 423");
            return GetOrCreateDerivedSprite(greenBase, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"V1x3_{color}") ?? greenBase;
        }

        private static Sprite GetTUpSprite(PieceColor color)
        {
            if (color == PieceColor.Green) return GetNewBlockSprite("Group 429");
            if (color == PieceColor.Yellow) return GetNewBlockSprite("Group 430");
            if (color == PieceColor.Red) return GetNewBlockSprite("Group 431");

            Sprite greenBase = GetNewBlockSprite("Group 429") ?? GetNewBlockSprite("Group 430");
            return GetOrCreateDerivedSprite(greenBase, color, sourceIsColored: true, rotateCW90: 0, flipX: false, $"T_Up_{color}") ?? greenBase;
        }

        private static Sprite GetOrCreateDerivedSprite(Sprite source, PieceColor targetColor, bool sourceIsColored, int rotateCW90, bool flipX, string cacheKey)
        {
            if (source == null || source.texture == null) return null;
            if (_derivedPieceSprites.TryGetValue(cacheKey, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Rect sr = source.textureRect;
            int srcW = Mathf.Max(4, Mathf.RoundToInt(sr.width));
            int srcH = Mathf.Max(4, Mathf.RoundToInt(sr.height));
            float downscale = Mathf.Min(1f, 256f / Mathf.Max(srcW, srcH));
            int w = Mathf.Max(4, Mathf.RoundToInt(srcW * downscale));
            int h = Mathf.Max(4, Mathf.RoundToInt(srcH * downscale));

            Texture2D readable = CopySpriteRectReadable(source, w, h);
            if (readable == null) return null;

            Color32[] srcPixels = readable.GetPixels32();
            UnityEngine.Object.DestroyImmediate(readable);

            int rot = ((rotateCW90 % 4) + 4) % 4;
            int outW = (rot == 1 || rot == 3) ? h : w;
            int outH = (rot == 1 || rot == 3) ? w : h;
            Color32[] dstPixels = new Color32[outW * outH];

            float targetH = GetTargetHue(targetColor);
            float satMult = GetTargetSatMultiplier(targetColor);
            float valMult = GetTargetValMultiplier(targetColor);

            // Estimate source hue dynamically from opaque saturated pixels
            float srcHueSumCos = 0f;
            float srcHueSumSin = 0f;
            if (sourceIsColored)
            {
                for (int i = 0; i < srcPixels.Length; i += 4)
                {
                    Color32 px = srcPixels[i];
                    if (px.a < 180) continue;
                    Color.RGBToHSV((Color)px, out float ph, out float ps, out float pv);
                    if (ps > 0.35f && pv > 0.25f)
                    {
                        float rad = ph * Mathf.PI * 2f;
                        srcHueSumCos += Mathf.Cos(rad);
                        srcHueSumSin += Mathf.Sin(rad);
                    }
                }
            }
            float sourceHue = (Mathf.Atan2(srcHueSumSin, srcHueSumCos) / (Mathf.PI * 2f) + 1f) % 1f;
            float hueDelta = sourceIsColored ? Mathf.DeltaAngle(sourceHue * 360f, targetH * 360f) / 360f : 0f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 px = srcPixels[y * w + x];
                    if (sourceIsColored && px.a > 4)
                    {
                        Color col = (Color)px;
                        Color.RGBToHSV(col, out float ch, out float cs, out float cv);
                        if (cs > 0.12f)
                        {
                            float nh = (ch + hueDelta + 1f) % 1f;
                            float ns = Mathf.Clamp01(cs * satMult);
                            float nv = Mathf.Clamp01(cv * valMult);
                            Color shifted = Color.HSVToRGB(nh, ns, nv);
                            float blend = Mathf.Clamp01((cs - 0.12f) / 0.22f);
                            Color finalCol = Color.Lerp(col, shifted, blend);
                            px = new Color32(
                                (byte)Mathf.RoundToInt(finalCol.r * 255f),
                                (byte)Mathf.RoundToInt(finalCol.g * 255f),
                                (byte)Mathf.RoundToInt(finalCol.b * 255f),
                                px.a);
                        }
                    }

                    int nx;
                    int ny;
                    switch (rot)
                    {
                        case 1: // 90 deg CW
                            nx = y;
                            ny = w - 1 - x;
                            break;
                        case 2: // 180 deg
                            nx = w - 1 - x;
                            ny = h - 1 - y;
                            break;
                        case 3: // 90 deg CCW
                            nx = h - 1 - y;
                            ny = x;
                            break;
                        default:
                            nx = x;
                            ny = y;
                            break;
                    }

                    if (flipX) nx = outW - 1 - nx;
                    dstPixels[ny * outW + nx] = px;
                }
            }

            Texture2D outTex = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
            outTex.filterMode = FilterMode.Bilinear;
            outTex.wrapMode = TextureWrapMode.Clamp;
            outTex.SetPixels32(dstPixels);
            outTex.Apply();

            Sprite result = Sprite.Create(outTex, new Rect(0, 0, outW, outH), new Vector2(0.5f, 0.5f), 100f);
            result.name = "block_derived_" + cacheKey;
            _derivedPieceSprites[cacheKey] = result;
            return result;
        }

        private static Texture2D CopySpriteRectReadable(Sprite source, int targetW, int targetH)
        {
            Texture tex = source.texture;
            Rect sr = source.textureRect;
            float uvX = sr.x / tex.width;
            float uvY = sr.y / tex.height;
            float uvW = sr.width / tex.width;
            float uvH = sr.height / tex.height;

            RenderTexture prev = RenderTexture.active;
            RenderTexture rt = RenderTexture.GetTemporary(targetW, targetH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
                GL.PushMatrix();
                GL.LoadOrtho();

                Material blitMat = GetUnlitSpriteBlitMaterial();
                if (blitMat != null)
                {
                    blitMat.mainTexture = tex;
                    blitMat.SetPass(0);
                }

                GL.Begin(GL.QUADS);
                GL.TexCoord2(uvX, uvY); GL.Vertex3(0f, 0f, 0f);
                GL.TexCoord2(uvX + uvW, uvY); GL.Vertex3(1f, 0f, 0f);
                GL.TexCoord2(uvX + uvW, uvY + uvH); GL.Vertex3(1f, 1f, 0f);
                GL.TexCoord2(uvX, uvY + uvH); GL.Vertex3(0f, 1f, 0f);
                GL.End();
                GL.PopMatrix();

                Texture2D copy = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
                copy.Apply();
                return copy;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GemSpriteManager] CopySpriteRectReadable failed: {ex.Message}");
                return null;
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static Material _unlitBlitMat;
        private static Material GetUnlitSpriteBlitMaterial()
        {
            if (_unlitBlitMat != null) return _unlitBlitMat;
            Shader s = Shader.Find("Sprites/Default") ?? Shader.Find("UI/Default");
            if (s != null) _unlitBlitMat = new Material(s);
            return _unlitBlitMat;
        }

        private static float GetTargetHue(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red:    return 0.992f;
                case PieceColor.Orange: return 0.073f;
                case PieceColor.Yellow: return 0.125f;
                case PieceColor.Green:  return 0.295f;
                case PieceColor.Blue:   return 0.568f;
                case PieceColor.Purple: return 0.742f;
                case PieceColor.Pink:   return 0.892f;
                default:                return 0.30f;
            }
        }

        private static float GetTargetSatMultiplier(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red:    return 1.05f;
                case PieceColor.Orange: return 1.28f;
                case PieceColor.Yellow: return 0.96f;
                case PieceColor.Blue:   return 1.02f;
                case PieceColor.Purple: return 1.00f;
                case PieceColor.Pink:   return 0.95f;
                default:                return 1.00f;
            }
        }

        private static float GetTargetValMultiplier(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Yellow: return 1.12f;
                case PieceColor.Orange: return 1.34f;
                case PieceColor.Pink:   return 1.04f;
                case PieceColor.Red:    return 0.96f;
                default:                return 1.00f;
            }
        }
    }
}
