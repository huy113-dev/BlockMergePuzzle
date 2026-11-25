#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        private enum EditorTab
        {
            VisualEditor = 0,
            PasteJson = 1,
            LevelsManager = 2
        }

        private enum PaintTool
        {
            PaintFloor = 0,
            EraseVoid = 1,
            PlaceGate = 2,
            PlaceBlock = 3,
            SelectEdit = 4,
            DeleteItem = 5,
            PanBoard = 6
        }

        private enum GateSideOption
        {
            Auto = 0,
            Top = 1,
            Bottom = 2,
            Left = 3,
            Right = 4
        }

        private static readonly PieceShape[] OrderedShapes =
        {
            PieceShape.Single_1x1,
            PieceShape.Vertical_1x2,
            PieceShape.Horizontal_2x1,
            PieceShape.Square_2x2,
            PieceShape.Vertical_1x3,
            PieceShape.Horizontal_3x1,
            PieceShape.ShapeL_TopRight,
            PieceShape.ShapeL_BottomRight,
            PieceShape.ShapeL_BottomLeft,
            PieceShape.ShapeL_TopLeft,
            PieceShape.ShapeT_Up,
            PieceShape.ShapeT_Right,
            PieceShape.ShapeT_Down,
            PieceShape.ShapeT_Left,
            PieceShape.Cross_3x3
        };

        private static readonly PieceColor[] OrderedColors =
        {
            PieceColor.Red,
            PieceColor.Orange,
            PieceColor.Yellow,
            PieceColor.Green,
            PieceColor.Blue,
            PieceColor.Purple,
            PieceColor.Pink
        };

        private EditorTab _activeTab = EditorTab.VisualEditor;
        private PaintTool _tool = PaintTool.PlaceBlock;

        private CustomLevelData _level;
        private int _resizeCols = 6;
        private int _resizeRows = 8;

        // Tool brush settings
        private PieceColor _brushColor = PieceColor.Red;
        private PieceShape _brushShape = PieceShape.Single_1x1;
        private int _brushValue = 1;
        private int _brushIceCount = 0;
        private MoveAxisConstraint _brushAxis = MoveAxisConstraint.None;
        private bool _brushIsStone = false;

        private PieceColor _gateColor = PieceColor.Red;
        private int _gateRequiredValue = 2;
        private int _gateSpan = 1;
        private GateSideOption _gateSideOption = GateSideOption.Auto;

        // Selection
        private int _selectedPieceIndex = -1;
        private int _selectedGateIndex = -1;

        // Undo / Redo
        private readonly List<string> _undoStack = new List<string>();
        private readonly List<string> _redoStack = new List<string>();

        // Paste JSON tab
        private string _pasteJsonText = string.Empty;
        private CustomLevelData _parsedPasteData;
        private bool _pasteOverrideLevelNumber = false;
        private int _pasteCustomLevelNumber = 11;
        private Vector2 _pasteScrollPos;
        private Vector2 _pasteErrorsScroll;

        // Scroll states
        private Vector2 _leftPaletteScroll;
        private Vector2 _managerScroll;
        private Vector2 _validationScroll;

        // Hover & Phone Preview tracking
        private Vector2Int _hoverCell = new Vector2Int(-1, -1);
        private bool _isDraggingFloorPaint = false;
        private bool _showPhoneFrame = true;
        private bool _isPanningBoard = false;
        private Vector2 _panLastMousePos;

        // Cached cosmic stone board preview textures
        private Texture2D _cachedBoardTex;
        private string _cachedBoardKey = string.Empty;
        private Texture2D _cachedPasteBoardTex;
        private string _cachedPasteBoardKey = string.Empty;

        [MenuItem("Tools/Block Merge Puzzle/Level Editor & Importer")]
        public static void ShowWindow()
        {
            var win = GetWindow<LevelEditorWindow>("Level Editor & Importer");
            win.minSize = new Vector2(1060f, 700f);
            win.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            if (_level == null)
            {
                int nextLvl = CustomLevelRepository.GetNextAvailableLevelNumber();
                _level = CustomLevelData.CreateDefault(nextLvl, 6, 8);
                _resizeCols = _level.columns;
                _resizeRows = _level.rows;
            }
            _pasteCustomLevelNumber = CustomLevelRepository.GetNextAvailableLevelNumber();
        }

        private void OnDisable()
        {
            if (_cachedBoardTex != null)
            {
                DestroyImmediate(_cachedBoardTex);
                _cachedBoardTex = null;
            }
            if (_cachedPasteBoardTex != null)
            {
                DestroyImmediate(_cachedPasteBoardTex);
                _cachedPasteBoardTex = null;
            }
            _cachedBoardKey = string.Empty;
            _cachedPasteBoardKey = string.Empty;
        }

        private void OnGUI()
        {
            DrawMainHeaderBar();
            EditorGUILayout.Space(4);

            switch (_activeTab)
            {
                case EditorTab.VisualEditor:
                    DrawVisualEditorTab();
                    break;
                case EditorTab.PasteJson:
                    DrawPasteJsonTab();
                    break;
                case EditorTab.LevelsManager:
                    DrawLevelsManagerTab();
                    break;
            }
        }

        private void DrawMainHeaderBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Toggle(_activeTab == EditorTab.VisualEditor, "🎨 Xếp Level (Visual)", EditorStyles.toolbarButton, GUILayout.Height(26f)))
                _activeTab = EditorTab.VisualEditor;
            if (GUILayout.Toggle(_activeTab == EditorTab.PasteJson, "📋 Paste JSON -> Thêm Level", EditorStyles.toolbarButton, GUILayout.Height(26f)))
            {
                if (_activeTab != EditorTab.PasteJson)
                    _pasteCustomLevelNumber = CustomLevelRepository.GetNextAvailableLevelNumber();
                _activeTab = EditorTab.PasteJson;
            }
            if (GUILayout.Toggle(_activeTab == EditorTab.LevelsManager, "📚 Danh Sách Level", EditorStyles.toolbarButton, GUILayout.Height(26f)))
                _activeTab = EditorTab.LevelsManager;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🌐 Mở Web Tool (HTML)", EditorStyles.toolbarButton, GUILayout.Width(165f), GUILayout.Height(26f)))
            {
                CustomLevelSceneBaker.OpenWebLevelEditor();
            }
            EditorGUILayout.EndHorizontal();
        }

        // ==================== TAB 1: VISUAL EDITOR ====================

        private void DrawVisualEditorTab()
        {
            _level.Normalize();

            // Top Level Metadata Bar
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Level #:", GUILayout.Width(54f));
            int newNum = EditorGUILayout.IntField(_level.levelNumber, GUILayout.Width(48f));
            if (newNum != _level.levelNumber && newNum >= 1)
            {
                RecordUndo();
                _level.levelNumber = newNum;
                _level.name = $"Level {newNum}";
            }

            GUILayout.Label("Tên:", GUILayout.Width(32f));
            string newName = EditorGUILayout.TextField(_level.name, GUILayout.Width(120f));
            if (newName != _level.name)
            {
                RecordUndo();
                _level.name = newName;
            }

            GUILayout.Label("Độ khó:", GUILayout.Width(50f));
            Enum.TryParse(_level.difficulty, true, out UI.HomeLevelDifficulty curDiff);
            var newDiff = (UI.HomeLevelDifficulty)EditorGUILayout.EnumPopup(curDiff, GUILayout.Width(96f));
            if (newDiff != curDiff)
            {
                RecordUndo();
                _level.difficulty = newDiff.ToString();
            }

            GUILayout.Label("Time (s):", GUILayout.Width(54f));
            int newTime = EditorGUILayout.IntField(_level.timeSeconds, GUILayout.Width(52f));
            if (newTime != _level.timeSeconds)
            {
                RecordUndo();
                _level.timeSeconds = Mathf.Clamp(newTime, 10, 3600);
            }
            GUILayout.Label($"({FormatMMSS(_level.timeSeconds)})", GUILayout.Width(52f));

            GUILayout.Space(8);
            GUILayout.Label("Grid:", GUILayout.Width(34f));
            _resizeCols = EditorGUILayout.IntField(_resizeCols, GUILayout.Width(32f));
            GUILayout.Label("x", GUILayout.Width(12f));
            _resizeRows = EditorGUILayout.IntField(_resizeRows, GUILayout.Width(32f));
            if (GUILayout.Button("Resize", GUILayout.Width(56f)))
            {
                RecordUndo();
                _level.ResizeGrid(_resizeCols, _resizeRows);
                _resizeCols = _level.columns;
                _resizeRows = _level.rows;
            }

            GUILayout.FlexibleSpace();
            GUI.enabled = _undoStack.Count > 0;
            if (GUILayout.Button($"↩ ({_undoStack.Count})", GUILayout.Width(48f))) PerformUndo();
            GUI.enabled = _redoStack.Count > 0;
            if (GUILayout.Button($"↪ ({_redoStack.Count})", GUILayout.Width(48f))) PerformRedo();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preset Map:", GUILayout.Width(76f));
            if (GUILayout.Button("✨ Cosmic 4 nhánh", GUILayout.Width(122f))) ApplyMapPreset(4);
            if (GUILayout.Button("Hình chữ nhật", GUILayout.Width(98f))) ApplyMapPreset(0);
            if (GUILayout.Button("Chữ thập", GUILayout.Width(74f))) ApplyMapPreset(1);
            if (GUILayout.Button("Kim cương", GUILayout.Width(82f))) ApplyMapPreset(2);
            if (GUILayout.Button("Có lỗ giữa", GUILayout.Width(82f))) ApplyMapPreset(3);
            if (GUILayout.Button("Xóa Block & Cổng", GUILayout.Width(120f)))
            {
                RecordUndo();
                _level.pieces.Clear();
                _level.gates.Clear();
                _selectedPieceIndex = -1;
                _selectedGateIndex = -1;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("📋 Copy JSON", GUILayout.Width(96f)))
            {
                GUIUtility.systemCopyBuffer = _level.ToJson(true);
                ShowNotification(new GUIContent($"Đã copy JSON Level {_level.levelNumber}!"));
            }
            if (GUILayout.Button("💾 Save to Game", GUILayout.Width(112f)))
            {
                SaveLevelWithConfirmation(_level);
            }
            if (GUILayout.Button("🏗️ Bake Scene", GUILayout.Width(100f)))
            {
                if (ConfirmBuiltInOverwriteIfNeeded(_level.levelNumber))
                {
                    CustomLevelSceneBaker.BakeCustomLevelToSceneAndPrefab(_level);
                }
            }
            GUI.backgroundColor = new Color(0.4f, 0.95f, 0.55f);
            if (GUILayout.Button("▶ Play Test", GUILayout.Width(98f)))
            {
                CustomLevelSceneBaker.StartPlayTest(_level);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Main Split Area: Left Palette | Center Interactive Grid
            EditorGUILayout.BeginHorizontal();

            // LEFT PALETTE
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(285f), GUILayout.ExpandHeight(true));
            _leftPaletteScroll = EditorGUILayout.BeginScrollView(_leftPaletteScroll);
            DrawToolPalette();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // CENTER CANVAS + PHONE ALIGNMENT BAR + VALIDATION
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawPhoneAlignmentToolbar(_level, isPasteTab: false);
            Rect canvasRect = GUILayoutUtility.GetRect(420f, 380f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawInteractiveBoard(canvasRect);
            DrawValidationPanel(_level);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolPalette()
        {
            GUILayout.Label("CÔNG CỤ VẼ (TOOLS)", EditorStyles.boldLabel);
            DrawToolButton(PaintTool.PaintFloor, "🟩 Vẽ Sàn Chơi (Floor)");
            DrawToolButton(PaintTool.EraseVoid, "⬛ Đục Lỗ / Viền (Void)");
            DrawToolButton(PaintTool.PlaceGate, "🚪 Đặt Cổng Thoát (Gate)");
            DrawToolButton(PaintTool.PlaceBlock, "💎 Đặt Block");
            DrawToolButton(PaintTool.SelectEdit, "👆 Chọn / Sửa Block & Cổng");
            DrawToolButton(PaintTool.DeleteItem, "🗑️ Xóa Block / Cổng (hoặc Right-Click)");
            DrawToolButton(PaintTool.PanBoard, "✋ Kéo Căn Vị Trí trên ĐT (Alt+Kéo)");

            EditorGUILayout.Space(8);

            if (_tool == PaintTool.PlaceBlock)
            {
                GUILayout.Label("THIẾT LẬP BLOCK", EditorStyles.boldLabel);
                DrawColorSelector(ref _brushColor);

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Số (Value):", GUILayout.Width(80f));
                if (GUILayout.Button("-", GUILayout.Width(26f))) _brushValue = Mathf.Max(1, _brushValue - 1);
                _brushValue = Mathf.Clamp(EditorGUILayout.IntField(_brushValue, GUILayout.Width(44f)), 1, 99);
                if (GUILayout.Button("+", GUILayout.Width(26f))) _brushValue = Mathf.Min(99, _brushValue + 1);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Hình Dáng [R xoay]:", EditorStyles.boldLabel);
                if (GUILayout.Button("↻ Xoay", GUILayout.Width(64f)))
                {
                    _brushShape = RotateShapeCW(_brushShape);
                }
                EditorGUILayout.EndHorizontal();
                DrawShapeSelector(ref _brushShape);

                EditorGUILayout.Space(6);
                GUILayout.Label("MECHANIC ĐẶC BIỆT", EditorStyles.boldLabel);
                _brushIsStone = EditorGUILayout.ToggleLeft("🪨 Đá Cố Định (Stone Block)", _brushIsStone);
                if (!_brushIsStone)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("❄️ Đóng Băng (Ice):", GUILayout.Width(124f));
                    _brushIceCount = Mathf.Clamp(EditorGUILayout.IntField(_brushIceCount, GUILayout.Width(48f)), 0, 30);
                    if (GUILayout.Button("0", GUILayout.Width(24f))) _brushIceCount = 0;
                    if (GUILayout.Button("2", GUILayout.Width(24f))) _brushIceCount = 2;
                    EditorGUILayout.EndHorizontal();

                    _brushAxis = (MoveAxisConstraint)EditorGUILayout.EnumPopup("↔ Khóa Trục Trượt:", _brushAxis);
                }
            }
            else if (_tool == PaintTool.PlaceGate)
            {
                GUILayout.Label("THIẾT LẬP CỔNG THOÁT", EditorStyles.boldLabel);
                DrawColorSelector(ref _gateColor);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Số yêu cầu:", GUILayout.Width(82f));
                if (GUILayout.Button("-", GUILayout.Width(26f))) _gateRequiredValue = Mathf.Max(1, _gateRequiredValue - 1);
                _gateRequiredValue = Mathf.Clamp(EditorGUILayout.IntField(_gateRequiredValue, GUILayout.Width(44f)), 1, 99);
                if (GUILayout.Button("+", GUILayout.Width(26f))) _gateRequiredValue = Mathf.Min(99, _gateRequiredValue + 1);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Độ rộng (Span):", GUILayout.Width(100f));
                for (int s = 1; s <= 4; s++)
                {
                    GUI.backgroundColor = (_gateSpan == s) ? new Color(0.45f, 0.85f, 1f) : Color.white;
                    if (GUILayout.Button(s.ToString(), GUILayout.Width(32f))) _gateSpan = s;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                _gateSideOption = (GateSideOption)EditorGUILayout.EnumPopup("Hướng Cổng:", _gateSideOption);
                EditorGUILayout.HelpBox("Click vào ô viền đen cạnh sàn chơi để đặt cổng (Auto sẽ tự nhận hướng ra ngoài).", MessageType.Info);
            }
            else if (_tool == PaintTool.SelectEdit)
            {
                DrawSelectedItemInspector();
            }
            else if (_tool == PaintTool.PanBoard)
            {
                EditorGUILayout.HelpBox("Kéo trực tiếp bàn cờ trong khung điện thoại (1440×3200) để căn vị trí, hoặc cuộn chuột / dùng thanh Zoom phía trên.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Left-click / kéo trên lưới để vẽ sàn chơi hoặc đục lỗ.\nRight-click để xóa nhanh.", MessageType.Info);
            }
        }

        private void DrawToolButton(PaintTool tool, string label)
        {
            GUI.backgroundColor = (_tool == tool) ? new Color(0.35f, 0.85f, 1f) : Color.white;
            if (GUILayout.Button(label, GUILayout.Height(26f)))
            {
                _tool = tool;
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawColorSelector(ref PieceColor selected)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < OrderedColors.Length; i++)
            {
                PieceColor c = OrderedColors[i];
                Color rgb = GameConfig.GetColorRGB(c);
                GUI.backgroundColor = rgb;
                string txt = (selected == c) ? "✓" : "";
                if (GUILayout.Button(txt, GUILayout.Width(32f), GUILayout.Height(26f)))
                {
                    selected = c;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawShapeSelector(ref PieceShape selected)
        {
            int perRow = 3;
            for (int i = 0; i < OrderedShapes.Length; i += perRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < perRow && (i + j) < OrderedShapes.Length; j++)
                {
                    PieceShape sh = OrderedShapes[i + j];
                    GUI.backgroundColor = (selected == sh) ? new Color(0.45f, 0.88f, 1f) : Color.white;
                    if (GUILayout.Button(GetShapeShortName(sh), GUILayout.Height(24f)))
                    {
                        selected = sh;
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSelectedItemInspector()
        {
            if (_selectedPieceIndex >= 0 && _selectedPieceIndex < _level.pieces.Count)
            {
                CustomPieceData p = _level.pieces[_selectedPieceIndex];
                GUILayout.Label($"ĐANG CHỌN BLOCK #{_selectedPieceIndex + 1}", EditorStyles.boldLabel);

                PieceColor col = p.GetPieceColor();
                DrawColorSelector(ref col);
                if (col != p.GetPieceColor()) { RecordUndo(); p.color = col.ToString(); }

                int newVal = Mathf.Clamp(EditorGUILayout.IntField("Số (Value):", p.value), 1, 99);
                if (newVal != p.value) { RecordUndo(); p.value = newVal; }

                PieceShape sh = p.GetPieceShape();
                DrawShapeSelector(ref sh);
                if (sh != p.GetPieceShape()) { RecordUndo(); p.shape = sh.ToString(); }

                bool newStone = EditorGUILayout.Toggle("🪨 Đá Cố Định:", p.isStone);
                if (newStone != p.isStone)
                {
                    RecordUndo();
                    p.isStone = newStone;
                    if (p.isStone) { p.iceCount = 0; p.moveAxis = MoveAxisConstraint.None.ToString(); }
                }
                if (!p.isStone)
                {
                    int newIce = Mathf.Clamp(EditorGUILayout.IntField("❄️ Đóng Băng:", p.iceCount), 0, 30);
                    if (newIce != p.iceCount) { RecordUndo(); p.iceCount = newIce; }

                    MoveAxisConstraint ax = (MoveAxisConstraint)EditorGUILayout.EnumPopup("↔ Khóa Trục:", p.GetMoveAxis());
                    if (ax != p.GetMoveAxis()) { RecordUndo(); p.moveAxis = ax.ToString(); }
                }

                GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
                if (GUILayout.Button("Xóa Block này"))
                {
                    RecordUndo();
                    _level.pieces.RemoveAt(_selectedPieceIndex);
                    _selectedPieceIndex = -1;
                }
                GUI.backgroundColor = Color.white;
            }
            else if (_selectedGateIndex >= 0 && _selectedGateIndex < _level.gates.Count)
            {
                CustomGateData g = _level.gates[_selectedGateIndex];
                GUILayout.Label($"ĐANG CHỌN CỔNG #{_selectedGateIndex + 1}", EditorStyles.boldLabel);

                PieceColor col = g.GetPieceColor();
                DrawColorSelector(ref col);
                if (col != g.GetPieceColor()) { RecordUndo(); g.color = col.ToString(); }

                int req = Mathf.Clamp(EditorGUILayout.IntField("Số yêu cầu:", g.requiredValue), 1, 99);
                if (req != g.requiredValue) { RecordUndo(); g.requiredValue = req; }

                int span = Mathf.Clamp(EditorGUILayout.IntSlider("Độ rộng (Span):", g.span, 1, 4), 1, 4);
                if (span != g.span) { RecordUndo(); g.span = span; }

                GateSide side = (GateSide)EditorGUILayout.EnumPopup("Hướng Cổng:", g.GetGateSide());
                if (side != g.GetGateSide()) { RecordUndo(); g.side = side.ToString(); }

                GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
                if (GUILayout.Button("Xóa Cổng này"))
                {
                    RecordUndo();
                    _level.gates.RemoveAt(_selectedGateIndex);
                    _selectedGateIndex = -1;
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("Click vào một Block hoặc Cổng trên lưới để chỉnh sửa thông số.", MessageType.Info);
            }
        }

        private void DrawInteractiveBoard(Rect areaRect)
        {
            EditorGUI.DrawRect(areaRect, new Color(0.05f, 0.04f, 0.11f, 1f));

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R && _tool == PaintTool.PlaceBlock)
            {
                _brushShape = RotateShapeCW(_brushShape);
                e.Use();
                Repaint();
            }

            ComputeBoardViewport(areaRect, _level, _showPhoneFrame,
                out Rect phoneRect, out float cellPx, out float pxPerUnit, out float startX, out float startY);

            if (_showPhoneFrame)
            {
                DrawPhoneScreenBackdrop(phoneRect, _level);
            }
            else
            {
                EditorGUI.DrawRect(areaRect, new Color(0.09f, 0.06f, 0.23f, 1f));
            }

            Vector2Int prevHover = _hoverCell;
            Rect hitRect = _showPhoneFrame ? phoneRect : areaRect;
            if (hitRect.Contains(e.mousePosition))
            {
                int hc = Mathf.FloorToInt((e.mousePosition.x - startX) / cellPx);
                int hr = Mathf.FloorToInt((e.mousePosition.y - startY) / cellPx);
                _hoverCell = (hc >= 0 && hc < _level.columns && hr >= 0 && hr < _level.rows)
                    ? new Vector2Int(hc, hr)
                    : new Vector2Int(-1, -1);
            }
            else
            {
                _hoverCell = new Vector2Int(-1, -1);
            }

            if (prevHover != _hoverCell && e.type == EventType.MouseMove)
            {
                Repaint();
            }

            DrawBoardCellsAndEntities(_level, startX, startY, cellPx, interactive: true);

            if (_showPhoneFrame)
            {
                DrawPhoneScreenMaskAndHud(areaRect, phoneRect, _level, showSafeGuides: true);
            }

            // Mouse wheel zoom inside phone preview
            if (_showPhoneFrame && e.type == EventType.ScrollWheel && areaRect.Contains(e.mousePosition))
            {
                _level.boardScale = Mathf.Clamp(_level.boardScale - e.delta.y * 0.03f, 0.45f, 1.80f);
                _level.Normalize();
                e.Use();
                Repaint();
                return;
            }

            // Handle panning (Pan tool, Alt + Left Drag, or Middle Mouse Drag)
            bool isPanTrigger = e.button == 2 || ((_tool == PaintTool.PanBoard || e.alt) && e.button == 0);
            if (e.type == EventType.MouseDown && isPanTrigger && areaRect.Contains(e.mousePosition))
            {
                _isPanningBoard = true;
                _panLastMousePos = e.mousePosition;
                RecordUndo();
                e.Use();
                return;
            }
            if (e.type == EventType.MouseDrag && _isPanningBoard)
            {
                Vector2 delta = e.mousePosition - _panLastMousePos;
                _panLastMousePos = e.mousePosition;
                float scaleMove = Mathf.Max(8f, pxPerUnit);
                _level.boardOffsetX = Mathf.Clamp(_level.boardOffsetX + delta.x / scaleMove, -3.5f, 3.5f);
                _level.boardOffsetY = Mathf.Clamp(_level.boardOffsetY - delta.y / scaleMove, -4.5f, 4.5f);
                Repaint();
                e.Use();
                return;
            }
            if (e.type == EventType.MouseUp && _isPanningBoard)
            {
                _isPanningBoard = false;
                e.Use();
                return;
            }

            // Handle normal board editing mouse clicks
            if (e.type == EventType.MouseDown && _hoverCell.x >= 0)
            {
                if (e.button == 1)
                {
                    HandleRightClick(_hoverCell.x, _hoverCell.y);
                    e.Use();
                }
                else if (e.button == 0 && _tool != PaintTool.PanBoard)
                {
                    if (_tool == PaintTool.PaintFloor || _tool == PaintTool.EraseVoid)
                    {
                        _isDraggingFloorPaint = true;
                        RecordUndo();
                        _level.SetPlayableCell(_hoverCell.x, _hoverCell.y, _tool == PaintTool.PaintFloor);
                    }
                    else
                    {
                        HandleLeftClick(_hoverCell.x, _hoverCell.y);
                    }
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && _isDraggingFloorPaint && e.button == 0 && _hoverCell.x >= 0)
            {
                bool targetPlay = (_tool == PaintTool.PaintFloor);
                if (_level.IsPlayableCell(_hoverCell.x, _hoverCell.y) != targetPlay)
                {
                    _level.SetPlayableCell(_hoverCell.x, _hoverCell.y, targetPlay);
                    Repaint();
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isDraggingFloorPaint = false;
            }
        }

        private void DrawBoardCellsAndEntities(CustomLevelData data, float startX, float startY, float cellPx, bool interactive)
        {
            bool hasActive = HasAnyActiveCell(data);
            Texture2D boardTex = null;
            if (hasActive)
            {
                boardTex = interactive
                    ? GetOrRebuildBoardPreview(data, ref _cachedBoardTex, ref _cachedBoardKey)
                    : GetOrRebuildBoardPreview(data, ref _cachedPasteBoardTex, ref _cachedPasteBoardKey);
            }

            if (boardTex != null)
            {
                data.GetActiveBounds(out int bMinC, out int bMaxC, out int bMinR, out int bMaxR);
                int activeCols = Mathf.Max(1, bMaxC - bMinC + 1);
                int activeRows = Mathf.Max(1, bMaxR - bMinR + 1);
                Rect texRect = new Rect(
                    startX + (bMinC - 0.50f) * cellPx,
                    startY + (bMinR - 0.50f) * cellPx,
                    (activeCols + 1.0f) * cellPx,
                    (activeRows + 1.0f) * cellPx);
                GUI.DrawTexture(texRect, boardTex, ScaleMode.StretchToFill, true);
            }

            // Faint clickable guide outlines & coordinates in interactive mode
            if (interactive)
            {
                GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 8,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.14f) }
                };

                for (int r = 0; r < data.rows; r++)
                {
                    for (int c = 0; c < data.columns; c++)
                    {
                        Rect cellRect = new Rect(startX + c * cellPx + 1f, startY + r * cellPx + 1f, cellPx - 2f, cellPx - 2f);
                        bool playable = data.IsPlayableCell(c, r);
                        if (!playable && !data.IsGateCell(c, r))
                        {
                            Handles.color = new Color(0.65f, 0.72f, 1f, 0.08f);
                            Handles.DrawWireCube(cellRect.center, cellRect.size);
                        }
                        if (cellPx >= 26f)
                        {
                            GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 1, 28, 12), $"{c},{r}", coordStyle);
                        }
                    }
                }
            }

            // Draw Gates: glowing inner L-brackets + ring + bold required value
            GUIStyle gateShadowStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(cellPx * 0.44f), 11, 24),
                normal = { textColor = new Color(0.04f, 0.03f, 0.14f, 0.85f) }
            };
            GUIStyle gateLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(cellPx * 0.44f), 11, 24),
                normal = { textColor = Color.white }
            };
            if (data.gates != null)
            {
                for (int i = 0; i < data.gates.Count; i++)
                {
                    var g = data.gates[i];
                    if (g == null) continue;
                    var coords = g.GetGateCoords();
                    if (coords.Count == 0) continue;

                    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
                    foreach (var gc in coords)
                    {
                        if (gc.x < minX) minX = gc.x;
                        if (gc.x > maxX) maxX = gc.x;
                        if (gc.y < minY) minY = gc.y;
                        if (gc.y > maxY) maxY = gc.y;
                    }

                    Rect gRect = new Rect(
                        startX + minX * cellPx + cellPx * 0.06f,
                        startY + minY * cellPx + cellPx * 0.06f,
                        (maxX - minX + 1) * cellPx - cellPx * 0.12f,
                        (maxY - minY + 1) * cellPx - cellPx * 0.12f);

                    if (boardTex == null)
                    {
                        EditorGUI.DrawRect(gRect, GameConfig.GetColorRGB(g.GetPieceColor()));
                    }

                    DrawGateCornerBrackets(gRect);

                    float ringRadius = Mathf.Min(gRect.width, gRect.height) * 0.32f;
                    Handles.color = new Color(1f, 1f, 1f, 0.65f);
                    Handles.DrawWireDisc(gRect.center, Vector3.forward, ringRadius);

                    if (interactive && _selectedGateIndex == i)
                    {
                        Handles.color = Color.white;
                        Handles.DrawWireCube(gRect.center, gRect.size + Vector2.one * 3f);
                    }

                    string label = g.requiredValue.ToString();
                    Rect shRect = new Rect(gRect.x + 1.5f, gRect.y + 1.5f, gRect.width, gRect.height);
                    GUI.Label(shRect, label, gateShadowStyle);
                    GUI.Label(gRect, label, gateLabelStyle);
                }
            }

            // Draw Pieces
            GUIStyle pieceStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(cellPx * 0.36f), 10, 22),
                normal = { textColor = Color.white }
            };
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(cellPx * 0.24f), 8, 13),
                normal = { textColor = new Color(0.82f, 0.98f, 1f) }
            };

            if (data.pieces != null)
            {
                for (int i = 0; i < data.pieces.Count; i++)
                {
                    var p = data.pieces[i];
                    if (p == null) continue;
                    var cellsOcc = p.GetOccupiedCells();
                    Color col = p.isStone ? new Color(0.28f, 0.30f, 0.42f, 1f) : GameConfig.GetColorRGB(p.GetPieceColor());

                    foreach (var cell in cellsOcc)
                    {
                        Rect pRect = new Rect(startX + cell.x * cellPx + 4f, startY + cell.y * cellPx + 4f, cellPx - 8f, cellPx - 8f);
                        EditorGUI.DrawRect(pRect, col);
                        if (p.iceCount > 0 && !p.isStone)
                        {
                            EditorGUI.DrawRect(pRect, new Color(0.35f, 0.85f, 1f, 0.46f));
                        }
                        if (interactive && _selectedPieceIndex == i)
                        {
                            Handles.color = Color.white;
                            Handles.DrawWireCube(pRect.center, pRect.size + Vector2.one * 2f);
                        }
                    }

                    if (cellsOcc.Count > 0)
                    {
                        Rect labelRect = new Rect(startX + cellsOcc[0].x * cellPx, startY + cellsOcc[0].y * cellPx, cellPx, cellPx);
                        if (p.isStone)
                        {
                            GUI.Label(labelRect, "🪨", pieceStyle);
                        }
                        else if (p.iceCount > 0)
                        {
                            GUI.Label(labelRect, $"❄{p.iceCount}", pieceStyle);
                        }
                        else
                        {
                            string axisBadge = p.GetMoveAxis() == MoveAxisConstraint.HorizontalOnly ? "↔"
                                : (p.GetMoveAxis() == MoveAxisConstraint.VerticalOnly ? "↕" : "");
                            GUI.Label(labelRect, p.value.ToString(), pieceStyle);
                            if (!string.IsNullOrEmpty(axisBadge))
                            {
                                GUI.Label(labelRect, axisBadge, badgeStyle);
                            }
                        }
                    }
                }
            }

            // Ghost Hover Preview
            if (interactive && _hoverCell.x >= 0)
            {
                if (_tool == PaintTool.PlaceBlock)
                {
                    var previewCells = GetShapeCells(_brushShape, _hoverCell);
                    bool canPlace = CanPlaceBlockAt(previewCells);
                    Color ghostCol = canPlace ? new Color(0.3f, 1f, 0.5f, 0.35f) : new Color(1f, 0.25f, 0.25f, 0.45f);
                    foreach (var c in previewCells)
                    {
                        if (c.x < 0 || c.x >= data.columns || c.y < 0 || c.y >= data.rows) continue;
                        EditorGUI.DrawRect(new Rect(startX + c.x * cellPx + 4f, startY + c.y * cellPx + 4f, cellPx - 8f, cellPx - 8f), ghostCol);
                    }
                }
                else if (_tool == PaintTool.PlaceGate)
                {
                    GateSide detected = ResolveGateSide(_hoverCell.x, _hoverCell.y);
                    var previewGate = new CustomGateData
                    {
                        side = detected.ToString(),
                        startCell = GridCoord.FromVector2Int(_hoverCell),
                        span = _gateSpan
                    };
                    foreach (var gc in previewGate.GetGateCoords())
                    {
                        if (gc.x < 0 || gc.x >= data.columns || gc.y < 0 || gc.y >= data.rows) continue;
                        EditorGUI.DrawRect(new Rect(startX + gc.x * cellPx + 4f, startY + gc.y * cellPx + 4f, cellPx - 8f, cellPx - 8f), new Color(1f, 1f, 1f, 0.35f));
                    }
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(startX + _hoverCell.x * cellPx + 2f, startY + _hoverCell.y * cellPx + 2f, cellPx - 4f, cellPx - 4f), new Color(1f, 1f, 1f, 0.16f));
                }
            }
        }

        private void HandleLeftClick(int col, int row)
        {
            if (_tool == PaintTool.PlaceBlock)
            {
                var cells = GetShapeCells(_brushShape, new Vector2Int(col, row));
                if (!CanPlaceBlockAt(cells))
                {
                    ShowNotification(new GUIContent("Vị trí đặt Block không hợp lệ hoặc bị chồng!"));
                    return;
                }
                RecordUndo();
                _level.pieces.Add(new CustomPieceData
                {
                    color = _brushColor.ToString(),
                    shape = _brushShape.ToString(),
                    value = _brushValue,
                    origin = new GridCoord(col, row),
                    iceCount = _brushIsStone ? 0 : _brushIceCount,
                    moveAxis = _brushIsStone ? MoveAxisConstraint.None.ToString() : _brushAxis.ToString(),
                    isStone = _brushIsStone
                });
                _selectedPieceIndex = _level.pieces.Count - 1;
                _selectedGateIndex = -1;
            }
            else if (_tool == PaintTool.PlaceGate)
            {
                GateSide side = ResolveGateSide(col, row);
                var candidate = new CustomGateData
                {
                    color = _gateColor.ToString(),
                    requiredValue = _gateRequiredValue,
                    side = side.ToString(),
                    startCell = new GridCoord(col, row),
                    span = _gateSpan
                };
                foreach (var gc in candidate.GetGateCoords())
                {
                    if (gc.x < 0 || gc.x >= _level.columns || gc.y < 0 || gc.y >= _level.rows || _level.IsPlayableCell(gc.x, gc.y))
                    {
                        ShowNotification(new GUIContent("Cổng phải nằm trên ô viền ('0') trong lưới!"));
                        return;
                    }
                }
                RecordUndo();
                // Replace overlapping gate if exists
                for (int i = _level.gates.Count - 1; i >= 0; i--)
                {
                    var existingCoords = _level.gates[i].GetGateCoords();
                    bool overlap = false;
                    foreach (var gc in candidate.GetGateCoords())
                    {
                        if (existingCoords.Contains(gc)) { overlap = true; break; }
                    }
                    if (overlap) _level.gates.RemoveAt(i);
                }
                _level.gates.Add(candidate);
                _selectedGateIndex = _level.gates.Count - 1;
                _selectedPieceIndex = -1;
            }
            else if (_tool == PaintTool.SelectEdit)
            {
                _selectedPieceIndex = FindPieceIndexAt(col, row);
                _selectedGateIndex = _selectedPieceIndex >= 0 ? -1 : FindGateIndexAt(col, row);
                Repaint();
            }
            else if (_tool == PaintTool.DeleteItem)
            {
                DeletePieceOrGateAt(col, row);
            }
        }

        private void HandleRightClick(int col, int row)
        {
            if (DeletePieceOrGateAt(col, row)) return;
            if (_level.IsPlayableCell(col, row))
            {
                RecordUndo();
                _level.SetPlayableCell(col, row, false);
            }
        }

        private bool DeletePieceOrGateAt(int col, int row)
        {
            int pIdx = FindPieceIndexAt(col, row);
            if (pIdx >= 0)
            {
                RecordUndo();
                _level.pieces.RemoveAt(pIdx);
                _selectedPieceIndex = -1;
                return true;
            }
            int gIdx = FindGateIndexAt(col, row);
            if (gIdx >= 0)
            {
                RecordUndo();
                _level.gates.RemoveAt(gIdx);
                _selectedGateIndex = -1;
                return true;
            }
            return false;
        }

        private int FindPieceIndexAt(int col, int row)
        {
            Vector2Int target = new Vector2Int(col, row);
            for (int i = 0; i < _level.pieces.Count; i++)
            {
                if (_level.pieces[i] != null && _level.pieces[i].GetOccupiedCells().Contains(target)) return i;
            }
            return -1;
        }

        private int FindGateIndexAt(int col, int row)
        {
            Vector2Int target = new Vector2Int(col, row);
            for (int i = 0; i < _level.gates.Count; i++)
            {
                if (_level.gates[i] != null && _level.gates[i].GetGateCoords().Contains(target)) return i;
            }
            return -1;
        }

        private GateSide ResolveGateSide(int col, int row)
        {
            if (_gateSideOption != GateSideOption.Auto)
            {
                return (GateSide)Enum.Parse(typeof(GateSide), _gateSideOption.ToString());
            }
            if (_level.IsPlayableCell(col, row + 1)) return GateSide.Top;
            if (_level.IsPlayableCell(col, row - 1)) return GateSide.Bottom;
            if (_level.IsPlayableCell(col + 1, row)) return GateSide.Left;
            if (_level.IsPlayableCell(col - 1, row)) return GateSide.Right;
            return GateSide.Top;
        }

        private bool CanPlaceBlockAt(List<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = cells[i];
                if (!_level.IsPlayableCell(c.x, c.y)) return false;
                if (FindPieceIndexAt(c.x, c.y) >= 0) return false;
            }
            return true;
        }

        private static List<Vector2Int> GetShapeCells(PieceShape shape, Vector2Int origin)
        {
            var offs = GameConfig.GetShapeOffsets(shape);
            var list = new List<Vector2Int>(offs.Length);
            for (int i = 0; i < offs.Length; i++) list.Add(origin + offs[i]);
            return list;
        }

        private void ApplyMapPreset(int preset)
        {
            RecordUndo();
            if (preset == 4) // Cosmic 4 alcoves (8x10)
            {
                _level.ResizeGrid(8, 10);
                _resizeCols = 8;
                _resizeRows = 10;
                _level.cells = new List<string>
                {
                    "00000000",
                    "00011000",
                    "00111100",
                    "01111110",
                    "00111100",
                    "00111100",
                    "01111110",
                    "00111100",
                    "00011000",
                    "00000000"
                };
                _level.gates.Clear();
                _level.gates.Add(new CustomGateData { color = "Green", requiredValue = 2, side = "Top", startCell = GridCoord.FromVector2Int(new Vector2Int(3, 0)), span = 2 });
                _level.gates.Add(new CustomGateData { color = "Pink", requiredValue = 2, side = "Left", startCell = GridCoord.FromVector2Int(new Vector2Int(0, 3)), span = 1 });
                _level.gates.Add(new CustomGateData { color = "Purple", requiredValue = 2, side = "Right", startCell = GridCoord.FromVector2Int(new Vector2Int(7, 6)), span = 1 });
                _level.gates.Add(new CustomGateData { color = "Orange", requiredValue = 2, side = "Bottom", startCell = GridCoord.FromVector2Int(new Vector2Int(3, 9)), span = 2 });
                _level.pieces.Clear();
                _selectedPieceIndex = -1;
                _selectedGateIndex = -1;
                return;
            }

            int cols = _level.columns;
            int rows = _level.rows;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    bool insideBorder = c > 0 && c < cols - 1 && r > 0 && r < rows - 1;
                    if (preset == 0) // Rect
                    {
                        _level.SetPlayableCell(c, r, insideBorder);
                    }
                    else if (preset == 1) // Cross
                    {
                        bool vArm = insideBorder && Mathf.Abs(c - (cols - 1) * 0.5f) <= 1.2f;
                        bool hArm = insideBorder && Mathf.Abs(r - (rows - 1) * 0.5f) <= 1.2f;
                        _level.SetPlayableCell(c, r, vArm || hArm);
                    }
                    else if (preset == 2) // Diamond
                    {
                        float cx = (cols - 1) * 0.5f;
                        float cy = (rows - 1) * 0.5f;
                        float norm = Mathf.Abs(c - cx) / Mathf.Max(1f, cx) + Mathf.Abs(r - cy) / Mathf.Max(1f, cy);
                        _level.SetPlayableCell(c, r, insideBorder && norm <= 1.15f);
                    }
                    else // Rect with center holes
                    {
                        bool hole = (c == cols / 2 && r == rows / 2) || (c == (cols - 1) / 2 && r == (rows - 1) / 2 - 1);
                        _level.SetPlayableCell(c, r, insideBorder && !hole);
                    }
                }
            }
        }

        // ==================== TAB 2: PASTE JSON -> ADD LEVEL ====================

        private void DrawPasteJsonTab()
        {
            EditorGUILayout.HelpBox(
                "Copy JSON từ Web Level Editor (Tools/LevelEditor.html) rồi bấm 'Paste from Clipboard' hoặc dán trực tiếp vào ô bên dưới để thêm level vào game (UI & cơ chế giống hệt các level trước).",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Paste from Clipboard", GUILayout.Height(30f), GUILayout.Width(190f)))
            {
                _pasteJsonText = GUIUtility.systemCopyBuffer ?? string.Empty;
                _parsedPasteData = CustomLevelData.FromJson(_pasteJsonText);
            }
            if (GUILayout.Button("🧹 Clear", GUILayout.Height(30f), GUILayout.Width(80f)))
            {
                _pasteJsonText = string.Empty;
                _parsedPasteData = null;
            }
            GUILayout.Space(12);
            _pasteOverrideLevelNumber = EditorGUILayout.ToggleLeft("Gán số Level mới tự động:", _pasteOverrideLevelNumber, GUILayout.Width(195f));
            if (_pasteOverrideLevelNumber)
            {
                _pasteCustomLevelNumber = Mathf.Max(1, EditorGUILayout.IntField(_pasteCustomLevelNumber, GUILayout.Width(54f)));
                if (GUILayout.Button("Next Free", GUILayout.Width(76f)))
                {
                    _pasteCustomLevelNumber = CustomLevelRepository.GetNextAvailableLevelNumber();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            // Left: JSON TextArea
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.48f), GUILayout.ExpandHeight(true));
            GUILayout.Label("JSON DATA:", EditorStyles.boldLabel);
            _pasteScrollPos = EditorGUILayout.BeginScrollView(_pasteScrollPos, GUILayout.ExpandHeight(true));
            EditorGUI.BeginChangeCheck();
            _pasteJsonText = EditorGUILayout.TextArea(_pasteJsonText, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                _parsedPasteData = CustomLevelData.FromJson(_pasteJsonText);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right: Live Preview & Import Actions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (_parsedPasteData == null)
            {
                EditorGUILayout.HelpBox("Chưa có dữ liệu JSON hợp lệ. Hãy paste JSON level vào ô bên trái.", MessageType.Warning);
            }
            else
            {
                CustomLevelData previewData = _parsedPasteData.Clone();
                if (_pasteOverrideLevelNumber)
                {
                    previewData.levelNumber = _pasteCustomLevelNumber;
                    previewData.name = $"Level {_pasteCustomLevelNumber}";
                }

                GUILayout.Label($"PREVIEW: Level {previewData.levelNumber} ({previewData.difficulty}) • Time: {FormatMMSS(previewData.timeSeconds)} • Grid: {previewData.columns}x{previewData.rows}", EditorStyles.boldLabel);
                GUILayout.Label($"Gates: {previewData.gates.Count}  |  Blocks: {previewData.pieces.Count}", EditorStyles.miniLabel);

                Rect previewRect = GUILayoutUtility.GetRect(240f, 300f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                if (DrawPhonePreview(previewRect, previewData))
                {
                    _parsedPasteData.boardScale = previewData.boardScale;
                    _parsedPasteData.boardOffsetX = previewData.boardOffsetX;
                    _parsedPasteData.boardOffsetY = previewData.boardOffsetY;
                    _pasteJsonText = _parsedPasteData.ToJson(true);
                    GUIUtility.keyboardControl = 0;
                    Repaint();
                }

                if (DrawPhoneAlignmentToolbar(previewData, isPasteTab: true))
                {
                    _parsedPasteData.boardScale = previewData.boardScale;
                    _parsedPasteData.boardOffsetX = previewData.boardOffsetX;
                    _parsedPasteData.boardOffsetY = previewData.boardOffsetY;
                    _pasteJsonText = _parsedPasteData.ToJson(true);
                    GUIUtility.keyboardControl = 0;
                    Repaint();
                }

                DrawValidationPanel(previewData);

                EditorGUILayout.Space(6);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.4f, 0.9f, 1f);
                if (GUILayout.Button("➕ Add Level to Game (JSON)", GUILayout.Height(34f)))
                {
                    if (SaveLevelWithConfirmation(previewData))
                    {
                        _level = previewData.Clone();
                    }
                }
                GUI.backgroundColor = new Color(0.95f, 0.78f, 0.35f);
                if (GUILayout.Button("🏗️ Add + Bake Scene & Prefab", GUILayout.Height(34f)))
                {
                    if (ConfirmBuiltInOverwriteIfNeeded(previewData.levelNumber))
                    {
                        CustomLevelSceneBaker.BakeCustomLevelToSceneAndPrefab(previewData);
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.4f, 0.95f, 0.55f);
                if (GUILayout.Button("▶ Play Test Ngay", GUILayout.Height(30f)))
                {
                    CustomLevelSceneBaker.StartPlayTest(previewData);
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("✏️ Mở trong Visual Editor", GUILayout.Height(30f)))
                {
                    RecordUndo();
                    _level = previewData.Clone();
                    _resizeCols = _level.columns;
                    _resizeRows = _level.rows;
                    _activeTab = EditorTab.VisualEditor;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        // ==================== TAB 3: LEVELS MANAGER ====================

        private void DrawLevelsManagerTab()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Reload Levels", GUILayout.Width(130f), GUILayout.Height(26f)))
            {
                CustomLevelRepository.Reload();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            _managerScroll = EditorGUILayout.BeginScrollView(_managerScroll);

            GUILayout.Label("CUSTOM LEVELS (Assets/Resources/Levels)", EditorStyles.boldLabel);
            List<CustomLevelData> customLevels = CustomLevelRepository.GetAllCustomLevels();
            if (customLevels.Count == 0)
            {
                EditorGUILayout.HelpBox("Chưa có Custom Level nào trong Assets/Resources/Levels/. Hãy tạo hoặc Paste JSON để thêm Level 11+.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < customLevels.Count; i++)
                {
                    CustomLevelData item = customLevels[i];
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label($"Level {item.levelNumber} — {item.name} ({item.difficulty}, {FormatMMSS(item.timeSeconds)}, {item.columns}x{item.rows}, {item.pieces.Count} blocks)", GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("✏️ Sửa", GUILayout.Width(64f)))
                    {
                        _level = item.Clone();
                        _resizeCols = _level.columns;
                        _resizeRows = _level.rows;
                        _activeTab = EditorTab.VisualEditor;
                    }
                    if (GUILayout.Button("📋 JSON", GUILayout.Width(64f)))
                    {
                        GUIUtility.systemCopyBuffer = item.ToJson(true);
                        ShowNotification(new GUIContent($"Đã copy JSON Level {item.levelNumber}"));
                    }
                    if (GUILayout.Button("🏗️ Bake", GUILayout.Width(64f)))
                    {
                        if (ConfirmBuiltInOverwriteIfNeeded(item.levelNumber))
                        {
                            CustomLevelSceneBaker.BakeCustomLevelToSceneAndPrefab(item);
                        }
                    }
                    GUI.backgroundColor = new Color(0.4f, 0.95f, 0.55f);
                    if (GUILayout.Button("▶ Play", GUILayout.Width(64f)))
                    {
                        CustomLevelSceneBaker.StartPlayTest(item);
                    }
                    GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
                    if (GUILayout.Button("🗑️", GUILayout.Width(32f)))
                    {
                        if (EditorUtility.DisplayDialog("Xóa Level", $"Xóa Level {item.levelNumber} khỏi Assets/Resources/Levels/?", "Xóa", "Hủy"))
                        {
                            CustomLevelRepository.DeleteLevelJson(item.levelNumber);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(12);
            GUILayout.Label("BUILT-IN LEVELS (1 – 10) — CLONE HOẶC COPY JSON MẪU", EditorStyles.boldLabel);
            for (int lv = 1; lv <= 10; lv++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label($"Built-in Level {lv}", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Clone sang Level mới", GUILayout.Width(148f)))
                {
                    var cloned = CustomLevelRepository.ExportBuiltInLevelToData((LevelId)lv);
                    int nextNum = CustomLevelRepository.GetNextAvailableLevelNumber();
                    cloned.levelNumber = nextNum;
                    cloned.name = $"Level {nextNum}";
                    _level = cloned;
                    _resizeCols = _level.columns;
                    _resizeRows = _level.rows;
                    _activeTab = EditorTab.VisualEditor;
                }
                if (GUILayout.Button("📋 Copy JSON", GUILayout.Width(96f)))
                {
                    var exported = CustomLevelRepository.ExportBuiltInLevelToData((LevelId)lv);
                    GUIUtility.systemCopyBuffer = exported.ToJson(true);
                    ShowNotification(new GUIContent($"Đã copy JSON Level {lv}"));
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // ==================== HELPERS ====================

        private void DrawValidationPanel(CustomLevelData data)
        {
            bool valid = data.Validate(out List<string> errors, out List<string> warnings);
            _validationScroll = EditorGUILayout.BeginScrollView(_validationScroll, GUILayout.MaxHeight(92f));
            if (valid && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ Level hợp lệ! Đủ điều kiện gộp & thoát cổng.", MessageType.Info);
            }
            else
            {
                foreach (string err in errors)
                {
                    EditorGUILayout.HelpBox(err, MessageType.Error);
                }
                foreach (string warn in warnings)
                {
                    EditorGUILayout.HelpBox(warn, MessageType.Warning);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private bool ConfirmBuiltInOverwriteIfNeeded(int levelNumber)
        {
            if (levelNumber >= 1 && levelNumber <= CustomLevelRepository.BuiltInLevelCount)
            {
                return EditorUtility.DisplayDialog(
                    "Ghi đè Built-in Level?",
                    $"Level {levelNumber} thuộc 10 level gốc (1–10). Bạn có chắc chắn muốn ghi đè không?",
                    "Ghi đè", "Hủy");
            }
            return true;
        }

        private bool SaveLevelWithConfirmation(CustomLevelData data)
        {
            if (data == null) return false;
            if (!ConfirmBuiltInOverwriteIfNeeded(data.levelNumber)) return false;

            if (!data.Validate(out List<string> errors, out _))
            {
                if (!EditorUtility.DisplayDialog("Level đang có lỗi kiểm tra",
                    "Level vẫn còn lỗi:\n• " + string.Join("\n• ", errors) + "\n\nVẫn lưu vào game?",
                    "Vẫn Lưu", "Quay lại sửa"))
                {
                    return false;
                }
            }

            CustomLevelRepository.SaveLevelJson(data);
            ShowNotification(new GUIContent($"Đã lưu Level {data.levelNumber} vào Assets/Resources/Levels/!"));
            return true;
        }

        private void RecordUndo()
        {
            if (_level == null) return;
            _undoStack.Add(_level.ToJson(false));
            if (_undoStack.Count > 50) _undoStack.RemoveAt(0);
            _redoStack.Clear();
        }

        private void PerformUndo()
        {
            if (_undoStack.Count == 0) return;
            _redoStack.Add(_level.ToJson(false));
            string snap = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            var restored = CustomLevelData.FromJson(snap);
            if (restored != null)
            {
                _level = restored;
                _resizeCols = _level.columns;
                _resizeRows = _level.rows;
            }
        }

        private void PerformRedo()
        {
            if (_redoStack.Count == 0) return;
            _undoStack.Add(_level.ToJson(false));
            string snap = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            var restored = CustomLevelData.FromJson(snap);
            if (restored != null)
            {
                _level = restored;
                _resizeCols = _level.columns;
                _resizeRows = _level.rows;
            }
        }

        private static PieceShape RotateShapeCW(PieceShape s)
        {
            switch (s)
            {
                case PieceShape.Vertical_1x2: return PieceShape.Horizontal_2x1;
                case PieceShape.Horizontal_2x1: return PieceShape.Vertical_1x2;
                case PieceShape.Vertical_1x3: return PieceShape.Horizontal_3x1;
                case PieceShape.Horizontal_3x1: return PieceShape.Vertical_1x3;
                case PieceShape.ShapeL_TopRight: return PieceShape.ShapeL_BottomRight;
                case PieceShape.ShapeL_BottomRight: return PieceShape.ShapeL_BottomLeft;
                case PieceShape.ShapeL_BottomLeft: return PieceShape.ShapeL_TopLeft;
                case PieceShape.ShapeL_TopLeft: return PieceShape.ShapeL_TopRight;
                case PieceShape.ShapeT_Up: return PieceShape.ShapeT_Right;
                case PieceShape.ShapeT_Right: return PieceShape.ShapeT_Down;
                case PieceShape.ShapeT_Down: return PieceShape.ShapeT_Left;
                case PieceShape.ShapeT_Left: return PieceShape.ShapeT_Up;
                default: return s;
            }
        }

        private static string GetShapeShortName(PieceShape s)
        {
            switch (s)
            {
                case PieceShape.Single_1x1: return "1x1";
                case PieceShape.Vertical_1x2: return "1x2 ▮";
                case PieceShape.Horizontal_2x1: return "2x1 ▬";
                case PieceShape.Square_2x2: return "2x2 ■";
                case PieceShape.Vertical_1x3: return "1x3 ┃";
                case PieceShape.Horizontal_3x1: return "3x1 ━";
                case PieceShape.ShapeL_TopRight: return "L ┐";
                case PieceShape.ShapeL_BottomRight: return "L ┘";
                case PieceShape.ShapeL_BottomLeft: return "L └";
                case PieceShape.ShapeL_TopLeft: return "L ┌";
                case PieceShape.ShapeT_Up: return "T ┴";
                case PieceShape.ShapeT_Right: return "T ├";
                case PieceShape.ShapeT_Down: return "T ┬";
                case PieceShape.ShapeT_Left: return "T ┤";
                case PieceShape.Cross_3x3: return "+ 3x3";
                default: return s.ToString();
            }
        }

        private static string GetSideArrow(GateSide side)
        {
            switch (side)
            {
                case GateSide.Top: return "↑";
                case GateSide.Bottom: return "↓";
                case GateSide.Left: return "←";
                case GateSide.Right: return "→";
                default: return "";
            }
        }

        private static bool HasAnyActiveCell(CustomLevelData data)
        {
            if (data == null) return false;
            for (int r = 0; r < data.rows; r++)
            {
                for (int c = 0; c < data.columns; c++)
                {
                    if (data.IsPlayableCell(c, r) || data.IsGateCell(c, r)) return true;
                }
            }
            return false;
        }

        private static string BuildBoardPreviewKey(CustomLevelData data)
        {
            var sb = new StringBuilder(256);
            sb.Append(data.levelNumber).Append('|')
              .Append(data.columns).Append('x').Append(data.rows).Append('|');
            if (data.cells != null)
            {
                for (int i = 0; i < data.cells.Count; i++) sb.Append(data.cells[i]).Append('/');
            }
            sb.Append('|');
            if (data.gates != null)
            {
                for (int i = 0; i < data.gates.Count; i++)
                {
                    var g = data.gates[i];
                    if (g == null) continue;
                    sb.Append(g.color).Append(':').Append(g.side).Append(':')
                      .Append(g.startCell.x).Append(',').Append(g.startCell.y).Append(':')
                      .Append(g.span).Append(';');
                }
            }
            return sb.ToString();
        }

        private static Texture2D GetOrRebuildBoardPreview(CustomLevelData data, ref Texture2D cacheTex, ref string cacheKey)
        {
            string key = BuildBoardPreviewKey(data);
            if (cacheTex != null && key == cacheKey) return cacheTex;

            if (cacheTex != null)
            {
                DestroyImmediate(cacheTex);
                cacheTex = null;
            }
            cacheTex = ProceduralBoardRenderer.GenerateBoardTexture(data, 64);
            cacheKey = key;
            return cacheTex;
        }

        private static void DrawGateCornerBrackets(Rect rect)
        {
            float inset = Mathf.Max(3f, Mathf.Min(rect.width, rect.height) * 0.14f);
            float arm = Mathf.Max(5f, Mathf.Min(rect.width, rect.height) * 0.22f);
            float thick = Mathf.Clamp(Mathf.Min(rect.width, rect.height) * 0.055f, 1.5f, 3f);
            Color col = new Color(1f, 1f, 1f, 0.72f);

            float x0 = rect.x + inset;
            float x1 = rect.xMax - inset;
            float y0 = rect.y + inset;
            float y1 = rect.yMax - inset;

            // Top-left L
            EditorGUI.DrawRect(new Rect(x0, y0, arm, thick), col);
            EditorGUI.DrawRect(new Rect(x0, y0, thick, arm), col);
            // Top-right L
            EditorGUI.DrawRect(new Rect(x1 - arm, y0, arm, thick), col);
            EditorGUI.DrawRect(new Rect(x1 - thick, y0, thick, arm), col);
            // Bottom-left L
            EditorGUI.DrawRect(new Rect(x0, y1 - thick, arm, thick), col);
            EditorGUI.DrawRect(new Rect(x0, y1 - arm, thick, arm), col);
            // Bottom-right L
            EditorGUI.DrawRect(new Rect(x1 - arm, y1 - thick, arm, thick), col);
            EditorGUI.DrawRect(new Rect(x1 - thick, y1 - arm, thick, arm), col);
        }

        private bool DrawPhoneAlignmentToolbar(CustomLevelData data, bool isPasteTab)
        {
            if (data == null) return false;
            bool changed = false;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (!isPasteTab)
            {
                GUI.backgroundColor = _showPhoneFrame ? new Color(0.35f, 0.85f, 1f) : Color.white;
                if (GUILayout.Button(_showPhoneFrame ? "📱 Khung ĐT Unity (1440×3200)" : "🔲 Phóng To Lưới", GUILayout.Width(205f), GUILayout.Height(22f)))
                {
                    _showPhoneFrame = !_showPhoneFrame;
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label("📱 Màn hình ĐT Unity (1440×3200) — Kéo để căn:", EditorStyles.miniBoldLabel, GUILayout.Width(275f));
            }

            if (_showPhoneFrame || isPasteTab)
            {
                GUILayout.Label("Zoom:", GUILayout.Width(38f));
                float newScale = EditorGUILayout.Slider(data.boardScale, 0.50f, 1.60f, GUILayout.MinWidth(95f));
                GUILayout.Label($"{Mathf.RoundToInt(newScale * 100f)}%", GUILayout.Width(36f));
                if (!Mathf.Approximately(newScale, data.boardScale))
                {
                    data.boardScale = Mathf.Round(newScale * 100f) / 100f;
                    changed = true;
                }

                if (GUILayout.Button("↺ Auto Fit", GUILayout.Width(72f), GUILayout.Height(20f)))
                {
                    if (!isPasteTab) RecordUndo();
                    data.boardScale = 1.0f;
                    data.boardOffsetX = 0f;
                    data.boardOffsetY = 0f;
                    changed = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_showPhoneFrame || isPasteTab)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Ngang (X):", GUILayout.Width(62f));
                float newX = EditorGUILayout.Slider(data.boardOffsetX, -2.50f, 2.50f, GUILayout.MinWidth(95f));
                if (!Mathf.Approximately(newX, data.boardOffsetX))
                {
                    data.boardOffsetX = Mathf.Round(newX * 100f) / 100f;
                    changed = true;
                }

                GUILayout.Space(8);
                GUILayout.Label("Dọc (Y):", GUILayout.Width(50f));
                float newY = EditorGUILayout.Slider(data.boardOffsetY, -3.50f, 3.50f, GUILayout.MinWidth(95f));
                if (!Mathf.Approximately(newY, data.boardOffsetY))
                {
                    data.boardOffsetY = Mathf.Round(newY * 100f) / 100f;
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            return changed;
        }

        private bool DrawPhonePreview(Rect areaRect, CustomLevelData data)
        {
            EditorGUI.DrawRect(areaRect, new Color(0.05f, 0.04f, 0.11f, 1f));
            ComputeBoardViewport(areaRect, data, usePhoneFrame: true,
                out Rect phoneRect, out float cellPx, out float pxPerUnit, out float startX, out float startY);

            DrawPhoneScreenBackdrop(phoneRect, data);
            DrawBoardCellsAndEntities(data, startX, startY, cellPx, interactive: false);
            DrawPhoneScreenMaskAndHud(areaRect, phoneRect, data, showSafeGuides: true);

            bool changed = false;
            Event e = Event.current;
            if (areaRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.ScrollWheel)
                {
                    data.boardScale = Mathf.Clamp(Mathf.Round((data.boardScale - e.delta.y * 0.03f) * 100f) / 100f, 0.45f, 1.80f);
                    changed = true;
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && (e.button == 0 || e.button == 2))
                {
                    float scaleMove = Mathf.Max(8f, pxPerUnit);
                    data.boardOffsetX = Mathf.Clamp(Mathf.Round((data.boardOffsetX + e.delta.x / scaleMove) * 100f) / 100f, -3.5f, 3.5f);
                    data.boardOffsetY = Mathf.Clamp(Mathf.Round((data.boardOffsetY - e.delta.y / scaleMove) * 100f) / 100f, -4.5f, 4.5f);
                    changed = true;
                    e.Use();
                }
            }
            return changed;
        }

        private static void ComputeBoardViewport(
            Rect areaRect,
            CustomLevelData data,
            bool usePhoneFrame,
            out Rect phoneRect,
            out float cellPx,
            out float pxPerUnit,
            out float startX,
            out float startY)
        {
            if (!usePhoneFrame)
            {
                phoneRect = areaRect;
                const float pad = 18f;
                float availW = Mathf.Max(60f, areaRect.width - pad * 2f);
                float availH = Mathf.Max(60f, areaRect.height - pad * 2f);
                cellPx = Mathf.Clamp(Mathf.Floor(Mathf.Min(availW / data.columns, availH / data.rows)), 16f, 66f);
                pxPerUnit = cellPx;
                float boardW = cellPx * data.columns;
                float boardH = cellPx * data.rows;
                startX = areaRect.x + (areaRect.width - boardW) * 0.5f;
                startY = areaRect.y + (areaRect.height - boardH) * 0.5f;
                return;
            }

            float availPhW = Mathf.Max(90f, areaRect.width - 20f);
            float availPhH = Mathf.Max(180f, areaRect.height - 26f);
            float phH = availPhH;
            float phW = phH * CustomLevelData.ReferencePhoneAspect;
            if (phW > availPhW)
            {
                phW = availPhW;
                phH = phW / CustomLevelData.ReferencePhoneAspect;
            }

            phoneRect = new Rect(
                areaRect.x + (areaRect.width - phW) * 0.5f,
                areaRect.y + (areaRect.height - phH) * 0.5f + 4f,
                phW,
                phH);

            float orthoSize = data.ComputeCameraOrthoSize(CustomLevelData.ReferencePhoneAspect);
            float viewH = orthoSize * 2f;
            float viewW = viewH * CustomLevelData.ReferencePhoneAspect;
            pxPerUnit = phoneRect.width / Mathf.Max(0.1f, viewW);
            float worldCell = data.ComputeWorldCellSize();
            cellPx = worldCell * pxPerUnit;

            data.GetActiveBounds(out int minC, out int maxC, out int minR, out int maxR);
            float midCol = (minC + maxC) * 0.5f;
            float midRow = (minR + maxR) * 0.5f;

            float wx0 = 0.015f + (-0.5f - midCol) * worldCell;
            float wy0 = 0.073f - (-0.5f - midRow) * worldCell;
            float camX = -data.boardOffsetX;
            float camY = -data.boardOffsetY;

            startX = phoneRect.x + ((wx0 - camX) / viewW + 0.5f) * phoneRect.width;
            startY = phoneRect.y + (0.5f - (wy0 - camY) / viewH) * phoneRect.height;
        }

        private static void DrawPhoneScreenBackdrop(Rect phoneRect, CustomLevelData data)
        {
            EditorGUI.DrawRect(phoneRect, new Color(0.08f, 0.06f, 0.21f, 1f));
            Sprite bgSprite = Utils.GemSpriteManager.GetBackgroundSprite();
            if (bgSprite != null && bgSprite.texture != null)
            {
                Texture2D tex = bgSprite.texture;
                Rect tr = bgSprite.textureRect;
                float phoneAspect = phoneRect.width / Mathf.Max(1f, phoneRect.height);
                float bgAspect = tr.width / Mathf.Max(1f, tr.height);
                Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
                if (bgAspect > phoneAspect)
                {
                    float keepW = uv.width * (phoneAspect / bgAspect);
                    uv.x += (uv.width - keepW) * 0.5f;
                    uv.width = keepW;
                }
                else
                {
                    float keepH = uv.height * (bgAspect / phoneAspect);
                    uv.y += (uv.height - keepH) * 0.5f;
                    uv.height = keepH;
                }
                Color prevCol = GUI.color;
                GUI.color = new Color(0.42f, 0.42f, 0.48f, 1f);
                GUI.DrawTextureWithTexCoords(phoneRect, tex, uv, false);
                GUI.color = prevCol;
            }
        }

        private static void DrawPhoneScreenMaskAndHud(Rect areaRect, Rect phoneRect, CustomLevelData data, bool showSafeGuides)
        {
            Color maskCol = new Color(0.05f, 0.04f, 0.11f, 1f);
            if (phoneRect.y > areaRect.y)
                EditorGUI.DrawRect(new Rect(areaRect.x, areaRect.y, areaRect.width, phoneRect.y - areaRect.y), maskCol);
            if (areaRect.yMax > phoneRect.yMax)
                EditorGUI.DrawRect(new Rect(areaRect.x, phoneRect.yMax, areaRect.width, areaRect.yMax - phoneRect.yMax), maskCol);
            if (phoneRect.x > areaRect.x)
                EditorGUI.DrawRect(new Rect(areaRect.x, phoneRect.y, phoneRect.x - areaRect.x, phoneRect.height), maskCol);
            if (areaRect.xMax > phoneRect.xMax)
                EditorGUI.DrawRect(new Rect(phoneRect.xMax, phoneRect.y, areaRect.xMax - phoneRect.xMax, phoneRect.height), maskCol);

            // Sleek device bezel frame
            Handles.color = new Color(0.32f, 0.30f, 0.65f, 0.95f);
            Handles.DrawWireCube(phoneRect.center, phoneRect.size + Vector2.one * 4f);
            Handles.color = new Color(0.14f, 0.12f, 0.28f, 1f);
            Handles.DrawWireCube(phoneRect.center, phoneRect.size + Vector2.one * 8f);

            DrawMainSceneHudOverlay(phoneRect, data, showSafeGuides);

            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.75f, 0.85f, 1f, 0.85f) }
            };
            GUI.Label(new Rect(areaRect.x + 8f, areaRect.y + 4f, 290f, 18f),
                $"📱 Unity Phone 1440×3200 | Zoom {Mathf.RoundToInt(data.boardScale * 100f)}%", badgeStyle);
        }

        private static void DrawMainSceneHudOverlay(Rect phoneRect, CustomLevelData data, bool showSafeGuides)
        {
            float s = phoneRect.width / 966f;
            float cx = phoneRect.x + phoneRect.width * 0.5f;
            float hdrY = phoneRect.y + 145f * s;

            // Header Replay & Settings buttons
            Sprite replaySp = Resources.Load<Sprite>("Sprites/LevelAssets/Return");
            Sprite pauseSp = Resources.Load<Sprite>("Sprites/LevelAssets/Pause");
            Rect replayRect = new Rect(cx - 378f * s - 56f * s, hdrY - 5.5f * s - 56f * s, 112f * s, 112f * s);
            Rect pauseRect = new Rect(cx + 374f * s - 54f * s, hdrY - 54f * s, 108f * s, 108f * s);
            DrawUiSpriteOrBox(replayRect, replaySp, new Color(0.45f, 0.25f, 0.75f, 0.9f), "↺");
            DrawUiSpriteOrBox(pauseRect, pauseSp, new Color(0.45f, 0.25f, 0.75f, 0.9f), "⚙");

            // LEVEL N Banner
            Sprite lvlSp = Utils.GemSpriteManager.GetMainUISprite("UI_Bn Level");
            Rect lvlRect = new Rect(cx + 21f * s - 138f * s, hdrY - 32f * s - 32f * s, 276f * s, 64f * s);
            DrawUiSpriteOrBox(lvlRect, lvlSp, new Color(0.14f, 0.18f, 0.36f, 0.94f), string.Empty);
            GUIStyle lvlTxtStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(28f * s), 7, 16),
                normal = { textColor = new Color(0.99f, 0.95f, 0.70f, 1f) }
            };
            GUI.Label(lvlRect, $"LEVEL {data.levelNumber}", lvlTxtStyle);

            // Timer Banner
            Sprite timeSp = Resources.Load<Sprite>("Sprites/LevelAssets/Time");
            Rect timeRect = new Rect(cx + 16f * s - 180f * s, hdrY + 80f * s - 54f * s, 360f * s, 108f * s);
            DrawUiSpriteOrBox(timeRect, timeSp, new Color(0.16f, 0.14f, 0.42f, 0.94f), string.Empty);
            GUIStyle timeTxtStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Mathf.RoundToInt(40f * s), 8, 20),
                normal = { textColor = Color.white }
            };
            Rect timeLabelRect = timeSp != null
                ? new Rect(timeRect.x + timeRect.width * 0.28f, timeRect.y, timeRect.width * 0.65f, timeRect.height)
                : timeRect;
            GUI.Label(timeLabelRect, FormatMMSS(data.timeSeconds), timeTxtStyle);

            // Bottom Booster Bar (3 slots)
            float boostY = phoneRect.yMax - 185f * s;
            float[] boostXs = { -285f, 0f, 285f };
            string[] boostIcons = { "❄", "🧲", "🪄" };
            int[] unlockLvls = { 8, 12, 16 };
            for (int i = 0; i < 3; i++)
            {
                float r = 74f * s;
                Rect br = new Rect(cx + boostXs[i] * s - r, boostY - r, r * 2f, r * 2f);
                bool unlocked = data.levelNumber >= unlockLvls[i];
                EditorGUI.DrawRect(br, unlocked ? new Color(0.18f, 0.45f, 0.85f, 0.88f) : new Color(0.14f, 0.13f, 0.28f, 0.88f));
                Handles.color = unlocked ? new Color(0.55f, 0.90f, 1f, 0.9f) : new Color(0.65f, 0.65f, 0.85f, 0.45f);
                Handles.DrawWireCube(br.center, br.size);
                GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Clamp(Mathf.RoundToInt(38f * s), 8, 18),
                    normal = { textColor = unlocked ? Color.white : new Color(0.95f, 0.78f, 0.25f) }
                };
                GUI.Label(br, unlocked ? boostIcons[i] : $"🔒{unlockLvls[i]}", iconStyle);
            }

            if (showSafeGuides)
            {
                Handles.color = new Color(0.22f, 0.91f, 1f, 0.38f);
                float topSafeY = phoneRect.y + phoneRect.height * 0.145f;
                float botSafeY = phoneRect.y + phoneRect.height * 0.79f;
                Handles.DrawLine(new Vector3(phoneRect.x + 6f, topSafeY, 0f), new Vector3(phoneRect.xMax - 6f, topSafeY, 0f));
                Handles.DrawLine(new Vector3(phoneRect.x + 6f, botSafeY, 0f), new Vector3(phoneRect.xMax - 6f, botSafeY, 0f));
            }
        }

        private static void DrawUiSpriteOrBox(Rect rect, Sprite sp, Color fallbackCol, string fallbackGlyph)
        {
            if (sp != null && sp.texture != null)
            {
                Texture2D tex = sp.texture;
                Rect tr = sp.textureRect;
                Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
                GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
                return;
            }
            EditorGUI.DrawRect(rect, fallbackCol);
            if (!string.IsNullOrEmpty(fallbackGlyph))
            {
                GUI.Label(rect, fallbackGlyph, new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                });
            }
        }

        private static string FormatMMSS(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:D2}:{s:D2}";
        }
    }
}
#endif
