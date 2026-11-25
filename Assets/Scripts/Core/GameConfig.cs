using System;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Core
{
    public enum PieceColor
    {
        Red,
        Pink,       // Magenta
        Purple,
        Orange,
        Green,
        Yellow,
        Blue
    }

    public enum PieceShape
    {
        Single_1x1,
        Vertical_1x2,
        Horizontal_2x1,
        ShapeL_TopRight,    // 2 cells top, 1 cell bottom right: [(0,0), (1,0), (1,1)]
        ShapeL_BottomLeft,  // 1 cell top left, 2 cells bottom: [(0,0), (0,1), (1,1)]
        ShapeL_TopLeft,     // 2 cells top, 1 cell bottom left: [(0,0), (1,0), (0,1)]
        Square_2x2,         // 2x2 square block: [(0,0), (1,0), (0,1), (1,1)]
        Cross_3x3,          // 3x3 plus/cross block: [(1,0), (0,1), (1,1), (2,1), (1,2)]
        ShapeL_BottomRight, // 1 cell top right, 2 cells bottom: [(1,0), (0,1), (1,1)]
        Vertical_1x3,       // 1x3 vertical bar: [(0,0), (0,1), (0,2)]
        Horizontal_3x1,     // 3x1 horizontal bar: [(0,0), (1,0), (2,0)]
        ShapeT_Up,          // T pointing up: [(1,0), (0,1), (1,1), (2,1)]
        ShapeT_Down,        // T pointing down: [(0,0), (1,0), (2,0), (1,1)]
        ShapeT_Left,        // T pointing left: [(1,0), (0,1), (1,1), (1,2)]
        ShapeT_Right        // T pointing right: [(0,0), (0,1), (1,1), (0,2)]
    }

    public enum MoveAxisConstraint
    {
        None = 0,
        HorizontalOnly = 1,
        VerticalOnly = 2
    }

    public enum GateSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public static class GameConfig
    {
        // 6 Columns x 8 Rows Board layout matching the design image
        public const int Columns = 6;
        public const int Rows = 8;

        // Frame and cell sizing in Unity world units
        public const float FrameHeight = 10.0f;
        public const float FrameWidth = 8.213f; // 10.0 * (841 / 1024)

        // Inner playable board bounds
        public const float InnerBoardWidth = 6.40f;
        public const float InnerBoardHeight = 8.18f;

        public static float CellWidth => InnerBoardWidth / Columns; // ~1.066f
        public static float CellHeight => InnerBoardHeight / Rows;  // ~1.023f

        // Display mode
        public static bool ShowNumbersOnBlocks = true;

        // Legacy compatibility properties
        public const float CellSize = 1.05f;
        public const float CellSpacing = 0.05f;
        public static float BoardWidth => Columns * (CellSize + CellSpacing) - CellSpacing;
        public static float BoardHeight => Rows * (CellSize + CellSpacing) - CellSpacing;

        public static Sprite GetBlockSprite(int value)
        {
            return GemSpriteManager.GetSprite("gem_green");
        }

        public static Color GetBlockColor(int value)
        {
            return Color.white;
        }

        public static Color GetTextColor(int value)
        {
            return Color.white;
        }

        public static Vector3 GridToWorldPosition(int col, int row, Vector3 boardOrigin)
        {
            float posX = boardOrigin.x + col * (CellSize + CellSpacing) + CellSize * 0.5f;
            float posY = boardOrigin.y + row * (CellSize + CellSpacing) + CellSize * 0.5f;
            return new Vector3(posX, posY, 0f);
        }

        public static int GetRandomSpawnValue(int maxBlockOnBoard)
        {
            return UnityEngine.Random.Range(1, 4);
        }

        public static string FormatNumber(int number)
        {
            return number.ToString();
        }

        public static string GetColorNameVietnamese(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red: return "Đỏ";
                case PieceColor.Pink: return "Hồng";
                case PieceColor.Purple: return "Tím";
                case PieceColor.Orange: return "Cam";
                case PieceColor.Green: return "Xanh lá";
                case PieceColor.Yellow: return "Vàng";
                case PieceColor.Blue: return "Xanh dương";
                default: return color.ToString();
            }
        }

        public static Color GetColorRGB(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red: return new Color(0.92f, 0.22f, 0.22f);
                case PieceColor.Pink: return new Color(0.94f, 0.35f, 0.70f);
                case PieceColor.Purple: return new Color(0.65f, 0.30f, 0.95f);
                case PieceColor.Orange: return new Color(0.96f, 0.55f, 0.12f);
                case PieceColor.Green: return new Color(0.28f, 0.82f, 0.25f);
                case PieceColor.Yellow: return new Color(0.96f, 0.78f, 0.15f);
                case PieceColor.Blue: return new Color(0.18f, 0.55f, 0.98f);
                default: return Color.white;
            }
        }

        private static readonly Dictionary<PieceShape, Vector2Int[]> ShapeOffsetsMap = new Dictionary<PieceShape, Vector2Int[]>
        {
            { PieceShape.Single_1x1,         new[] { new Vector2Int(0, 0) } },
            { PieceShape.Vertical_1x2,       new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) } },
            { PieceShape.Horizontal_2x1,     new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) } },
            { PieceShape.ShapeL_TopRight,    new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) } },
            { PieceShape.ShapeL_BottomLeft,  new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) } },
            { PieceShape.ShapeL_TopLeft,     new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) } },
            { PieceShape.Square_2x2,         new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) } },
            { PieceShape.Cross_3x3,          new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) } },
            { PieceShape.ShapeL_BottomRight, new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) } },
            { PieceShape.Vertical_1x3,       new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) } },
            { PieceShape.Horizontal_3x1,     new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) } },
            { PieceShape.ShapeT_Up,          new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) } },
            { PieceShape.ShapeT_Down,        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) } },
            { PieceShape.ShapeT_Left,        new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) } },
            { PieceShape.ShapeT_Right,       new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2) } }
        };

        /// <summary>Returns the grid cell offsets (from top-left origin) for the given PieceShape.</summary>
        public static Vector2Int[] GetShapeOffsets(PieceShape shape)
        {
            if (ShapeOffsetsMap.TryGetValue(shape, out Vector2Int[] offsets))
            {
                return offsets;
            }
            return ShapeOffsetsMap[PieceShape.Single_1x1];
        }

        /// <summary>Returns the bounding grid dimensions (width, height) in cells for the given PieceShape.</summary>
        public static Vector2Int GetShapeBoundingSize(PieceShape shape)
        {
            Vector2Int[] offsets = GetShapeOffsets(shape);
            int maxX = 0;
            int maxY = 0;
            for (int i = 0; i < offsets.Length; i++)
            {
                if (offsets[i].x > maxX) maxX = offsets[i].x;
                if (offsets[i].y > maxY) maxY = offsets[i].y;
            }
            return new Vector2Int(maxX + 1, maxY + 1);
        }
    }
}
