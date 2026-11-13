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

        public static void ClearCache()
        {
            _cachedSprites.Clear();
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
            return GetSpriteExplicit("Sprites/Main Scene/UI/booster", spriteName);
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

        public static Sprite GetPieceSprite(PieceColor color, PieceShape shape)
        {
            switch (shape)
            {
                case PieceShape.Single_1x1:
                    switch (color)
                    {
                        case PieceColor.Green: return GetNewBlockSprite("Group 346") ?? GetSprite("Vector 109") ?? GetSprite("gem_green");
                        case PieceColor.Pink: return GetNewBlockSprite("Group 344") ?? GetSprite("Vector 112") ?? GetSprite("gem_magenta");
                        case PieceColor.Red: return GetNewBlockSprite("Group 345") ?? GetSprite("Vector 113") ?? GetSprite("gem_red");
                        case PieceColor.Yellow: return GetNewBlockSprite("Group 347") ?? GetSprite("gem_yellow");
                        case PieceColor.Purple: return GetSprite("gem_purple");
                        case PieceColor.Orange: return GetNewBlockSprite("Group 348") ?? GetSprite("gem_orange_1x1");
                        case PieceColor.Blue: return GetSprite("gem_blue_1x1");
                        default: return GetNewBlockSprite("Group 346") ?? GetSprite("Vector 109") ?? GetSprite("gem_green");
                    }

                case PieceShape.Vertical_1x2:
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

                case PieceShape.Horizontal_2x1:
                    switch (color)
                    {
                        case PieceColor.Blue: return GetSprite("gem_blue_2x1");
                        case PieceColor.Yellow: return GetSprite("gem_yellow_2x1");
                        case PieceColor.Red: return GetSprite("gem_red_2x1");
                        case PieceColor.Green: return GetSprite("gem_green_2x1");
                        case PieceColor.Orange: return GetSprite("gem_orange_2x1");
                        case PieceColor.Purple: return GetSprite("gem_purple_2x1");
                        default: return GetSprite("gem_blue_2x1");
                    }

                case PieceShape.ShapeL_TopRight:
                    return GetNewBlockSprite("Group 356 (2)") ?? GetSprite("Vector 114") ?? GetSprite("gem_green_L");

                case PieceShape.ShapeL_BottomLeft:
                    return GetNewBlockSprite("Group 358") ?? GetSprite("Vector 115") ?? GetSprite("gem_orange_L");

                default:
                    return GetNewBlockSprite("Group 346") ?? GetSprite("Vector 109") ?? GetSprite("gem_green");
            }
        }
    }
}
