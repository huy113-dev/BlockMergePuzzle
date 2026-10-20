using System;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private Vector3 _boardCenter = new Vector3(0.015f, 0.073f, 0f);
        private PuzzlePiece[,] _grid = new PuzzlePiece[GameConfig.Columns, GameConfig.Rows];
        private List<PuzzlePiece> _activePieces = new List<PuzzlePiece>();
        private List<ExitGateView> _activeGates = new List<ExitGateView>();

        private Transform _frameRoot;
        private Transform _gatesRoot;
        private Transform _piecesRoot;

        public int Score { get; private set; }
        public int RemainingBlocksCount => _activePieces.Count;
        public IReadOnlyList<ExitGateView> Gates => _activeGates;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnRemainingBlocksChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeLevel();
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Level In Edit Mode")]
        public void BakeEditModeLevel()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _activePieces.Clear();
            _activeGates.Clear();
            _grid = new PuzzlePiece[GameConfig.Columns, GameConfig.Rows];

            _frameRoot = new GameObject("Board_Frame").transform;
            _frameRoot.SetParent(transform);

            _gatesRoot = new GameObject("Exit_Gates").transform;
            _gatesRoot.SetParent(transform);

            _piecesRoot = new GameObject("Active_Pieces").transform;
            _piecesRoot.SetParent(transform);

            BuildBoardFrame();
            BuildExitGates();
            SpawnReferenceLevelPieces();
        }
#endif

        public void InitializeLevel()
        {
            Score = 0;

            _frameRoot = transform.Find("Board_Frame");
            _gatesRoot = transform.Find("Exit_Gates");
            _piecesRoot = transform.Find("Active_Pieces");

            bool hasSceneObjects = (_frameRoot != null && _gatesRoot != null && _piecesRoot != null && _piecesRoot.childCount > 0);

            if (!hasSceneObjects)
            {
                ClearCurrentLevel();
                BuildBoardFrame();
                BuildExitGates();
                SpawnReferenceLevelPieces();
            }
            else
            {
                BindSceneObjects();
            }

            OnScoreChanged?.Invoke(Score);
            OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
            FitCameraToBoard();
        }

        private void FitCameraToBoard()
        {
            var cam = Camera.main;
            if (cam == null) return;
            float screenRatio = (float)Screen.width / Screen.height;
            float paddingX = 0.5f;
            float paddingY = 0.8f;
            float sizeByWidth  = (GameConfig.FrameWidth  / 2f + paddingX) / screenRatio;
            float sizeByHeight =  GameConfig.FrameHeight / 2f + paddingY;
            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void BindSceneObjects()
        {
            _activePieces.Clear();
            _activeGates.Clear();
            _grid = new PuzzlePiece[GameConfig.Columns, GameConfig.Rows];

            foreach (var gate in _gatesRoot.GetComponentsInChildren<ExitGateView>(true))
            {
                _activeGates.Add(gate);
            }

            foreach (var piece in _piecesRoot.GetComponentsInChildren<PuzzlePiece>(true))
            {
                _activePieces.Add(piece);
                foreach (var cell in piece.OccupiedCells)
                {
                    if (IsValidCoord(cell.x, cell.y))
                    {
                        _grid[cell.x, cell.y] = piece;
                    }
                }
            }
        }

        private void ClearCurrentLevel()
        {
            if (_frameRoot != null) Destroy(_frameRoot.gameObject);
            if (_gatesRoot != null) Destroy(_gatesRoot.gameObject);
            if (_piecesRoot != null) Destroy(_piecesRoot.gameObject);

            _activePieces.Clear();
            _activeGates.Clear();
            _grid = new PuzzlePiece[GameConfig.Columns, GameConfig.Rows];

            _frameRoot = new GameObject("Board_Frame").transform;
            _frameRoot.SetParent(transform);

            _gatesRoot = new GameObject("Exit_Gates").transform;
            _gatesRoot.SetParent(transform);

            _piecesRoot = new GameObject("Active_Pieces").transform;
            _piecesRoot.SetParent(transform);
        }

        private void BuildBoardFrame()
        {
            GameObject frameObj = new GameObject("WallFrame");
            frameObj.transform.SetParent(_frameRoot);
            frameObj.transform.position = Vector3.zero;

            SpriteRenderer sr = frameObj.AddComponent<SpriteRenderer>();
            sr.sprite = GemSpriteManager.GetFrameSprite();
            sr.sortingOrder = 2;

            if (sr.sprite != null)
            {
                float nativeW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                frameObj.transform.localScale = new Vector3(
                    GameConfig.FrameWidth / nativeW,
                    GameConfig.FrameHeight / nativeH,
                    1f);
            }
        }

        private void BuildExitGates()
        {
            float cw = GameConfig.CellWidth;
            float ch = GameConfig.CellHeight;
            float bw = GameConfig.InnerBoardWidth;
            float bh = GameConfig.InnerBoardHeight;

            // 1. Top Blue Gate (Cols 1..2, Row -1): Needs 2
            Vector3 topBluePos = new Vector3(
                (GridToWorld(1, 0).x + GridToWorld(2, 0).x) * 0.5f,
                _boardCenter.y + bh * 0.5f + 0.48f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Blue, 2, GateSide.Top, Vector2.up, topBluePos, new Vector2(cw * 2.1f, 0.95f), _gatesRoot));

            // 2. Top Green Gate (Cols 3..4, Row -1): Needs 3
            Vector3 topGreenPos = new Vector3(
                (GridToWorld(3, 0).x + GridToWorld(4, 0).x) * 0.5f,
                _boardCenter.y + bh * 0.5f + 0.48f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Green, 3, GateSide.Top, Vector2.up, topGreenPos, new Vector2(cw * 2.1f, 0.95f), _gatesRoot));

            // 3. Right Red Gate (Rows 0..1, Col 6): Needs 2
            Vector3 rightRedPos = new Vector3(
                _boardCenter.x + bw * 0.5f + 0.48f,
                (GridToWorld(0, 0).y + GridToWorld(0, 1).y) * 0.5f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Red, 2, GateSide.Right, Vector2.right, rightRedPos, new Vector2(0.95f, ch * 2.1f), _gatesRoot));

            // 4. Right Pink Gate (Rows 2..3, Col 6): Needs 3
            Vector3 rightPinkPos = new Vector3(
                _boardCenter.x + bw * 0.5f + 0.48f,
                (GridToWorld(0, 2).y + GridToWorld(0, 3).y) * 0.5f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Pink, 3, GateSide.Right, Vector2.right, rightPinkPos, new Vector2(0.95f, ch * 2.1f), _gatesRoot));

            // 5. Bottom Orange Gate (Cols 3..4, Row 8): Needs 2
            Vector3 botOrangePos = new Vector3(
                (GridToWorld(3, 7).x + GridToWorld(4, 7).x) * 0.5f,
                _boardCenter.y - bh * 0.5f - 0.48f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Orange, 2, GateSide.Bottom, Vector2.down, botOrangePos, new Vector2(cw * 2.1f, 0.95f), _gatesRoot));

            // 6. Left Purple Gate (Rows 1..2, Col -1): Needs 4
            Vector3 leftPurplePos = new Vector3(
                _boardCenter.x - bw * 0.5f - 0.48f,
                (GridToWorld(0, 1).y + GridToWorld(0, 2).y) * 0.5f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Purple, 4, GateSide.Left, Vector2.left, leftPurplePos, new Vector2(0.95f, ch * 2.1f), _gatesRoot));

            // 7. Left Yellow Gate (Rows 6..7, Col -1): Needs 2
            Vector3 leftYellowPos = new Vector3(
                _boardCenter.x - bw * 0.5f - 0.48f,
                (GridToWorld(0, 6).y + GridToWorld(0, 7).y) * 0.5f,
                0f);
            _activeGates.Add(ExitGateView.Create(
                PieceColor.Yellow, 2, GateSide.Left, Vector2.left, leftYellowPos, new Vector2(0.95f, ch * 2.1f), _gatesRoot));
        }

        private void SpawnReferenceLevelPieces()
        {
            // 21 Pieces mapped strictly from reference layout:

            // Row 0 & 1
            SpawnPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, new Vector2Int(0, 0));
            SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 2, new Vector2Int(1, 0));
            SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 3, new Vector2Int(2, 0));
            SpawnPiece(PieceColor.Orange, PieceShape.ShapeL_BottomLeft, 1, new Vector2Int(3, 0));
            SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 2, new Vector2Int(4, 0));
            SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(1, 1));

            // Row 2 & 3
            SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(0, 2));
            SpawnPiece(PieceColor.Red, PieceShape.Vertical_1x2, 2, new Vector2Int(2, 2));
            SpawnPiece(PieceColor.Blue, PieceShape.Horizontal_2x1, 1, new Vector2Int(3, 2));
            SpawnPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, new Vector2Int(5, 2));

            // Row 3 & 4
            SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, new Vector2Int(0, 3));
            SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 2, new Vector2Int(3, 3));

            // Row 4 & 5
            SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(0, 4));
            SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(5, 4));
            SpawnPiece(PieceColor.Blue, PieceShape.Horizontal_2x1, 1, new Vector2Int(2, 5));
            SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(4, 5));

            // Row 6 & 7
            SpawnPiece(PieceColor.Orange, PieceShape.ShapeL_BottomLeft, 1, new Vector2Int(0, 6));
            SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(1, 6));
            SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(2, 6));
            SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, new Vector2Int(4, 6));
            SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(4, 7));
        }

        private PuzzlePiece SpawnPiece(PieceColor color, PieceShape shape, int value, Vector2Int origin)
        {
            Vector3 worldPos = CalculatePieceWorldCenter(shape, origin);
            PuzzlePiece piece = PuzzlePiece.Create(color, shape, value, origin, worldPos, _piecesRoot);

            foreach (var cell in piece.OccupiedCells)
            {
                if (IsValidCoord(cell.x, cell.y))
                {
                    _grid[cell.x, cell.y] = piece;
                }
            }

            _activePieces.Add(piece);
            return piece;
        }

        public Vector3 CalculatePieceWorldCenter(PieceShape shape, Vector2Int origin)
        {
            switch (shape)
            {
                case PieceShape.Single_1x1:
                    return GridToWorld(origin.x, origin.y);

                case PieceShape.Vertical_1x2:
                    return (GridToWorld(origin.x, origin.y) + GridToWorld(origin.x, origin.y + 1)) * 0.5f;

                case PieceShape.Horizontal_2x1:
                    return (GridToWorld(origin.x, origin.y) + GridToWorld(origin.x + 1, origin.y)) * 0.5f;

                case PieceShape.ShapeL_TopRight:
                case PieceShape.ShapeL_BottomLeft:
                    // 2x2 bounding box center
                    return (GridToWorld(origin.x, origin.y) + GridToWorld(origin.x + 1, origin.y + 1)) * 0.5f;

                default:
                    return GridToWorld(origin.x, origin.y);
            }
        }

        public Vector3 GridToWorld(int col, int row)
        {
            float x = _boardCenter.x - (GameConfig.InnerBoardWidth * 0.5f) + (col + 0.5f) * GameConfig.CellWidth;
            float y = _boardCenter.y + (GameConfig.InnerBoardHeight * 0.5f) - (row + 0.5f) * GameConfig.CellHeight;
            return new Vector3(x, y, 0f);
        }

        public bool IsValidCoord(int col, int row)
        {
            return col >= 0 && col < GameConfig.Columns && row >= 0 && row < GameConfig.Rows;
        }

        public PuzzlePiece GetPieceAt(int col, int row)
        {
            if (!IsValidCoord(col, row)) return null;
            return _grid[col, row];
        }

        public void ClearPieceFromGrid(PuzzlePiece piece)
        {
            if (piece == null) return;
            foreach (var cell in piece.OccupiedCells)
            {
                if (IsValidCoord(cell.x, cell.y) && _grid[cell.x, cell.y] == piece)
                {
                    _grid[cell.x, cell.y] = null;
                }
            }
        }

        public void AssignPieceToGrid(PuzzlePiece piece, Vector2Int newOrigin)
        {
            ClearPieceFromGrid(piece);
            piece.SetGridOrigin(newOrigin);
            foreach (var cell in piece.OccupiedCells)
            {
                if (IsValidCoord(cell.x, cell.y))
                {
                    _grid[cell.x, cell.y] = piece;
                }
            }
        }

        public bool CanPieceFitAt(PuzzlePiece piece, Vector2Int origin)
        {
            if (piece == null) return false;
            List<Vector2Int> cells = piece.GetOccupiedCellsForOrigin(origin);
            foreach (var c in cells)
            {
                if (!IsValidCoord(c.x, c.y)) return false;
                PuzzlePiece occupant = _grid[c.x, c.y];
                if (occupant != null && occupant != piece) return false;
            }
            return true;
        }

        public bool CheckMergeOpportunity(PuzzlePiece piece, Vector2Int origin, Vector2Int direction, out PuzzlePiece targetPiece)
        {
            targetPiece = null;
            if (piece == null || direction == Vector2Int.zero) return false;

            Vector2Int candidateOrigin = origin + direction;
            List<Vector2Int> candidateCells = piece.GetOccupiedCellsForOrigin(candidateOrigin);

            foreach (var c in candidateCells)
            {
                if (!IsValidCoord(c.x, c.y)) return false;
            }

            PuzzlePiece hit = null;
            bool multiple = false;
            foreach (var c in candidateCells)
            {
                PuzzlePiece occ = GetPieceAt(c.x, c.y);
                if (occ != null && occ != piece)
                {
                    if (hit == null) hit = occ;
                    else if (hit != occ) multiple = true;
                }
            }

            if (hit != null && !multiple)
            {
                bool sameShape = (piece.Shape == hit.Shape);
                bool sameColor = (piece.PieceColor == hit.PieceColor);
                bool sameValue = (piece.Value == hit.Value);

                bool isAligned = false;
                if (direction.x != 0 && origin.y == hit.GridOrigin.y) isAligned = true;
                if (direction.y != 0 && origin.x == hit.GridOrigin.x) isAligned = true;

                if (sameShape && sameColor && sameValue && isAligned)
                {
                    targetPiece = hit;
                    return true;
                }
            }
            return false;
        }

        public bool CheckEscapeOpportunity(PuzzlePiece piece, Vector2Int origin, Vector2Int direction, out ExitGateView targetGate)
        {
            targetGate = null;
            if (piece == null || direction == Vector2Int.zero) return false;

            Vector2Int candidateOrigin = origin + direction;
            List<Vector2Int> candidateCells = piece.GetOccupiedCellsForOrigin(candidateOrigin);

            bool anyOutside = false;
            foreach (var c in candidateCells)
            {
                if (!IsValidCoord(c.x, c.y))
                {
                    anyOutside = true;
                    break;
                }
            }

            if (anyOutside)
            {
                ExitGateView gate = CheckGateAlignment(piece, direction, candidateCells);
                if (gate != null && piece.PieceColor == gate.GateColor && piece.Value >= gate.RequiredValue)
                {
                    targetGate = gate;
                    return true;
                }
            }
            return false;
        }

        // ==================== SLIDING PHYSICS & RAYCASTING ====================

        public enum SlideActionType
        {
            Blocked,
            Slide,
            Merge,
            Escape
        }

        public struct SlideResult
        {
            public SlideActionType Action;
            public int Steps;
            public Vector2Int TargetOrigin;
            public PuzzlePiece TargetPiece;
            public ExitGateView TargetGate;
            public string FailReason;
        }

        public SlideResult RaycastSlide(PuzzlePiece piece, Vector2Int direction, int maxStepsRequested = 8)
        {
            SlideResult result = new SlideResult
            {
                Action = SlideActionType.Blocked,
                Steps = 0,
                TargetOrigin = piece.GridOrigin,
                TargetPiece = null,
                TargetGate = null,
                FailReason = string.Empty
            };

            int maxStepsPossible = Mathf.Clamp(maxStepsRequested, 1, 8);
            Vector2Int currentOrigin = piece.GridOrigin;

            for (int s = 1; s <= maxStepsPossible; s++)
            {
                Vector2Int candidateOrigin = currentOrigin + direction * s;
                List<Vector2Int> candidateCells = piece.GetOccupiedCellsForOrigin(candidateOrigin);

                // Check 1: Did we hit or pass the boundary (Wall or Gate)?
                bool anyOutside = false;
                foreach (var cell in candidateCells)
                {
                    if (!IsValidCoord(cell.x, cell.y))
                    {
                        anyOutside = true;
                        break;
                    }
                }

                if (anyOutside)
                {
                    // Check if the entire piece aligns with an active exit gate in this direction
                    ExitGateView gate = CheckGateAlignment(piece, direction, candidateCells);
                    if (gate != null)
                    {
                        // Check gate conditions: matching color & sufficient value
                        if (piece.PieceColor != gate.GateColor)
                        {
                            result.FailReason = $"Sai màu cổng! Cổng này màu {gate.ColorDisplayName}, khối của bạn màu {piece.ColorDisplayName}.";
                            break; // Gate acts as a wall
                        }
                        if (piece.Value < gate.RequiredValue)
                        {
                            result.FailReason = $"Chưa đủ số yêu cầu! Cổng cần số {gate.RequiredValue}, khối mới có số {piece.Value}.";
                            break; // Gate acts as a wall
                        }

                        // Valid Escape!
                        result.Action = SlideActionType.Escape;
                        result.Steps = s;
                        result.TargetGate = gate;
                        result.TargetOrigin = candidateOrigin;
                        return result;
                    }
                    else
                    {
                        // Hit outer stone wall
                        break;
                    }
                }

                // Check 2: Check inside grid cells for other blocks or merge
                PuzzlePiece hitPiece = null;
                bool multipleHits = false;

                foreach (var cell in candidateCells)
                {
                    PuzzlePiece occupant = GetPieceAt(cell.x, cell.y);
                    if (occupant != null && occupant != piece)
                    {
                        if (hitPiece == null)
                        {
                            hitPiece = occupant;
                        }
                        else if (hitPiece != occupant)
                        {
                            multipleHits = true;
                        }
                    }
                }

                if (hitPiece != null)
                {
                    // Check merge condition on straight-line collision:
                    // 1) Cùng Shape
                    // 2) Cùng Color
                    // 3) Cùng Number
                    // 4) Thẳng nhau trên dưới trái phải (KHÔNG ĐƯỢC MERGE CHÉO)
                    bool sameShape = (piece.Shape == hitPiece.Shape);
                    bool sameColor = (piece.PieceColor == hitPiece.PieceColor);
                    bool sameValue = (piece.Value == hitPiece.Value);

                    bool isAligned = false;
                    if (direction.x != 0 && piece.GridOrigin.y == hitPiece.GridOrigin.y) isAligned = true;
                    if (direction.y != 0 && piece.GridOrigin.x == hitPiece.GridOrigin.x) isAligned = true;

                    if (!multipleHits && sameShape && sameColor && sameValue && isAligned)
                    {
                        // Valid Merge!
                        result.Action = SlideActionType.Merge;
                        result.TargetOrigin = hitPiece.GridOrigin;
                        result.TargetPiece = hitPiece;
                        int dist = Mathf.Abs(hitPiece.GridOrigin.x - piece.GridOrigin.x) + Mathf.Abs(hitPiece.GridOrigin.y - piece.GridOrigin.y);
                        result.Steps = dist;
                        return result;
                    }
                    else
                    {
                        // Incompatible or misaligned: acts as obstacle, stop before it
                        if (sameColor && sameShape && sameValue && !isAligned)
                        {
                            result.FailReason = "Khối bị lệch hàng/cột, không thể gộp!";
                        }
                        else if (sameColor && !sameValue && sameShape)
                        {
                            result.FailReason = $"Khác số! Chỉ gộp được 2 khối cùng số ({piece.Value} != {hitPiece.Value}).";
                        }
                        else if (!sameColor && sameShape && sameValue)
                        {
                            result.FailReason = $"Khác màu! Không thể gộp khối màu {piece.ColorDisplayName} với màu {hitPiece.ColorDisplayName}.";
                        }
                        else if (!sameShape && sameColor && sameValue)
                        {
                            result.FailReason = "Khác hình dạng! Chỉ gộp được 2 khối cùng hình dáng.";
                        }
                        break;
                    }
                }

                // Step s is completely empty: can slide here
                result.Action = SlideActionType.Slide;
                result.Steps = s;
                result.TargetOrigin = candidateOrigin;
            }

            return result;
        }

        private ExitGateView CheckGateAlignment(PuzzlePiece piece, Vector2Int direction, List<Vector2Int> candidateCells)
        {
            if (direction == new Vector2Int(0, -1)) // Up (towards row -1)
            {
                int minCol = int.MaxValue, maxCol = int.MinValue;
                bool hasExitCell = false;
                foreach (var c in candidateCells)
                {
                    if (c.y < 0) hasExitCell = true;
                    if (c.x < minCol) minCol = c.x;
                    if (c.x > maxCol) maxCol = c.x;
                }
                if (!hasExitCell) return null;

                // Blue Gate: Cols [1, 2]
                if (minCol >= 1 && maxCol <= 2) return FindGate(PieceColor.Blue, GateSide.Top);
                // Green Gate: Cols [3, 4]
                if (minCol >= 3 && maxCol <= 4) return FindGate(PieceColor.Green, GateSide.Top);
            }
            else if (direction == new Vector2Int(1, 0)) // Right
            {
                int minRow = int.MaxValue, maxRow = int.MinValue;
                bool hasExitCell = false;
                foreach (var c in candidateCells)
                {
                    if (c.x >= GameConfig.Columns) hasExitCell = true;
                    if (c.y < minRow) minRow = c.y;
                    if (c.y > maxRow) maxRow = c.y;
                }
                if (!hasExitCell) return null;

                // Red Gate: Rows [0, 1]
                if (minRow >= 0 && maxRow <= 1) return FindGate(PieceColor.Red, GateSide.Right);
                // Pink Gate: Rows [2, 3]
                if (minRow >= 2 && maxRow <= 3) return FindGate(PieceColor.Pink, GateSide.Right);
            }
            else if (direction == new Vector2Int(0, 1)) // Down (towards row GameConfig.Rows)
            {
                int minCol = int.MaxValue, maxCol = int.MinValue;
                bool hasExitCell = false;
                foreach (var c in candidateCells)
                {
                    if (c.y >= GameConfig.Rows) hasExitCell = true;
                    if (c.x < minCol) minCol = c.x;
                    if (c.x > maxCol) maxCol = c.x;
                }
                if (!hasExitCell) return null;

                // Orange Gate: Cols [3, 4]
                if (minCol >= 3 && maxCol <= 4) return FindGate(PieceColor.Orange, GateSide.Bottom);
            }
            else if (direction == new Vector2Int(-1, 0)) // Left
            {
                int minRow = int.MaxValue, maxRow = int.MinValue;
                bool hasExitCell = false;
                foreach (var c in candidateCells)
                {
                    if (c.x < 0) hasExitCell = true;
                    if (c.y < minRow) minRow = c.y;
                    if (c.y > maxRow) maxRow = c.y;
                }
                if (!hasExitCell) return null;

                // Purple Gate: Rows [1, 2]
                if (minRow >= 1 && maxRow <= 2) return FindGate(PieceColor.Purple, GateSide.Left);
                // Yellow Gate: Rows [6, 7]
                if (minRow >= 6 && maxRow <= 7) return FindGate(PieceColor.Yellow, GateSide.Left);
            }

            return null;
        }

        private ExitGateView FindGate(PieceColor color, GateSide side)
        {
            foreach (var g in _activeGates)
            {
                if (g != null && g.GateColor == color && g.Side == side) return g;
            }
            return null;
        }

        public bool TrySlidePiece(PuzzlePiece piece, Vector2Int direction)
        {
            return TrySlidePieceSteps(piece, direction, 8);
        }

        public bool TrySlidePieceSteps(PuzzlePiece piece, Vector2Int direction, int maxStepsRequested)
        {
            if (piece == null || piece.IsSliding || piece.IsEscaping) return false;

            SlideResult result = RaycastSlide(piece, direction, maxStepsRequested);

            if (result.Action == SlideActionType.Blocked || result.Steps == 0)
            {
                // Can't move in this direction -> bump obstacle
                piece.Bump(new Vector2(direction.x, -direction.y)); // Convert grid Y to Unity Y
                SoundManager.Instance.PlayDrop();
                if (!string.IsNullOrEmpty(result.FailReason))
                {
                    GameUI.Instance.ShowFeedbackError(result.FailReason);
                    SoundManager.Instance.PlayError();
                }
                return false;
            }

            // Execute slide motion
            Vector3 targetWorldPos;
            if (result.Action == SlideActionType.Escape && result.TargetGate != null)
            {
                // Escaping blocks must exit dead-center of the gate
                targetWorldPos = result.TargetGate.transform.position;
            }
            else
            {
                targetWorldPos = CalculatePieceWorldCenter(piece.Shape, result.TargetOrigin);
            }

            // Smooth speed based on remaining distance to target (prevents drag-and-snap stutter)
            float remainingDist = Vector3.Distance(piece.transform.position, targetWorldPos);
            float duration = Mathf.Clamp(remainingDist / 9.5f, 0.08f, 0.25f);

            // Temporarily clear grid occupancy while sliding
            ClearPieceFromGrid(piece);

            if (result.Action == SlideActionType.Slide)
            {
                ExecuteSlide(piece, result.TargetOrigin, duration);
                return true;
            }
            else if (result.Action == SlideActionType.Merge)
            {
                ExecuteMerge(piece, result.TargetPiece, duration);
                return true;
            }
            else if (result.Action == SlideActionType.Escape)
            {
                ExecuteEscape(piece, result.TargetGate, duration);
                return true;
            }

            return false;
        }

        public void ExecuteSlide(PuzzlePiece piece, Vector2Int newOrigin, float duration = 0.12f)
        {
            if (piece == null) return;
            Vector3 targetWorldPos = CalculatePieceWorldCenter(piece.Shape, newOrigin);
            float actualDist = Vector3.Distance(piece.transform.position, targetWorldPos);
            float actualDuration = Mathf.Clamp(actualDist / 9.5f, 0.05f, duration);

            ClearPieceFromGrid(piece);
            SoundManager.Instance.PlayDrop();
            piece.SlideTo(targetWorldPos, newOrigin, actualDuration, () =>
            {
                AssignPieceToGrid(piece, newOrigin);
            });
        }

        public void ExecuteMerge(PuzzlePiece piece, PuzzlePiece target, float duration = 0.12f)
        {
            if (piece == null || target == null) return;
            Vector3 targetWorldPos = target.transform.position;
            Vector2Int targetOrigin = target.GridOrigin;
            float actualDist = Vector3.Distance(piece.transform.position, targetWorldPos);
            float actualDuration = Mathf.Clamp(actualDist / 9.5f, 0.05f, duration);

            ClearPieceFromGrid(piece);
            SoundManager.Instance.PlayDrop();
            piece.SlideTo(targetWorldPos, targetOrigin, actualDuration, () =>
            {
                ClearPieceFromGrid(target);
                _activePieces.Remove(target);
                Destroy(target.gameObject);

                piece.SetValue(piece.Value + 1);
                piece.PlayMergeEffect();
                AssignPieceToGrid(piece, targetOrigin);

                Score += piece.Value * 50;
                OnScoreChanged?.Invoke(Score);
                OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);

                SoundManager.Instance.PlayMerge(piece.Value);
                GameUI.Instance.ShowFloatingText($"+50 HỢP NHẤT [{piece.Value}]!", targetWorldPos, UnityEngine.Color.cyan);
            });
        }

        public void ExecuteEscape(PuzzlePiece piece, ExitGateView gate, float duration = 0.12f)
        {
            if (piece == null || gate == null) return;
            Vector3 gatePos = gate.transform.position;
            float actualDist = Vector3.Distance(piece.transform.position, gatePos);
            float actualDuration = Mathf.Clamp(actualDist / 9.5f, 0.05f, duration);

            ClearPieceFromGrid(piece);
            SoundManager.Instance.PlayDrop();
            piece.SlideTo(gatePos, piece.GridOrigin, actualDuration, () =>
            {
                piece.PlayEscapeAnimation(gate.ExitDirection, () =>
                {
                    _activePieces.Remove(piece);
                    Score += piece.Value * 150;
                    OnScoreChanged?.Invoke(Score);
                    OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
                    CheckVictoryCondition();
                });

                SoundManager.Instance.PlayEscape();
                GameUI.Instance.ShowFloatingText("+150 ĐÃ THOÁT CỔNG!", gatePos, UnityEngine.Color.yellow);
            });
        }

        private void CheckVictoryCondition()
        {
            if (_activePieces.Count == 0)
            {
                SoundManager.Instance.PlayGameOver();
                GameUI.Instance.ShowVictoryPopup();
            }
        }
    }
}
