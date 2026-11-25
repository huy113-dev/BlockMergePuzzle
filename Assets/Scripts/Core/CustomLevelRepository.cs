using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlockMergePuzzle.Core
{
    public static class CustomLevelRepository
    {
        public const int BuiltInLevelCount = 10;
        public const string ResourcesFolder = "Levels";
        public const string EditorLevelsAssetFolder = "Assets/Resources/Levels";

        private const string PrefPlayTestActive = "BMP_PlayTestActive";
        private const string PrefPlayTestLevelJson = "BMP_PlayTestLevelJson";
        private const string PrefPlayTestReturnScene = "BMP_PlayTestReturnScene";
        private const string PrefPlayTestSavedLevel = "BMP_PlayTestSavedLevel";
        private const string PrefPlayTestSavedCoins = "BMP_PlayTestSavedCoins";

        private static Dictionary<int, CustomLevelData> _cachedLevels;

        /// <summary>Transient runtime bridge when transitioning from HomeScene/NextLevel into MainScene.</summary>
        public static int PendingRuntimeLevelNumber { get; set; } = 0;

        public static bool IsEditorPlayTestActive
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(PrefPlayTestActive, false);
#else
                return false;
#endif
            }
        }

        private static void EnsureLoaded()
        {
            if (_cachedLevels == null)
            {
                Reload();
            }
        }

        public static void Reload()
        {
            _cachedLevels = new Dictionary<int, CustomLevelData>();

            TextAsset[] assets = Resources.LoadAll<TextAsset>(ResourcesFolder);
            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] == null) continue;
                    CustomLevelData data = CustomLevelData.FromJson(assets[i].text);
                    if (data != null && data.levelNumber > 0)
                    {
                        _cachedLevels[data.levelNumber] = data;
                    }
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && Directory.Exists(EditorLevelsAssetFolder))
            {
                string[] files = Directory.GetFiles(EditorLevelsAssetFolder, "*.json", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        string text = File.ReadAllText(files[i]);
                        CustomLevelData data = CustomLevelData.FromJson(text);
                        if (data != null && data.levelNumber > 0)
                        {
                            _cachedLevels[data.levelNumber] = data;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[CustomLevelRepository] Failed to read {files[i]}: {ex.Message}");
                    }
                }
            }
#endif
        }

        public static int GetMaxLevelNumber()
        {
            EnsureLoaded();
            int max = BuiltInLevelCount;
            foreach (var kv in _cachedLevels)
            {
                if (kv.Key > max) max = kv.Key;
            }
            return max;
        }

        public static bool HasLevel(int levelNumber)
        {
            EnsureLoaded();
            return _cachedLevels.ContainsKey(levelNumber);
        }

        public static bool TryGetLevel(int levelNumber, out CustomLevelData data)
        {
            EnsureLoaded();
            if (_cachedLevels.TryGetValue(levelNumber, out CustomLevelData found) && found != null)
            {
                data = found.Clone();
                return true;
            }
            data = null;
            return false;
        }

        public static List<CustomLevelData> GetAllCustomLevels()
        {
            EnsureLoaded();
            var list = new List<CustomLevelData>(_cachedLevels.Count);
            foreach (var kv in _cachedLevels)
            {
                if (kv.Value != null) list.Add(kv.Value.Clone());
            }
            list.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
            return list;
        }

        public static int GetNextAvailableLevelNumber()
        {
            EnsureLoaded();
            int candidate = BuiltInLevelCount + 1;
            while (_cachedLevels.ContainsKey(candidate))
            {
                candidate++;
            }
            return candidate;
        }

        public static bool TryGetEditorPlayTestData(out CustomLevelData data)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool(PrefPlayTestActive, false))
            {
                string json = UnityEditor.EditorPrefs.GetString(PrefPlayTestLevelJson, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    data = CustomLevelData.FromJson(json);
                    if (data != null) return true;
                }
            }
#endif
            data = null;
            return false;
        }

#if UNITY_EDITOR
        public static void BeginEditorPlayTest(CustomLevelData data, string returnScenePath)
        {
            if (data == null) return;
            UnityEditor.EditorPrefs.SetBool(PrefPlayTestActive, true);
            UnityEditor.EditorPrefs.SetString(PrefPlayTestLevelJson, data.ToJson(false));
            UnityEditor.EditorPrefs.SetString(PrefPlayTestReturnScene, returnScenePath ?? string.Empty);
            UnityEditor.EditorPrefs.SetInt(PrefPlayTestSavedLevel, PlayerPrefs.GetInt("BMP_CurrentLevel", 1));
            UnityEditor.EditorPrefs.SetInt(PrefPlayTestSavedCoins, PlayerPrefs.GetInt("BMP_TotalCoins", 2000));
        }

        public static string GetPlayTestReturnScene()
        {
            return UnityEditor.EditorPrefs.GetString(PrefPlayTestReturnScene, string.Empty);
        }

        public static void EndEditorPlayTest()
        {
            if (!UnityEditor.EditorPrefs.GetBool(PrefPlayTestActive, false)) return;
            int savedLvl = UnityEditor.EditorPrefs.GetInt(PrefPlayTestSavedLevel, 1);
            int savedCoins = UnityEditor.EditorPrefs.GetInt(PrefPlayTestSavedCoins, 2000);
            PlayerPrefs.SetInt("BMP_CurrentLevel", savedLvl);
            PlayerPrefs.SetInt("BMP_TotalCoins", savedCoins);
            PlayerPrefs.Save();

            UnityEditor.EditorPrefs.SetBool(PrefPlayTestActive, false);
            UnityEditor.EditorPrefs.DeleteKey(PrefPlayTestLevelJson);
            UnityEditor.EditorPrefs.DeleteKey(PrefPlayTestReturnScene);
        }

        public static string GetLevelAssetPath(int levelNumber)
        {
            return $"{EditorLevelsAssetFolder}/level_{levelNumber}.json";
        }

        public static TextAsset SaveLevelJson(CustomLevelData data)
        {
            if (data == null) return null;
            data.Normalize();
            if (!Directory.Exists(EditorLevelsAssetFolder))
            {
                Directory.CreateDirectory(EditorLevelsAssetFolder);
            }

            // Remove any stale JSON files claiming the same levelNumber so Reload stays unambiguous
            string targetPath = GetLevelAssetPath(data.levelNumber);
            foreach (string f in Directory.GetFiles(EditorLevelsAssetFolder, "*.json", SearchOption.TopDirectoryOnly))
            {
                string normPath = f.Replace("\\", "/");
                if (string.Equals(normPath, targetPath, StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    var existing = CustomLevelData.FromJson(File.ReadAllText(normPath));
                    if (existing != null && existing.levelNumber == data.levelNumber)
                    {
                        UnityEditor.AssetDatabase.DeleteAsset(normPath);
                    }
                }
                catch { }
            }

            File.WriteAllText(targetPath, data.ToJson(true));
            UnityEditor.AssetDatabase.ImportAsset(targetPath, UnityEditor.ImportAssetOptions.ForceUpdate);
            UnityEditor.AssetDatabase.Refresh();
            Reload();
            return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(targetPath);
        }

        public static bool DeleteLevelJson(int levelNumber)
        {
            if (!Directory.Exists(EditorLevelsAssetFolder)) return false;
            bool deleted = false;
            foreach (string f in Directory.GetFiles(EditorLevelsAssetFolder, "*.json", SearchOption.TopDirectoryOnly))
            {
                string normPath = f.Replace("\\", "/");
                try
                {
                    var existing = CustomLevelData.FromJson(File.ReadAllText(normPath));
                    if (existing != null && existing.levelNumber == levelNumber)
                    {
                        if (UnityEditor.AssetDatabase.DeleteAsset(normPath)) deleted = true;
                    }
                }
                catch { }
            }
            if (deleted)
            {
                UnityEditor.AssetDatabase.Refresh();
                Reload();
            }
            return deleted;
        }
#endif

        /// <summary>Converts built-in Levels 1..10 into CustomLevelData for inspection, cloning, or JSON export.</summary>
        public static CustomLevelData ExportBuiltInLevelToData(LevelId level)
        {
            int idx = (int)level;
            int cols = LevelManager.GetLevelColumns(level);
            int rows = LevelManager.GetLevelRows(level);
            var data = CustomLevelData.CreateDefault(idx, cols, rows);
            data.name = $"Level {idx}";
            data.difficulty = (idx == 6 || idx == 10) ? "Hard" : "Easy";
            data.timeSeconds = idx <= 5 ? 300 : ((idx == 6 || idx == 7) ? 105 : 100);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    data.SetPlayableCell(c, r, LevelManager.CheckLevelValidCoord(level, c, r));
                }
            }

            void AddGate(PieceColor col, int req, GateSide side, int sx, int sy, int span)
            {
                var g = new CustomGateData
                {
                    color = col.ToString(),
                    requiredValue = req,
                    side = side.ToString(),
                    startCell = new GridCoord(sx, sy),
                    span = span
                };
                data.gates.Add(g);
                foreach (var gc in g.GetGateCoords())
                {
                    data.SetPlayableCell(gc.x, gc.y, false);
                }
            }

            void AddPiece(PieceColor col, PieceShape shape, int val, int ox, int oy, int ice = 0)
            {
                data.pieces.Add(new CustomPieceData
                {
                    color = col.ToString(),
                    shape = shape.ToString(),
                    value = val,
                    origin = new GridCoord(ox, oy),
                    iceCount = ice,
                    moveAxis = "None",
                    isStone = false
                });
            }

            switch (level)
            {
                case LevelId.Level1:
                    AddGate(PieceColor.Red, 2, GateSide.Top, 2, 0, 1);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 0, 2);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 4, 2);
                    break;
                case LevelId.Level2:
                    AddGate(PieceColor.Green, 2, GateSide.Top, 2, 0, 1);
                    AddGate(PieceColor.Red, 3, GateSide.Right, 5, 3, 1);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 2, 1);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 2, 2, 3);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 2, 5);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 0, 3);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 4, 3);
                    break;
                case LevelId.Level3:
                    AddGate(PieceColor.Green, 4, GateSide.Left, 0, 0, 1);
                    AddGate(PieceColor.Red, 4, GateSide.Right, 4, 3, 1);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 0, 6);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 2, 4);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 2, 2, 5);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 3, 2, 6);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 3, 3);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 3, 6);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 2, 1, 6);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 3, 4, 6);
                    break;
                case LevelId.Level4:
                    AddGate(PieceColor.Red, 3, GateSide.Right, 3, 0, 1);
                    AddGate(PieceColor.Green, 3, GateSide.Right, 3, 2, 1);
                    AddGate(PieceColor.Pink, 2, GateSide.Right, 3, 4, 1);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 2, 0, 0);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 0, 4);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 2, 4);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 2, 1, 0);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 2, 0);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 1, 4);
                    AddPiece(PieceColor.Pink, PieceShape.Single_1x1, 2, 2, 2);
                    break;
                case LevelId.Level5:
                    AddGate(PieceColor.Yellow, 5, GateSide.Top, 3, 0, 2);
                    AddGate(PieceColor.Green, 2, GateSide.Bottom, 3, 10, 2);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, 3, 1);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 2, 2, 2);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, 4, 2);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, 2, 3);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 3, 3, 3);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, 3, 4);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, 5, 3);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, 0, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 1, 2, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 2, 3, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 3, 4, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Vertical_1x2, 1, 5, 5);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_TopLeft, 1, 6, 5);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_TopRight, 1, 1, 7);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, 3, 7);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_TopLeft, 1, 5, 7);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 2, 3, 8);
                    break;
                case LevelId.Level6:
                    AddGate(PieceColor.Red, 2, GateSide.Top, 2, 0, 3);
                    AddGate(PieceColor.Green, 2, GateSide.Right, 6, 1, 2);
                    AddGate(PieceColor.Orange, 2, GateSide.Left, 0, 3, 1);
                    AddGate(PieceColor.Pink, 3, GateSide.Left, 0, 5, 2);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 1, 1);
                    AddPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 2, 2, 1);
                    AddPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, 3, 1);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, 4, 1);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 5, 1);
                    AddPiece(PieceColor.Red, PieceShape.Cross_3x3, 2, 2, 2);
                    AddPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, 1, 5);
                    AddPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 1, 1, 6);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, 3, 6);
                    AddPiece(PieceColor.Pink, PieceShape.Vertical_1x2, 1, 5, 6);
                    break;
                case LevelId.Level7:
                    AddGate(PieceColor.Green, 2, GateSide.Top, 3, 0, 2);
                    AddGate(PieceColor.Yellow, 2, GateSide.Left, 0, 3, 1);
                    AddGate(PieceColor.Pink, 2, GateSide.Right, 7, 3, 1);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 2, 3, 1, ice: 2);
                    AddPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, 2, 2);
                    AddPiece(PieceColor.Pink, PieceShape.Single_1x1, 1, 5, 2);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, 1, 3);
                    AddPiece(PieceColor.Yellow, PieceShape.Horizontal_2x1, 1, 5, 3);
                    break;
                case LevelId.Level8:
                    AddGate(PieceColor.Green, 2, GateSide.Top, 4, 0, 2);
                    AddGate(PieceColor.Orange, 2, GateSide.Right, 7, 1, 1);
                    AddGate(PieceColor.Purple, 2, GateSide.Left, 0, 3, 2);
                    AddGate(PieceColor.Blue, 2, GateSide.Right, 7, 3, 2);
                    AddGate(PieceColor.Yellow, 2, GateSide.Left, 0, 6, 1);
                    AddPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, 1, 0);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 2, 0);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 1, 1);
                    AddPiece(PieceColor.Orange, PieceShape.Single_1x1, 1, 2, 1);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 1, 1, 2);
                    AddPiece(PieceColor.Blue, PieceShape.Vertical_1x2, 1, 3, 1);
                    AddPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, 4, 1);
                    AddPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, 3, 5);
                    AddPiece(PieceColor.Blue, PieceShape.Vertical_1x2, 1, 4, 5);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 1, 5, 4);
                    AddPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, 5, 6);
                    AddPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, 6, 6);
                    AddPiece(PieceColor.Purple, PieceShape.Single_1x1, 1, 5, 7);
                    AddPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, 6, 7);
                    AddPiece(PieceColor.Purple, PieceShape.Horizontal_2x1, 2, 5, 8);
                    break;
                case LevelId.Level9:
                    AddGate(PieceColor.Red, 2, GateSide.Top, 0, 0, 1);
                    AddGate(PieceColor.Purple, 3, GateSide.Top, 3, 0, 1);
                    AddGate(PieceColor.Yellow, 3, GateSide.Right, 4, 3, 1);
                    AddGate(PieceColor.Blue, 2, GateSide.Right, 4, 4, 1);
                    AddGate(PieceColor.Green, 3, GateSide.Bottom, 1, 7, 2);
                    AddPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, 0, 1);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 2, 1, 1);
                    AddPiece(PieceColor.Red, PieceShape.Vertical_1x2, 1, 3, 1);
                    AddPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 2, 0, 3);
                    AddPiece(PieceColor.Green, PieceShape.Square_2x2, 2, 1, 3);
                    AddPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, 3, 3);
                    AddPiece(PieceColor.Purple, PieceShape.Vertical_1x2, 1, 0, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, 1, 5);
                    AddPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, 2, 5);
                    AddPiece(PieceColor.Yellow, PieceShape.Single_1x1, 2, 3, 5);
                    AddPiece(PieceColor.Blue, PieceShape.Single_1x1, 1, 1, 6);
                    AddPiece(PieceColor.Yellow, PieceShape.Single_1x1, 1, 2, 6);
                    break;
                case LevelId.Level10:
                    AddGate(PieceColor.Red, 4, GateSide.Top, 4, 0, 1);
                    AddGate(PieceColor.Orange, 2, GateSide.Bottom, 4, 10, 1);
                    AddGate(PieceColor.Green, 5, GateSide.Left, 0, 6, 3);
                    AddGate(PieceColor.Green, 4, GateSide.Right, 8, 6, 3);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 4, 1);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 2, 3, 2);
                    AddPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 1, 4, 2);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 2, 3, 3);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, 4, 3);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 2, 5, 3);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 2, 4);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 3, 3, 4);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 3, 5, 4);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 2, 6, 4);
                    AddPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 2, 2, 5);
                    AddPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 3, 4, 5);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 3, 1, 6);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_BottomLeft, 3, 2, 6);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 3, 6);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 4, 4, 6);
                    AddPiece(PieceColor.Green, PieceShape.ShapeL_BottomLeft, 3, 5, 6);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 2, 6, 6);
                    AddPiece(PieceColor.Green, PieceShape.Vertical_1x2, 1, 7, 6);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 1, 1, 8);
                    AddPiece(PieceColor.Green, PieceShape.Horizontal_2x1, 1, 2, 8);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 3, 4, 8);
                    AddPiece(PieceColor.Red, PieceShape.Single_1x1, 3, 5, 8);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 2, 6, 8);
                    AddPiece(PieceColor.Green, PieceShape.Single_1x1, 1, 7, 8);
                    AddPiece(PieceColor.Orange, PieceShape.Single_1x1, 2, 4, 9);
                    break;
            }

            data.Normalize();
            return data;
        }
    }
}
