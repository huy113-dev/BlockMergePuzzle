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
        ShapeL_BottomLeft   // 1 cell top left, 2 cells bottom: [(0,0), (0,1), (1,1)]
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
    }
}
