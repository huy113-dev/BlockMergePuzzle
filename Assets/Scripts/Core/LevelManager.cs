using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [Header("Tuỳ Chỉnh Giá Trị Băng (Frozen Block)")]
        [Tooltip("Giá trị số trên lớp băng của khối bị đóng băng (VD: Level 7 mặc định = 2; chỉnh = 0 để tắt băng).")]
        [SerializeField] private int _frozenBlockIceCount = 2;
        public int FrozenBlockIceCount
        {
            get => _frozenBlockIceCount;
            set
            {
                _frozenBlockIceCount = Mathf.Max(0, value);
                ApplyFrozenBlockIceCountToScene();
            }
        }

        private void OnValidate()
        {
            _frozenBlockIceCount = Mathf.Max(0, _frozenBlockIceCount);
            if (_currentLevel == LevelId.Level7)
            {
                ApplyFrozenBlockIceCountToScene();
            }
        }

        public void SyncInspectorIceCountFromPiece(int newIceCount)
        {
            _frozenBlockIceCount = Mathf.Max(0, newIceCount);
        }

        private void ApplyFrozenBlockIceCountToScene()
        {
            if (_piecesRoot == null) _piecesRoot = transform.Find("Active_Pieces");
            if (_piecesRoot == null) return;

            foreach (var piece in _piecesRoot.GetComponentsInChildren<PuzzlePiece>(true))
            {
                if (piece == null) continue;
                if (piece.IsFrozen || (_currentLevel == LevelId.Level7 && piece.Shape == PieceShape.Square_2x2))
                {
                    piece.SetIceCount(_frozenBlockIceCount);
                }
            }
        }

        [Header("Custom Level (JSON)")]
        [Tooltip("Dữ liệu level dạng JSON (Resources/Levels). Tự động load khi mở scene hoặc Play Test.")]
        [SerializeField] private TextAsset _customLevelJson;

        public CustomLevelData ActiveCustomLevelData { get; private set; }
        public bool HasActiveCustomLevel => ActiveCustomLevelData != null;
        public int CurrentLevelNumber => ActiveCustomLevelData != null ? ActiveCustomLevelData.levelNumber : (int)_currentLevel;

        public int CurrentColumns => ActiveCustomLevelData != null ? ActiveCustomLevelData.columns : GetLevelColumns(_currentLevel);
        public int CurrentRows => ActiveCustomLevelData != null ? ActiveCustomLevelData.rows : GetLevelRows(_currentLevel);

        public float CurrentCellWidth => ActiveCustomLevelData != null ? _customCellSize : GetLevelCellWidth(_currentLevel);
        public float CurrentCellHeight => ActiveCustomLevelData != null ? _customCellSize : GetLevelCellHeight(_currentLevel);

        public float CurrentFrameWidth => ActiveCustomLevelData != null ? _customFrameWidth : GetLevelFrameWidth(_currentLevel);
        public float CurrentFrameHeight => ActiveCustomLevelData != null ? _customFrameHeight : GetLevelFrameHeight(_currentLevel);

        private float _customCellSize = 1.0f;
        private int _customMinCol, _customMaxCol, _customMinRow, _customMaxRow;
        private float _customFrameWidth = 8.0f;
        private float _customFrameHeight = 8.0f;
        private Sprite _customPrebakedMapSprite;
        private bool _forceDynamicCustomRebuild = false;

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
        public int RemainingBlocksCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _activePieces.Count; i++)
                {
                    if (_activePieces[i] != null && !_activePieces[i].IsStone) count++;
                }
                return count;
            }
        }
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
            ResolveInitialCustomLevel();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            bool force = _forceDynamicCustomRebuild;
            _forceDynamicCustomRebuild = false;
            InitializeLevel(force);
        }

        private void ResolveInitialCustomLevel()
        {
            if (CustomLevelRepository.TryGetEditorPlayTestData(out CustomLevelData playTestData))
            {
                SetActiveCustomData(playTestData);
                _forceDynamicCustomRebuild = true;
                return;
            }

            if (CustomLevelRepository.PendingRuntimeLevelNumber > 0)
            {
                int pending = CustomLevelRepository.PendingRuntimeLevelNumber;
                CustomLevelRepository.PendingRuntimeLevelNumber = 0;
                if (CustomLevelRepository.TryGetLevel(pending, out CustomLevelData pendingData))
                {
                    SetActiveCustomData(pendingData);
                    _forceDynamicCustomRebuild = true;
                    return;
                }
            }

            if (_customLevelJson != null)
            {
                CustomLevelData bakedData = CustomLevelData.FromJson(_customLevelJson.text);
                if (bakedData != null)
                {
                    SetActiveCustomData(bakedData);
                    return;
                }
            }

            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                int prefLevel = PlayerPrefs.GetInt("BMP_CurrentLevel", 1);
                if (prefLevel > CustomLevelRepository.BuiltInLevelCount &&
                    CustomLevelRepository.TryGetLevel(prefLevel, out CustomLevelData prefData))
                {
                    SetActiveCustomData(prefData);
                    _forceDynamicCustomRebuild = true;
                }
            }
        }

        private void SetActiveCustomData(CustomLevelData data, Sprite prebakedMapSprite = null)
        {
            ActiveCustomLevelData = data;
            _customPrebakedMapSprite = prebakedMapSprite;
            if (data != null)
            {
                _currentLevel = (LevelId)data.levelNumber;
                ConfigureCustomLayout(data);
            }
        }

        private void ConfigureCustomLayout(CustomLevelData data)
        {
            if (data == null) return;
            data.Normalize();
            data.GetActiveBounds(out _customMinCol, out _customMaxCol, out _customMinRow, out _customMaxRow);
            _customCellSize = data.ComputeWorldCellSize();
            int activeCols = Mathf.Max(1, _customMaxCol - _customMinCol + 1);
            int activeRows = Mathf.Max(1, _customMaxRow - _customMinRow + 1);
            const float wallMargin = 0.50f;
            _customFrameWidth = (activeCols + wallMargin * 2f) * _customCellSize;
            _customFrameHeight = (activeRows + wallMargin * 2f) * _customCellSize;
        }

        public void LoadCustomLevel(CustomLevelData data, bool forceRebuild = true)
        {
            if (_initialPiecesTemplate != null)
            {
                Destroy(_initialPiecesTemplate);
                _initialPiecesTemplate = null;
            }
            SetActiveCustomData(data != null ? data.Clone() : null);
            InitializeLevel(forceRebuild);
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
            if (ActiveCustomLevelData != null ||
                _currentLevel == LevelId.Level1 || _currentLevel == LevelId.Level2 || _currentLevel == LevelId.Level3 ||
                _currentLevel == LevelId.Level4 || _currentLevel == LevelId.Level5 || _currentLevel == LevelId.Level6 ||
                _currentLevel == LevelId.Level7 || _currentLevel == LevelId.Level8 || _currentLevel == LevelId.Level9 ||
                _currentLevel == LevelId.Level10)
            {
                SpawnPieces();
            }
            FitCameraToBoard();
            FitBackgroundToCamera();
            ApplyLevel25DTilt();
        }

        public void BakeCustomLevelInEditMode(CustomLevelData data, Sprite prebakedMapSprite = null, TextAsset jsonAsset = null)
        {
            _customLevelJson = jsonAsset;
            SetActiveCustomData(data != null ? data.Clone() : null, prebakedMapSprite);
            BakeEditModeLevel();
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

            if (forceRebuild && _snapshotLevel != _currentLevel && _initialPiecesTemplate != null)
            {
                Destroy(_initialPiecesTemplate);
                _initialPiecesTemplate = null;
            }

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
            ApplyLevel25DTilt();
            OnLevelInitialized?.Invoke();
        }

        private void ApplyLevel25DTilt()
        {
            if (_frameRoot == null) _frameRoot = transform.Find("Board_Frame");
            if (_gatesRoot == null) _gatesRoot = transform.Find("Exit_Gates");
            if (_piecesRoot == null) _piecesRoot = transform.Find("Active_Pieces");

            if (_frameRoot != null)
            {
                _frameRoot.localPosition = Vector3.zero;
                _frameRoot.localRotation = Quaternion.identity;
                _frameRoot.localScale = Vector3.one;
            }
            if (_gatesRoot != null)
            {
                _gatesRoot.localPosition = Vector3.zero;
                _gatesRoot.localRotation = Quaternion.identity;
                _gatesRoot.localScale = Vector3.one;
            }
            if (_piecesRoot != null)
            {
                _piecesRoot.localPosition = Vector3.zero;
                _piecesRoot.localRotation = Quaternion.identity;
                _piecesRoot.localScale = Vector3.one;
            }
        }

        private void FitCameraToBoard()
        {
            var cam = Camera.main;
            if (cam == null) return;
            float screenRatio = cam.aspect > 0.05f ? cam.aspect : (1440f / 3200f);
            if (screenRatio > 0.75f) screenRatio = 1440f / 3200f; // Force target portrait ratio (0.45) in Edit Mode

            if (ActiveCustomLevelData != null)
            {
                cam.orthographicSize = ActiveCustomLevelData.ComputeCameraOrthoSize(screenRatio);
                cam.transform.position = new Vector3(-ActiveCustomLevelData.boardOffsetX, -ActiveCustomLevelData.boardOffsetY, -10f);
                return;
            }

            float paddingX = 0.12f;
            float paddingY = 0.40f;
            bool useFixedRef = _currentLevel == LevelId.Level8 || _currentLevel == LevelId.Level9 || _currentLevel == LevelId.Level10;
            float refW = useFixedRef ? 8.000f : CurrentFrameWidth;
            float refH = useFixedRef ? 8.000f : CurrentFrameHeight;
            float sizeByWidth  = (refW / 2f + paddingX) / screenRatio;
            float sizeByHeight =  refH / 2f + paddingY;
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

            if (ActiveCustomLevelData != null)
            {
                Sprite customSprite = _customPrebakedMapSprite != null
                    ? _customPrebakedMapSprite
                    : ProceduralBoardRenderer.GenerateBoardSprite(ActiveCustomLevelData, out _, out _, 128);
                sr.sprite = customSprite;
                frameObj.transform.position = _boardCenter;
                if (sr.sprite != null)
                {
                    float nativeW = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                    float nativeH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                    frameObj.transform.localScale = new Vector3(
                        CurrentFrameWidth / nativeW,
                        CurrentFrameHeight / nativeH,
                        1f);
                }
                return;
            }

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
            if (ActiveCustomLevelData != null)
            {
                BuildCustomExitGates(ActiveCustomLevelData);
                return;
            }
            BuildLevelGates(_currentLevel);
        }

        private void BuildCustomExitGates(CustomLevelData data)
        {
            if (data == null || data.gates == null) return;
            float cs = _customCellSize;
            for (int i = 0; i < data.gates.Count; i++)
            {
                CustomGateData g = data.gates[i];
                if (g == null) continue;
                List<Vector2Int> gateCoords = g.GetGateCoords();
                List<Vector2Int> triggerCoords = g.GetTriggerCoords();
                if (gateCoords.Count == 0) continue;

                Vector3 sum = Vector3.zero;
                for (int k = 0; k < gateCoords.Count; k++)
                {
                    sum += GridToWorld(gateCoords[k].x, gateCoords[k].y);
                }
                Vector3 worldPos = sum / gateCoords.Count;

                GateSide side = g.GetGateSide();
                bool horizontal = (side == GateSide.Top || side == GateSide.Bottom);
                Vector2 gateSize = horizontal
                    ? new Vector2(cs * (gateCoords.Count - 0.12f), cs * 0.88f)
                    : new Vector2(cs * 0.88f, cs * (gateCoords.Count - 0.12f));

                ExitGateView gateView = ExitGateView.Create(
                    g.GetPieceColor(),
                    g.requiredValue,
                    side,
                    g.GetExitDirection(),
                    worldPos,
                    gateSize,
                    _gatesRoot,
                    triggerCoords.ToArray(),
                    g.GetTriggerDirection(),
                    true,
                    gateCoords.ToArray());
                gateView.ApplyResponsiveVisualScale(cs, gateSize);
                _activeGates.Add(gateView);
            }
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
                        GridToWorld(2, 0),
                        new Vector2(1.15f, 0.90f), _gatesRoot,
                        new[] { new Vector2Int(2, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(2, 0) }));
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
                        new Vector2(0.74f, 1.04f), _gatesRoot,
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
                        new Vector2(0.66f, 1.04f), _gatesRoot,
                        new[] { new Vector2Int(-1, 0) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 0) }));

                    // Right Red Gate (col 4, row 3 -> exit at (5, 3)) with High-Tech Holographic Glowing Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 4, GateSide.Right, Vector2.right,
                        GridToWorld(4, 3),
                        new Vector2(0.74f, 0.98f), _gatesRoot,
                        new[] { new Vector2Int(5, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 3) }));
                    break;

                case LevelId.Level4:
                    // Top-Right Red Gate (col 3, row 0 -> exit at (4, 0)) with High-Tech Holographic Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 3, GateSide.Right, Vector2.right,
                        GridToWorld(3, 0),
                        new Vector2(1.20f, 1.25f), _gatesRoot,
                        new[] { new Vector2Int(4, 0) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 0) }));

                    // Middle-Right Green Gate (col 3, row 2 -> exit at (4, 2)) with High-Tech Holographic Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 3, GateSide.Right, Vector2.right,
                        GridToWorld(3, 2),
                        new Vector2(1.20f, 1.25f), _gatesRoot,
                        new[] { new Vector2Int(4, 2) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 2) }));

                    // Bottom-Right Pink Gate (col 3, row 4 -> exit at (4, 4)) with High-Tech Holographic Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Right, Vector2.right,
                        GridToWorld(3, 4),
                        new Vector2(1.20f, 1.25f), _gatesRoot,
                        new[] { new Vector2Int(4, 4) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 4) }));
                    break;

                case LevelId.Level5:
                    // Top Yellow Gate (cols 3..4, row 0 -> exit at (3, -1), (4, -1)) with High-Tech Holographic Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Yellow, 5, GateSide.Top, Vector2.up,
                        (GridToWorld(3, 0) + GridToWorld(4, 0)) * 0.5f,
                        new Vector2(1.50f, 0.65f), _gatesRoot,
                        new[] { new Vector2Int(3, -1), new Vector2Int(4, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 0), new Vector2Int(4, 0) }));

                    // Bottom Green Gate (cols 3..4, row 10 -> exit at (3, 11), (4, 11)) with High-Tech Holographic Display
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Bottom, Vector2.down,
                        (GridToWorld(3, 10) + GridToWorld(4, 10)) * 0.5f,
                        new Vector2(1.50f, 0.65f), _gatesRoot,
                        new[] { new Vector2Int(3, 11), new Vector2Int(4, 11) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 10), new Vector2Int(4, 10) }));
                    break;

                case LevelId.Level6:
                    // Top Red Gate (cols 2..4, row 0 -> exit at (2,-1), (3,-1), (4,-1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Top, Vector2.up,
                        GridToWorld(3, 0),
                        new Vector2(2.85f, 0.65f), _gatesRoot,
                        new[] { new Vector2Int(2, -1), new Vector2Int(3, -1), new Vector2Int(4, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0) }));

                    // Right Green Gate (col 6, rows 1..2 -> exit at (7,1), (7,2))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Right, Vector2.right,
                        (GridToWorld(6, 1) + GridToWorld(6, 2)) * 0.5f,
                        new Vector2(0.65f, 1.85f), _gatesRoot,
                        new[] { new Vector2Int(7, 1), new Vector2Int(7, 2) },
                        default,
                        true,
                        new[] { new Vector2Int(6, 1), new Vector2Int(6, 2) }));

                    // Left Orange Gate (col 0, row 3 -> exit at (-1,3))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 3),
                        new Vector2(0.65f, 0.92f), _gatesRoot,
                        new[] { new Vector2Int(-1, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 3) }));

                    // Left Pink Gate (col 0, rows 5..6 -> exit at (-1,5), (-1,6))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 3, GateSide.Left, Vector2.left,
                        (GridToWorld(0, 5) + GridToWorld(0, 6)) * 0.5f,
                        new Vector2(0.65f, 1.85f), _gatesRoot,
                        new[] { new Vector2Int(-1, 5), new Vector2Int(-1, 6) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 5), new Vector2Int(0, 6) }));
                    break;

                case LevelId.Level7:
                    // Top Green Gate (cols 3..4, row 0 -> exit at (3,-1), (4,-1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Top, Vector2.up,
                        (GridToWorld(3, 0) + GridToWorld(4, 0)) * 0.5f,
                        new Vector2(1.78f, 0.63f), _gatesRoot,
                        new[] { new Vector2Int(3, -1), new Vector2Int(4, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 0), new Vector2Int(4, 0) }));

                    // Left Yellow Gate (col 0, row 3 -> exit at (-1,3))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Yellow, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 3),
                        new Vector2(0.62f, 0.96f), _gatesRoot,
                        new[] { new Vector2Int(-1, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 3) }));

                    // Right Pink Gate (col 7, row 3 -> exit at (8,3))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Pink, 2, GateSide.Right, Vector2.right,
                        GridToWorld(7, 3),
                        new Vector2(0.62f, 0.96f), _gatesRoot,
                        new[] { new Vector2Int(8, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(7, 3) }));
                    break;

                case LevelId.Level8:
                    // Top Green Gate (cols 4..5, row 0 -> exit at (4,-1), (5,-1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 2, GateSide.Top, Vector2.up,
                        (GridToWorld(4, 0) + GridToWorld(5, 0)) * 0.5f,
                        new Vector2(1.71f, 0.68f), _gatesRoot,
                        new[] { new Vector2Int(4, -1), new Vector2Int(5, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 0), new Vector2Int(5, 0) }));

                    // Right Orange Gate (col 7, row 1 -> exit at (8,1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Right, Vector2.right,
                        GridToWorld(7, 1),
                        new Vector2(0.64f, 0.91f), _gatesRoot,
                        new[] { new Vector2Int(8, 1) },
                        default,
                        true,
                        new[] { new Vector2Int(7, 1) }));

                    // Left Purple Gate (col 0, rows 3..4 -> exit at (-1,3), (-1,4))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Purple, 2, GateSide.Left, Vector2.left,
                        (GridToWorld(0, 3) + GridToWorld(0, 4)) * 0.5f,
                        new Vector2(0.64f, 1.75f), _gatesRoot,
                        new[] { new Vector2Int(-1, 3), new Vector2Int(-1, 4) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 3), new Vector2Int(0, 4) }));

                    // Right Blue Gate (col 7, rows 3..4 -> exit at (8,3), (8,4))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Blue, 2, GateSide.Right, Vector2.right,
                        (GridToWorld(7, 3) + GridToWorld(7, 4)) * 0.5f,
                        new Vector2(0.64f, 1.75f), _gatesRoot,
                        new[] { new Vector2Int(8, 3), new Vector2Int(8, 4) },
                        default,
                        true,
                        new[] { new Vector2Int(7, 3), new Vector2Int(7, 4) }));

                    // Left Yellow Gate (col 0, row 6 -> exit at (-1,6))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Yellow, 2, GateSide.Left, Vector2.left,
                        GridToWorld(0, 6),
                        new Vector2(0.64f, 0.91f), _gatesRoot,
                        new[] { new Vector2Int(-1, 6) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 6) }));
                    break;

                case LevelId.Level9:
                    // Top-Left Red Gate (col 0, row 0 -> exit at (0, -1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Red, 2, GateSide.Top, Vector2.up,
                        GridToWorld(0, 0),
                        new Vector2(1.07f, 0.64f), _gatesRoot,
                        new[] { new Vector2Int(0, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 0) }));

                    // Top-Right Purple Gate (col 3, row 0 -> exit at (3, -1))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Purple, 3, GateSide.Top, Vector2.up,
                        GridToWorld(3, 0),
                        new Vector2(1.08f, 0.65f), _gatesRoot,
                        new[] { new Vector2Int(3, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(3, 0) }));

                    // Right-Upper Yellow Gate (col 4, row 3 -> exit at (5, 3))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Yellow, 3, GateSide.Right, Vector2.right,
                        GridToWorld(4, 3),
                        new Vector2(0.64f, 1.05f), _gatesRoot,
                        new[] { new Vector2Int(5, 3) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 3) }));

                    // Right-Lower Blue Gate (col 4, row 4 -> exit at (5, 4))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Blue, 2, GateSide.Right, Vector2.right,
                        GridToWorld(4, 4),
                        new Vector2(0.64f, 1.05f), _gatesRoot,
                        new[] { new Vector2Int(5, 4) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 4) }));

                    // Bottom-Center Green Gate (cols 1..2, row 7 -> exit at (1, 8), (2, 8))
                    _activeGates.Add(ExitGateView.Create(
                        PieceColor.Green, 3, GateSide.Bottom, Vector2.down,
                        (GridToWorld(1, 7) + GridToWorld(2, 7)) * 0.5f,
                        new Vector2(2.20f, 0.65f), _gatesRoot,
                        new[] { new Vector2Int(1, 8), new Vector2Int(2, 8) },
                        default,
                        true,
                        new[] { new Vector2Int(1, 7), new Vector2Int(2, 7) }));
                    break;

                case LevelId.Level10:
                    // Christmas-tree board (map_lv10). Grid: cols 0..8 (0 & 8 = side gate pads), rows 0..10 (0 & 10 = top/bottom gate pads)
                    // Top Red Gate (col 4, row 0 -> exit at (4, -1)): Needs 4
                    ExitGateView topRedGate = ExitGateView.Create(
                        PieceColor.Red, 4, GateSide.Top, Vector2.up,
                        GridToWorld(4, 0),
                        new Vector2(0.77f, 0.38f), _gatesRoot,
                        new[] { new Vector2Int(4, -1) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 0) });
                    ApplyLevel10GateVisualScale(topRedGate, 0.52f, new Vector2(0.74f, 0.36f), 0.038f);
                    _activeGates.Add(topRedGate);

                    // Bottom Orange Gate (col 4, row 10 -> exit at (4, 11)): Needs 2
                    ExitGateView bottomOrangeGate = ExitGateView.Create(
                        PieceColor.Orange, 2, GateSide.Bottom, Vector2.down,
                        GridToWorld(4, 10),
                        new Vector2(0.75f, 0.40f), _gatesRoot,
                        new[] { new Vector2Int(4, 11) },
                        default,
                        true,
                        new[] { new Vector2Int(4, 10) });
                    ApplyLevel10GateVisualScale(bottomOrangeGate, 0.52f, new Vector2(0.72f, 0.38f), 0.038f);
                    _activeGates.Add(bottomOrangeGate);

                    // Left Green Gate (col 0, rows 6..8 -> exit at (-1, 6..8)): Needs 5
                    ExitGateView leftGreenGate = ExitGateView.Create(
                        PieceColor.Green, 5, GateSide.Left, Vector2.left,
                        GridToWorld(0, 7),
                        new Vector2(0.42f, 2.18f), _gatesRoot,
                        new[] { new Vector2Int(-1, 6), new Vector2Int(-1, 7), new Vector2Int(-1, 8) },
                        default,
                        true,
                        new[] { new Vector2Int(0, 6), new Vector2Int(0, 7), new Vector2Int(0, 8) });
                    ApplyLevel10GateVisualScale(leftGreenGate, 0.55f, new Vector2(0.40f, 1.78f), 0.038f);
                    _activeGates.Add(leftGreenGate);

                    // Right Green Gate (col 8, rows 6..8 -> exit at (9, 6..8)): Needs 4
                    ExitGateView rightGreenGate = ExitGateView.Create(
                        PieceColor.Green, 4, GateSide.Right, Vector2.right,
                        GridToWorld(8, 7),
                        new Vector2(0.40f, 2.18f), _gatesRoot,
                        new[] { new Vector2Int(9, 6), new Vector2Int(9, 7), new Vector2Int(9, 8) },
                        default,
                        true,
                        new[] { new Vector2Int(8, 6), new Vector2Int(8, 7), new Vector2Int(8, 8) });
                    ApplyLevel10GateVisualScale(rightGreenGate, 0.55f, new Vector2(0.38f, 1.78f), 0.038f);
                    _activeGates.Add(rightGreenGate);
                    break;
            }
        }

        private static void ApplyLevel10GateVisualScale(ExitGateView gate, float rootScale, Vector2 padWorldSize, float digitCharSize)
        {
            if (gate == null) return;
            Transform labelRoot = gate.transform.Find("LabelRoot");
            if (labelRoot == null) return;

            labelRoot.localScale = new Vector3(rootScale, rootScale, 1f);

            Transform pad = labelRoot.Find("GateHoloPad");
            if (pad != null)
            {
                pad.localScale = new Vector3(
                    padWorldSize.x / (2.40f * rootScale),
                    padWorldSize.y / (2.40f * rootScale),
                    1f);
            }

            Transform outerRing = labelRoot.Find("GateTechRing");
            if (outerRing != null)
            {
                outerRing.localScale = new Vector3(0.345f, 0.345f, 1f);
            }

            Transform innerRing = labelRoot.Find("GateInnerRing");
            if (innerRing != null)
            {
                // Matches ExitGateView.Update() pulse baseline (0.340f) so scale stays consistent in Play Mode
                innerRing.localScale = new Vector3(0.340f, 0.340f, 1f);
            }

            foreach (var tm in labelRoot.GetComponentsInChildren<TextMesh>(true))
            {
                if (tm != null) tm.characterSize = digitCharSize;
            }
        }

        private void SpawnPieces()
        {
            if (ActiveCustomLevelData != null)
            {
                SpawnCustomPieces(ActiveCustomLevelData);
                return;
            }
            SpawnLevelPieces(_currentLevel);
        }

        private void SpawnCustomPieces(CustomLevelData data)
        {
            if (data == null || data.pieces == null) return;
            Vector2 cellSizeVec = new Vector2(_customCellSize * 0.97f, _customCellSize * 0.97f);
            for (int i = 0; i < data.pieces.Count; i++)
            {
                CustomPieceData p = data.pieces[i];
                if (p == null) continue;
                PieceColor pColor = p.GetPieceColor();
                PieceShape pShape = p.GetPieceShape();
                Vector2Int origin = p.origin.ToVector2Int();
                Vector3 worldPos = CalculatePieceWorldCenter(pShape, origin);

                PuzzlePiece piece = PuzzlePiece.Create(pColor, pShape, p.value, origin, worldPos, _piecesRoot, cellSizeVec);
                if (p.isStone)
                {
                    piece.SetStone(true);
                }
                else
                {
                    if (p.iceCount > 0) piece.SetIceCount(p.iceCount);
                    MoveAxisConstraint axis = p.GetMoveAxis();
                    if (axis != MoveAxisConstraint.None) piece.SetMoveAxis(axis);
                }

                foreach (var cell in piece.OccupiedCells)
                {
                    if (IsValidCoord(cell.x, cell.y))
                    {
                        _grid[cell.x, cell.y] = piece;
                    }
                }
                _activePieces.Add(piece);
            }
        }

        private void SpawnLevelPieces(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1:
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 2));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(4, 2));
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
                    // 3 Red pieces: Red [2] at (0,0), Red [1] at (0,4), Red [1] at (2,4) -> merges to Red 3
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(0, 0));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(0, 4));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));

                    // 3 Green pieces: Green [2] at (1,0), Green [1] at (2,0), Green [1] at (1,4) -> merges to Green 3
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(1, 0));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 0));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 4));

                    // 1 Pink piece: Pink [2] at (2,2) -> exits Pink 2 gate
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 2, new Vector2Int(2, 2));
                    break;

                case LevelId.Level5:
                    // Row 1 (cols 3..4): Yellow 2x1 [3]
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, new Vector2Int(3, 1));

                    // Row 2 (cols 2..5): Yellow 2x1 [2] at (2,2), Yellow 2x1 [3] at (4,2)
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 2, new Vector2Int(2, 2));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, new Vector2Int(4, 2));

                    // Rows 3..4 (cols 2..5): Yellow 1x2 [3] at (2,3), Yellow 2x1 [3] at (3,3), Yellow 2x1 [1] at (3,4), Yellow 1x2 [3] at (5,3)
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, new Vector2Int(2, 3));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, new Vector2Int(3, 3));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(3, 4));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, new Vector2Int(5, 3));

                    // Rows 5..6 (cols 0..7 / 1..6): Green L [1] at (0,5), Yellow 1x2 [1, 2, 3, 1] at cols 2..5, Green L [1] at (6,5)
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, new Vector2Int(0, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 1, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 2, new Vector2Int(3, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, new Vector2Int(4, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 1, new Vector2Int(5, 5));
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopLeft, 1, new Vector2Int(6, 5));

                    // Rows 7..8 (cols 1..6 / 2..5): Green L [1] at (1,7), Yellow 2x1 [1] at (3,7), Green L [1] at (5,7)
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, new Vector2Int(1, 7));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(3, 7));
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_TopLeft, 1, new Vector2Int(5, 7));

                    // Rows 8..9 (cols 3..4): Green 2x2 [2] at (3,8)
                    SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 2, new Vector2Int(3, 8));
                    break;

                case LevelId.Level6:
                    // Top row (row 1): Green 1x1 [1] at (1,1), Pink 1x2 [2] at (2,1), Orange 1x1 [1] at (3,1), Green 1x2 [1] at (4,1), Green 1x1 [1] at (5,1)
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 2, new Vector2Int(2, 1));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(3, 1));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, new Vector2Int(4, 1));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(5, 1));

                    // Center Red Cross 3x3 [2] at origin (2,2) -> occupies (3,2), (2,3), (3,3), (4,3), (3,4)
                    SpawnPiece(PieceColor.Red, PieceShape.Cross_3x3, 2, new Vector2Int(2, 2));

                    // Lower section: Orange 1x1 [1] at (1,5), Pink 1x2 [1] at (1,6), Green 1x2 [1] at (3,6), Pink 1x2 [1] at (5,6)
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 5));
                    SpawnPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 1, new Vector2Int(1, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, new Vector2Int(3, 6));
                    SpawnPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 1, new Vector2Int(5, 6));
                    break;

                case LevelId.Level7:
                    // Top-center Green 2x2 [2] at (3,1) encased in Ice [_frozenBlockIceCount]
                    PuzzlePiece frozenGreen2x2 = SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 2, new Vector2Int(3, 1));
                    frozenGreen2x2.SetIceCount(Mathf.Max(0, _frozenBlockIceCount));

                    // Middle row (row 2): Pink 1x1 [1] at (2,2) and Pink 1x1 [1] at (5,2)
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(2, 2));
                    SpawnPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, new Vector2Int(5, 2));

                    // Bottom row (row 3): Yellow 2x1 [1] at (1,3) and Yellow 2x1 [1] at (5,3)
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(1, 3));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, new Vector2Int(5, 3));
                    break;

                case LevelId.Level8:
                    // Top-left 1x1 cluster (cols 1..2, rows 0..1)
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(1, 0));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 0));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, new Vector2Int(2, 1));

                    // Upper-left Green 2x2 [1] at (1, 2) (cols 1..2, rows 2..3)
                    SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 1, new Vector2Int(1, 2));

                    // Upper-middle vertical 1x2 blocks (cols 3..4, rows 1..2)
                    SpawnPiece(PieceColor.Blue, PieceShape.Vertical_1x2, 1, new Vector2Int(3, 1));
                    SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(4, 1));

                    // Lower-middle vertical 1x2 blocks (cols 3..4, rows 5..6)
                    SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(3, 5));
                    SpawnPiece(PieceColor.Blue, PieceShape.Vertical_1x2, 1, new Vector2Int(4, 5));

                    // Middle-right Green 2x2 [1] at (5, 4) (cols 5..6, rows 4..5)
                    SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 1, new Vector2Int(5, 4));

                    // Lower-right 1x1 cluster (cols 5..6, rows 6..7)
                    SpawnPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, new Vector2Int(5, 6));
                    SpawnPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, new Vector2Int(6, 6));
                    SpawnPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, new Vector2Int(5, 7));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, new Vector2Int(6, 7));

                    // Bottom-right Purple 2x1 [2] at (5, 8) (cols 5..6, row 8)
                    SpawnPiece(PieceColor.Purple, PieceShape.Horizontal_2x1, 2, new Vector2Int(5, 8));
                    break;

                case LevelId.Level9:
                    // Upper section (rows 1..2): Red 1x2 [1] at (0,1), Green 2x2 [2] at (1,1), Red 1x2 [1] at (3,1)
                    SpawnPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, new Vector2Int(0, 1));
                    SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 2, new Vector2Int(1, 1));
                    SpawnPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, new Vector2Int(3, 1));

                    // Middle section (rows 3..4): Purple 1x2 [2] at (0,3), Green 2x2 [2] at (1,3), Purple 1x2 [1] at (3,3)
                    SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 2, new Vector2Int(0, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Square_2x2, 2, new Vector2Int(1, 3));
                    SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(3, 3));

                    // Lower section (rows 5..6): Purple 1x2 [1] at (0,5)
                    SpawnPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, new Vector2Int(0, 5));

                    // Row 5 (cols 1..3): Yellow 1x1 [1] at (1,5), Blue 1x1 [1] at (2,5), Yellow 1x1 [2] at (3,5)
                    SpawnPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, new Vector2Int(1, 5));
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Single_1x1, 2, new Vector2Int(3, 5));

                    // Row 6 (cols 1..2): Blue 1x1 [1] at (1,6), Yellow 1x1 [1] at (2,6) ((3,6) is empty)
                    SpawnPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, new Vector2Int(1, 6));
                    SpawnPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, new Vector2Int(2, 6));
                    break;

                case LevelId.Level10:
                    // Row 1 (tree top, col 4)
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(4, 1));

                    // Row 2 (cols 3..5)
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(3, 2));
                    SpawnPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 1, new Vector2Int(4, 2));

                    // Row 3 (cols 3..5)
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(3, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, new Vector2Int(4, 3));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(5, 3));

                    // Row 4 (cols 2..6)
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(2, 4));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 3, new Vector2Int(3, 4));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 3, new Vector2Int(5, 4));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 2, new Vector2Int(6, 4));

                    // Row 5 (cols 2..6)
                    SpawnPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 2, new Vector2Int(2, 5));
                    SpawnPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 3, new Vector2Int(4, 5));

                    // Rows 6..7 (cols 1..7)
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 3, new Vector2Int(1, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_BottomLeft, 3, new Vector2Int(2, 6));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(3, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 4, new Vector2Int(4, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.ShapeL_BottomLeft, 3, new Vector2Int(5, 6));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 2, new Vector2Int(6, 6));
                    SpawnPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, new Vector2Int(7, 6));

                    // Row 8 (cols 1..7)
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 1, new Vector2Int(1, 8));
                    SpawnPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 1, new Vector2Int(2, 8));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 3, new Vector2Int(4, 8));
                    SpawnPiece(PieceColor.Red, PieceShape.Single_1x1, 3, new Vector2Int(5, 8));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 2, new Vector2Int(6, 8));
                    SpawnPiece(PieceColor.Green, PieceShape.Single_1x1, 1, new Vector2Int(7, 8));

                    // Row 9 (tree trunk, col 4)
                    SpawnPiece(PieceColor.Orange, PieceShape.Single_1x1, 2, new Vector2Int(4, 9));
                    break;
            }
        }

        private PuzzlePiece SpawnPiece(PieceColor color, PieceShape shape, int value, Vector2Int origin)
        {
            Vector3 worldPos = CalculatePieceWorldCenter(shape, origin);
            Vector2? customCellSize = _currentLevel == LevelId.Level1
                ? new Vector2(1.31f, 1.16f)
                : (_currentLevel == LevelId.Level2
                    ? new Vector2(1.19f, 1.04f)
                    : (_currentLevel == LevelId.Level3
                        ? new Vector2(1.20f, 1.13f)
                        : (_currentLevel == LevelId.Level4
                            ? new Vector2(1.30f, 1.25f)
                            : (_currentLevel == LevelId.Level5
                                ? new Vector2(0.772f, 0.718f)
                                : (_currentLevel == LevelId.Level6
                                    ? new Vector2(0.955f, 0.940f)
                                    : (_currentLevel == LevelId.Level7
                                        ? new Vector2(0.895f, 0.945f)
                                        : (_currentLevel == LevelId.Level8
                                            ? new Vector2(0.872f, 0.872f)
                                            : (_currentLevel == LevelId.Level9
                                                ? new Vector2(1.162f, 1.058f)
                                                : (_currentLevel == LevelId.Level10
                                                    ? new Vector2(0.780f, 0.710f)
                                                    : (Vector2?)null)))))))));
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
                case PieceShape.ShapeL_TopLeft:
                case PieceShape.ShapeL_BottomLeft:
                case PieceShape.ShapeL_BottomRight:
                case PieceShape.Square_2x2:
                    // 2x2 bounding box center
                    return (GridToWorld(origin.x, origin.y) + GridToWorld(origin.x + 1, origin.y + 1)) * 0.5f;

                case PieceShape.Cross_3x3:
                    return GridToWorld(origin.x + 1, origin.y + 1);

                default:
                {
                    Vector2Int b = GameConfig.GetShapeBoundingSize(shape);
                    return (GridToWorld(origin.x, origin.y) + GridToWorld(origin.x + b.x - 1, origin.y + b.y - 1)) * 0.5f;
                }
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
            { new Vector2Int(5, 3), new Vector3(3.481f, -0.059f, 0f) },
            { new Vector2Int(2, 4), new Vector3(0.030f, -1.137f, 0f) },
            { new Vector2Int(2, 5), new Vector3(0.030f, -2.240f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level3CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            { new Vector2Int(0, 0), new Vector3( -2.133f,   3.641f, 0f) },
            { new Vector2Int(1, 0), new Vector3( -1.116f,   3.627f, 0f) },
            { new Vector2Int(2, 0), new Vector3(  0.153f,   3.621f, 0f) },
            { new Vector2Int(2, 1), new Vector3(  0.153f,   2.418f, 0f) },
            { new Vector2Int(2, 2), new Vector3(  0.159f,   1.222f, 0f) },
            { new Vector2Int(2, 3), new Vector3(  0.159f,   0.027f, 0f) },
            { new Vector2Int(3, 3), new Vector3(  1.422f,   0.086f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  2.505f,   0.073f, 0f) },
            { new Vector2Int(2, 4), new Vector3(  0.179f,  -1.169f, 0f) },
            { new Vector2Int(2, 5), new Vector3(  0.186f,  -2.358f, 0f) },
            { new Vector2Int(0, 6), new Vector3( -2.385f,  -3.528f, 0f) },
            { new Vector2Int(1, 6), new Vector3( -1.116f,  -3.528f, 0f) },
            { new Vector2Int(2, 6), new Vector3(  0.153f,  -3.528f, 0f) },
            { new Vector2Int(3, 6), new Vector3(  1.422f,  -3.528f, 0f) },
            { new Vector2Int(4, 6), new Vector3(  2.691f,  -3.528f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level4CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Top corridor) - Y = 3.215f
            { new Vector2Int(0, 0), new Vector3( -2.001f,   3.215f, 0f) },
            { new Vector2Int(1, 0), new Vector3( -0.440f,   3.215f, 0f) },
            { new Vector2Int(2, 0), new Vector3(  1.131f,   3.215f, 0f) },
            { new Vector2Int(3, 0), new Vector3(  2.460f,   3.215f, 0f) },

            // Row 1 (Vertical spine) - Y = 1.770f
            { new Vector2Int(0, 1), new Vector3( -2.010f,   1.770f, 0f) },

            // Row 2 (Middle corridor) - Y = 0.325f
            { new Vector2Int(0, 2), new Vector3( -2.010f,   0.325f, 0f) },
            { new Vector2Int(1, 2), new Vector3( -0.450f,   0.325f, 0f) },
            { new Vector2Int(2, 2), new Vector3(  1.126f,   0.325f, 0f) },
            { new Vector2Int(3, 2), new Vector3(  2.460f,   0.325f, 0f) },

            // Row 3 (Vertical spine) - Y = -1.068f
            { new Vector2Int(0, 3), new Vector3( -2.010f,  -1.068f, 0f) },

            // Row 4 (Bottom corridor) - Y = -2.460f
            { new Vector2Int(0, 4), new Vector3( -2.020f,  -2.460f, 0f) },
            { new Vector2Int(1, 4), new Vector3( -0.460f,  -2.460f, 0f) },
            { new Vector2Int(2, 4), new Vector3(  1.140f,  -2.460f, 0f) },
            { new Vector2Int(3, 4), new Vector3(  2.460f,  -2.460f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level5CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Top Yellow Gate Pad) - Y = 3.856f
            { new Vector2Int(3, 0), new Vector3( -0.400f,   3.856f, 0f) },
            { new Vector2Int(4, 0), new Vector3(  0.394f,   3.856f, 0f) },

            // Row 1 - Y = 3.133f
            { new Vector2Int(3, 1), new Vector3( -0.400f,   3.133f, 0f) },
            { new Vector2Int(4, 1), new Vector3(  0.394f,   3.133f, 0f) },

            // Row 2 - Y = 2.394f
            { new Vector2Int(2, 2), new Vector3( -1.187f,   2.394f, 0f) },
            { new Vector2Int(3, 2), new Vector3( -0.400f,   2.394f, 0f) },
            { new Vector2Int(4, 2), new Vector3(  0.394f,   2.394f, 0f) },
            { new Vector2Int(5, 2), new Vector3(  1.181f,   2.394f, 0f) },

            // Row 3 - Y = 1.658f
            { new Vector2Int(2, 3), new Vector3( -1.187f,   1.658f, 0f) },
            { new Vector2Int(3, 3), new Vector3( -0.400f,   1.658f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  0.394f,   1.658f, 0f) },
            { new Vector2Int(5, 3), new Vector3(  1.181f,   1.658f, 0f) },

            // Row 4 - Y = 0.924f
            { new Vector2Int(2, 4), new Vector3( -1.187f,   0.924f, 0f) },
            { new Vector2Int(3, 4), new Vector3( -0.400f,   0.924f, 0f) },
            { new Vector2Int(4, 4), new Vector3(  0.394f,   0.924f, 0f) },
            { new Vector2Int(5, 4), new Vector3(  1.181f,   0.924f, 0f) },

            // Row 5 (Widest row, cols 0..7) - Y = 0.189f
            { new Vector2Int(0, 5), new Vector3( -2.766f,   0.189f, 0f) },
            { new Vector2Int(1, 5), new Vector3( -1.974f,   0.189f, 0f) },
            { new Vector2Int(2, 5), new Vector3( -1.187f,   0.189f, 0f) },
            { new Vector2Int(3, 5), new Vector3( -0.400f,   0.189f, 0f) },
            { new Vector2Int(4, 5), new Vector3(  0.394f,   0.189f, 0f) },
            { new Vector2Int(5, 5), new Vector3(  1.181f,   0.189f, 0f) },
            { new Vector2Int(6, 5), new Vector3(  1.970f,   0.189f, 0f) },
            { new Vector2Int(7, 5), new Vector3(  2.755f,   0.189f, 0f) },

            // Row 6 - Y = -0.544f
            { new Vector2Int(1, 6), new Vector3( -1.974f,  -0.544f, 0f) },
            { new Vector2Int(2, 6), new Vector3( -1.187f,  -0.544f, 0f) },
            { new Vector2Int(3, 6), new Vector3( -0.400f,  -0.544f, 0f) },
            { new Vector2Int(4, 6), new Vector3(  0.394f,  -0.544f, 0f) },
            { new Vector2Int(5, 6), new Vector3(  1.181f,  -0.544f, 0f) },
            { new Vector2Int(6, 6), new Vector3(  1.970f,  -0.544f, 0f) },

            // Row 7 - Y = -1.277f
            { new Vector2Int(1, 7), new Vector3( -1.974f,  -1.277f, 0f) },
            { new Vector2Int(2, 7), new Vector3( -1.187f,  -1.277f, 0f) },
            { new Vector2Int(3, 7), new Vector3( -0.400f,  -1.277f, 0f) },
            { new Vector2Int(4, 7), new Vector3(  0.394f,  -1.277f, 0f) },
            { new Vector2Int(5, 7), new Vector3(  1.181f,  -1.277f, 0f) },
            { new Vector2Int(6, 7), new Vector3(  1.970f,  -1.277f, 0f) },

            // Row 8 - Y = -2.009f
            { new Vector2Int(2, 8), new Vector3( -1.187f,  -2.009f, 0f) },
            { new Vector2Int(3, 8), new Vector3( -0.400f,  -2.009f, 0f) },
            { new Vector2Int(4, 8), new Vector3(  0.394f,  -2.009f, 0f) },
            { new Vector2Int(5, 8), new Vector3(  1.181f,  -2.009f, 0f) },

            // Row 9 - Y = -2.742f
            { new Vector2Int(3, 9), new Vector3( -0.400f,  -2.742f, 0f) },
            { new Vector2Int(4, 9), new Vector3(  0.394f,  -2.742f, 0f) },

            // Row 10 (Bottom Green Gate Pad) - Y = -3.437f
            { new Vector2Int(3, 10), new Vector3( -0.400f,  -3.437f, 0f) },
            { new Vector2Int(4, 10), new Vector3(  0.394f,  -3.437f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level6CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Top Red Gate Pad, cols 2..4) - Y = 3.253f
            { new Vector2Int(2, 0), new Vector3( -0.882f,   3.253f, 0f) },
            { new Vector2Int(3, 0), new Vector3(  0.121f,   3.253f, 0f) },
            { new Vector2Int(4, 0), new Vector3(  1.125f,   3.253f, 0f) },

            // Row 1 (cols 1..5 + Right Green Gate top at col 6) - Y = 2.395f
            { new Vector2Int(1, 1), new Vector3( -1.893f,   2.395f, 0f) },
            { new Vector2Int(2, 1), new Vector3( -0.882f,   2.395f, 0f) },
            { new Vector2Int(3, 1), new Vector3(  0.121f,   2.395f, 0f) },
            { new Vector2Int(4, 1), new Vector3(  1.125f,   2.395f, 0f) },
            { new Vector2Int(5, 1), new Vector3(  2.133f,   2.395f, 0f) },
            { new Vector2Int(6, 1), new Vector3(  2.962f,   2.395f, 0f) },

            // Row 2 (cols 2..5 + Right Green Gate bottom at col 6) - Y = 1.413f
            { new Vector2Int(2, 2), new Vector3( -0.882f,   1.413f, 0f) },
            { new Vector2Int(3, 2), new Vector3(  0.121f,   1.413f, 0f) },
            { new Vector2Int(4, 2), new Vector3(  1.125f,   1.413f, 0f) },
            { new Vector2Int(5, 2), new Vector3(  2.133f,   1.413f, 0f) },
            { new Vector2Int(6, 2), new Vector3(  2.962f,   1.413f, 0f) },

            // Row 3 (Left Orange Gate at col 0 + cols 1..4) - Y = 0.430f
            { new Vector2Int(0, 3), new Vector3( -2.730f,   0.430f, 0f) },
            { new Vector2Int(1, 3), new Vector3( -1.893f,   0.430f, 0f) },
            { new Vector2Int(2, 3), new Vector3( -0.882f,   0.430f, 0f) },
            { new Vector2Int(3, 3), new Vector3(  0.121f,   0.430f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  1.125f,   0.430f, 0f) },

            // Row 4 (cols 2..4) - Y = -0.549f
            { new Vector2Int(2, 4), new Vector3( -0.882f,  -0.549f, 0f) },
            { new Vector2Int(3, 4), new Vector3(  0.121f,  -0.549f, 0f) },
            { new Vector2Int(4, 4), new Vector3(  1.125f,  -0.549f, 0f) },

            // Row 5 (Left Pink Gate top at col 0 + cols 1..5) - Y = -1.534f
            { new Vector2Int(0, 5), new Vector3( -2.730f,  -1.534f, 0f) },
            { new Vector2Int(1, 5), new Vector3( -1.893f,  -1.534f, 0f) },
            { new Vector2Int(2, 5), new Vector3( -0.882f,  -1.534f, 0f) },
            { new Vector2Int(3, 5), new Vector3(  0.121f,  -1.534f, 0f) },
            { new Vector2Int(4, 5), new Vector3(  1.125f,  -1.534f, 0f) },
            { new Vector2Int(5, 5), new Vector3(  2.133f,  -1.534f, 0f) },

            // Row 6 (Left Pink Gate bottom at col 0 + cols 1..5) - Y = -2.514f
            { new Vector2Int(0, 6), new Vector3( -2.730f,  -2.514f, 0f) },
            { new Vector2Int(1, 6), new Vector3( -1.893f,  -2.514f, 0f) },
            { new Vector2Int(2, 6), new Vector3( -0.882f,  -2.514f, 0f) },
            { new Vector2Int(3, 6), new Vector3(  0.121f,  -2.514f, 0f) },
            { new Vector2Int(4, 6), new Vector3(  1.125f,  -2.514f, 0f) },
            { new Vector2Int(5, 6), new Vector3(  2.133f,  -2.514f, 0f) },

            // Row 7 (Bottom 3 prongs at cols 1, 3, 5) - Y = -3.496f
            { new Vector2Int(1, 7), new Vector3( -1.893f,  -3.496f, 0f) },
            { new Vector2Int(3, 7), new Vector3(  0.121f,  -3.496f, 0f) },
            { new Vector2Int(5, 7), new Vector3(  2.133f,  -3.496f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level7CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Top Green Gate Pad, cols 3..4) - Y = 1.533f
            { new Vector2Int(3, 0), new Vector3( -0.443f,   1.533f, 0f) },
            { new Vector2Int(4, 0), new Vector3(  0.476f,   1.533f, 0f) },

            // Row 1 (Top playable row, cols 3..4) - Y = 0.720f
            { new Vector2Int(3, 1), new Vector3( -0.443f,   0.720f, 0f) },
            { new Vector2Int(4, 1), new Vector3(  0.476f,   0.720f, 0f) },

            // Row 2 (Middle playable row, cols 2..5) - Y = -0.265f
            { new Vector2Int(2, 2), new Vector3( -1.361f,  -0.265f, 0f) },
            { new Vector2Int(3, 2), new Vector3( -0.443f,  -0.265f, 0f) },
            { new Vector2Int(4, 2), new Vector3(  0.476f,  -0.265f, 0f) },
            { new Vector2Int(5, 2), new Vector3(  1.395f,  -0.265f, 0f) },

            // Row 3 (Bottom row: Left Yellow Gate at col 0, playable cols 1..6, Right Pink Gate at col 7) - Y = -1.252f
            { new Vector2Int(0, 3), new Vector3( -3.069f,  -1.252f, 0f) },
            { new Vector2Int(1, 3), new Vector3( -2.293f,  -1.252f, 0f) },
            { new Vector2Int(2, 3), new Vector3( -1.361f,  -1.252f, 0f) },
            { new Vector2Int(3, 3), new Vector3( -0.443f,  -1.252f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  0.476f,  -1.252f, 0f) },
            { new Vector2Int(5, 3), new Vector3(  1.395f,  -1.252f, 0f) },
            { new Vector2Int(6, 3), new Vector3(  2.333f,  -1.252f, 0f) },
            { new Vector2Int(7, 3), new Vector3(  3.085f,  -1.252f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level8CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Playable cols 1..2 at Y = 3.642f; Top Green Gate Pad cols 4..5 at Y = 3.473f)
            { new Vector2Int(1, 0), new Vector3( -2.241f,   3.642f, 0f) },
            { new Vector2Int(2, 0), new Vector3( -1.346f,   3.642f, 0f) },
            { new Vector2Int(4, 0), new Vector3(  0.462f,   3.473f, 0f) },
            { new Vector2Int(5, 0), new Vector3(  1.364f,   3.473f, 0f) },

            // Row 1 (Playable cols 1..6 + Right Orange Gate at col 7) - Y = 2.746f
            { new Vector2Int(1, 1), new Vector3( -2.241f,   2.746f, 0f) },
            { new Vector2Int(2, 1), new Vector3( -1.346f,   2.746f, 0f) },
            { new Vector2Int(3, 1), new Vector3( -0.439f,   2.746f, 0f) },
            { new Vector2Int(4, 1), new Vector3(  0.462f,   2.746f, 0f) },
            { new Vector2Int(5, 1), new Vector3(  1.364f,   2.746f, 0f) },
            { new Vector2Int(6, 1), new Vector3(  2.253f,   2.746f, 0f) },
            { new Vector2Int(7, 1), new Vector3(  2.941f,   2.746f, 0f) },

            // Row 2 (Playable cols 1..6) - Y = 1.849f
            { new Vector2Int(1, 2), new Vector3( -2.241f,   1.849f, 0f) },
            { new Vector2Int(2, 2), new Vector3( -1.346f,   1.849f, 0f) },
            { new Vector2Int(3, 2), new Vector3( -0.439f,   1.849f, 0f) },
            { new Vector2Int(4, 2), new Vector3(  0.462f,   1.849f, 0f) },
            { new Vector2Int(5, 2), new Vector3(  1.364f,   1.849f, 0f) },
            { new Vector2Int(6, 2), new Vector3(  2.253f,   1.849f, 0f) },

            // Row 3 (Left Purple Gate top at col 0, Playable cols 1,2,4,5,6 [col 3 is obstacle hole], Right Blue Gate top at col 7) - Y = 0.935f
            { new Vector2Int(0, 3), new Vector3( -2.927f,   0.935f, 0f) },
            { new Vector2Int(1, 3), new Vector3( -2.241f,   0.935f, 0f) },
            { new Vector2Int(2, 3), new Vector3( -1.346f,   0.935f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  0.462f,   0.935f, 0f) },
            { new Vector2Int(5, 3), new Vector3(  1.364f,   0.935f, 0f) },
            { new Vector2Int(6, 3), new Vector3(  2.253f,   0.935f, 0f) },
            { new Vector2Int(7, 3), new Vector3(  2.941f,   0.935f, 0f) },

            // Row 4 (Left Purple Gate bottom at col 0, Playable cols 1,2,3,5,6 [col 4 is obstacle hole], Right Blue Gate bottom at col 7) - Y = 0.029f
            { new Vector2Int(0, 4), new Vector3( -2.927f,   0.029f, 0f) },
            { new Vector2Int(1, 4), new Vector3( -2.241f,   0.029f, 0f) },
            { new Vector2Int(2, 4), new Vector3( -1.346f,   0.029f, 0f) },
            { new Vector2Int(3, 4), new Vector3( -0.439f,   0.029f, 0f) },
            { new Vector2Int(5, 4), new Vector3(  1.364f,   0.029f, 0f) },
            { new Vector2Int(6, 4), new Vector3(  2.253f,   0.029f, 0f) },
            { new Vector2Int(7, 4), new Vector3(  2.941f,   0.029f, 0f) },

            // Row 5 (Playable cols 1..6) - Y = -0.868f
            { new Vector2Int(1, 5), new Vector3( -2.241f,  -0.868f, 0f) },
            { new Vector2Int(2, 5), new Vector3( -1.346f,  -0.868f, 0f) },
            { new Vector2Int(3, 5), new Vector3( -0.439f,  -0.868f, 0f) },
            { new Vector2Int(4, 5), new Vector3(  0.462f,  -0.868f, 0f) },
            { new Vector2Int(5, 5), new Vector3(  1.364f,  -0.868f, 0f) },
            { new Vector2Int(6, 5), new Vector3(  2.253f,  -0.868f, 0f) },

            // Row 6 (Left Yellow Gate at col 0, Playable cols 1..6) - Y = -1.776f
            { new Vector2Int(0, 6), new Vector3( -2.927f,  -1.776f, 0f) },
            { new Vector2Int(1, 6), new Vector3( -2.241f,  -1.776f, 0f) },
            { new Vector2Int(2, 6), new Vector3( -1.346f,  -1.776f, 0f) },
            { new Vector2Int(3, 6), new Vector3( -0.439f,  -1.776f, 0f) },
            { new Vector2Int(4, 6), new Vector3(  0.462f,  -1.776f, 0f) },
            { new Vector2Int(5, 6), new Vector3(  1.364f,  -1.776f, 0f) },
            { new Vector2Int(6, 6), new Vector3(  1.777f + 0.476f,  -1.776f, 0f) },

            // Row 7 (Bottom-right extension cols 5..6) - Y = -2.665f
            { new Vector2Int(5, 7), new Vector3(  1.364f,  -2.665f, 0f) },
            { new Vector2Int(6, 7), new Vector3(  2.253f,  -2.665f, 0f) },

            // Row 8 (Bottom-right extension cols 5..6) - Y = -3.549f
            { new Vector2Int(5, 8), new Vector3(  1.364f,  -3.549f, 0f) },
            { new Vector2Int(6, 8), new Vector3(  2.253f,  -3.549f, 0f) },
        };

        private static readonly Dictionary<Vector2Int, Vector3> Level9CellPositions = new Dictionary<Vector2Int, Vector3>()
        {
            // Row 0 (Top Gate Pads: Red at col 0, Purple at col 3) - Y = 3.627f
            { new Vector2Int(0, 0), new Vector3( -2.129f,   3.627f, 0f) },
            { new Vector2Int(3, 0), new Vector3(  1.466f,   3.627f, 0f) },

            // Row 1 (Playable cols 0..3) - Y = 2.738f
            { new Vector2Int(0, 1), new Vector3( -2.129f,   2.738f, 0f) },
            { new Vector2Int(1, 1), new Vector3( -0.929f,   2.738f, 0f) },
            { new Vector2Int(2, 1), new Vector3(  0.265f,   2.738f, 0f) },
            { new Vector2Int(3, 1), new Vector3(  1.466f,   2.738f, 0f) },

            // Row 2 (Playable cols 0..3) - Y = 1.648f
            { new Vector2Int(0, 2), new Vector3( -2.129f,   1.648f, 0f) },
            { new Vector2Int(1, 2), new Vector3( -0.929f,   1.648f, 0f) },
            { new Vector2Int(2, 2), new Vector3(  0.265f,   1.648f, 0f) },
            { new Vector2Int(3, 2), new Vector3(  1.466f,   1.648f, 0f) },

            // Row 3 (Playable cols 0..3 + Right Yellow Gate at col 4) - Y = 0.554f
            { new Vector2Int(0, 3), new Vector3( -2.129f,   0.554f, 0f) },
            { new Vector2Int(1, 3), new Vector3( -0.929f,   0.554f, 0f) },
            { new Vector2Int(2, 3), new Vector3(  0.265f,   0.554f, 0f) },
            { new Vector2Int(3, 3), new Vector3(  1.466f,   0.554f, 0f) },
            { new Vector2Int(4, 3), new Vector3(  2.405f,   0.554f, 0f) },

            // Row 4 (Playable cols 0..3 + Right Blue Gate at col 4) - Y = -0.534f
            { new Vector2Int(0, 4), new Vector3( -2.129f,  -0.534f, 0f) },
            { new Vector2Int(1, 4), new Vector3( -0.929f,  -0.534f, 0f) },
            { new Vector2Int(2, 4), new Vector3(  0.265f,  -0.534f, 0f) },
            { new Vector2Int(3, 4), new Vector3(  1.466f,  -0.534f, 0f) },
            { new Vector2Int(4, 4), new Vector3(  2.405f,  -0.534f, 0f) },

            // Row 5 (Playable cols 0..3) - Y = -1.629f
            { new Vector2Int(0, 5), new Vector3( -2.129f,  -1.629f, 0f) },
            { new Vector2Int(1, 5), new Vector3( -0.929f,  -1.629f, 0f) },
            { new Vector2Int(2, 5), new Vector3(  0.265f,  -1.629f, 0f) },
            { new Vector2Int(3, 5), new Vector3(  1.466f,  -1.629f, 0f) },

            // Row 6 (Playable cols 0..3) - Y = -2.720f
            { new Vector2Int(0, 6), new Vector3( -2.129f,  -2.720f, 0f) },
            { new Vector2Int(1, 6), new Vector3( -0.929f,  -2.720f, 0f) },
            { new Vector2Int(2, 6), new Vector3(  0.265f,  -2.720f, 0f) },
            { new Vector2Int(3, 6), new Vector3(  1.466f,  -2.720f, 0f) },

            // Row 7 (Bottom Green Gate Pad cols 1..2) - Y = -3.605f
            { new Vector2Int(1, 7), new Vector3( -0.929f,  -3.605f, 0f) },
            { new Vector2Int(2, 7), new Vector3(  0.265f,  -3.605f, 0f) },
        };

        public Vector3 GridToWorld(int col, int row)
        {
            if (ActiveCustomLevelData != null)
            {
                float midCol = (_customMinCol + _customMaxCol) * 0.5f;
                float midRow = (_customMinRow + _customMaxRow) * 0.5f;
                float wx = _boardCenter.x + (col - midCol) * _customCellSize;
                float wy = _boardCenter.y - (row - midRow) * _customCellSize;
                return new Vector3(wx, wy, 0f);
            }

            if (_currentLevel == LevelId.Level1)
            {
                float[] colX = { -2.728f, -1.358f, -0.013f, 1.335f, 2.712f };
                float[] rowY = { 1.212f, 0.073f, -1.125f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-2.728f + col * 1.360f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (1.212f - row * 1.169f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

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

            if (_currentLevel == LevelId.Level4)
            {
                if (Level4CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                if (col > 3 && (row == 0 || row == 2 || row == 4))
                {
                    return _boardCenter + Level4CellPositions[new Vector2Int(3, row)] + Vector3.right * ((col - 3) * 1.55f);
                }
            }

            if (_currentLevel == LevelId.Level5)
            {
                if (Level5CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                float[] colX = { -2.766f, -1.974f, -1.187f, -0.400f, 0.394f, 1.181f, 1.970f, 2.755f };
                float[] rowY = { 3.856f, 3.133f, 2.394f, 1.658f, 0.924f, 0.189f, -0.544f, -1.277f, -2.009f, -2.742f, -3.437f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-2.766f + col * 0.790f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (3.856f - row * 0.734f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

            if (_currentLevel == LevelId.Level6)
            {
                if (Level6CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                float[] colX = { -2.730f, -1.893f, -0.882f, 0.121f, 1.125f, 2.133f, 2.962f };
                float[] rowY = { 3.253f, 2.395f, 1.413f, 0.430f, -0.549f, -1.534f, -2.514f, -3.496f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-2.730f + col * 1.005f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (3.253f - row * 0.982f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

            if (_currentLevel == LevelId.Level7)
            {
                if (Level7CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                float[] colX = { -3.069f, -2.293f, -1.361f, -0.443f, 0.476f, 1.395f, 2.333f, 3.085f };
                float[] rowY = { 1.533f, 0.720f, -0.265f, -1.252f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-3.069f + col * 0.925f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (1.533f - row * 0.986f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

            if (_currentLevel == LevelId.Level8)
            {
                if (Level8CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                float[] colX = { -2.927f, -2.241f, -1.346f, -0.439f, 0.462f, 1.364f, 2.253f, 2.941f };
                float[] rowY = { 3.642f, 2.746f, 1.849f, 0.935f, 0.029f, -0.868f, -1.776f, -2.665f, -3.549f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-2.927f + col * 0.899f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (3.642f - row * 0.899f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

            if (_currentLevel == LevelId.Level9)
            {
                if (Level9CellPositions.TryGetValue(new Vector2Int(col, row), out Vector3 pos))
                {
                    return _boardCenter + pos;
                }
                float[] colX = { -2.129f, -0.929f, 0.265f, 1.466f, 2.405f };
                float[] rowY = { 3.627f, 2.738f, 1.648f, 0.554f, -0.534f, -1.629f, -2.720f, -3.605f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-2.129f + col * 1.198f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (3.627f - row * 1.092f);
                return _boardCenter + new Vector3(cx, ry, 0f);
            }

            if (_currentLevel == LevelId.Level10)
            {
                // Pixel-measured cell centers of map_lv10 (500x500 sprite, 11 world units) relative to frame center
                float[] colX = { -3.058f, -2.442f, -1.639f, -0.831f, -0.011f, 0.787f, 1.606f, 2.409f, 3.025f };
                float[] rowY = { 4.488f, 3.916f, 3.179f, 2.442f, 1.716f, 0.979f, 0.242f, -0.495f, -1.232f, -1.969f, -2.563f };
                float cx = (col >= 0 && col < colX.Length) ? colX[col] : (-3.058f + col * 0.808f);
                float ry = (row >= 0 && row < rowY.Length) ? rowY[row] : (4.488f - row * 0.736f);
                return _boardCenter + GetLevelFrameOffset(LevelId.Level10) + new Vector3(cx, ry, 0f);
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
            if (ActiveCustomLevelData != null)
            {
                return ActiveCustomLevelData.IsValidCoord(col, row);
            }
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
                case LevelId.Level5: return 8;
                case LevelId.Level6: return 7;
                case LevelId.Level7: return 8;
                case LevelId.Level8: return 8;
                case LevelId.Level9: return 5;
                case LevelId.Level10: return 9;
                default: return 7;
            }
        }

        public static int GetLevelRows(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return 3;
                case LevelId.Level2: return 6;
                case LevelId.Level3: return 7;
                case LevelId.Level4: return 5;
                case LevelId.Level5: return 11;
                case LevelId.Level6: return 8;
                case LevelId.Level7: return 4;
                case LevelId.Level8: return 9;
                case LevelId.Level9: return 8;
                case LevelId.Level10: return 11;
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
                case LevelId.Level4: return 1.565f;
                case LevelId.Level5: return 0.790f;
                case LevelId.Level6: return 1.005f;
                case LevelId.Level7: return 0.925f;
                case LevelId.Level8: return 0.899f;
                case LevelId.Level9: return 1.198f;
                case LevelId.Level10: return 0.808f;
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
                case LevelId.Level4: return 1.420f;
                case LevelId.Level5: return 0.734f;
                case LevelId.Level6: return 0.982f;
                case LevelId.Level7: return 0.986f;
                case LevelId.Level8: return 0.899f;
                case LevelId.Level9: return 1.092f;
                case LevelId.Level10: return 0.736f;
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
                case LevelId.Level4: return 7.088f;
                case LevelId.Level5: return 8.000f;
                case LevelId.Level6: return 8.000f;
                case LevelId.Level7: return 8.000f;
                case LevelId.Level8: return 9.360f;
                case LevelId.Level9: return 7.467f;
                case LevelId.Level10: return 11.000f;
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
                case LevelId.Level4: return 8.390f;
                case LevelId.Level5: return 9.948f;
                case LevelId.Level6: return 9.943f;
                case LevelId.Level7: return 5.362f;
                case LevelId.Level8: return 10.502f;
                case LevelId.Level9: return 10.000f;
                case LevelId.Level10: return 11.000f;
                default: return 7.535f;
            }
        }

        public static Vector3 GetLevelFrameOffset(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return new Vector3(0f, -0.20f, 0f);
                case LevelId.Level4: return new Vector3(0.07f, 0.29f, 0f);
                case LevelId.Level8: return new Vector3(0.174f, 0f, 0f);
                case LevelId.Level9: return new Vector3(-0.060f, 0.048f, 0f);
                case LevelId.Level10: return new Vector3(0f, -0.650f, 0f);
                default: return Vector3.zero;
            }
        }

        public static Vector3 GetLevelGridOffset(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return new Vector3(0f, -0.526f, 0f);
                case LevelId.Level2: return new Vector3(0.617f, 0.426f, 0f);
                default: return Vector3.zero;
            }
        }

        public static bool CheckLevelValidCoord(LevelId level, int col, int row)
        {
            switch (level)
            {
                case LevelId.Level1:
                    return (col == 2 && (row == 0 || row == 1)) || (row == 2 && col >= 0 && col < 5);

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
                    if (row == 0 || row == 2 || row == 4) return col >= 0 && col <= 3;
                    if (row == 1 || row == 3) return col == 0;
                    return false;

                case LevelId.Level5:
                    if (row == 0 || row == 1 || row == 9 || row == 10) return col >= 3 && col <= 4;
                    if (row == 2 || row == 3 || row == 4 || row == 8) return col >= 2 && col <= 5;
                    if (row == 5) return col >= 0 && col <= 7;
                    if (row == 6 || row == 7) return col >= 1 && col <= 6;
                    return false;

                case LevelId.Level6:
                    if (row == 0) return col >= 2 && col <= 4;
                    if (row == 1) return col >= 1 && col <= 6;
                    if (row == 2) return col >= 2 && col <= 6;
                    if (row == 3) return col >= 0 && col <= 4;
                    if (row == 4) return col >= 2 && col <= 4;
                    if (row == 5 || row == 6) return col >= 0 && col <= 5;
                    if (row == 7) return col == 1 || col == 3 || col == 5;
                    return false;

                case LevelId.Level7:
                    if (row == 0 || row == 1) return col >= 3 && col <= 4;
                    if (row == 2) return col >= 2 && col <= 5;
                    if (row == 3) return col >= 0 && col <= 7;
                    return false;

                case LevelId.Level8:
                    if (row == 0) return col == 1 || col == 2 || col == 4 || col == 5;
                    if (row == 1) return col >= 1 && col <= 7;
                    if (row == 2 || row == 5) return col >= 1 && col <= 6;
                    if (row == 3) return col >= 0 && col <= 7 && col != 3;
                    if (row == 4) return col >= 0 && col <= 7 && col != 4;
                    if (row == 6) return col >= 0 && col <= 6;
                    if (row == 7 || row == 8) return col == 5 || col == 6;
                    return false;

                case LevelId.Level9:
                    if (row == 0) return col == 0 || col == 3;
                    if (row == 1 || row == 2 || row == 5 || row == 6) return col >= 0 && col <= 3;
                    if (row == 3 || row == 4) return col >= 0 && col <= 4;
                    if (row == 7) return col == 1 || col == 2;
                    return false;

                case LevelId.Level10:
                    if (row == 0 || row == 1 || row == 9 || row == 10) return col == 4;
                    if (row == 2 || row == 3) return col >= 3 && col <= 5;
                    if (row == 4 || row == 5) return col >= 2 && col <= 6;
                    if (row >= 6 && row <= 8) return col >= 0 && col <= 8;
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

        private static bool AreShapesMergeable(PieceShape a, PieceShape b)
        {
            return a == b;
        }

        public static bool CanPiecesMagnetMerge(PuzzlePiece a, PuzzlePiece b)
        {
            return a != null && b != null && a != b
                && !a.IsStone && !b.IsStone
                && !a.IsFrozen && !b.IsFrozen
                && !a.IsSliding && !b.IsSliding
                && !a.IsEscaping && !b.IsEscaping
                && AreShapesMergeable(a.Shape, b.Shape)
                && a.PieceColor == b.PieceColor
                && a.Value == b.Value;
        }

        public static bool CanPieceMagnetEscapeGate(PuzzlePiece piece, ExitGateView gate)
        {
            return piece != null && gate != null
                && !piece.IsStone && !piece.IsFrozen
                && !piece.IsSliding && !piece.IsEscaping
                && piece.PieceColor == gate.GateColor
                && piece.Value == gate.RequiredValue;
        }

        public static bool CanPieceMoveInDirection(PuzzlePiece piece, Vector2Int dir)
        {
            if (piece == null || piece.IsStone || dir == Vector2Int.zero) return false;
            if (piece.MoveAxis == MoveAxisConstraint.HorizontalOnly && dir.y != 0) return false;
            if (piece.MoveAxis == MoveAxisConstraint.VerticalOnly && dir.x != 0) return false;
            return true;
        }

        public bool CheckMergeOpportunity(PuzzlePiece piece, Vector2Int origin, Vector2Int direction, out PuzzlePiece targetPiece)
        {
            targetPiece = null;
            if (piece == null || piece.IsFrozen || piece.IsStone || !CanPieceMoveInDirection(piece, direction)) return false;

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

            if (hit != null && !multiple && !hit.IsFrozen && !hit.IsStone)
            {
                bool sameShape = AreShapesMergeable(piece.Shape, hit.Shape);
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

        private static bool IsPieceSpanAlignedWithGate(ExitGateView gate, Vector2Int direction, List<Vector2Int> candidateCells)
        {
            if (gate == null) return false;
            List<Vector2Int> coords = (gate.GateCoords != null && gate.GateCoords.Count > 0) ? gate.GateCoords : gate.TriggerCoords;
            if (coords == null || coords.Count == 0) return true;

            foreach (var c in candidateCells)
            {
                bool matched = false;
                foreach (var gc in coords)
                {
                    if (direction.y != 0 && gc.x == c.x) { matched = true; break; }
                    if (direction.x != 0 && gc.y == c.y) { matched = true; break; }
                }
                if (!matched) return false;
            }
            return true;
        }

        public bool CheckEscapeOpportunity(PuzzlePiece piece, Vector2Int origin, Vector2Int direction, out ExitGateView targetGate)
        {
            targetGate = null;
            if (piece == null || piece.IsFrozen || piece.IsStone || !CanPieceMoveInDirection(piece, direction)) return false;

            Vector2Int candidateOrigin = origin + direction;
            List<Vector2Int> candidateCells = piece.GetOccupiedCellsForOrigin(candidateOrigin);

            ExitGateView gate = null;
            foreach (var cell in candidateCells)
            {
                gate = GetGateAt(cell);
                if (gate != null) break;
            }

            bool anyOutside = false;
            bool hitOtherPiece = false;
            foreach (var c in candidateCells)
            {
                if (!IsValidCoord(c.x, c.y))
                {
                    anyOutside = true;
                }
                else
                {
                    PuzzlePiece occ = GetPieceAt(c.x, c.y);
                    if (occ != null && occ != piece)
                    {
                        hitOtherPiece = true;
                    }
                }
            }

            if (gate != null)
            {
                if (anyOutside || hitOtherPiece || !IsPieceSpanAlignedWithGate(gate, direction, candidateCells)) return false;
            }
            else if (anyOutside && !hitOtherPiece)
            {
                gate = CheckGateAlignment(piece, direction, candidateCells);
            }

            if (gate != null && gate.TriggerDirection == direction && piece.PieceColor == gate.GateColor && piece.Value == gate.RequiredValue)
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

            if (piece != null && piece.IsStone)
            {
                result.FailReason = "Khối đá cố định không thể di chuyển!";
                return result;
            }

            if (piece != null && piece.IsFrozen)
            {
                result.FailReason = $"Khối đang bị đóng băng! Hãy đưa {piece.IceCount} khối khác thoát cổng để phá băng.";
                return result;
            }

            if (piece != null && !CanPieceMoveInDirection(piece, direction))
            {
                result.FailReason = piece.MoveAxis == MoveAxisConstraint.HorizontalOnly
                    ? "Khối này chỉ trượt theo chiều ngang!"
                    : "Khối này chỉ trượt theo chiều dọc!";
                return result;
            }

            int maxGridSteps = Mathf.Max(8, Mathf.Max(CurrentColumns, CurrentRows));
            int maxStepsPossible = Mathf.Clamp(maxStepsRequested, 1, maxGridSteps);
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
                    bool blockedByWallOrPiece = false;
                    foreach (var c in candidateCells)
                    {
                        if (!IsValidCoord(c.x, c.y) && !gate.ContainsGateCell(c))
                        {
                            blockedByWallOrPiece = true;
                            break;
                        }
                        if (IsValidCoord(c.x, c.y))
                        {
                            PuzzlePiece occ = GetPieceAt(c.x, c.y);
                            if (occ != null && occ != piece)
                            {
                                blockedByWallOrPiece = true;
                                break;
                            }
                        }
                    }

                    if (blockedByWallOrPiece || !IsPieceSpanAlignedWithGate(gate, direction, candidateCells))
                    {
                        break;
                    }

                    if (gate.TriggerDirection == direction)
                    {
                        // Check gate conditions: matching color & exact required value
                        if (piece.PieceColor != gate.GateColor)
                        {
                            result.FailReason = $"Sai màu cổng! Cổng này màu {gate.ColorDisplayName}, khối của bạn màu {piece.ColorDisplayName}.";
                            break; // Gate acts as a wall - DO NOT STEP ON IT
                        }
                        if (piece.Value != gate.RequiredValue)
                        {
                            result.FailReason = piece.Value < gate.RequiredValue
                                ? $"Chưa đủ số yêu cầu! Cổng cần đúng số {gate.RequiredValue}, khối mới có số {piece.Value}."
                                : $"Số quá lớn! Cổng chỉ nhận đúng số {gate.RequiredValue}, khối đang có số {piece.Value}.";
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
                    if (hitPiece.IsStone)
                    {
                        break;
                    }
                    if (hitPiece.IsFrozen)
                    {
                        result.FailReason = "Khối mục tiêu đang bị đóng băng!";
                        break;
                    }

                    // Check merge condition on straight-line collision:
                    // 1) CÃ¹ng Shape
                    // 2) CÃ¹ng Color
                    // 3) CÃ¹ng Number
                    // 4) Tháº³ng nhau trÃªn dÆ°á»›i trÃ¡i pháº£i (KHÃ”NG ÄÆ¯á»¢C MERGE CHÃ‰O)
                    bool sameShape = AreShapesMergeable(piece.Shape, hitPiece.Shape);
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
            return TrySlidePieceSteps(piece, direction, Mathf.Max(8, Mathf.Max(CurrentColumns, CurrentRows)));
        }

        public bool TrySlidePieceSteps(PuzzlePiece piece, Vector2Int direction, int maxStepsRequested)
        {
            if (piece == null || piece.IsSliding || piece.IsEscaping) return false;
            if (piece.IsStone)
            {
                piece.PlayFrozenShake();
                SoundManager.Instance?.PlayError();
                GameUI.Instance?.ShowFeedbackError("Khối đá cố định không thể di chuyển!");
                return false;
            }
            if (piece.IsFrozen)
            {
                piece.PlayFrozenShake();
                SoundManager.Instance?.PlayError();
                GameUI.Instance?.ShowFeedbackError($"Khối đang bị đóng băng! Hãy đưa {piece.IceCount} khối khác thoát cổng để phá băng.");
                return false;
            }
            if (!CanPieceMoveInDirection(piece, direction))
            {
                piece.Bump(new Vector2(direction.x, -direction.y));
                SoundManager.Instance?.PlayError();
                GameUI.Instance?.ShowFeedbackError(piece.MoveAxis == MoveAxisConstraint.HorizontalOnly
                    ? "Khối này chỉ trượt theo chiều ngang!"
                    : "Khối này chỉ trượt theo chiều dọc!");
                return false;
            }

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
                // Each block escaping a gate decrements ice counters on any frozen blocks
                for (int i = 0; i < _activePieces.Count; i++)
                {
                    PuzzlePiece other = _activePieces[i];
                    if (other != null && other != piece && other.IsFrozen)
                    {
                        other.DecrementIceCount();
                    }
                }

                piece.PlayEscapeAnimation(gate.ExitDirection, () =>
                {
                    _activePieces.Remove(piece);
                    Score += piece.Value * 150;
                    OnScoreChanged?.Invoke(Score);
                    OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
                    CheckVictoryCondition();
                });

                SoundManager.Instance.PlayEscape();
            });
        }

        public void ExecuteMagnetJumpMerge(PuzzlePiece piece, PuzzlePiece target, Action onComplete = null)
        {
            if (piece == null || target == null)
            {
                onComplete?.Invoke();
                return;
            }

            Vector3 targetWorldPos = target.transform.position;
            Vector2Int targetOrigin = target.GridOrigin;

            ClearPieceFromGrid(piece);
            target.LockForMerge();
            SoundManager.Instance?.PlayMagnetPull();

            piece.JumpAndSlamMerge(targetWorldPos, targetOrigin, target,
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
                    KickCameraImpact(0.08f, 0.11f);
                },
                onArrival: onComplete);
        }

        public ExitGateView FindBestGateForColor(PieceColor color, Vector3 fromWorldPos)
        {
            ExitGateView bestMatch = null;
            float bestDistSq = float.MaxValue;
            if (_activeGates != null)
            {
                for (int i = 0; i < _activeGates.Count; i++)
                {
                    ExitGateView g = _activeGates[i];
                    if (g == null || g.GateColor != color) continue;
                    float dSq = (g.transform.position - fromWorldPos).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        bestMatch = g;
                    }
                }
                if (bestMatch != null) return bestMatch;

                for (int i = 0; i < _activeGates.Count; i++)
                {
                    ExitGateView g = _activeGates[i];
                    if (g == null) continue;
                    float dSq = (g.transform.position - fromWorldPos).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        bestMatch = g;
                    }
                }
            }
            return bestMatch;
        }

        public void ExecuteWandJumpEscape(PuzzlePiece piece, ExitGateView gate, Action onComplete = null, float jumpDuration = 0.30f)
        {
            if (piece == null)
            {
                onComplete?.Invoke();
                return;
            }

            ClearPieceFromGrid(piece);
            if (piece.IsFrozen)
            {
                piece.SetIceCount(0);
            }

            Vector3 targetPos = gate != null ? gate.transform.position : piece.transform.position + Vector3.up * 3.5f;
            Vector2 exitDir = gate != null ? gate.ExitDirection : Vector2.up;

            piece.JumpToGate(targetPos, () =>
            {
                for (int i = 0; i < _activePieces.Count; i++)
                {
                    PuzzlePiece other = _activePieces[i];
                    if (other != null && other != piece && other.IsFrozen && !other.IsSliding && !other.IsEscaping)
                    {
                        other.DecrementIceCount();
                    }
                }

                SoundManager.Instance?.PlayEscape();
                KickCameraImpact(0.045f, 0.065f);

                piece.PlayEscapeAnimation(exitDir, () =>
                {
                    _activePieces.Remove(piece);
                    Score += piece.Value * 150;
                    OnScoreChanged?.Invoke(Score);
                    OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
                    CheckVictoryCondition();
                    onComplete?.Invoke();
                });
            }, jumpDuration);
        }

        public void ExecuteMagnetJumpEscape(PuzzlePiece piece, ExitGateView gate, Action onComplete = null)
        {
            if (piece == null || gate == null)
            {
                onComplete?.Invoke();
                return;
            }

            Vector3 gatePos = gate.transform.position;
            ClearPieceFromGrid(piece);
            SoundManager.Instance?.PlayMagnetPull();

            piece.JumpToGate(gatePos, () =>
            {
                for (int i = 0; i < _activePieces.Count; i++)
                {
                    PuzzlePiece other = _activePieces[i];
                    if (other != null && other != piece && other.IsFrozen)
                    {
                        other.DecrementIceCount();
                    }
                }

                SoundManager.Instance?.PlayEscape();
                KickCameraImpact(0.055f, 0.08f);

                piece.PlayEscapeAnimation(gate.ExitDirection, () =>
                {
                    _activePieces.Remove(piece);
                    Score += piece.Value * 150;
                    OnScoreChanged?.Invoke(Score);
                    OnRemainingBlocksChanged?.Invoke(RemainingBlocksCount);
                    CheckVictoryCondition();
                    onComplete?.Invoke();
                });
            });
        }

        private void CheckVictoryCondition()
        {
            if (RemainingBlocksCount == 0)
            {
                SoundManager.Instance?.PlayVictory();
                GameUI.Instance?.ShowVictoryPopup();
            }
        }

        public void SwitchLevel(LevelId level)
        {
            if (CustomLevelRepository.TryGetLevel((int)level, out CustomLevelData customData))
            {
                LoadCustomLevel(customData, true);
                return;
            }
            SetActiveCustomData(null);
            _currentLevel = level;
            if (_initialPiecesTemplate != null)
            {
                Destroy(_initialPiecesTemplate);
                _initialPiecesTemplate = null;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                BakeEditModeLevel();
            }
            else
            {
                InitializeLevel(true);
            }
#else
            InitializeLevel(true);
#endif
        }
    }
}



