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

        public static Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            if (_cachedSprites.TryGetValue(spriteName, out Sprite cached) && cached != null)
                return cached;

            // 1. Try Resources.Load
            Sprite sprite = Resources.Load<Sprite>($"Sprites/LevelAssets/{spriteName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Sprites/Blocks/{spriteName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Sprites/UI B1 (1)/{spriteName}");

            if (sprite != null)
            {
                _cachedSprites[spriteName] = sprite;
                return sprite;
            }

            // 2. Try loading raw PNG bytes from disk across all directories
            string[] searchDirs = new string[]
            {
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

        public static Sprite GetFrameSprite()
        {
            // First check Group 23 (clean frame from Figma)
            Sprite frame = GetSprite("Group 23");
            if (frame == null) frame = GetSprite("wall_frame_clean");
            return frame;
        }

        public static Sprite GetPieceSprite(PieceColor color, PieceShape shape)
        {
            switch (shape)
            {
                case PieceShape.Single_1x1:
                    switch (color)
                    {
                        case PieceColor.Green: return GetSprite("Vector 109") ?? GetSprite("gem_green");
                        case PieceColor.Pink: return GetSprite("Vector 112") ?? GetSprite("gem_magenta");
                        case PieceColor.Red: return GetSprite("Vector 113") ?? GetSprite("gem_red");
                        case PieceColor.Yellow: return GetSprite("gem_yellow");
                        case PieceColor.Purple: return GetSprite("gem_purple");
                        case PieceColor.Orange: return GetSprite("gem_orange_1x1");
                        case PieceColor.Blue: return GetSprite("gem_blue_1x1");
                        default: return GetSprite("Vector 109") ?? GetSprite("gem_green");
                    }

                case PieceShape.Vertical_1x2:
                    switch (color)
                    {
                        case PieceColor.Red: return GetSprite("Vector 110") ?? GetSprite("gem_red_1x2");
                        case PieceColor.Yellow: return GetSprite("Vector 111") ?? GetSprite("gem_yellow_1x2");
                        case PieceColor.Blue: return GetSprite("Vector 116") ?? GetSprite("gem_blue_1x2");
                        case PieceColor.Purple: return GetSprite("Vector 117") ?? GetSprite("gem_purple_1x2");
                        case PieceColor.Green: return GetSprite("gem_green_1x2");
                        case PieceColor.Orange: return GetSprite("gem_orange_1x2");
                        default: return GetSprite("Vector 116") ?? GetSprite("gem_blue_1x2");
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
                    return GetSprite("Vector 114") ?? GetSprite("gem_green_L");

                case PieceShape.ShapeL_BottomLeft:
                    return GetSprite("Vector 115") ?? GetSprite("gem_orange_L");

                default:
                    return GetSprite("Vector 109") ?? GetSprite("gem_green");
            }
        }
    }
}
