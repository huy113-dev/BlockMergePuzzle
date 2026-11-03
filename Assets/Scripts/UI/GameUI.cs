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

        // Settings Toggles State (Pause Dialog matching [Image #7])
        private const string PrefSoundKey = "BMP_SoundEnabled";
        private const string PrefMusicKey = "BMP_MusicEnabled";
        private const string PrefVibrationKey = "BMP_VibrationEnabled";

        private bool _soundEnabled = true;
        private bool _musicEnabled = true;
        private bool _vibrationEnabled = true;

        private Image _soundIconImg;
        private Image _soundToggleImg;
        private Image _musicIconImg;
        private Image _musicToggleImg;
        private Image _vibrationIconImg;
        private Image _vibrationToggleImg;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _soundEnabled = PlayerPrefs.GetInt(PrefSoundKey, 1) == 1;
            _musicEnabled = PlayerPrefs.GetInt(PrefMusicKey, 1) == 1;
            _vibrationEnabled = PlayerPrefs.GetInt(PrefVibrationKey, 1) == 1;
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SoundEnabled = _soundEnabled;
            }

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
            _returnButtonImg.sprite = GemSpriteManager.GetHeaderReturnSprite();
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
            // 1. Fullscreen dark backdrop dimming the active game view underneath
            _pausePanel = CreateUIPanel("PausePanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetImageColor(_pausePanel, new Color(0.012f, 0.02f, 0.055f, 0.82f)); // Darkens game board behind

            // 2. Central Sci-Fi Dialog Box (Asset: panel.png, 490x623)
            GameObject dialog = CreateUIPanel("DialogBox", _pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(820f, 1040f));
            Image dialogImg = dialog.GetComponent<Image>();
            if (dialogImg != null)
            {
                dialogImg.sprite = GemSpriteManager.GetPauseSprite("panel");
                dialogImg.preserveAspect = true;
                dialogImg.color = Color.white;
            }

            // 3. Top Title Banner with Ice Crystals (Asset: Panel game pause.png, 320x85)
            GameObject titleBanner = CreateUIPanel("TitleBanner", dialog.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 15f), new Vector2(540f, 143f));
            Image titleImg = titleBanner.GetComponent<Image>();
            if (titleImg != null)
            {
                titleImg.sprite = GemSpriteManager.GetPauseSprite("Panel game pause");
                titleImg.preserveAspect = true;
                titleImg.color = Color.white;
            }

            // 4. Close 'X' Button on upper right shoulder (Asset: X.png, 78x78)
            GameObject closeObj = CreateUIPanel("Btn_Close", dialog.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-65f, -25f), new Vector2(105f, 105f));
            Image closeImg = closeObj.GetComponent<Image>();
            if (closeImg != null)
            {
                closeImg.sprite = GemSpriteManager.GetPauseSprite("X");
                closeImg.preserveAspect = true;
                closeImg.color = Color.white;
            }
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(ResumeGame);

            // 5. Settings Container Card
            GameObject settingsCard = CreateUIPanel("SettingsCard", dialog.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(650f, 380f));
            SetImageColor(settingsCard, new Color(0.065f, 0.082f, 0.176f, 0.96f)); // Deep navy #11152d
            RectTransform cardRect = settingsCard.GetComponent<RectTransform>();
            cardRect.pivot = new Vector2(0.5f, 1f);
            cardRect.anchoredPosition = new Vector2(0f, -145f);

            Outline cardOutline = settingsCard.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.14f, 0.26f, 0.52f, 0.65f);
            cardOutline.effectDistance = new Vector2(2f, -2f);

            // Row 1: Sound
            BuildSettingRow(settingsCard.transform, "SoundRow", -64f, "Sound", font,
                ref _soundIconImg, _soundEnabled ? "Group 308" : "Group 314",
                ref _soundToggleImg, _soundEnabled ? "on" : "off",
                ToggleSound);

            // Divider 1
            CreateDivider(settingsCard.transform, -127f, 580f);

            // Row 2: Music
            BuildSettingRow(settingsCard.transform, "MusicRow", -190f, "Music", font,
                ref _musicIconImg, _musicEnabled ? "Group 312" : "Group 315",
                ref _musicToggleImg, _musicEnabled ? "on" : "off",
                ToggleMusic);

            // Divider 2
            CreateDivider(settingsCard.transform, -253f, 580f);

            // Row 3: Vibration
            BuildSettingRow(settingsCard.transform, "VibrationRow", -316f, "Vibration", font,
                ref _vibrationIconImg, _vibrationEnabled ? "Group 313" : "Group 316",
                ref _vibrationToggleImg, _vibrationEnabled ? "on" : "off",
                ToggleVibration);

            // 6. "No Ads" Wide Button (Asset: No Ads.png, 377x95)
            GameObject noAdsObj = CreateUIPanel("Btn_NoAds", dialog.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -615f), new Vector2(620f, 156f));
            Image noAdsImg = noAdsObj.GetComponent<Image>();
            if (noAdsImg != null)
            {
                noAdsImg.sprite = GemSpriteManager.GetPauseSprite("No Ads");
                noAdsImg.preserveAspect = true;
                noAdsImg.color = Color.white;
            }
            Button noAdsBtn = noAdsObj.AddComponent<Button>();
            noAdsBtn.targetGraphic = noAdsImg;
            noAdsBtn.onClick.AddListener(OnNoAdsClicked);

            // 7. Bottom Navigation Buttons
            // a. Return Button (Asset: Return.png, 92x93, Cyan circular arrow)
            GameObject retObj = CreateUIPanel("Btn_ReturnLevel", dialog.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-190f, -815f), new Vector2(150f, 150f));
            Image retImg = retObj.GetComponent<Image>();
            if (retImg != null)
            {
                retImg.sprite = GemSpriteManager.GetPauseSprite("Return");
                retImg.preserveAspect = true;
                retImg.color = Color.white;
            }
            Button retBtn = retObj.AddComponent<Button>();
            retBtn.targetGraphic = retImg;
            retBtn.onClick.AddListener(() =>
            {
                ResumeGame();
                RestartLevel();
            });

            // b. Home Button (Asset: Home.png, 230x83, Green hexagonal button)
            GameObject homeObj = CreateUIPanel("Btn_Home", dialog.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(95f, -815f), new Vector2(390f, 141f));
            Image homeImg = homeObj.GetComponent<Image>();
            if (homeImg != null)
            {
                homeImg.sprite = GemSpriteManager.GetPauseSprite("Home");
                homeImg.preserveAspect = true;
                homeImg.color = Color.white;
            }
            Button homeBtn = homeObj.AddComponent<Button>();
            homeBtn.targetGraphic = homeImg;
            homeBtn.onClick.AddListener(() =>
            {
                ResumeGame();
                RestartLevel();
            });

            UpdatePauseSettingViews();
            _pausePanel.SetActive(false);
        }

        private void BuildSettingRow(Transform parent, string rowName, float posY, string title, Font font,
            ref Image iconImgRef, string iconSpriteName,
            ref Image toggleImgRef, string toggleSpriteName,
            UnityEngine.Events.UnityAction onToggle)
        {
            GameObject rowObj = new GameObject(rowName);
            rowObj.transform.SetParent(parent, false);
            RectTransform rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, posY);
            rowRect.sizeDelta = new Vector2(610f, 90f);

            // Icon (Left)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(rowObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(-235f, 0f);
            iconRect.sizeDelta = new Vector2(65f, 65f);

            iconImgRef = iconObj.AddComponent<Image>();
            iconImgRef.sprite = GemSpriteManager.GetPauseSprite(iconSpriteName);
            iconImgRef.preserveAspect = true;
            iconImgRef.color = Color.white;

            // Title Label (Center-Left)
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(-170f, 0f);
            labelRect.sizeDelta = new Vector2(250f, 60f);

            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = title;
            labelText.font = font;
            labelText.fontSize = 36;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            Shadow labelShadow = labelObj.AddComponent<Shadow>();
            labelShadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            labelShadow.effectDistance = new Vector2(2f, -2f);

            // Toggle Switch Button (Right)
            GameObject toggleObj = new GameObject("Switch");
            toggleObj.transform.SetParent(rowObj.transform, false);
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
            toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(215f, 0f);
            toggleRect.sizeDelta = new Vector2(130f, 58f);

            toggleImgRef = toggleObj.AddComponent<Image>();
            toggleImgRef.sprite = GemSpriteManager.GetPauseSprite(toggleSpriteName);
            toggleImgRef.preserveAspect = true;
            toggleImgRef.color = Color.white;

            Button toggleBtn = toggleObj.AddComponent<Button>();
            toggleBtn.targetGraphic = toggleImgRef;
            if (onToggle != null) toggleBtn.onClick.AddListener(onToggle);
        }

        private void CreateDivider(Transform parent, float posY, float width)
        {
            GameObject div = new GameObject("Divider");
            div.transform.SetParent(parent, false);
            RectTransform rect = div.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, posY);
            rect.sizeDelta = new Vector2(width, 2f);

            Image img = div.AddComponent<Image>();
            img.color = new Color(0.18f, 0.25f, 0.45f, 0.50f);
            img.raycastTarget = false;
        }

        public void ToggleSound()
        {
            _soundEnabled = !_soundEnabled;
            PlayerPrefs.SetInt(PrefSoundKey, _soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SoundEnabled = _soundEnabled;
                if (_soundEnabled) SoundManager.Instance.PlayButton();
            }
            if (_soundToggleImg != null) StartCoroutine(CoButtonPunch(_soundToggleImg.transform));
            UpdatePauseSettingViews();
        }

        public void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;
            PlayerPrefs.SetInt(PrefMusicKey, _musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null && _soundEnabled)
            {
                SoundManager.Instance.PlayButton();
            }
            if (_musicToggleImg != null) StartCoroutine(CoButtonPunch(_musicToggleImg.transform));
            UpdatePauseSettingViews();
        }

        public void ToggleVibration()
        {
            _vibrationEnabled = !_vibrationEnabled;
            PlayerPrefs.SetInt(PrefVibrationKey, _vibrationEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null && _soundEnabled)
            {
                SoundManager.Instance.PlayButton();
            }
            if (_vibrationToggleImg != null) StartCoroutine(CoButtonPunch(_vibrationToggleImg.transform));
            UpdatePauseSettingViews();
        }

        public void OnNoAdsClicked()
        {
            if (SoundManager.Instance != null && _soundEnabled) SoundManager.Instance.PlayButton();
            ShowFeedbackError("No Ads: Tính năng đang phát triển!");
        }

        private void UpdatePauseSettingViews()
        {
            if (_soundIconImg != null)
                _soundIconImg.sprite = GemSpriteManager.GetPauseSprite(_soundEnabled ? "Group 308" : "Group 314");
            if (_soundToggleImg != null)
                _soundToggleImg.sprite = GemSpriteManager.GetPauseSprite(_soundEnabled ? "on" : "off");

            if (_musicIconImg != null)
                _musicIconImg.sprite = GemSpriteManager.GetPauseSprite(_musicEnabled ? "Group 312" : "Group 315");
            if (_musicToggleImg != null)
                _musicToggleImg.sprite = GemSpriteManager.GetPauseSprite(_musicEnabled ? "on" : "off");

            if (_vibrationIconImg != null)
                _vibrationIconImg.sprite = GemSpriteManager.GetPauseSprite(_vibrationEnabled ? "Group 313" : "Group 316");
            if (_vibrationToggleImg != null)
                _vibrationToggleImg.sprite = GemSpriteManager.GetPauseSprite(_vibrationEnabled ? "on" : "off");
        }

        public void SetPausePanelActive(bool active)
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(active);
            }
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
