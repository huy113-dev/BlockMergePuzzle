using UnityEngine;
using UnityEngine.SceneManagement;
using BlockMergePuzzle.Audio;

namespace BlockMergePuzzle.Core
{
    /// <summary>
    /// Resets all saved gameplay progress whenever its host scene loads,
    /// then launches the configured start scene so the full game flow can be reviewed from the beginning.
    /// </summary>
    public class FreshGameLauncher : MonoBehaviour
    {
        [Tooltip("Scene khởi đầu sau khi reset toàn bộ tiến trình (mặc định HomeScene để xem tổng quan từ màn hình chính, hoặc đổi thành Level1 để vào thẳng màn 1).")]
        [SerializeField] private string _startSceneName = "HomeScene";

        [Tooltip("Nếu bật, xóa luôn cài đặt Âm thanh / Nhạc nền / Rung về mặc định bật.")]
        [SerializeField] private bool _resetAudioAndVibrationSettings = false;

        private static readonly string[] GameplayProgressKeys =
        {
            "BMP_CurrentLevel",
            "BMP_JustWonLevel",
            "BMP_TotalCoins",
            "BMP_Lives",
            "BMP_FbRewardClaimed",
            "BMP_NoAdsPurchased",
            "BMP_BoosterFreeze",
            "BMP_BoosterMagnet",
            "BMP_BoosterWand",
            "BMP_FreezeTutorialSeen",
            "BMP_MagnetTutorialSeen",
            "BMP_WandTutorialSeen",
            "BlockMerge_BestScore"
        };

        private static readonly string[] AudioAndVibrationKeys =
        {
            "BMP_SoundEnabled",
            "BMP_MusicEnabled",
            "BMP_VibrationEnabled"
        };

        private void Start()
        {
            ResetAllGameProgress(_resetAudioAndVibrationSettings);

            string target = string.IsNullOrWhiteSpace(_startSceneName) ? "HomeScene" : _startSceneName.Trim();
            SceneManager.LoadScene(target);
        }

        [ContextMenu("Reset Saved Game Progress Now")]
        private void ResetSavedGameProgressContextMenu()
        {
            ResetAllGameProgress(_resetAudioAndVibrationSettings);
        }

        /// <summary>
        /// Clears all saved gameplay progress keys and static runtime state back to a fresh new-player state.
        /// </summary>
        public static void ResetAllGameProgress(bool resetAudioAndVibrationSettings = false)
        {
            Time.timeScale = 1f;

            for (int i = 0; i < GameplayProgressKeys.Length; i++)
            {
                PlayerPrefs.DeleteKey(GameplayProgressKeys[i]);
            }

            if (resetAudioAndVibrationSettings)
            {
                for (int i = 0; i < AudioAndVibrationKeys.Length; i++)
                {
                    PlayerPrefs.DeleteKey(AudioAndVibrationKeys[i]);
                }
            }

            PlayerPrefs.Save();

            CustomLevelRepository.PendingRuntimeLevelNumber = 0;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ResetMergeCombo();
            }
        }
    }
}
