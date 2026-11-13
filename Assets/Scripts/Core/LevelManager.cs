using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Core
{
    public enum LevelId
    {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Level6 = 6,
        Level7 = 7,
        Level8 = 8,
        Level9 = 9,
        Level10 = 10,

        // Backwards compatibility aliases
        Level1_TutorialCross = 1,
        Level2_ReferenceBoard = 2
    }

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelId _currentLevel = LevelId.Level1;
        public LevelId CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }

        public int CurrentColumns => GetLevelColumns(_currentLevel);
        public int CurrentRows => GetLevelRows(_currentLevel);

        public float CurrentCellWidth => GetLevelCellWidth(_currentLevel);
        public float CurrentCellHeight => GetLevelCellHeight(_currentLevel);

        public float CurrentFrameWidth => GetLevelFrameWidth(_currentLevel);
        public float CurrentFrameHeight => GetLevelFrameHeight(_currentLevel);

        private Vector3 _boardCenter = new Vector3(0.015f, 0.073f, 0f);
        private PuzzlePiece[,] _grid;
        private List<PuzzlePiece> _activePieces = new List<PuzzlePiece>();
        private List<ExitGateView> _activeGates = new List<ExitGateView>();

        private Transform _backgroundRoot;
        private Transform _frameRoot;
        private Transform _gatesRoot;
        private Transform _piecesRoot;
        private GameObject _initialPiecesTemplate;
        private LevelId _snapshotLevel;

        public LevelId InitialSceneLevel { get; private set; }

        public int Score { get; private set; }
        public int RemainingBlocksCount => _activePieces.Count;
        public IReadOnlyList<ExitGateView> Gates => _activeGates;
        public IReadOnlyList<PuzzlePiece> ActivePieces => _activePieces;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnRemainingBlocksChanged;
        public event Action OnLevelInitialized;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitialSceneLevel = _currentLevel;
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
            _grid = new PuzzlePiece[CurrentColumns, CurrentRows];

            _frameRoot = new GameObject("Board_Frame").transform;
            _frameRoot.SetParent(transform);

            _gatesRoot = new GameObject("Exit_Gates").transform;
            _gatesRoot.SetParent(transform);

            _piecesRoot = new GameObject("Active_Pieces").transform;
            _piecesRoot.SetParent(transform);

            BuildBackground();
            BuildBoardFrame();
            BuildExitGates();
            if (_currentLevel == LevelId.Level1 || _currentLevel == LevelId.Level2 || _currentLevel == LevelId.Level3)
            {
                SpawnPieces();
            }
            FitCameraToBoard();
            FitBackgroundToCamera();
        }
#endif

        public void InitializeLevel(bool forceRebuild = false)
        {
            StopAllCoroutines();
            PlayerPieceController.Instance?.ResetSelection();
            Score = 0;

            _backgroundRoot = transform.Find("Background");
            _frameRoot = transform.Find("Board_Frame");
            _gatesRoot = transform.Find("Exit_Gates");
            _piecesRoot = transform.Find("Active_Pieces");

            if (forceRebuild && _initialPiecesTemplate != null && _snapshotLevel == _currentLevel && _frameRoot != null && _gatesRoot != null)
            {
                if (_piecesRoot != null)
                {
                    _piecesRoot.name = "__DestroyedPieces";
                    _piecesRoot.gameObject.SetActive(false);
                    Destroy(_piecesRoot.gameObject);
                }

                GameObject restored = Instantiate(_initialPiecesTemplate, transform);
                restored.name = "Active_Pieces";
                restored.SetActive(true);
                _piecesRoot = restored.transform;

                if (_backgroundRoot == null || _backgroundRoot.childCount == 0)
                {
                    BuildBackground();
                }

                BindSceneObjects();
                OnScoreChanged?.Invoke(Score);
                OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
                SoundManager.Instance?.ResetMergeCombo();
                FitCameraToBoard();
                FitBackgroundToCamera();
                OnLevelInitialized?.Invoke();
                return;
            }

            bool hasSceneObjects = !forceRebuild && (_frameRoot != null && _gatesRoot != null);

            if (!hasSceneObjects)
            {
                ClearCurrentLevel();
                BuildBackground();
                BuildBoardFrame();
                BuildExitGates();
                SpawnPieces();
            }
            else
            {
                if (_backgroundRoot == null || _backgroundRoot.childCount == 0)
                {
                    BuildBackground();
                }
                if (_piecesRoot == null || _piecesRoot.childCount == 0)
                {
                    if (_piecesRoot == null)
                    {
                        _piecesRoot = new GameObject("Active_Pieces").transform;
                        _piecesRoot.SetParent(transform);
                    }
                    SpawnPieces();
                }
                BindSceneObjects();
            }

            if (_initialPiecesTemplate == null && _piecesRoot != null)
            {
                _snapshotLevel = _currentLevel;
                _initialPiecesTemplate = Instantiate(_piecesRoot.gameObject, transform);
                _initialPiecesTemplate.name = "_InitialPiecesSnapshot";
                _initialPiecesTemplate.SetActive(false);
            }

            OnScoreChanged?.Invoke(Score);
            OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
            SoundManager.Instance?.ResetMergeCombo();
            FitCameraToBoard();
            FitBackgroundToCamera();
            OnLevelInitialized?.Invoke();
        }

        private void FitCameraToBoard()
        {
            var cam = Camera.main;
            if (cam == null) return;
            float screenRatio = cam.aspect > 0.05f ? cam.aspect : (1440f / 3200f);
            if (screenRatio > 0.75f) screenRatio = 1440f / 3200f; // Force target portrait ratio (0.45) in Edit Mode
            float paddingX = 0.12f;
            float paddingY = 0.40f;
            float sizeByWidth  = (CurrentFrameWidth  / 2f + paddingX) / screenRatio;
            float sizeByHeight =  CurrentFrameHeight / 2f + paddingY;
            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void BindSceneObjects()
        {
            if (_gatesRoot == null) _gatesRoot = transform.Find("Exit_Gates");
            if (_piecesRoot == null) _piecesRoot = transform.Find("Active_Pieces");
            if (_frameRoot == null) _frameRoot = transform.Find("Board_Frame");
            if (_backgroundRoot == null) _backgroundRoot = transform.Find("Background");

            _activePieces.Clear();
            _activeGates.Clear();
            _grid = new PuzzlePiece[CurrentColumns, CurrentRows];

            if (_gatesRoot != null)
            {
                foreach (var gate in _gatesRoot.GetComponentsInChildren<ExitGateView>(true))
                {
                    gate.EnsureGateCoords();
                    _activeGates.Add(gate);
                }
            }

            if (_piecesRoot != null)
            {
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
        }

        private void ClearCurrentLevel()
        {
            DestroyRootSafely(ref _backgroundRoot);
            DestroyRootSafely(ref _frameRoot);
            DestroyRootSafely(ref _gatesRoot);
            DestroyRootSafely(ref _piecesRoot);

            _activePieces.Clear();
            _activeGates.Clear();
            _grid = new PuzzlePiece[CurrentColumns, CurrentRows];

            _frameRoot = new GameObject("Board_Frame").transform;
            _frameRoot.SetParent(transform);

            _gatesRoot = new GameObject("Exit_Gates").transform;
            _gatesRoot.SetParent(transform);

            _piecesRoot = new GameObject("Active_Pieces").transform;
            _piecesRoot.SetParent(transform);
        }

        private static void DestroyRootSafely(ref Transform root)
        {
            if (root == null) return;
            root.name = "__Destroyed";
            root.gameObject.SetActive(false);
            Destroy(root.gameObject);
            root = null;
        }

        private void BuildBackground()
        {
            if (_backgroundRoot == null)
            {
                _backgroundRoot = transform.Find("Background");
                if (_backgroundRoot == null)
                {
                    _backgroundRoot = new GameObject("Background").transform;
                    _backgroundRoot.SetParent(transform);
                }
            }

            GameObject bgObj = _backgroundRoot.childCount > 0
                ? _backgroundRoot.GetChild(0).gameObject
                : new GameObject("BackgroundSprite");
            bgObj.transform.SetParent(_backgroundRoot);

            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -10;
            sr.color = new Color(0.42f, 0.42f, 0.48f, 1f);

            Sprite bgSprite = GemSpriteManager.GetBackgroundSprite();
            if (bgSprite != null) sr.sprite = bgSprite;

            FitBackgroundToCamera();
        }

        public void FitBackgroundToCamera()
        {
            if (_backgroundRoot == null)
            {
                _backgroundRoot = transform.Find("Background");
                if (_backgroundRoot == null) return;
            }

            SpriteRenderer sr = _backgroundRoot.GetComponentInChildren<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;
            sr.color = new Color(0.42f, 0.42f, 0.48f, 1f);

            var cam = Camera.main;
            if (cam == null) return;

            float screenRatio = cam.aspect > 0.05f ? cam.aspect : (1440f / 3200f);
            if (screenRatio > 0.75f) screenRatio = 1440f / 3200f;

            float camH = 2f * cam.orthographicSize;
            float camW = camH * screenRatio;

            float nativeW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

            float scale = Mathf.Max(camH / nativeH, camW / nativeW);
            sr.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void BuildBoardFrame()
        {
            GameObject frameObj = new GameObject("WallFrame");
            frameObj.transform.SetParent(_frameRoot);

            SpriteRenderer sr = frameObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;

            int levelIndex = (int)_currentLevel;
            Sprite mapSprite = GemSpriteManager.GetMapSprite(levelIndex);
            sr.sprite = mapSprite;

            Vector3 frameOffset = GetLevelFrameOffset(_currentLevel);
            frameObj.transform.position = _boardCenter + frameOffset;

            if (sr.sprite != null)
            {
                float nativeW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                frameObj.transform.localScale = new Vector3(
                    CurrentFrameWidth / nativeW,
                    CurrentFrameHeight / nativeH,
                    1f);
            }
        }

        private void BuildExitGates()
        {
            BuildLevelGates(_currentLevel);
        }

        private void BuildLevelGates(LevelId level)
        {
            float cw = CurrentCellWidth;
            float ch = CurrentCellHeight;
            Vector2 hGateSize = new Vector2(cw * 1.25f, 0.70f);
            Vector2 vGateSize = new Vector2(0.70f, ch * 1.25f);

            switch (level)
            {
                case LevelId.Level1:
                    // Top Red Gate (col 2, row 0 -> exit at (2, -1)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Top, Vector2.up,
                        _boardCenter + GetLevelFrameOffset(LevelId.Level1) + new Vector3(0f, 1.409f, 0f),
                        new Vector2(1.15f, 0.90f), _gatesRoot,
                        new[] { new Vector2Int(2, -1) },
                        default,
                        true));
                    break;

                case LevelId.Level2:
                    // Top Green Gate (col 2, row 0 -> exit at (2, -1)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Top, Vector2.up,
                        GridToWorld(2, 0),
                        new Vector2(1.15f, 0.95f), _gatesRoot,
                        new[] { new Vector2Int(2, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(2, 0) }));

                    // Right Red Gate (col 5, row 3 -> exit at (6, 3)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 3, GateSide.Right, Vector2.right,
                        GridToWorld(5, 3),
                        new Vector2(0.95f, 1.15f), _gatesRoot,
                        new[] { new Vector2Int(6, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(5, 3) }));
                    break;

                case LevelId.Level3:
                    // Top-Left Green Gate (col 0, row 0 -> exit at (-1, 0)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 4, GateSide.Left, Vector2.left,
                        GridToWorld(0, 0),
                        new Vector2(0.95f, 1.15f), _gatesRoot,
                        new[] { new Vector2Int(-1, 0) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 0) }));

                    // Right Red Gate (col 4, row 3 -> exit at (5, 3)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 4, GateSide.Right, Vector2.right,
                        GridToWorld(4, 3),
                        new Vector2(0.95f, 1.15f), _gatesRoot,
                        new[] { new Vector2Int(5, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 3) }));
                    break;

                case LevelId.Level4:
                    // Right Red Gate (col 3, row 1)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 1) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(4, 1) }));
                    // Right Green Gate (col 3, row 3)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 3) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(4, 3) }));
                    // Right Pink Gate (col 3, row 5)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 5) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(4, 5) }));
                    break;

                case LevelId.Level5:
                    // Top Orange Gate (col 2, row 0)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Top, Vector2.up,
                        GridToWorld(2, 0) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(2, -1) }));
                    // Bottom Green Gate (col 2, row 6)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Bottom, Vector2.down,
                        GridToWorld(2, 6) + Vector3.down * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(2, 7) }));
                    break;

                case LevelId.Level6:
                    // Top Red Gate (col 2, row 1)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Top, Vector2.up,
                        GridToWorld(2, 1) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(2, 0) }));
                    // Right Green Gate (col 4, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Right, Vector2.right,
                        GridToWorld(4, 2) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(5, 2) }));
                    // Left Orange Gate (col 0, row 4)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 4) + Vector3.left * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(-1, 4) }));
                    // Left Pink Gate (col 0, row 5)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 5) + Vector3.left * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(-1, 5) }));
                    break;

                case LevelId.Level7:
                    // Top Green Gate (col 3, row 0)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Top, Vector2.up,
                        GridToWorld(3, 0) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(3, -1) }));
                    // Left Orange Gate (col 0, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 2) + Vector3.left * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(-1, 2) }));
                    // Right Pink Gate (col 6, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Right, Vector2.right,
                        GridToWorld(6, 2) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(7, 2) }));
                    break;

                case LevelId.Level8:
                    // Top Red Gate (col 0, row 0)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Top, Vector2.up,
                        GridToWorld(0, 0) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(0, -1) }));
                    // Right Pink Gate (col 3, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 2) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(4, 2) }));
                    // Right Blue Gate (col 3, row 4)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Blue, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 4) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(4, 4) }));
                    // Bottom Green Gate (col 1, row 5)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Bottom, Vector2.down,
                        GridToWorld(1, 5) + Vector3.down * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(1, 6) }));
                    break;

                case LevelId.Level9:
                    // Top Purple Gate (col 2, row 0)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Purple, 2, GateSide.Top, Vector2.up,
                        GridToWorld(2, 0) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(2, -1) }));
                    // Left Orange Gate (col 0, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 2) + Vector3.left * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(-1, 2) }));
                    // Right Green Gate (col 4, row 2)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Right, Vector2.right,
                        GridToWorld(4, 2) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(5, 2) }));
                    // Bottom-Left Red Gate (col 1, row 6)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Bottom, Vector2.down,
                        GridToWorld(1, 6) + Vector3.down * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(1, 7) }));
                    // Bottom-Right Blue Gate (col 3, row 6)
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Blue, 2, GateSide.Bottom, Vector2.down,
                        GridToWorld(3, 6) + Vector3.down * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(3, 7) }));
                    break;

                case LevelId.Level10:
                    // Top Red Gate (col 3, row 0): Needs 3
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 3, GateSide.Top, Vector2.up,
                        GridToWorld(3, 0) + Vector3.up * (ch * 0.85f), hGateSize, _gatesRoot,
                        new[] { new Vector2Int(3, -1) }));
                    // Left Green Gate (col 1, row 3): Needs 2
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Left, Vector2.left,
                        GridToWorld(1, 3) + Vector3.left * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(0, 3) }));
                    // Right Green Gate (col 5, row 3): Needs 2
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Right, Vector2.right,
                        GridToWorld(5, 3) + Vector3.right * (cw * 0.85f), vGateSize, _gatesRoot,
                        new[] { new Vector2Int(6, 3) }));
                    break;
            }
        }

        private void SpawnPieces()
        {
            SpawnLevelPieces(_currentLevel);
        }

        private void SpawnLevelPieces(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(4, 1));
                    break;

                case LevelId.Level2:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(2, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(0, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(4, 3));
                    break;

                case LevelId.Level3:
                    // 4 Red pieces: 1, 1, 2, 3 -> Merges into Red 4
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 6));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 3, new Vector2Int(2, 6));

                    // 4 Green pieces: 1, 1, 2, 3 -> Merges into Green 4
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(3, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(3, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(1, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 3, new Vector2Int(4, 6));
                    break;

                case LevelId.Level4:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 0));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(0, 2));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(1, 5));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(0, 6));
                    break;

                case LevelId.Level5:
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 2));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(3, 2));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 4));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(3, 4));
                    SpawnPiece(PieceColor.Blue, PieceShape.Horizontal_2x1, 1, new Vector2Int(1, 3));
                    break;

                case LevelId.Level6:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(3, 1));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 2));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(3, 3));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 4));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(1, 5));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(2, 5));
                    break;

                case LevelId.Level7:
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 1));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(4, 1));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 2));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(5, 2));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(4, 3));
                    break;

                case LevelId.Level8:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(2, 2));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(1, 4));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(0, 5));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 3));
                    break;

                case LevelId.Level9:
                    SpawnPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, new Vector2Int(3, 1));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 2));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(3, 2));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(1, 5));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(3, 5));
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(2, 5));
                    break;

                case LevelId.Level10:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(3, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(3, 2));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 4));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(4, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(5, 4));
                    SpawnPiece(PieceColor.Blue, PieceShape.Horizontal_2x1, 1, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 1, new Vector2Int(4, 5));
                    break;
            }
        }

        private PuzzlePiece SpawnPiece(PieceColor color, PieceShape shape, int value, Vector2Int origin)
        {
            Vector3 worldPos = CalculatePieceWorldCenter(shape, origin);
            Vector2? customCellSize = _currentLevel == LevelId.Level1
                ? new Vector2(1.125f, 1.06f)
                : (_currentLevel == LevelId.Level3 ? new Vector2(1.05f, 1.00f) : (Vector2?)null);
            PuzzlePiece piece = PuzzlePiece.Create(color, shape, value, origin, worldPos, _piecesRoot, customCellSize);

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

        private static readonly Dictionary<Vector2Int, Vector3> Level2CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            { new Vector2Int(2, 0), new Vector3(0.030f, 3.019f, 0f) },
            { new Vector2Int(2, 1), new Vector3(0.030f, 2.061f, 0f) },
            { new Vector2Int(2, 2), new Vector3(0.030f, 1.028f, 0f) },
            { new Vector2Int(0, 3), new Vector3(-2.672f, -0.047f, 0f) },
            { new Vector2Int(1, 3), new Vector3(-1.300f, -0.047f, 0f) },
            { new Vector2Int(2, 3), new Vector3(0.030f, -0.047f, 0f) },
            { new Vector2Int(3, 3), new Vector3(1.284f, -0.047f, 0f) },
            { new Vector2Int(4, 3), new Vector3(2.496f, -0.047f, 0f) },
            { new Vector2Int(5, 3), new Vector3(3.508f, -0.047f, 0f) },
            { new Vector2Int(2, 4), new Vector3(0.030f, -1.137f, 0f) },
            { new Vector2Int(2, 5), new Vector3(0.030f, -2.240f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level3CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            { new Vector2Int(0, 0), new Vector3( -2.385f,   3.627f, 0f) },
            { new Vector2Int(1, 0), new Vector3( -1.116f,   3.627f, 0f) },
            { new Vector2Int(2, 0), new Vector3(  0.153f,   3.621f, 0f) },
            { new Vector2Int(2, 1), new Vector3(  0.153f,   2.418f, 0f) },
            { new Vector2Int(2, 2), new Vector3(  0.159f,   1.222f, 0f) },
            { new Vector2Int(2, 3), new Vector3(  0.159f,   0.027f, 0f) },
            { new Vector2Int(3, 3), new Vector3(  1.422f,   0.086f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  2.691f,   0.086f, 0f) },
            { new Vector2Int(2, 4), new Vector3(  0.179f,  -1.169f, 0f) },
            { new Vector2Int(2, 5), new Vector3(  0.186f,  -2.358f, 0f) },
            { new Vector2Int(0, 6), new Vector3( -2.385f,  -3.528f, 0f) },
            { new Vector2Int(1, 6), new Vector3( -1.116f,  -3.528f, 0f) },
            { new Vector2Int(2, 6), new Vector3(  0.153f,  -3.528f, 0f) },
            { new Vector2Int(3, 6), new Vector3(  1.422f,  -3.528f, 0f) },
            { new Vector2Int(4, 6), new Vector3(  2.691f,  -3.528f, 0f) },
        };

        public Vector3 GridToWorld(int col, int row)
        {
            if (_currentLevel == LevelId.Level2)
            {
                if (Level2CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return pos;
                }
                if (col == 2 && row < 0) return Level2CellPositions[new Vector2Int(2, 0)] + Vector3.up * (-row * 1.0f);
                if (col == 2 && row > 5) return Level2CellPositions[new Vector2Int(2, 5)] + Vector3.down * ((row - 5) * 1.0f);
                if (row == 3 && col < 0) return Level2CellPositions[new Vector2Int(0, 3)] + Vector3.left * (-col * 1.0f);
                if (row == 3 && col > 5) return Level2CellPositions[new Vector2Int(5, 3)] + Vector3.right * ((col - 5) * 1.0f);
            }

            if (_currentLevel == LevelId.Level3)
            {
                if (Level3CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                if (col < 0 && row == 0) return _boardCenter + Level3CellPositions[new Vector2Int(0, 0)] + Vector3.left * (-col * 1.25f);
                if (col > 4 && row == 3) return _boardCenter + Level3CellPositions[new Vector2Int(4, 3)] + Vector3.right * ((col - 4) * 1.25f);
            }

            float cw = CurrentCellWidth;
            float ch = CurrentCellHeight;
            float totalW = CurrentColumns * cw;
            float totalH = CurrentRows * ch;

            Vector3 center = _boardCenter + GetLevelGridOffset(_currentLevel);
            float x = center.x - (totalW * 0.5f) + (col + 0.5f) * cw;
            float y = center.y + (totalH * 0.5f) - (row + 0.5f) * ch;
            return new Vector3(x, y, 0f);
        }

        public bool IsValidCoord(int col, int row)
        {
            return CheckLevelValidCoord(_currentLevel, col, row);
        }

        // ==================== 10 LEVEL STATIC CONFIGURATION ====================

        public static int GetLevelColumns(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 5;
                case LevelId.Level2: return 6;
                case LevelId.Level3: return 5;
                case LevelId.Level4: return 4;
                case LevelId.Level5: return 5;
                case LevelId.Level6: return 5;
                case LevelId.Level7: return 7;
                case LevelId.Level8: return 4;
                case LevelId.Level9: return 5;
                case LevelId.Level10: return 7;
                default: return 7;
            }
        }

        public static int GetLevelRows(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 2;
                case LevelId.Level2: return 6;
                case LevelId.Level3: return 7;
                case LevelId.Level4: return 7;
                case LevelId.Level5: return 7;
                case LevelId.Level6: return 7;
                case LevelId.Level7: return 4;
                case LevelId.Level8: return 6;
                case LevelId.Level9: return 7;
                case LevelId.Level10: return 7;
                default: return 6;
            }
        }

        public static float GetLevelCellWidth(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 1.36f;
                case LevelId.Level2: return 1.16f;
                case LevelId.Level3: return 1.269f;
                case LevelId.Level4: return 1.10f;
                case LevelId.Level5: return 1.10f;
                case LevelId.Level6: return 1.05f;
                case LevelId.Level7: return 0.98f;
                case LevelId.Level8: return 1.15f;
                case LevelId.Level9: return 1.05f;
                case LevelId.Level10: return 0.98f;
                default: return 1.00f;
            }
        }

        public static float GetLevelCellHeight(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 1.20f;
                case LevelId.Level2: return 1.03f;
                case LevelId.Level3: return 1.192f;
                case LevelId.Level4: return 1.10f;
                case LevelId.Level5: return 1.10f;
                case LevelId.Level6: return 1.05f;
                case LevelId.Level7: return 0.85f;
                case LevelId.Level8: return 1.10f;
                case LevelId.Level9: return 1.05f;
                case LevelId.Level10: return 0.98f;
                default: return 1.00f;
            }
        }

        public static float GetLevelFrameWidth(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 8.000f;
                case LevelId.Level2: return 8.000f;
                case LevelId.Level3: return 7.600f;
                case LevelId.Level4: return 7.459f;
                case LevelId.Level5: return 8.000f;
                case LevelId.Level6: return 7.519f;
                case LevelId.Level7: return 8.000f;
                case LevelId.Level8: return 8.000f;
                case LevelId.Level9: return 7.947f;
                case LevelId.Level10: return 8.000f;
                default: return 8.000f;
            }
        }

        public static float GetLevelFrameHeight(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 4.413f;
                case LevelId.Level2: return 7.263f;
                case LevelId.Level3: return 9.673f;
                case LevelId.Level4: return 10.000f;
                case LevelId.Level5: return 9.890f;
                case LevelId.Level6: return 10.000f;
                case LevelId.Level7: return 3.878f;
                case LevelId.Level8: return 9.811f;
                case LevelId.Level9: return 10.000f;
                case LevelId.Level10: return 8.209f;
                default: return 7.535f;
            }
        }

        public static Vector3 GetLevelFrameOffset(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return new Vector3(0f, -0.20f, 0f);
                case LevelId.Level7: return new Vector3(0f, -0.15f, 0f);
                default: return Vector3.zero;
            }
        }

        public static Vector3 GetLevelGridOffset(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return new Vector3(0f, -0.526f, 0f);
                case LevelId.Level2: return new Vector3(0.617f, 0.426f, 0f);
                case LevelId.Level7: return new Vector3(0f, -0.15f, 0f);
                default: return Vector3.zero;
            }
        }

        public static bool CheckLevelValidCoord(LevelId level, int col, int row)
        {
            switch (level)
            {
                case LevelId.Level1:
                    return (col == 2 && row == 0) || (row == 1 && col >= 0 && col < 5);

                case LevelId.Level2:
                    return (col == 2 && row >= 0 && row < 6) || (row == 3 && col >= 0 && col < 6);

                case LevelId.Level3:
                    if (row == 0) return col >= 0 && col <= 2;
                    if (row == 1 || row == 2) return col == 2;
                    if (row == 3) return col >= 2 && col <= 4;
                    if (row == 4 || row == 5) return col == 2;
                    if (row == 6) return col >= 0 && col <= 4;
                    return false;

                case LevelId.Level4:
                    return (col == 0 && row >= 0 && row <= 6) ||
                           (row == 1 && col >= 0 && col <= 3) ||
                           (row == 3 && col >= 0 && col <= 3) ||
                           (row == 5 && col >= 0 && col <= 3);

                case LevelId.Level5:
                    if (row == 0 || row == 6) return col == 2;
                    if (row == 1 || row == 5) return col >= 1 && col <= 3;
                    if (row >= 2 && row <= 4) return col >= 0 && col <= 4;
                    return false;

                case LevelId.Level6:
                    if (row == 0) return false;
                    if (row == 1) return col >= 1 && col <= 3;
                    if (row == 2) return col >= 2 && col <= 4;
                    if (row == 3) return col >= 1 && col <= 4;
                    if (row == 4 || row == 5) return col >= 0 && col <= 3;
                    if (row == 6) return col >= 0 && col <= 2;
                    return false;

                case LevelId.Level7:
                    if (row == 0) return col >= 2 && col <= 4;
                    if (row == 1) return col >= 1 && col <= 5;
                    if (row == 2 || row == 3) return col >= 0 && col <= 6;
                    return false;

                case LevelId.Level8:
                    if (col >= 0 && col <= 1 && row >= 0 && row <= 5) return true;
                    if (col >= 2 && col <= 3 && row >= 1 && row <= 5) return true;
                    return false;

                case LevelId.Level9:
                    if (col >= 1 && col <= 3 && row >= 0 && row <= 6) return true;
                    if ((row == 2 || row == 5) && col >= 0 && col <= 4) return true;
                    return false;

                case LevelId.Level10:
                    if (row == 0) return col == 3;
                    if (row == 1 || row == 2) return col >= 2 && col <= 4;
                    if (row == 3 || row == 4) return col >= 1 && col <= 5;
                    if (row == 5) return col >= 0 && col <= 6;
                    if (row == 6) return col >= 2 && col <= 4;
                    return false;

                default:
                    return col >= 0 && col < GetLevelColumns(level) && row >= 0 && row < GetLevelRows(level);
            }
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

        public ExitGateView GetGateAt(Vector2Int coord)
        {
            if (_activeGates == null) return null;
            foreach (var gate in _activeGates)
            {
                if (gate != null && gate.ContainsGateCell(coord)) return gate;
            }
            return null;
        }

        public bool CanPieceFitAt(PuzzlePiece piece, Vector2Int origin)
        {
            if (piece == null) return false;
            List<Vector2Int> cells = piece.GetOccupiedCellsForOrigin(origin);
            foreach (var c in cells)
            {
                if (!IsValidCoord(c.x, c.y)) return false;
                if (GetGateAt(c) != null) return false;
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

            ExitGateView gate = null;
            foreach (var cell in candidateCells)
            {
                gate = GetGateAt(cell);
                if (gate != null) break;
            }

            if (gate == null)
            {
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
                    gate = CheckGateAlignment(piece, direction, candidateCells);
                }
            }

            if (gate != null && gate.TriggerDirection == direction && piece.PieceColor == gate.GateColor && piece.Value >= gate.RequiredValue)
            {
                targetGate = gate;
                return true;
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

                // Check 1: Did we hit a Gate or Boundary Wall?
                ExitGateView gate = null;
                foreach (var cell in candidateCells)
                {
                    gate = GetGateAt(cell);
                    if (gate != null) break;
                }

                bool anyOutside = false;
                foreach (var cell in candidateCells)
                {
                    if (!IsValidCoord(cell.x, cell.y))
                    {
                        anyOutside = true;
                        break;
                    }
                }

                if (gate == null && anyOutside)
                {
                    gate = CheckGateAlignment(piece, direction, candidateCells);
                }

                if (gate != null)
                {
                    if (gate.TriggerDirection == direction)
                    {
                        // Check gate conditions: matching color & sufficient value
                        if (piece.PieceColor != gate.GateColor)
                        {
                            result.FailReason = $"Sai màu cổng! Cổng này màu {gate.ColorDisplayName}, khối của bạn màu {piece.ColorDisplayName}.";
                            break; // Gate acts as a wall - DO NOT STEP ON IT
                        }
                        if (piece.Value < gate.RequiredValue)
                        {
                            result.FailReason = $"Chưa đủ số yêu cầu! Cổng cần số {gate.RequiredValue}, khối mới có số {piece.Value}.";
                            break; // Gate acts as a wall - DO NOT STEP ON IT
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
                        // Wrong direction hitting gate
                        result.FailReason = "Cổng không mở theo hướng này!";
                        break;
                    }
                }

                if (anyOutside)
                {
                    // Hit outer stone wall
                    break;
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
                    // 1) CÃ¹ng Shape
                    // 2) CÃ¹ng Color
                    // 3) CÃ¹ng Number
                    // 4) Tháº³ng nhau trÃªn dÆ°á»›i trÃ¡i pháº£i (KHÃ”NG ÄÆ¯á»¢C MERGE CHÃ‰O)
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
                            result.FailReason = "Khá»‘i bá»‹ lá»‡ch hÃ ng/cá»™t, khÃ´ng thá»ƒ gá»™p!";
                        }
                        else if (sameColor && !sameValue && sameShape)
                        {
                            result.FailReason = $"KhÃ¡c sá»‘! Chá»‰ gá»™p Ä‘Æ°á»£c 2 khá»‘i cÃ¹ng sá»‘ ({piece.Value} != {hitPiece.Value}).";
                        }
                        else if (!sameColor && sameShape && sameValue)
                        {
                            result.FailReason = $"KhÃ¡c mÃ u! KhÃ´ng thá»ƒ gá»™p khá»‘i mÃ u {piece.ColorDisplayName} vá»›i mÃ u {hitPiece.ColorDisplayName}.";
                        }
                        else if (!sameShape && sameColor && sameValue)
                        {
                            result.FailReason = "KhÃ¡c hÃ¬nh dáº¡ng! Chá»‰ gá»™p Ä‘Æ°á»£c 2 khá»‘i cÃ¹ng hÃ¬nh dÃ¡ng.";
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
            foreach (var gate in _activeGates)
            {
                if (gate == null) continue;
                if (gate.TriggerDirection != direction) continue;

                foreach (var c in candidateCells)
                {
                    if (gate.TriggerCoords != null && gate.TriggerCoords.Contains(c))
                    {
                        return gate;
                    }
                }
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
            target.LockForMerge();
            SoundManager.Instance?.PlayDrop();

            piece.SlideAndSlamMerge(targetWorldPos, targetOrigin, target,
                onImpact: () =>
                {
                    if (target != null)
                    {
                        ClearPieceFromGrid(target);
                        _activePieces.Remove(target);
                        Destroy(target.gameObject);
                    }

                    piece.SetValue(piece.Value + 1);
                    AssignPieceToGrid(piece, targetOrigin);

                    Score += piece.Value * 50;
                    OnScoreChanged?.Invoke(Score);
                    OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);

                    SoundManager.Instance?.PlayMerge(piece.Value);
                    SpawnMergeFlash(targetWorldPos);
                    GameUI.Instance?.ShowFloatingText($"+50 HỢP NHẤT [{piece.Value}]!", targetWorldPos, UnityEngine.Color.cyan);
                },
                onArrival: null,
                approachDuration: actualDuration);
        }

        public void SpawnMergeFlash(Vector3 worldPos)
        {
            StartCoroutine(CoMergeFlash(worldPos));
        }

        private IEnumerator CoMergeFlash(Vector3 worldPos)
        {
            GameObject flashObj = new GameObject("MergeFlashEffect");
            flashObj.transform.position = new Vector3(worldPos.x, worldPos.y, -0.5f);

            GameObject glowObj = new GameObject("RadialGlow");
            glowObj.transform.SetParent(flashObj.transform, false);
            SpriteRenderer glowSr = glowObj.AddComponent<SpriteRenderer>();
            glowSr.sprite = SpriteGenerator.GetRadialGlowSprite();
            glowSr.sortingOrder = 90;
            glowSr.color = new UnityEngine.Color(1f, 0.98f, 0.82f, 0.95f);

            GameObject flareObj = new GameObject("StarFlare");
            flareObj.transform.SetParent(flashObj.transform, false);
            SpriteRenderer flareSr = flareObj.AddComponent<SpriteRenderer>();
            flareSr.sprite = SpriteGenerator.GetStarFlareSprite();
            flareSr.sortingOrder = 92;
            flareSr.color = new UnityEngine.Color(1f, 1f, 1f, 1.0f);

            KickCameraImpact(0.045f, 0.07f);

            float duration = 0.22f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float haloScale = Mathf.Lerp(0.40f, 2.40f, Mathf.Sqrt(t));
                glowObj.transform.localScale = new Vector3(haloScale, haloScale, 1f);
                glowSr.color = new UnityEngine.Color(1f, 0.98f, 0.82f, (1f - t) * 0.95f);

                float flareScale = Mathf.Lerp(0.25f, 1.75f, Mathf.Sqrt(t));
                flareObj.transform.localScale = new Vector3(flareScale, flareScale, 1f);
                flareObj.transform.localRotation = Quaternion.Euler(0f, 0f, t * 30f);
                flareSr.color = new UnityEngine.Color(1f, 1f, 1f, Mathf.Pow(1f - t, 1.5f));

                yield return null;
            }

            Destroy(flashObj);
        }

        private Coroutine _cameraKickCoroutine;

        public void KickCameraImpact(float intensity = 0.045f, float duration = 0.07f)
        {
            if (_cameraKickCoroutine != null) StopCoroutine(_cameraKickCoroutine);
            _cameraKickCoroutine = StartCoroutine(CoCameraKick(intensity, duration));
        }

        private IEnumerator CoCameraKick(float intensity, float duration)
        {
            Camera cam = Camera.main;
            if (cam == null) yield break;
            Vector3 basePos = cam.transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float kickY = -Mathf.Sin(t * Mathf.PI) * intensity;
                cam.transform.position = new Vector3(basePos.x, basePos.y + kickY, basePos.z);
                yield return null;
            }

            cam.transform.position = basePos;
            _cameraKickCoroutine = null;
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
                GameUI.Instance.ShowFloatingText("+150 ÄÃƒ THOÃT Cá»”NG!", gatePos, UnityEngine.Color.yellow);
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

        public void SwitchLevel(LevelId level)
        {
            _currentLevel = level;
            if (_initialPiecesTemplate != null)
            {
                Destroy(_initialPiecesTemplate);
                _initialPiecesTemplate = null;
            }
#if UNITY_EDITOR
            BakeEditModeLevel();
#else
            InitializeLevel(true);
#endif
        }
    }
}



