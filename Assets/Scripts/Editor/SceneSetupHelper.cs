#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;
using MCPForUnity.Editor.Services;

namespace BlockMergePuzzle.Editor
{
    public static class SceneSetupHelper
    {
        private static T FindObj<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        [MenuItem("Tools/Block Merge Puzzle/Load Level 1")]
        public static void LoadLevel1() => LoadLevel(LevelId.Level1);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 2")]
        public static void LoadLevel2() => LoadLevel(LevelId.Level2);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 3")]
        public static void LoadLevel3() => LoadLevel(LevelId.Level3);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 4")]
        public static void LoadLevel4() => LoadLevel(LevelId.Level4);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 5")]
        public static void LoadLevel5() => LoadLevel(LevelId.Level5);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 6")]
        public static void LoadLevel6() => LoadLevel(LevelId.Level6);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 7")]
        public static void LoadLevel7() => LoadLevel(LevelId.Level7);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 8")]
        public static void LoadLevel8() => LoadLevel(LevelId.Level8);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 9")]
        public static void LoadLevel9() => LoadLevel(LevelId.Level9);

        [MenuItem("Tools/Block Merge Puzzle/Load Level 10")]
        public static void LoadLevel10() => LoadLevel(LevelId.Level10);

        public static void LoadLevel(LevelId level)
        {
            string scenePath = $"Assets/Scenes/{level}.unity";
            if (System.IO.File.Exists(scenePath))
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                if (activeScene.isDirty)
                {
                    EditorSceneManager.SaveScene(activeScene);
                }
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Debug.Log($"[Block Merge Puzzle] Loaded saved scene: {scenePath}");
            }
            else
            {
                LevelManager lm = FindObj<LevelManager>();
                if (lm != null)
                {
                    lm.CurrentLevel = level;
                }
                BakeLevelToScene();
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Bake All 10 Levels (Scenes & Prefabs)")]
        public static void BakeAll10Levels()
        {
            string scenesDir = "Assets/Scenes";
            string prefabsDir = "Assets/Prefabs/Levels";
            if (!System.IO.Directory.Exists(scenesDir)) System.IO.Directory.CreateDirectory(scenesDir);
            if (!System.IO.Directory.Exists(prefabsDir)) System.IO.Directory.CreateDirectory(prefabsDir);

            string mainScenePath = $"{scenesDir}/MainScene.unity";
            if (System.IO.File.Exists(mainScenePath))
            {
                EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
            }
            var activeScene = EditorSceneManager.GetActiveScene();

            for (int i = 1; i <= 10; i++)
            {
                if (i >= 1 && i <= 10) continue; // Protect finalized Level 1..10 scenes

                LevelId lid = (LevelId)i;
                string scenePath = $"{scenesDir}/Level{i}.unity";

                LevelManager lm = FindObj<LevelManager>();
                if (lm == null)
                {
                    GameObject lmObj = GameObject.Find("LevelManager");
                    if (lmObj == null) lmObj = new GameObject("LevelManager");
                    lm = lmObj.GetComponent<LevelManager>();
                    if (lm == null) lm = lmObj.AddComponent<LevelManager>();
                }
                lm.CurrentLevel = lid;

                SetupSceneHierarchy(lm);

                // Save level scene copy directly
                EditorSceneManager.SaveScene(activeScene, scenePath, true);

                // Save Level Prefab
                GameObject boardFrame = GameObject.Find("Board_Frame");
                GameObject exitGates = GameObject.Find("Exit_Gates");
                GameObject activePieces = GameObject.Find("Active_Pieces");
                if (boardFrame != null && exitGates != null && activePieces != null)
                {
                    GameObject levelRoot = new GameObject($"Level{i}_Board");
                    GameObject frameClone = Object.Instantiate(boardFrame, levelRoot.transform);
                    frameClone.name = "Board_Frame";
                    GameObject gatesClone = Object.Instantiate(exitGates, levelRoot.transform);
                    gatesClone.name = "Exit_Gates";
                    GameObject piecesClone = Object.Instantiate(activePieces, levelRoot.transform);
                    piecesClone.name = "Active_Pieces";

                    string prefabPath = $"{prefabsDir}/Level{i}_Board.prefab";
                    PrefabUtility.SaveAsPrefabAsset(levelRoot, prefabPath);
                    Object.DestroyImmediate(levelRoot);
                }

                Debug.Log($"[BakeAll10Levels] Level {i} baked and saved successfully!");
            }

            // Restore Level 1 as default active level in current scene
            LevelManager finalLm = FindObj<LevelManager>();
            if (finalLm != null)
            {
                finalLm.CurrentLevel = LevelId.Level1;
                SetupSceneHierarchy(finalLm);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BakeAll10Levels] ALL 10 LEVELS BAKED AND SAVED SUCCESSFULLY!");
        }

        public static void SetupSceneHierarchy(LevelManager lm)
        {
            // 1. Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camObj, "Create Camera");
            }

            float screenRatio = cam.aspect > 0.05f ? cam.aspect : (1440f / 3200f);
            if (screenRatio > 0.75f) screenRatio = 1440f / 3200f; // Force target portrait (0.45) in Edit Mode
            float paddingX = 0.12f;
            float paddingY = 0.40f;
            bool useFixedRef = lm != null && (lm.CurrentLevel == LevelId.Level8 || lm.CurrentLevel == LevelId.Level9 || lm.CurrentLevel == LevelId.Level10);
            float targetW = lm != null ? (useFixedRef ? 8.000f : lm.CurrentFrameWidth) : GameConfig.FrameWidth;
            float targetH = lm != null ? (useFixedRef ? 8.000f : lm.CurrentFrameHeight) : GameConfig.FrameHeight;
            float sizeByWidth = (targetW / 2f + paddingX) / screenRatio;
            float sizeByHeight = targetH / 2f + paddingY;

            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.11f, 0.16f);

            // 2. Event System
            if (FindObj<EventSystem>() == null && GameObject.Find("EventSystem") == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // 3. Audio & Control Singletons
            if (FindObj<SoundManager>() == null && GameObject.Find("SoundManager") == null)
            {
                GameObject sm = new GameObject("SoundManager");
                sm.AddComponent<SoundManager>();
                Undo.RegisterCreatedObjectUndo(sm, "Create SoundManager");
            }

            if (FindObj<PlayerPieceController>() == null && GameObject.Find("PlayerPieceController") == null)
            {
                GameObject pc = new GameObject("PlayerPieceController");
                pc.AddComponent<PlayerPieceController>();
                Undo.RegisterCreatedObjectUndo(pc, "Create PlayerPieceController");
            }

            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj == null)
                {
                    uiObj = new GameObject("GameUI");
                    Undo.RegisterCreatedObjectUndo(uiObj, "Create GameUI");
                }
                ui = uiObj.GetComponent<GameUI>();
                if (ui == null) ui = uiObj.AddComponent<GameUI>();
            }
            if (ui != null)
            {
                if (ui.transform.childCount == 0) ui.RebuildUI();
                else ui.EnsureBoosterBar();
                ui.UpdateLevelDisplay();
                ui.EnsureTimeText();
            }

            lm.BakeEditModeLevel();

            // Keep Level 1..10 pieces, clear unfinished residual blocks on other levels
            if (lm != null && (int)lm.CurrentLevel > 10)
            {
                GameObject piecesObj = GameObject.Find("Active_Pieces");
                if (piecesObj != null)
                {
                    for (int i = piecesObj.transform.childCount - 1; i >= 0; i--)
                    {
                        Object.DestroyImmediate(piecesObj.transform.GetChild(i).gameObject);
                    }
                }
            }

            TutorialHandGuide guide = FindObj<TutorialHandGuide>();
            if (guide != null)
            {
                guide.EvaluatePhase();
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)")]
        public static void BakeLevelToScene()
        {
            LevelManager lm = FindObj<LevelManager>();
            if (lm == null)
            {
                GameObject lmObj = GameObject.Find("LevelManager");
                if (lmObj == null)
                {
                    lmObj = new GameObject("LevelManager");
                    Undo.RegisterCreatedObjectUndo(lmObj, "Create LevelManager");
                }
                lm = lmObj.GetComponent<LevelManager>();
                if (lm == null) lm = lmObj.AddComponent<LevelManager>();
            }

            SetupSceneHierarchy(lm);

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            if (lm != null)
            {
                string prefabsDir = "Assets/Prefabs/Levels";
                if (!System.IO.Directory.Exists(prefabsDir)) System.IO.Directory.CreateDirectory(prefabsDir);

                GameObject boardFrame = GameObject.Find("Board_Frame");
                GameObject exitGates = GameObject.Find("Exit_Gates");
                GameObject activePieces = GameObject.Find("Active_Pieces");
                if (boardFrame != null && exitGates != null && activePieces != null)
                {
                    GameObject levelRoot = new GameObject($"{lm.CurrentLevel}_Board");
                    GameObject frameClone = Object.Instantiate(boardFrame, levelRoot.transform);
                    frameClone.name = "Board_Frame";
                    GameObject gatesClone = Object.Instantiate(exitGates, levelRoot.transform);
                    gatesClone.name = "Exit_Gates";
                    GameObject piecesClone = Object.Instantiate(activePieces, levelRoot.transform);
                    piecesClone.name = "Active_Pieces";

                    string prefabPath = $"{prefabsDir}/{lm.CurrentLevel}_Board.prefab";
                    PrefabUtility.SaveAsPrefabAsset(levelRoot, prefabPath);
                    Object.DestroyImmediate(levelRoot);
                }
            }

            Debug.Log($"[Block Merge Puzzle] Level {lm.CurrentLevel} baked successfully to scene {activeScene.name}! Board, gates and blocks are now visible in idle Edit Mode.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Setup Scene Now")]
        public static void SetupScene()
        {
            BakeLevelToScene();
        }

        [MenuItem("Tools/Block Merge Puzzle/Add Booster UI to Scene")]
        public static void AddBoosterUIToSceneMenu()
        {
            AddBoosterUIToScene(saveScene: true);
        }

        public static void AddBoosterUIToScene(bool saveScene)
        {
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui == null) return;

            if (ui.transform.Find("BoosterBar") == null)
            {
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Add Booster UI");
                ui.EnsureBoosterBar();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                if (saveScene) EditorSceneManager.SaveScene(activeScene);
                Debug.Log("[AddBoosterUIToScene] BoosterBar added to GameUI (existing UI layout preserved).");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Verify Merge Slam")]
        public static void VerifyMergeSlam()
        {
            Debug.Assert(BlockMergePuzzle.Utils.SpriteGenerator.GetRadialGlowSprite() != null, "RadialGlow sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.SpriteGenerator.GetStarFlareSprite() != null, "StarFlare sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.SpriteGenerator.GetBlockSprite() != null, "Block sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.SpriteGenerator.GetShardSprite() != null, "Shard sprite missing");
            Debug.Assert(Resources.Load<Shader>("Shaders/SpritePortalDissolve") != null, "SpritePortalDissolve shader missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetBoosterSprite("BG booster") != null, "BG booster sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetBoosterSprite("booster1") != null, "booster1 sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetBoosterSprite("booster2") != null, "booster2 sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetBoosterSprite("booster3") != null, "booster3 sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetBoosterSprite("lock") != null, "lock sprite missing");
            Debug.Assert(Resources.Load<AudioClip>("Sound/block_move") != null, "Sound/block_move clip missing");
            Debug.Assert(Resources.Load<AudioClip>("Sound/merge_block") != null, "Sound/merge_block clip missing");
            Debug.Assert(Resources.Load<AudioClip>("Sound/block_vanish") != null, "Sound/block_vanish clip missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Group 331") != null, "Group 331 setting sprite missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Group 333") != null, "Group 333 setting toggle off missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Group 334") != null, "Group 334 setting toggle on missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Vector 389") != null, "Vector 389 sound icon missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Vector 392") != null, "Vector 392 music icon missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("Vector 394") != null, "Vector 394 vibration icon missing");
            Debug.Assert(BlockMergePuzzle.Utils.GemSpriteManager.GetSettingSprite("X (1)") != null, "X (1) close icon missing");
            Debug.Assert(BlockMergePuzzle.Utils.SpriteGenerator.GetLevelBannerSprite() != null, "LevelBanner sprite missing");
            Debug.Log("[VerifyMergeSlam] PASS: merge flash, portal dissolve shader, booster sprites, setting UI & sounds verified.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Upgrade Settings Panel to New Style")]
        public static void UpgradeSettingsPanel()
        {
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Upgrade Settings Panel");
                ui.RebuildPausePanel();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log("[UpgradeSettingsPanel] Settings panel upgraded to new purple crystal style matching Image #11.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Update Level Banner & Settings Close X")]
        public static void UpdateLevelBannerAndCloseX()
        {
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Update Level Banner & Close X");
                Sprite bannerSprite = AutoMcpStarter.EnsureLevelBannerAsset();
                ui.RefreshPauseCloseSprite();
                ui.EnsureLevelBanner();

                Transform lvlTr = ui.transform.Find("HeaderPanel/Banner_Level");
                if (lvlTr != null && bannerSprite != null)
                {
                    var img = lvlTr.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) img.sprite = bannerSprite;
                }

                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log("[UpdateLevelBannerAndCloseX] Banner_Level added & Btn_Close updated to X (1).");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Ensure Time Text on Banner")]
        public static void EnsureTimeTextMenuItem()
        {
            AutoMcpStarter.EnsureTimeTextInOpenScene();
        }

        public static void EnsureWinSceneSpriteAssets()
        {
            const string winAssetsDir = "Assets/Sprites/Win&Lose scene/win scene/UI B1 (15)";
            if (System.IO.Directory.Exists(winAssetsDir))
            {
                string[] pngs = System.IO.Directory.GetFiles(winAssetsDir, "*.png", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string f in pngs)
                {
                    string assetPath = f.Replace("\\", "/");
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer != null && (importer.textureType != TextureImporterType.Sprite ||
                                             importer.spriteImportMode != SpriteImportMode.Single ||
                                             !importer.alphaIsTransparency ||
                                             importer.mipmapEnabled ||
                                             importer.maxTextureSize < 2048))
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.mipmapEnabled = false;
                        importer.maxTextureSize = 2048;
                        importer.SaveAndReimport();
                    }
                }
            }

            const string winUiDir = "Assets/Resources/Sprites/WinUI";
            if (!System.IO.Directory.Exists(winUiDir))
            {
                System.IO.Directory.CreateDirectory(winUiDir);
            }

            ExportProceduralWinSprite(winUiDir + "/win_radial_glow.png", BlockMergePuzzle.Utils.SpriteGenerator.GetRadialGlowSprite());
            ExportProceduralWinSprite(winUiDir + "/win_star_flare.png", BlockMergePuzzle.Utils.SpriteGenerator.GetStarFlareSprite());
            ExportProceduralWinSprite(winUiDir + "/win_shard.png", BlockMergePuzzle.Utils.SpriteGenerator.GetShardSprite());
            ExportProceduralWinSprite(winUiDir + "/win_progress_track.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinProgressTrackSprite());
            ExportProceduralWinSprite(winUiDir + "/win_progress_fill.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinProgressFillSprite());
            ExportProceduralWinSprite(winUiDir + "/win_progress_node.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinProgressNodeSprite());
            ExportProceduralWinSprite(winUiDir + "/win_speech_bubble.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinSpeechBubbleSprite());
            ExportProceduralWinSprite(winUiDir + "/win_sunburst.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinSunburstSprite());
            ExportProceduralWinSprite(winUiDir + "/win_shine_sweep.png", BlockMergePuzzle.Utils.SpriteGenerator.GetWinShineSweepSprite());
            AssetDatabase.Refresh();
        }

        private static void ExportProceduralWinSprite(string assetPath, Sprite sourceSprite)
        {
            if (sourceSprite == null || sourceSprite.texture == null) return;
            byte[] png = sourceSprite.texture.EncodeToPNG();
            if (png == null || png.Length == 0) return;

            bool needsWrite = !System.IO.File.Exists(assetPath);
            if (!needsWrite)
            {
                byte[] existing = System.IO.File.ReadAllBytes(assetPath);
                if (existing.Length != png.Length) needsWrite = true;
            }
            if (needsWrite)
            {
                System.IO.File.WriteAllBytes(assetPath, png);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite ||
                                     importer.spriteImportMode != SpriteImportMode.Single ||
                                     !importer.alphaIsTransparency ||
                                     importer.mipmapEnabled ||
                                     importer.filterMode != FilterMode.Bilinear))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Upgrade Win Screen (Active Scene)")]
        public static void UpgradeWinScreenActiveScene()
        {
            EnsureWinSceneSpriteAssets();
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Upgrade Win Screen");
                ui.RebuildVictoryPanel();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[UpgradeWinScreen] Upgraded VictoryPanel on {activeScene.name}.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Upgrade Win Screen (All 10 Levels + MainScene)")]
        public static void UpgradeWinScreenAllScenes()
        {
            EnsureWinSceneSpriteAssets();

            var startScene = EditorSceneManager.GetActiveScene();
            string originalScenePath = startScene.path;
            if (startScene.isDirty)
            {
                EditorSceneManager.SaveScene(startScene);
            }

            string[] scenePaths = new string[11];
            scenePaths[0] = "Assets/Scenes/MainScene.unity";
            for (int i = 1; i <= 10; i++)
            {
                scenePaths[i] = $"Assets/Scenes/Level{i}.unity";
            }

            int upgradedCount = 0;
            foreach (string path in scenePaths)
            {
                if (!System.IO.File.Exists(path)) continue;
                var opened = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameUI ui = FindObj<GameUI>();
                if (ui == null)
                {
                    GameObject uiObj = GameObject.Find("GameUI");
                    if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
                }
                if (ui != null)
                {
                    ui.RebuildVictoryPanel();
                    EditorSceneManager.MarkSceneDirty(opened);
                    EditorSceneManager.SaveScene(opened);
                    upgradedCount++;
                }
            }

            if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[UpgradeWinScreen] Upgraded VictoryPanel across {upgradedCount} scenes.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Toggle Win Screen Preview")]
        public static void ToggleWinScreenPreview()
        {
            EnsureWinSceneSpriteAssets();
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                ui.SetVictoryPreviewActive(!ui.IsVictoryShowing);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log($"[ToggleWinScreenPreview] VictoryPanel preview = {ui.IsVictoryShowing}");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Play Win Animation (Play Mode)")]
        public static void PlayWinAnimationInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[PlayWinAnimation] Please enter Play Mode first to preview the live Win Screen animation.");
                return;
            }
            GameUI ui = GameUI.Instance ?? FindObj<GameUI>();
            if (ui != null)
            {
                ui.ReplayVictoryAnimation();
                Debug.Log("[PlayWinAnimation] Replaying live Win Screen animation.");
            }
        }

        public static void EnsureLoseSceneSpriteAssets()
        {
            const string boosterDir = "Assets/Sprites/Main Scene/UI/booster";
            string[] boosterFiles = { "booster1.png", "booster2.png", "booster3.png" };
            foreach (string bf in boosterFiles)
            {
                string path = $"{boosterDir}/{bf}";
                TextureImporter bImp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (bImp != null && (bImp.textureType != TextureImporterType.Sprite ||
                                     bImp.spriteImportMode != SpriteImportMode.Single ||
                                     !bImp.alphaIsTransparency ||
                                     bImp.mipmapEnabled))
                {
                    bImp.textureType = TextureImporterType.Sprite;
                    bImp.spriteImportMode = SpriteImportMode.Single;
                    bImp.alphaIsTransparency = true;
                    bImp.mipmapEnabled = false;
                    bImp.SaveAndReimport();
                }
            }

            const string loseUiDir = "Assets/Resources/Sprites/LoseUI";
            if (!System.IO.Directory.Exists(loseUiDir))
            {
                System.IO.Directory.CreateDirectory(loseUiDir);
            }

            ExportProceduralLoseSprite(loseUiDir + "/lose_card_frame.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseCardFrameSprite(), new Vector4(68f, 68f, 68f, 68f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_header_banner.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseHeaderBannerSprite(), new Vector4(84f, 28f, 84f, 28f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_offer_card.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseOfferCardSprite(), new Vector4(48f, 48f, 48f, 48f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_best_offer_ribbon.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseBestOfferRibbonSprite(), new Vector4(22f, 16f, 36f, 16f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_btn_green.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseGreenButtonSprite(), new Vector4(42f, 42f, 42f, 42f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_resource_pill.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseResourcePillSprite(), new Vector4(38f, 26f, 38f, 26f));
            ExportProceduralLoseSprite(loseUiDir + "/lose_plus_badge.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLosePlusBadgeSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_stopwatch.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseStopwatchSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_heart_left.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseHeartLeftSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_heart_right.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseHeartRightSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_heart_small.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseHeartSmallSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_revive_wings.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLoseReviveWingsSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_planet_ringed.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLosePlanetRingedSprite(), Vector4.zero);
            ExportProceduralLoseSprite(loseUiDir + "/lose_planet_mini.png", BlockMergePuzzle.Utils.SpriteGenerator.GetLosePlanetMiniSprite(), Vector4.zero);

            AssetDatabase.Refresh();
        }

        private static void ExportProceduralLoseSprite(string assetPath, Sprite sourceSprite, Vector4 sliceBorder)
        {
            if (sourceSprite == null || sourceSprite.texture == null) return;
            byte[] png = sourceSprite.texture.EncodeToPNG();
            if (png == null || png.Length == 0) return;

            bool needsWrite = !System.IO.File.Exists(assetPath);
            if (!needsWrite)
            {
                byte[] existing = System.IO.File.ReadAllBytes(assetPath);
                if (existing.Length != png.Length) needsWrite = true;
            }
            if (needsWrite)
            {
                System.IO.File.WriteAllBytes(assetPath, png);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite ||
                                     importer.spriteImportMode != SpriteImportMode.Single ||
                                     !importer.alphaIsTransparency ||
                                     importer.mipmapEnabled ||
                                     importer.filterMode != FilterMode.Bilinear ||
                                     importer.spriteBorder != sliceBorder))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteBorder = sliceBorder;
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Upgrade Time Out Screen (Active Scene)")]
        public static void UpgradeTimeOutScreenActiveScene()
        {
            EnsureLoseSceneSpriteAssets();
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Upgrade Time Out Screen");
                ui.RebuildTimeOutPanel();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[UpgradeTimeOutScreen] Upgraded TimeOutPanel on {activeScene.name}.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Upgrade Time Out Screen (All 10 Levels + MainScene)")]
        public static void UpgradeTimeOutScreenAllScenes()
        {
            EnsureLoseSceneSpriteAssets();

            var startScene = EditorSceneManager.GetActiveScene();
            string originalScenePath = startScene.path;
            if (startScene.isDirty)
            {
                EditorSceneManager.SaveScene(startScene);
            }

            string[] scenePaths = new string[11];
            scenePaths[0] = "Assets/Scenes/MainScene.unity";
            for (int i = 1; i <= 10; i++)
            {
                scenePaths[i] = $"Assets/Scenes/Level{i}.unity";
            }

            int upgradedCount = 0;
            foreach (string path in scenePaths)
            {
                if (!System.IO.File.Exists(path)) continue;
                var opened = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameUI ui = FindObj<GameUI>();
                if (ui == null)
                {
                    GameObject uiObj = GameObject.Find("GameUI");
                    if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
                }
                if (ui != null)
                {
                    ui.RebuildTimeOutPanel();
                    EditorSceneManager.MarkSceneDirty(opened);
                    EditorSceneManager.SaveScene(opened);
                    upgradedCount++;
                }
            }

            if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[UpgradeTimeOutScreen] Upgraded TimeOutPanel across {upgradedCount} scenes.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Preview Time Out Popup (Edit Mode)")]
        public static void PreviewTimeOutPopupEditMode()
        {
            EnsureLoseSceneSpriteAssets();
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                ui.SetTimeOutPreviewActive(true, showFailedState: false);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log("[PreviewTimeOutPopup] Time Out! preview activated.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Preview Level Failed Popup (Edit Mode)")]
        public static void PreviewLevelFailedPopupEditMode()
        {
            EnsureLoseSceneSpriteAssets();
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                ui.SetTimeOutPreviewActive(true, showFailedState: true);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log("[PreviewLevelFailedPopup] Level Failed preview activated.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Hide Time Out Preview (Edit Mode)")]
        public static void HideTimeOutPreviewEditMode()
        {
            GameUI ui = FindObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = GameObject.Find("GameUI");
                if (uiObj != null) ui = uiObj.GetComponent<GameUI>();
            }
            if (ui != null)
            {
                ui.SetTimeOutPreviewActive(false);
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log("[HideTimeOutPreview] Time Out preview hidden and scene saved.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Play Time Out Animation (Play Mode)")]
        public static void PlayTimeOutAnimationInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[PlayTimeOutAnimation] Please enter Play Mode first to preview the live Time Out animation.");
                return;
            }
            GameUI ui = GameUI.Instance ?? FindObj<GameUI>();
            if (ui != null)
            {
                ui.ReplayTimeOutFlow();
                Debug.Log("[PlayTimeOutAnimation] Replaying live Time Out flow.");
            }
        }

        public static void EnsureHomeLevelFontAsset()
        {
            const string fontsDir = "Assets/Resources/Fonts";
            if (!System.IO.Directory.Exists(fontsDir))
            {
                System.IO.Directory.CreateDirectory(fontsDir);
            }

            const string targetPath = fontsDir + "/HomeLevelFont.ttf";
            if (!System.IO.File.Exists(targetPath))
            {
                string[] candidates =
                {
                    @"C:\Windows\Fonts\seguibl.ttf",
                    @"C:\Windows\Fonts\ariblk.ttf",
                    @"C:\Windows\Fonts\BRLNSB.TTF"
                };

                foreach (string src in candidates)
                {
                    if (System.IO.File.Exists(src))
                    {
                        System.IO.File.Copy(src, targetPath, overwrite: true);
                        AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"[HomeFont] Copied display font '{src}' to '{targetPath}'.");
                        break;
                    }
                }
            }
        }

        public static void EnsureHomeSceneSpriteAssets()
        {
            EnsureHomeLevelFontAsset();

            const string homeDir = "Assets/Sprites/Home scene/UI B1 (16)";
            if (System.IO.Directory.Exists(homeDir))
            {
                string[] pngs = System.IO.Directory.GetFiles(homeDir, "*.png", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string f in pngs)
                {
                    ConfigureSpriteImporter(f.Replace("\\", "/"), Vector4.zero, maxSize: 2048);
                }
            }

            const string bgDir = "Assets/Sprites/Background";
            if (System.IO.Directory.Exists(bgDir))
            {
                string[] bgPngs = System.IO.Directory.GetFiles(bgDir, "*.png", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string f in bgPngs)
                {
                    ConfigureSpriteImporter(f.Replace("\\", "/"), Vector4.zero, maxSize: 4096);
                }
            }

            const string homeUiDir = "Assets/Resources/Sprites/HomeUI";
            if (!System.IO.Directory.Exists(homeUiDir))
            {
                System.IO.Directory.CreateDirectory(homeUiDir);
            }

            ExportProceduralLoseSprite(homeUiDir + "/home_ladder_beam.png", BlockMergePuzzle.Utils.SpriteGenerator.GetHomeLadderBeamSprite(), Vector4.zero);
            ExportProceduralLoseSprite(homeUiDir + "/home_fb_icon.png", BlockMergePuzzle.Utils.SpriteGenerator.GetHomeFacebookIconSprite(), Vector4.zero);
            ExportProceduralLoseSprite(homeUiDir + "/home_badge_pill.png", BlockMergePuzzle.Utils.SpriteGenerator.GetHomeBadgePillSprite(), new Vector4(20f, 14f, 20f, 14f));
            EnsureRemoveAdsSpriteAssets();
            AssetDatabase.Refresh();
        }

        public static void EnsureRemoveAdsSpriteAssets()
        {
            const string removeAdsDir = "Assets/Resources/Sprites/RemoveAdsUI";
            if (!System.IO.Directory.Exists(removeAdsDir))
            {
                System.IO.Directory.CreateDirectory(removeAdsDir);
            }

            ExportProceduralLoseSprite(removeAdsDir + "/remove_ads_reward_bar.png", BlockMergePuzzle.Utils.SpriteGenerator.GetRemoveAdsRewardBarSprite(), new Vector4(48f, 48f, 48f, 48f));
            ExportProceduralLoseSprite(removeAdsDir + "/remove_ads_badge.png", BlockMergePuzzle.Utils.SpriteGenerator.GetRemoveAdsMiniBadgeSprite(), Vector4.zero);
            ExportProceduralLoseSprite(removeAdsDir + "/remove_ads_gold_btn.png", BlockMergePuzzle.Utils.SpriteGenerator.GetRemoveAdsGoldButtonSprite(), new Vector4(72f, 44f, 72f, 44f));
            ExportProceduralLoseSprite(removeAdsDir + "/remove_ads_shadow_blob.png", BlockMergePuzzle.Utils.SpriteGenerator.GetRemoveAdsShadowBlobSprite(), Vector4.zero);
        }

        public static string ImportRemoveAdsCardFromChromaGreen(string rawImagePath)
        {
            if (string.IsNullOrEmpty(rawImagePath) || !System.IO.File.Exists(rawImagePath))
            {
                Debug.LogError($"[ImportRemoveAdsCard] Source image not found: {rawImagePath}");
                return "Missing source image";
            }

            byte[] rawBytes = System.IO.File.ReadAllBytes(rawImagePath);
            Texture2D src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(src, rawBytes))
            {
                Object.DestroyImmediate(src);
                Debug.LogError("[ImportRemoveAdsCard] Failed to decode source image.");
                return "Decode failed";
            }

            int w = src.width;
            int h = src.height;
            Color32[] pixels = src.GetPixels32();
            bool[] isExterior = new bool[w * h];
            var queue = new System.Collections.Generic.Queue<int>(w * 4);

            bool IsChromaGreen(Color32 c)
            {
                int maxRB = c.r > c.b ? c.r : c.b;
                return c.g > 96 && (c.g - maxRB) > 36;
            }

            void EnqueueIfGreen(int x, int y)
            {
                int idx = y * w + x;
                if (!isExterior[idx] && IsChromaGreen(pixels[idx]))
                {
                    isExterior[idx] = true;
                    queue.Enqueue(idx);
                }
            }

            for (int x = 0; x < w; x++)
            {
                EnqueueIfGreen(x, 0);
                EnqueueIfGreen(x, h - 1);
            }
            for (int y = 1; y < h - 1; y++)
            {
                EnqueueIfGreen(0, y);
                EnqueueIfGreen(w - 1, y);
            }

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                int x = idx % w;
                int y = idx / w;
                if (x > 0) EnqueueIfGreen(x - 1, y);
                if (x < w - 1) EnqueueIfGreen(x + 1, y);
                if (y > 0) EnqueueIfGreen(x, y - 1);
                if (y < h - 1) EnqueueIfGreen(x, y + 1);
            }

            // Anti-alias edge feather & despill within 2px of transparent exterior
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (isExterior[idx])
                    {
                        pixels[idx] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    int adjExtCount = 0;
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= h) continue;
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= w) continue;
                            if (isExterior[ny * w + nx]) adjExtCount++;
                        }
                    }

                    if (adjExtCount > 0)
                    {
                        Color32 c = pixels[idx];
                        int maxRB = c.r > c.b ? c.r : c.b;
                        int greenExcess = c.g - maxRB;
                        if (greenExcess > 6)
                        {
                            c.g = (byte)maxRB;
                            if (greenExcess > 24 && adjExtCount >= 4)
                            {
                                c.a = (byte)Mathf.Clamp(255 - (greenExcess - 24) * 3, 24, 210);
                            }
                        }
                        if (adjExtCount >= 14)
                        {
                            c.a = (byte)Mathf.Min(c.a, 140);
                        }
                        pixels[idx] = c;
                    }
                }
            }

            // Compute tight opaque bounds with 4px padding
            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a > 8)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < minX || maxY < minY)
            {
                Object.DestroyImmediate(src);
                return "Empty crop bounds";
            }

            minX = Mathf.Max(0, minX - 4);
            minY = Mathf.Max(0, minY - 4);
            maxX = Mathf.Min(w - 1, maxX + 4);
            maxY = Mathf.Min(h - 1, maxY + 4);

            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;
            Color32[] cropped = new Color32[cropW * cropH];
            for (int cy = 0; cy < cropH; cy++)
            {
                System.Array.Copy(pixels, (minY + cy) * w + minX, cropped, cy * cropW, cropW);
            }

            Texture2D dst = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            dst.SetPixels32(cropped);
            dst.Apply();
            byte[] outPng = dst.EncodeToPNG();

            Object.DestroyImmediate(src);
            Object.DestroyImmediate(dst);

            const string removeAdsDir = "Assets/Resources/Sprites/RemoveAdsUI";
            if (!System.IO.Directory.Exists(removeAdsDir))
            {
                System.IO.Directory.CreateDirectory(removeAdsDir);
            }
            const string outAssetPath = removeAdsDir + "/remove_ads_card_bg.png";
            System.IO.File.WriteAllBytes(outAssetPath, outPng);
            AssetDatabase.ImportAsset(outAssetPath, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(outAssetPath, Vector4.zero, maxSize: 2048);

            Debug.Log($"[ImportRemoveAdsCard] Exported cropped transparent card ({cropW}x{cropH}) to {outAssetPath}");
            return $"OK ({cropW}x{cropH}) -> {outAssetPath}";
        }

        private static void ConfigureSpriteImporter(string assetPath, Vector4 sliceBorder, int maxSize = 2048)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                !importer.alphaIsTransparency ||
                importer.mipmapEnabled ||
                importer.filterMode != FilterMode.Bilinear ||
                importer.maxTextureSize < maxSize ||
                importer.spriteBorder != sliceBorder)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = maxSize;
                importer.spriteBorder = sliceBorder;
                importer.SaveAndReimport();
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Build & Open Home Scene")]
        public static void BuildAndOpenHomeScene()
        {
            EnsureHomeSceneSpriteAssets();
            EnsureWinSceneSpriteAssets();
            EnsureLoseSceneSpriteAssets();

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty && !string.IsNullOrEmpty(activeScene.path))
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            var homeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.05f, 0.18f);

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();

            GameObject smObj = new GameObject("SoundManager");
            smObj.AddComponent<SoundManager>();

            GameObject canvasObj = new GameObject("HomeCanvas");
            HomeUI homeUi = canvasObj.AddComponent<HomeUI>();
            homeUi.RebuildHomeUI();

            const string homeScenePath = "Assets/Scenes/HomeScene.unity";
            EditorSceneManager.SaveScene(homeScene, homeScenePath);

            // Register HomeScene.unity at Build Settings index 0
            var existingScenes = EditorBuildSettings.scenes;
            var newBuildList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            newBuildList.Add(new EditorBuildSettingsScene(homeScenePath, true));
            foreach (var s in existingScenes)
            {
                if (!string.Equals(s.path, homeScenePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    newBuildList.Add(s);
                }
            }
            EditorBuildSettings.scenes = newBuildList.ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildAndOpenHomeScene] Built & saved Assets/Scenes/HomeScene.unity and registered at Build Index 0.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Preview Home Difficulty - Easy (Lv 1)")]
        public static void PreviewHomeDifficultyEasy() => PreviewHomeDifficultyLevel(1);

        [MenuItem("Tools/Block Merge Puzzle/Preview Home Difficulty - Hard (Lv 6)")]
        public static void PreviewHomeDifficultyHard() => PreviewHomeDifficultyLevel(6);

        [MenuItem("Tools/Block Merge Puzzle/Preview Home Difficulty - Super Hard (Lv 15)")]
        public static void PreviewHomeDifficultySuperHard() => PreviewHomeDifficultyLevel(15);

        [MenuItem("Tools/Block Merge Puzzle/Preview Remove Ads Popup (Edit Mode)")]
        public static void PreviewRemoveAdsPopupEditMode()
        {
            EnsureHomeSceneSpriteAssets();
            EnsureLoseSceneSpriteAssets();
            EnsureWinSceneSpriteAssets();

            HomeUI homeUi = FindObj<HomeUI>();
            if (homeUi == null)
            {
                BuildAndOpenHomeScene();
                homeUi = FindObj<HomeUI>();
            }
            if (homeUi != null)
            {
                homeUi.SetRemoveAdsOpen(true, animate: false);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log("[PreviewRemoveAds] RemoveAdsPanel preview opened.");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Hide Remove Ads Preview (Edit Mode)")]
        public static void HideRemoveAdsPreviewEditMode()
        {
            HomeUI homeUi = FindObj<HomeUI>();
            if (homeUi != null)
            {
                homeUi.SetRemoveAdsOpen(false, animate: false);
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log("[HideRemoveAdsPreview] RemoveAdsPanel hidden and scene saved.");
            }
        }

        private static void PreviewHomeDifficultyLevel(int levelNumber)
        {
            HomeUI homeUi = FindObj<HomeUI>();
            if (homeUi == null)
            {
                BuildAndOpenHomeScene();
                homeUi = FindObj<HomeUI>();
            }
            if (homeUi != null)
            {
                Undo.RecordObject(homeUi, "Preview Home Difficulty");
                homeUi.SetPreviewLevel(levelNumber);
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                Debug.Log($"[PreviewHomeDifficulty] Previewing Level {levelNumber} ({HomeUI.GetLevelDifficulty(levelNumber)}).");
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Step Tutorial Verification")]
        public static void StepTutorialVerification()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[StepTutorialVerification] Must be in Play Mode to test tutorial transition.");
                return;
            }

            LevelManager lm = FindObj<LevelManager>();
            TutorialHandGuide guide = FindObj<TutorialHandGuide>();
            if (lm == null || guide == null)
            {
                Debug.LogError("[StepTutorialVerification] LevelManager or TutorialHandGuide not found!");
                return;
            }

            var pieces = lm.ActivePieces;
            if (pieces != null && pieces.Count >= 2)
            {
                Debug.Log($"[StepTutorialVerification] Phase before: {guide.CurrentPhase}. Merging piece 0 into piece 1...");
                lm.ExecuteMerge(pieces[0], pieces[1], 0.05f);
            }
            else if (pieces != null && pieces.Count == 1)
            {
                Debug.Log($"[StepTutorialVerification] Phase before: {guide.CurrentPhase}. Escaping piece through gate...");
                if (lm.Gates != null && lm.Gates.Count > 0)
                {
                    lm.ExecuteEscape(pieces[0], lm.Gates[0], 0.05f);
                }
            }
            else
            {
                Debug.Log($"[StepTutorialVerification] No pieces left. Phase before: {guide.CurrentPhase}. Restarting level...");
                GameUI ui = FindObj<GameUI>();
                if (ui != null) ui.RestartLevel();
            }
        }
    }

    [InitializeOnLoad]
    public static class AutoMcpStarter
    {
        static AutoMcpStarter()
        {
            EditorApplication.delayCall += EnsureMcpBridgeRunning;
            EditorApplication.delayCall += EnsureBoosterBarInOpenScene;
            EditorApplication.delayCall += EnsureSettingsPanelInOpenScene;
            EditorApplication.delayCall += EnsureLevelBannerAndCloseXInOpenScene;
            EditorApplication.delayCall += EnsureTimeTextInOpenScene;
        }

        public static void EnsureTimeTextInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            try
            {
                if (EditorUtility.audioMasterMute)
                {
                    EditorUtility.audioMasterMute = false;
                }
#if UNITY_2023_1_OR_NEWER
                GameUI ui = Object.FindAnyObjectByType<GameUI>();
#else
                GameUI ui = Object.FindObjectOfType<GameUI>();
#endif
                if (ui != null)
                {
                    Transform timeTr = ui.transform.Find("HeaderPanel/Banner_Time");
                    if (timeTr != null && timeTr.GetComponentInChildren<UnityEngine.UI.Text>(true) == null)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Auto Add Time Text");
                        ui.EnsureTimeText();
                        var scene = EditorSceneManager.GetActiveScene();
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        Debug.Log("[AutoTimeText] TimeVal text added to Banner_Time and saved.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AutoTimeText] {ex.Message}");
            }
        }

        public static Sprite EnsureLevelBannerAsset()
        {
            const string assetPath = "Assets/Sprites/Main Scene/UI/UI_Bn Level.png";
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null) return existing;

            Sprite proc = BlockMergePuzzle.Utils.SpriteGenerator.GetLevelBannerSprite();
            if (proc != null && proc.texture != null)
            {
                byte[] png = proc.texture.EncodeToPNG();
                if (png != null && png.Length > 0)
                {
                    System.IO.File.WriteAllBytes(assetPath, png);
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.alphaIsTransparency = true;
                        importer.mipmapEnabled = false;
                        importer.SaveAndReimport();
                    }
                    return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                }
            }
            return proc;
        }

        public static void EnsureLevelBannerAndCloseXInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("BMP_LevelBanner_CloseX_v1", false)) return;
            try
            {
#if UNITY_2023_1_OR_NEWER
                GameUI ui = Object.FindAnyObjectByType<GameUI>();
#else
                GameUI ui = Object.FindObjectOfType<GameUI>();
#endif
                if (ui != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Auto Add Level Banner & Update Close X");
                    Sprite bannerSprite = EnsureLevelBannerAsset();
                    ui.RefreshPauseCloseSprite();
                    ui.EnsureLevelBanner();

                    Transform lvlTr = ui.transform.Find("HeaderPanel/Banner_Level");
                    if (lvlTr != null && bannerSprite != null)
                    {
                        var img = lvlTr.GetComponent<UnityEngine.UI.Image>();
                        if (img != null) img.sprite = bannerSprite;
                    }

                    var scene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    EditorPrefs.SetBool("BMP_LevelBanner_CloseX_v1", true);
                    Debug.Log("[AutoLevelBanner] Banner_Level added below Banner_Time and Btn_Close updated to X (1).");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AutoLevelBanner] {ex.Message}");
            }
        }

        public static void EnsureSettingsPanelInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("BMP_SettingUI_Upgraded_v1", false)) return;
            try
            {
#if UNITY_2023_1_OR_NEWER
                GameUI ui = Object.FindAnyObjectByType<GameUI>();
#else
                GameUI ui = Object.FindObjectOfType<GameUI>();
#endif
                if (ui != null)
                {
                    Transform pause = ui.transform.Find("PausePanel");
                    if (pause != null)
                    {
                        Transform dialog = pause.Find("DialogBox");
                        if (dialog != null && (dialog.Find("TitleBanner") != null || dialog.Find("Btn_NoAds") != null))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Auto Upgrade Settings UI");
                            ui.RebuildPausePanel();
                            var scene = EditorSceneManager.GetActiveScene();
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                            EditorPrefs.SetBool("BMP_SettingUI_Upgraded_v1", true);
                            Debug.Log("[AutoUpgradeSettings] SettingsPanel automatically upgraded to new style.");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AutoUpgradeSettings] {ex.Message}");
            }
        }

        public static void EnsureBoosterBarInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool("BMP_BoosterUI_Added_v1", false)) return;
            try
            {
#if UNITY_2023_1_OR_NEWER
                GameUI ui = Object.FindAnyObjectByType<GameUI>();
#else
                GameUI ui = Object.FindObjectOfType<GameUI>();
#endif
                if (ui != null && ui.transform.Find("BoosterBar") == null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Auto Add Booster UI");
                    ui.EnsureBoosterBar();
                    var scene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    EditorPrefs.SetBool("BMP_BoosterUI_Added_v1", true);
                    Debug.Log("[AutoAddBooster] BoosterBar automatically added to active scene.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AutoAddBooster] {ex.Message}");
            }
        }

        public static async void EnsureMcpBridgeRunning()
        {
            try
            {
                var bridge = MCPServiceLocator.Bridge;
                if (bridge != null && !bridge.IsRunning)
                {
                    Debug.Log("[AutoMcpStarter] Auto-starting MCP Bridge...");
                    await bridge.StartAsync();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[AutoMcpStarter] {ex.Message}");
            }
        }
    }
}
#endif



