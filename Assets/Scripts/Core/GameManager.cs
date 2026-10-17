using UnityEngine;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.UI;

namespace BlockMergePuzzle.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }
        public bool IsGameOver { get; private set; }

        private const string BestScoreKey = "BlockMerge_BestScore";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        private void Start()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnScoreGained += AddScore;
            }

            StartNewGame();
        }

        public void StartNewGame()
        {
            IsGameOver = false;
            CurrentScore = 0;

            if (GameUI.Instance != null)
            {
                GameUI.Instance.HideGameOver();
                GameUI.Instance.UpdateScore(CurrentScore, BestScore);
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.InitializeBoard();
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.InitializeGame();
            }
        }

        public void RestartGame()
        {
            StartNewGame();
        }

        public void AddScore(int amount)
        {
            if (IsGameOver) return;

            CurrentScore += amount;
            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
                PlayerPrefs.Save();
            }

            if (GameUI.Instance != null)
            {
                GameUI.Instance.UpdateScore(CurrentScore, BestScore);
            }
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.InputEnabled = false;
            }

            SoundManager.Instance.PlayGameOver();

            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowGameOver(CurrentScore, BestScore);
            }
        }
    }
}
