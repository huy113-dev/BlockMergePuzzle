using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;

namespace BlockMergePuzzle.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        private Canvas _canvas;
        private Text _scoreText;
        private Text _gatesText;
        private GameObject _victoryPanel;

        // Feedback error banner components
        private GameObject _feedbackPanel;
        private Text _feedbackText;
        private CanvasGroup _feedbackCanvasGroup;
        private Coroutine _feedbackCoroutine;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUIHierarchy();
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnScoreChanged += UpdateScore;
                LevelManager.Instance.OnRemainingBlocksChanged += UpdateRemainingBlocks;
                UpdateScore(LevelManager.Instance.Score);
                UpdateRemainingBlocks(LevelManager.Instance.RemainingBlocksCount);
            }
        }

        private void BuildUIHierarchy()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Top Header Panel - Positioned cleanly at top of screen without obscuring upper exit gates
            GameObject header = CreateUIPanel("HeaderPanel", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1000f, 120f));

            // Remaining Blocks Box
            GameObject gatesBox = CreateUIPanel("GatesBox", header.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(190f, 0f), new Vector2(320f, 110f));
            SetImageColor(gatesBox, new Color(0.14f, 0.16f, 0.24f, 0.92f));
            CreateUIText("GatesLabel", gatesBox.transform, "CÒN LẠI", defaultFont, 22, TextAnchor.UpperCenter, new Vector2(0f, -12f), new Color(0.7f, 0.8f, 1f));
            _gatesText = CreateUIText("GatesVal", gatesBox.transform, "21 KHỐI", defaultFont, 32, TextAnchor.MiddleCenter, new Vector2(0f, -16f), new Color(0.4f, 0.95f, 0.6f));

            // Score Box
            GameObject scoreBox = CreateUIPanel("ScoreBox", header.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(260f, 110f));
            SetImageColor(scoreBox, new Color(0.14f, 0.16f, 0.24f, 0.92f));
            CreateUIText("ScoreLabel", scoreBox.transform, "ĐIỂM", defaultFont, 22, TextAnchor.UpperCenter, new Vector2(0f, -12f), Color.gray);
            _scoreText = CreateUIText("ScoreVal", scoreBox.transform, "0", defaultFont, 38, TextAnchor.MiddleCenter, new Vector2(0f, -16f), Color.white);

            // Restart Button
            GameObject restartBtnObj = CreateUIPanel("RestartBtn", header.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(180f, 110f));
            SetImageColor(restartBtnObj, new Color(0.24f, 0.30f, 0.42f, 0.95f));
            CreateUIText("RestartText", restartBtnObj.transform, "CHƠI LẠI", defaultFont, 24, TextAnchor.MiddleCenter, Vector2.zero, Color.white);
            Button restartBtn = restartBtnObj.AddComponent<Button>();
            restartBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayButton();
                LevelManager.Instance.InitializeLevel();
            });

            // 2. Red Error Feedback Banner (Bottom Screen)
            BuildFeedbackBanner(defaultFont);

            // 3. Victory Panel
            BuildVictoryPanel(defaultFont);
        }

        private void BuildFeedbackBanner(Font font)
        {
            // Positioned at bottom center
            _feedbackPanel = CreateUIPanel("FeedbackBanner", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(960f, 110f));
            SetImageColor(_feedbackPanel, new Color(0.12f, 0.04f, 0.06f, 0.95f));

            // Soft glowing red border outline
            Outline outline = _feedbackPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.9f, 0.18f, 0.22f, 0.75f);
            outline.effectDistance = new Vector2(3f, -3f);

            _feedbackCanvasGroup = _feedbackPanel.AddComponent<CanvasGroup>();
            _feedbackCanvasGroup.alpha = 0f;
            _feedbackCanvasGroup.blocksRaycasts = false;

            _feedbackText = CreateUIText(
                "FeedbackText",
                _feedbackPanel.transform,
                "",
                font,
                29,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                new Color(1f, 0.32f, 0.32f, 1f));
        }

        private void BuildVictoryPanel(Font font)
        {
            _victoryPanel = CreateUIPanel("VictoryPanel", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850f, 650f));
            SetImageColor(_victoryPanel, new Color(0.08f, 0.10f, 0.16f, 0.98f));

            Outline outline = _victoryPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.8f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(4f, -4f);

            CreateUIText("Title", _victoryPanel.transform, "🎉 CHIẾN THẮNG!", font, 52, TextAnchor.UpperCenter, new Vector2(0f, -60f), new Color(1f, 0.88f, 0.25f));
            CreateUIText("Subtitle", _victoryPanel.transform, "Bạn đã giải phóng toàn bộ các cổng!", font, 28, TextAnchor.MiddleCenter, new Vector2(0f, 30f), Color.white);

            GameObject playAgainBtn = CreateUIPanel("PlayAgainBtn", _victoryPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(360f, 110f));
            SetImageColor(playAgainBtn, new Color(0.18f, 0.65f, 0.35f, 1f));
            CreateUIText("BtnText", playAgainBtn.transform, "CHƠI LẠI", font, 36, TextAnchor.MiddleCenter, Vector2.zero, Color.white);

            Button btn = playAgainBtn.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayButton();
                _victoryPanel.SetActive(false);
                LevelManager.Instance.InitializeLevel();
            });

            _victoryPanel.SetActive(false);
        }

        public void ShowFeedbackError(string message)
        {
            if (_feedbackPanel == null || _feedbackText == null) return;

            _feedbackText.text = "⚠️ " + message;

            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
            }
            _feedbackCoroutine = StartCoroutine(CoAnimateFeedback());
        }

        private IEnumerator CoAnimateFeedback()
        {
            _feedbackPanel.SetActive(true);
            _feedbackCanvasGroup.alpha = 1f;

            // Punch scale animation
            float elapsed = 0f;
            float punchDuration = 0.20f;
            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / punchDuration;
                float scale = 1.0f + Mathf.Sin(t * Mathf.PI) * 0.10f;
                _feedbackPanel.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            _feedbackPanel.transform.localScale = Vector3.one;

            // Wait visible
            yield return new WaitForSeconds(2.4f);

            // Fade out
            float fadeElapsed = 0f;
            float fadeDuration = 0.35f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                _feedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
                yield return null;
            }

            _feedbackCanvasGroup.alpha = 0f;
            _feedbackCoroutine = null;
        }

        public void ShowFloatingText(string text, Vector3 worldPos, Color color)
        {
            StartCoroutine(CoFloatingText(text, worldPos, color));
        }

        private IEnumerator CoFloatingText(string text, Vector3 worldPos, Color color)
        {
            GameObject floatObj = new GameObject("FloatingText");
            floatObj.transform.position = worldPos + new Vector3(0f, 0.3f, -1f);

            TextMesh tm = floatObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 48;
            tm.characterSize = 0.075f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = floatObj.GetComponent<MeshRenderer>();
            mr.sortingOrder = 150;

            float duration = 0.85f;
            float elapsed = 0f;
            Vector3 start = floatObj.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                floatObj.transform.position = start + new Vector3(0f, t * 1.2f, 0f);

                Color c = color;
                c.a = 1f - (t * t);
                tm.color = c;
                yield return null;
            }

            Destroy(floatObj);
        }

        public void UpdateScore(int newScore)
        {
            if (_scoreText != null) _scoreText.text = newScore.ToString();
        }

        public void UpdateScore(int currentScore, int bestScore)
        {
            UpdateScore(currentScore);
        }

        public void UpdateRemainingBlocks(int count)
        {
            if (_gatesText != null) _gatesText.text = $"{count} KHỐI";
        }

        public void UpdateProgress(int escaped, int total)
        {
            if (_gatesText != null) _gatesText.text = $"{total - escaped} KHỐI";
        }

        public void ShowVictoryPopup()
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(true);
        }

        public void ShowGameOver(int score, int bestScore)
        {
            ShowVictoryPopup();
        }

        public void HideGameOver()
        {
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
        }

        private GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            obj.AddComponent<Image>();
            return obj;
        }

        private Text CreateUIText(string name, Transform parent, string text, Font font, int fontSize, TextAnchor anchor, Vector2 anchoredPos, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = Vector2.zero;

            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color;
            t.raycastTarget = false;
            return t;
        }

        private void SetImageColor(GameObject obj, Color color)
        {
            Image img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }
        }
    }
}
