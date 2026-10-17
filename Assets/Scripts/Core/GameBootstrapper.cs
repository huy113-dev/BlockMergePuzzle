using UnityEngine;
using UnityEngine.EventSystems;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;

namespace BlockMergePuzzle.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        private static T FindObj<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapIfEmpty()
        {
            if (FindObj<GameBootstrapper>() == null && FindObj<LevelManager>() == null)
            {
                GameObject bootstrapperObj = new GameObject("GameBootstrapper");
                bootstrapperObj.AddComponent<GameBootstrapper>();
            }
        }

        private void Awake()
        {
            Application.runInBackground = true;
            SetupCamera();
            SetupEventSystem();
            SetupCoreSystems();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.8f;
            cam.transform.position = new Vector3(0f, 0.85f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.11f, 0.16f); // Deep dark slate background
        }

        private void SetupEventSystem()
        {
            if (FindObj<EventSystem>() == null && GameObject.Find("EventSystem") == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void SetupCoreSystems()
        {
            if (SoundManager.Instance == null && FindObj<SoundManager>() == null)
            {
                GameObject smObj = new GameObject("SoundManager");
                smObj.AddComponent<SoundManager>();
            }

            if (LevelManager.Instance == null && FindObj<LevelManager>() == null)
            {
                GameObject lmObj = new GameObject("LevelManager");
                lmObj.AddComponent<LevelManager>();
            }

            if (PlayerPieceController.Instance == null && FindObj<PlayerPieceController>() == null)
            {
                GameObject pcObj = new GameObject("PlayerPieceController");
                pcObj.AddComponent<PlayerPieceController>();
            }

            if (GameUI.Instance == null && FindObj<GameUI>() == null)
            {
                GameObject uiObj = new GameObject("GameUI");
                uiObj.AddComponent<GameUI>();
            }
        }
    }
}
