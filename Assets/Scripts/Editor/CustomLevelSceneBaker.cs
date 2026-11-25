#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Editor
{
    [InitializeOnLoad]
    public static class CustomLevelSceneBaker
    {
        static CustomLevelSceneBaker()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && CustomLevelRepository.IsEditorPlayTestActive)
            {
                string returnScene = CustomLevelRepository.GetPlayTestReturnScene();
                CustomLevelRepository.EndEditorPlayTest();

                if (!string.IsNullOrEmpty(returnScene) && File.Exists(returnScene))
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                            EditorSceneManager.GetActiveScene().path != returnScene)
                        {
                            EditorSceneManager.OpenScene(returnScene, OpenSceneMode.Single);
                        }
                    };
                }
            }
        }

        [MenuItem("Tools/Block Merge Puzzle/Open Web Level Editor (HTML)")]
        public static void OpenWebLevelEditor()
        {
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Tools/LevelEditor.html"));
            if (!File.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("Không tìm thấy Tool Web",
                    $"Không tìm thấy file:\n{fullPath}", "OK");
                return;
            }
            string url = "file:///" + fullPath.Replace("\\", "/");
            Application.OpenURL(url);
        }

        public static void StartPlayTest(CustomLevelData data)
        {
            if (data == null) return;
            data.Normalize();

            if (EditorApplication.isPlaying)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.LoadCustomLevel(data, true);
                    return;
                }
                EditorUtility.DisplayDialog("Đang ở Play Mode",
                    "Hãy dừng Play Mode hiện tại hoặc mở MainScene trước khi Play Test.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string returnScene = EditorSceneManager.GetActiveScene().path;
            const string mainScenePath = "Assets/Scenes/MainScene.unity";
            if (!File.Exists(mainScenePath))
            {
                EditorUtility.DisplayDialog("Thiếu MainScene",
                    $"Không tìm thấy {mainScenePath} để chạy Play Test.", "OK");
                return;
            }

            if (EditorSceneManager.GetActiveScene().path != mainScenePath)
            {
                EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
            }

            CustomLevelRepository.BeginEditorPlayTest(data, returnScene);
            EditorApplication.isPlaying = true;
        }

        public static bool BakeCustomLevelToSceneAndPrefab(CustomLevelData data, bool showDialog = true)
        {
            if (data == null) return false;
            data.Normalize();

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("Đang ở Play Mode", "Vui lòng thoát Play Mode trước khi Bake Scene & Prefab.", "OK");
                }
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            string originalScenePath = EditorSceneManager.GetActiveScene().path;

            TextAsset jsonAsset = CustomLevelRepository.SaveLevelJson(data);
            string mapAssetPath = $"Assets/Resources/Sprites/CustomMaps/map_custom_lv{data.levelNumber}.png";
            Sprite mapSprite = ProceduralBoardRenderer.SaveBoardSpriteAsset(data, mapAssetPath, 128);

            SceneSetupHelper.EnsureWinSceneSpriteAssets();
            SceneSetupHelper.EnsureLoseSceneSpriteAssets();

            const string mainTemplatePath = "Assets/Scenes/MainScene.unity";
            const string level10TemplatePath = "Assets/Scenes/Level10.unity";
            string templatePath = File.Exists(mainTemplatePath)
                ? mainTemplatePath
                : (File.Exists(level10TemplatePath) ? level10TemplatePath : null);

            bool usedTemplate = !string.IsNullOrEmpty(templatePath);
            var scene = usedTemplate
                ? EditorSceneManager.OpenScene(templatePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = FindSceneObj<Camera>() ?? Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
                cam.orthographic = true;
                cam.orthographicSize = 7.5f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.10f, 0.11f, 0.16f);
            }

            if (FindSceneObj<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            if (FindSceneObj<SoundManager>() == null)
            {
                GameObject smObj = new GameObject("SoundManager");
                smObj.AddComponent<SoundManager>();
            }

            if (FindSceneObj<PlayerPieceController>() == null)
            {
                GameObject pcObj = new GameObject("PlayerPieceController");
                pcObj.AddComponent<PlayerPieceController>();
            }

            LevelManager lm = FindSceneObj<LevelManager>();
            if (lm == null)
            {
                GameObject lmObj = new GameObject("LevelManager");
                lm = lmObj.AddComponent<LevelManager>();
            }

            GameUI ui = FindSceneObj<GameUI>();
            if (ui == null)
            {
                GameObject uiObj = new GameObject("GameUI");
                ui = uiObj.AddComponent<GameUI>();
            }

            lm.BakeCustomLevelInEditMode(data, mapSprite, jsonAsset);

            if (ui.transform.childCount == 0)
            {
                ui.RebuildUI();
            }
            ui.EnsureBoosterBar();
            ui.EnsureLevelBanner();
            ui.EnsureTimeText();
            ui.SetPlayDurationSeconds(data.timeSeconds);
            ui.UpdateLevelDisplay();
            ui.UpdateBoosterUnlockStates();

            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                if (rootGo != null && rootGo.name == "TutorialHand_World")
                {
                    rootGo.SetActive(false);
                }
            }
            Transform tutorialBannerTr = ui.transform.Find("TutorialLayer/InstructionBanner");
            if (tutorialBannerTr != null)
            {
                tutorialBannerTr.gameObject.SetActive(false);
            }

            // Save Prefab matching Level1..10 convention
            const string prefabsDir = "Assets/Prefabs/Levels";
            if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

            Transform boardFrame = lm.transform.Find("Board_Frame");
            Transform exitGates = lm.transform.Find("Exit_Gates");
            Transform activePieces = lm.transform.Find("Active_Pieces");
            if (boardFrame != null && exitGates != null && activePieces != null)
            {
                GameObject levelRoot = new GameObject($"Level{data.levelNumber}_Board");
                Object.Instantiate(boardFrame.gameObject, levelRoot.transform).name = "Board_Frame";
                Object.Instantiate(exitGates.gameObject, levelRoot.transform).name = "Exit_Gates";
                Object.Instantiate(activePieces.gameObject, levelRoot.transform).name = "Active_Pieces";

                string prefabPath = $"{prefabsDir}/Level{data.levelNumber}_Board.prefab";
                PrefabUtility.SaveAsPrefabAsset(levelRoot, prefabPath);
                Object.DestroyImmediate(levelRoot);
            }

            const string scenesDir = "Assets/Scenes";
            if (!Directory.Exists(scenesDir)) Directory.CreateDirectory(scenesDir);
            string scenePath = $"{scenesDir}/Level{data.levelNumber}.unity";
            EditorSceneManager.SaveScene(scene, scenePath, usedTemplate);
            AssetDatabase.ImportAsset(scenePath);

            RegisterSceneInBuildSettings(scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string reopenPath = (!string.IsNullOrEmpty(originalScenePath) && File.Exists(originalScenePath))
                ? originalScenePath
                : scenePath;
            EditorSceneManager.OpenScene(reopenPath, OpenSceneMode.Single);

            Debug.Log($"[CustomLevelSceneBaker] Baked Level {data.levelNumber} -> {scenePath}");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Bake Thành Công",
                    $"Đã lưu JSON + tạo Scene & Prefab cho Level {data.levelNumber}:\n• {scenePath}\n• Assets/Prefabs/Levels/Level{data.levelNumber}_Board.prefab",
                    "OK");
            }
            return true;
        }

        private static T FindSceneObj<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var existing = EditorBuildSettings.scenes;
            for (int i = 0; i < existing.Length; i++)
            {
                if (string.Equals(existing[i].path, scenePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!existing[i].enabled)
                    {
                        existing[i].enabled = true;
                        EditorBuildSettings.scenes = existing;
                    }
                    return;
                }
            }

            var updated = new List<EditorBuildSettingsScene>(existing)
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = updated.ToArray();
        }
    }
}
#endif
