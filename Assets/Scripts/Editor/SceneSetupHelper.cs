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

        [MenuItem("Tools/Block Merge Puzzle/Load Level 1 (Tutorial Cross)")]
        public static void LoadLevel1()
        {
            LevelManager lm = FindObj<LevelManager>();
            if (lm != null)
            {
                lm.CurrentLevel = LevelId.Level1_TutorialCross;
            }
            BakeLevelToScene();
        }

        [MenuItem("Tools/Block Merge Puzzle/Load Level 2 (Reference 6x8)")]
        public static void LoadLevel2()
        {
            LevelManager lm = FindObj<LevelManager>();
            if (lm != null)
            {
                lm.CurrentLevel = LevelId.Level2_ReferenceBoard;
            }
            BakeLevelToScene();
        }

        [MenuItem("Tools/Block Merge Puzzle/Bake Level to Scene (Idle Display)")]
        public static void BakeLevelToScene()
        {
            // 1. Level Manager & Baked Level first to determine frame size
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

            // 2. Camera
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
            float paddingX = 0.55f;
            float paddingY = 0.85f;
            float targetW = lm != null ? lm.CurrentFrameWidth : GameConfig.FrameWidth;
            float targetH = lm != null ? lm.CurrentFrameHeight : GameConfig.FrameHeight;
            float sizeByWidth = (targetW / 2f + paddingX) / screenRatio;
            float sizeByHeight = targetH / 2f + paddingY;

            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.11f, 0.16f);

            // 3. Event System
            if (FindObj<EventSystem>() == null && GameObject.Find("EventSystem") == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            // 4. Audio & Control Singletons
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

            if (FindObj<GameUI>() == null && GameObject.Find("GameUI") == null)
            {
                GameObject ui = new GameObject("GameUI");
                ui.AddComponent<GameUI>();
                Undo.RegisterCreatedObjectUndo(ui, "Create GameUI");
            }

            lm.BakeEditModeLevel();

            // 5. Mark Scene Dirty and Save
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[Block Merge Puzzle] Level {lm.CurrentLevel} baked successfully to scene {activeScene.name}! Board, gates and blocks are now visible in idle Edit Mode.");
        }

        [MenuItem("Tools/Block Merge Puzzle/Setup Scene Now")]
        public static void SetupScene()
        {
            BakeLevelToScene();
        }
    }

    [InitializeOnLoad]
    public static class AutoMcpStarter
    {
        static AutoMcpStarter()
        {
            EditorApplication.delayCall += EnsureMcpBridgeRunning;
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
