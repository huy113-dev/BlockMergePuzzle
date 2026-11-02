using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        private Canvas _canvas;
        private CanvasScaler _scaler;

        // Top Header matching design [Image #5]
        private GameObject _headerPanel;
        private Image _returnButtonImg;
        private Image _timeBannerImg;
        private Image _pauseButtonImg;
        private Text _timeText;

        // Timer state
        private const float DefaultLevelTime = 100f; // 1:40 default
        private float _timeRemaining = DefaultLevelTime;
        private bool _isTimerRunning = false;
        private bool _isPaused = false;

        // Modals
        private GameObject _pausePanel;
        private GameObject _victoryPanel;
        private GameObject _feedbackPanel;
        private Text _feedbackText;
        private CanvasGroup _feedbackCanvasGroup;
        private Coroutine _feedbackCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
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
                LevelManager.Instance.OnRemainingBlocksChanged += HandleRemainingBlocksChanged;
            }

            StartTimer();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (_isTimerRunning && !_isPaused)
            {
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = 0f;
                    _isTimerRunning = false;
                    UpdateTimeDisplay();
                    OnTimeExpired();
                }
                else
                {
                    UpdateTimeDisplay();
                }
            }
        }

        [ContextMenu("Rebuild UI Hierarchy")]
        public void RebuildUI()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }

            BuildUIHierarchy();
        }

        private void BuildUIHierarchy()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;
            _canvas.planeDistance = 5f;
            _canvas.sortingOrder = 100;

            _scaler = GetComponent<CanvasScaler>();
            if (_scaler == null) _scaler = gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1080, 1920);
            _scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Top Header Bar (Return, Time 1:40, Pause)
            BuildTopHeader(font);

            // 2. Pause Modal
            BuildPausePanel(font);

            // 3. Victory Modal
            BuildVictoryPanel(font);

            // 4. Feedback Banner
            BuildFeedbackBanner(font);

            UpdateTimeDisplay();
        }

        private void BuildTopHeader(Font font)
        {
            // Container anchored to Top Center
            _headerPanel = new GameObject("HeaderPanel");
            _headerPanel.transform.SetParent(transform, false);

            RectTransform headerRect = _headerPanel.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -80f);
            headerRect.sizeDelta = new Vector2(1000f, 180f);

            // a. Return Button (Left: x = -340)
            GameObject returnObj = new GameObject("Btn_Return");
            returnObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform retRect = returnObj.AddComponent<RectTransform>();
            retRect.anchorMin = new Vector2(0.5f, 0.5f);
            retRect.anchorMax = new Vector2(0.5f, 0.5f);
            retRect.pivot = new Vector2(0.5f, 0.5f);
            retRect.anchoredPosition = new Vector2(-340f, 0f);
            retRect.sizeDelta = new Vector2(145f, 145f);

            _returnButtonImg = returnObj.AddComponent<Image>();
            _returnButtonImg.sprite = GemSpriteManager.GetSprite("Return");
            _returnButtonImg.preserveAspect = true;

            Button returnBtn = returnObj.AddComponent<Button>();
            returnBtn.targetGraphic = _returnButtonImg;
            returnBtn.onClick.AddListener(OnReturnClicked);

            // b. Time Banner (Center: x = 0)
            GameObject timeObj = new GameObject("Banner_Time");
            timeObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.5f);
            timeRect.pivot = new Vector2(0.5f, 0.5f);
            timeRect.anchoredPosition = new Vector2(0f, 0f);
            timeRect.sizeDelta = new Vector2(360f, 157f);

            _timeBannerImg = timeObj.AddComponent<Image>();
            _timeBannerImg.sprite = GemSpriteManager.GetSprite("Time");
            _timeBannerImg.preserveAspect = true;

            // Dark inner patch overlay over the "1:40" text area in the sprite
            GameObject patchObj = new GameObject("TimeTextPatch");
            patchObj.transform.SetParent(timeObj.transform, false);
            RectTransform patchRect = patchObj.AddComponent<RectTransform>();
            patchRect.anchorMin = new Vector2(0.5f, 0.5f);
            patchRect.anchorMax = new Vector2(0.5f, 0.5f);
            patchRect.pivot = new Vector2(0.5f, 0.5f);
            patchRect.anchoredPosition = new Vector2(34f, 2f);
            patchRect.sizeDelta = new Vector2(160f, 60f);

            Image patchImg = patchObj.AddComponent<Image>();
            patchImg.color = new Color(0.082f, 0.094f, 0.278f, 1f); // #151847 matching inner navy
            patchImg.raycastTarget = false;

            // Live Time Text
            GameObject textObj = new GameObject("TimeVal");
            textObj.transform.SetParent(patchObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _timeText = textObj.AddComponent<Text>();
            _timeText.text = "1:40";
            _timeText.font = font;
            _timeText.fontSize = 46;
            _timeText.fontStyle = FontStyle.Bold;
            _timeText.alignment = TextAnchor.MiddleCenter;
            _timeText.color = new Color(1f, 0.96f, 0.92f); // Cream white matching sprite font
            _timeText.raycastTarget = false;

            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // c. Pause Button (Right: x = +340)
            GameObject pauseObj = new GameObject("Btn_Pause");
            pauseObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform pauseRect = pauseObj.AddComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(0.5f, 0.5f);
            pauseRect.anchorMax = new Vector2(0.5f, 0.5f);
            pauseRect.pivot = new Vector2(0.5f, 0.5f);
            pauseRect.anchoredPosition = new Vector2(340f, 0f);
            pauseRect.sizeDelta = new Vector2(145f, 145f);

            _pauseButtonImg = pauseObj.AddComponent<Image>();
            _pauseButtonImg.sprite = GemSpriteManager.GetSprite("Pause");
            _pauseButtonImg.preserveAspect = true;

            Button pauseBtn = pauseObj.AddComponent<Button>();
            pauseBtn.targetGraphic = _pauseButtonImg;
            pauseBtn.onClick.AddListener(OnPauseClicked);
        }

        private void BuildPausePanel(Font font)
        {
            _pausePanel = CreateUIPanel("PausePanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetImageColor(_pausePanel, new Color(0.04f, 0.05f, 0.10f, 0.82f));

            // Dialog Window
            GameObject dialog = CreateUIPanel("DialogBox", _pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 520f));
            SetImageColor(dialog, new Color(0.10f, 0.12f, 0.22f, 0.98f));

            Outline outline = dialog.AddComponent<Outline>();
            outline.effectColor = new Color(0.20f, 0.65f, 1f, 0.85f);
            outline.effectDistance = new Vector2(3.5f, -3.5f);

            // Title
            CreateUIText("Title", dialog.transform, "⏸️ TẠM DỪNG", font, 48, TextAnchor.UpperCenter, new Vector2(0f, -50f), new Color(0.44f, 0.84f, 1f));

            // Resume Button
            GameObject resumeBtn = CreateUIPanel("ResumeBtn", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(380f, 95f));
            SetImageColor(resumeBtn, new Color(0.18f, 0.70f, 0.38f, 1f));
            CreateUIText("Text", resumeBtn.transform, "TIẾP TỤC", font, 32, TextAnchor.MiddleCenter, Vector2.zero, Color.white);

            Button rBtn = resumeBtn.AddComponent<Button>();
            rBtn.onClick.AddListener(ResumeGame);

            // Restart Button
            GameObject restartBtn = CreateUIPanel("RestartBtn", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(380f, 95f));
            SetImageColor(restartBtn, new Color(0.25f, 0.45f, 0.85f, 1f));
            CreateUIText("Text", restartBtn.transform, "CHƠI LẠI", font, 32, TextAnchor.MiddleCenter, Vector2.zero, Color.white);

            Button reBtn = restartBtn.AddComponent<Button>();
            reBtn.onClick.AddListener(() =>
            {
                ResumeGame();
                RestartLevel();
            });

            _pausePanel.SetActive(false);
        }

        private void BuildVictoryPanel(Font font)
        {
            _victoryPanel = CreateUIPanel("VictoryPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetImageColor(_victoryPanel, new Color(0.04f, 0.05f, 0.10f, 0.85f));

            GameObject dialog = CreateUIPanel("DialogBox", _victoryPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 580f));
            SetImageColor(dialog, new Color(0.10f, 0.12f, 0.22f, 0.98f));

            Outline outline = dialog.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.8f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(4f, -4f);

            CreateUIText("Title", dialog.transform, "🎉 CHIẾN THẮNG!", font, 52, TextAnchor.UpperCenter, new Vector2(0f, -60f), new Color(1f, 0.88f, 0.25f));
            CreateUIText("Subtitle", dialog.transform, "Bạn đã giải phóng toàn bộ các cổng!", font, 28, TextAnchor.MiddleCenter, new Vector2(0f, 30f), Color.white);

            GameObject playAgainBtn = CreateUIPanel("PlayAgainBtn", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(380f, 100f));
            SetImageColor(playAgainBtn, new Color(0.18f, 0.65f, 0.35f, 1f));
            CreateUIText("BtnText", playAgainBtn.transform, "CHƠI LẠI", font, 34, TextAnchor.MiddleCenter, Vector2.zero, Color.white);

            Button btn = playAgainBtn.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
                _victoryPanel.SetActive(false);
                RestartLevel();
            });

            _victoryPanel.SetActive(false);
        }

        private void BuildFeedbackBanner(Font font)
        {
            _feedbackPanel = CreateUIPanel("FeedbackBanner", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(960f, 110f));
            SetImageColor(_feedbackPanel, new Color(0.12f, 0.04f, 0.06f, 0.95f));

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

        public void StartTimer()
        {
            _timeRemaining = DefaultLevelTime;
            _isTimerRunning = true;
            _isPaused = false;
            UpdateTimeDisplay();
        }

        public void StopTimer()
        {
            _isTimerRunning = false;
        }

        public void ResetTimer()
        {
            _timeRemaining = DefaultLevelTime;
            _isTimerRunning = true;
            _isPaused = false;
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            if (_timeText == null) return;
            int mins = Mathf.FloorToInt(_timeRemaining / 60f);
            int secs = Mathf.FloorToInt(_timeRemaining % 60f);
            _timeText.text = string.Format("{0}:{1:D2}", mins, secs);

            if (_timeRemaining < 20f && _timeRemaining > 0f)
            {
                _timeText.color = new Color(1f, 0.35f, 0.35f); // Red warning when low on time
            }
            else
            {
                _timeText.color = new Color(1f, 0.96f, 0.92f);
            }
        }

        private void OnTimeExpired()
        {
            ShowFeedbackError("Hết giờ! Hãy thử lại.");
        }

        public void OnReturnClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_returnButtonImg != null) StartCoroutine(CoButtonPunch(_returnButtonImg.transform));
            RestartLevel();
        }

        public void OnPauseClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_pauseButtonImg != null) StartCoroutine(CoButtonPunch(_pauseButtonImg.transform));
            TogglePause();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(_isPaused);
            }
            Time.timeScale = _isPaused ? 0f : 1f;
        }

        public void ResumeGame()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            _isPaused = false;
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        public void RestartLevel()
        {
            ResetTimer();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.InitializeLevel();
            }
        }

        private IEnumerator CoButtonPunch(Transform tr)
        {
            float elapsed = 0f;
            float dur = 0.14f;
            Vector3 original = Vector3.one;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / dur;
                float s = 1.0f - Mathf.Sin(t * Mathf.PI) * 0.12f;
                tr.localScale = original * s;
                yield return null;
            }
            tr.localScale = original;
        }

        private void HandleRemainingBlocksChanged(int count)
        {
            if (count == 0)
            {
                StopTimer();
                ShowVictoryPopup();
            }
        }

        public void ShowFeedbackError(string message)
        {
            if (_feedbackPanel == null || _feedbackText == null) return;
            _feedbackText.text = "⚠️ " + message;

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(CoAnimateFeedback());
        }

        private IEnumerator CoAnimateFeedback()
        {
            _feedbackPanel.SetActive(true);
            _feedbackCanvasGroup.alpha = 1f;

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

            yield return new WaitForSeconds(2.4f);

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

        public void UpdateScore(int newScore) { }
        public void UpdateScore(int currentScore, int bestScore) { }
        public void UpdateRemainingBlocks(int count) { }
        public void UpdateProgress(int escaped, int total) { }

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
