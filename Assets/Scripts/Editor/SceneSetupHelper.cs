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
                if (i == 1 || i == 2 || i == 3) continue; // Protect finalized Level 1, Level 2 and Level 3 scenes

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
            float targetW = lm != null ? lm.CurrentFrameWidth : GameConfig.FrameWidth;
            float targetH = lm != null ? lm.CurrentFrameHeight : GameConfig.FrameHeight;
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

            // Keep Level 1 & 2 pieces, clear unfinished residual blocks on other levels
            if (lm != null && lm.CurrentLevel != LevelId.Level1 && lm.CurrentLevel != LevelId.Level2)
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



