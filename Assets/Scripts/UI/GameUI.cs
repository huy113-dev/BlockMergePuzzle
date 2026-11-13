using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        // Top Header matching design [Image #9]
        private GameObject _headerPanel;
        private Image _returnButtonImg;
        private Image _timeBannerImg;
        private Image _pauseButtonImg;
        private Text _timeText;
        private GameObject _levelBannerObj;
        private Image _levelBannerImg;
        private Text _levelText;
        private TutorialHandGuide _tutorialGuide;

        // Timer state
        private const float DefaultLevelTime = 100f; // 1:40 default
        private const float Level1Time = 300f; // 5:00 for Level 1
        private float _timeRemaining = Level1Time;
        private bool _isTimerRunning = false;
        private bool _isPaused = false;

        private float GetLevelDurationSeconds()
        {
            return GetCurrentLevelNumber() <= 3 ? Level1Time : DefaultLevelTime;
        }

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

        // Boosters Bar
        public const int BoosterFreezeUnlockLevel = 6;
        public const int BoosterMagnetUnlockLevel = 12;
        public const int BoosterWandUnlockLevel = 16;

        private GameObject _boosterBar;
        private Button _boosterFreezeBtn;
        private Button _boosterMagnetBtn;
        private Button _boosterWandBtn;
        private Text _boosterFreezeCountText;
        private Text _boosterMagnetCountText;
        private Text _boosterWandCountText;
        private GameObject _boosterFreezeLock;
        private GameObject _boosterMagnetLock;
        private GameObject _boosterWandLock;
        private int _freezeCount = 3;
        private int _magnetCount = 2;
        private int _wandCount = 1;
        private float _freezeTimerRemaining = 0f;

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

            if (transform.childCount > 0)
            {
                BindExistingUI();
            }
            else
            {
                BuildUIHierarchy();
            }
        }

        private void BindExistingUI()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas != null && _canvas.worldCamera == null)
            {
                _canvas.worldCamera = Camera.main;
            }
            _scaler = GetComponent<CanvasScaler>();

            _headerPanel = transform.Find("HeaderPanel")?.gameObject;
            if (_headerPanel != null)
            {
                Transform replayTr = _headerPanel.transform.Find("Btn_Replay");
                if (replayTr != null)
                {
                    _returnButtonImg = replayTr.GetComponent<Image>();
                    Button btn = replayTr.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(OnReturnClicked);
                    }
                }

                Transform pauseTr = _headerPanel.transform.Find("Btn_Pause");
                if (pauseTr != null)
                {
                    _pauseButtonImg = pauseTr.GetComponent<Image>();
                    Button btn = pauseTr.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(OnPauseClicked);
                    }
                }
            }

            Transform tutTr = transform.Find("TutorialLayer");
            if (tutTr != null)
            {
                _tutorialGuide = tutTr.GetComponent<TutorialHandGuide>();
                if (_tutorialGuide == null) _tutorialGuide = tutTr.gameObject.AddComponent<TutorialHandGuide>();

                Transform bannerTr = tutTr.Find("InstructionBanner");
                Text bannerText = bannerTr != null ? bannerTr.GetComponentInChildren<Text>(true) : null;
                if (bannerTr != null)
                {
                    _tutorialGuide.SetupExistingComponents(bannerTr.gameObject, bannerText);
                }
            }

            _pausePanel = transform.Find("PausePanel")?.gameObject;
            if (_pausePanel != null)
            {
                BindPausePanelUI();
            }

            _victoryPanel = transform.Find("VictoryPanel")?.gameObject;
            if (_victoryPanel != null)
            {
                Button againBtn = _victoryPanel.GetComponentInChildren<Button>(true);
                if (againBtn != null)
                {
                    againBtn.onClick.RemoveAllListeners();
                    againBtn.onClick.AddListener(() =>
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
                        _victoryPanel.SetActive(false);
                        RestartLevel();
                    });
                }
                _victoryPanel.SetActive(false);
            }

            _feedbackPanel = transform.Find("FeedbackBanner")?.gameObject;
            if (_feedbackPanel != null)
            {
                _feedbackCanvasGroup = _feedbackPanel.GetComponent<CanvasGroup>();
                _feedbackText = _feedbackPanel.GetComponentInChildren<Text>(true);
            }

            EnsureLevelBanner();
            EnsureTimeText();
            EnsureBoosterBar();
            UpdateLevelDisplay();
        }

        [ContextMenu("Ensure Time Text on Banner")]
        public void EnsureTimeText()
        {
            if (_headerPanel == null)
            {
                _headerPanel = transform.Find("HeaderPanel")?.gameObject;
            }
            if (_headerPanel == null) return;

            Transform timeTr = _headerPanel.transform.Find("Banner_Time");
            if (timeTr == null) return;

            _timeBannerImg = timeTr.GetComponent<Image>();
            _timeText = timeTr.GetComponentInChildren<Text>(true);
            if (_timeText == null)
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                CreateTimeTextOnBanner(timeTr, font);
            }
        }

        [ContextMenu("Add Level Banner (Below Timer)")]
        public void EnsureLevelBanner()
        {
            if (_headerPanel == null)
            {
                _headerPanel = transform.Find("HeaderPanel")?.gameObject;
            }
            if (_headerPanel == null) return;

            Transform existingLvl = _headerPanel.transform.Find("Banner_Level");
            if (existingLvl != null)
            {
                _levelBannerObj = existingLvl.gameObject;
                _levelBannerImg = existingLvl.GetComponent<Image>();
                if (_levelBannerImg != null && _levelBannerImg.sprite == null)
                {
                    _levelBannerImg.sprite = GemSpriteManager.GetMainUISprite("UI_Bn Level") ?? SpriteGenerator.GetLevelBannerSprite();
                    _levelBannerImg.preserveAspect = true;
                }
                _levelText = existingLvl.GetComponentInChildren<Text>(true);
                UpdateLevelDisplay();
                return;
            }

            Transform timeTr = _headerPanel.transform.Find("Banner_Time");
            Vector2 anchorMin = new Vector2(0.5f, 0.5f);
            Vector2 anchorMax = new Vector2(0.5f, 0.5f);
            float levelWidth = 276f;
            float levelHeight = 64f;
            Vector2 pos = new Vector2(8f, -212f);

            if (timeTr != null)
            {
                RectTransform timeRect = timeTr.GetComponent<RectTransform>();
                if (timeRect != null)
                {
                    anchorMin = timeRect.anchorMin;
                    anchorMax = timeRect.anchorMax;
                    float gap = 6f;
                    pos = new Vector2(
                        timeRect.anchoredPosition.x,
                        timeRect.anchoredPosition.y - (timeRect.sizeDelta.y * 0.5f) - (levelHeight * 0.5f) - gap
                    );
                }
            }

            _levelBannerObj = new GameObject("Banner_Level");
            _levelBannerObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform lvlRect = _levelBannerObj.AddComponent<RectTransform>();
            lvlRect.anchorMin = anchorMin;
            lvlRect.anchorMax = anchorMax;
            lvlRect.pivot = new Vector2(0.5f, 0.5f);
            lvlRect.anchoredPosition = pos;
            lvlRect.sizeDelta = new Vector2(levelWidth, levelHeight);

            _levelBannerImg = _levelBannerObj.AddComponent<Image>();
            _levelBannerImg.sprite = GemSpriteManager.GetMainUISprite("UI_Bn Level") ?? SpriteGenerator.GetLevelBannerSprite();
            _levelBannerImg.preserveAspect = true;
            _levelBannerImg.raycastTarget = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject textObj = new GameObject("LevelText");
            textObj.transform.SetParent(_levelBannerObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 4f);
            textRect.offsetMax = new Vector2(-16f, -4f);

            _levelText = textObj.AddComponent<Text>();
            _levelText.text = $"LEVEL {GetCurrentLevelNumber()}";
            _levelText.font = font;
            _levelText.fontSize = 31;
            _levelText.fontStyle = FontStyle.Bold;
            _levelText.alignment = TextAnchor.MiddleCenter;
            _levelText.color = new Color(0.99f, 0.95f, 0.70f);
            _levelText.raycastTarget = false;

            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.04f, 0.06f, 0.12f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        public void UpdateLevelDisplay()
        {
            if (_levelText == null)
            {
                var lvlObj = GameObject.Find("LevelText");
                if (lvlObj != null) _levelText = lvlObj.GetComponent<Text>();
                else if (_headerPanel != null)
                {
                    Transform lvlTr = _headerPanel.transform.Find("Banner_Level");
                    if (lvlTr != null) _levelText = lvlTr.GetComponentInChildren<Text>(true);
                }
            }

            if (_levelText != null)
            {
                _levelText.text = $"LEVEL {GetCurrentLevelNumber()}";
            }
        }

        [ContextMenu("Add Booster UI (Keep Existing Layout)")]
        public void EnsureBoosterBar()
        {
            Transform boosterTr = transform.Find("BoosterBar");
            if (boosterTr == null)
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                BuildBoosterBar(font);
                if (_boosterBar != null && _headerPanel != null)
                {
                    int targetIdx = _headerPanel.transform.GetSiblingIndex() + 1;
                    _boosterBar.transform.SetSiblingIndex(targetIdx);
                }
            }
            else
            {
                _boosterBar = boosterTr.gameObject;
                BindBoosterSlot(boosterTr.Find("Booster_Freeze"), ref _boosterFreezeBtn, ref _boosterFreezeCountText, ref _boosterFreezeLock, OnBoosterFreezeClicked);
                BindBoosterSlot(boosterTr.Find("Booster_Magnet"), ref _boosterMagnetBtn, ref _boosterMagnetCountText, ref _boosterMagnetLock, OnBoosterMagnetClicked);
                BindBoosterSlot(boosterTr.Find("Booster_Wand"), ref _boosterWandBtn, ref _boosterWandCountText, ref _boosterWandLock, OnBoosterWandClicked);
            }

            UpdateBoosterUnlockStates();
        }

        [ContextMenu("Rebuild Settings Panel")]
        public void RebuildPausePanel()
        {
            Transform oldPause = transform.Find("PausePanel");
            int siblingIdx = oldPause != null ? oldPause.GetSiblingIndex() : -1;
            if (oldPause != null)
            {
                if (Application.isPlaying) Destroy(oldPause.gameObject);
                else DestroyImmediate(oldPause.gameObject);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildPausePanel(font);

            if (_pausePanel != null && siblingIdx >= 0)
            {
                _pausePanel.transform.SetSiblingIndex(Mathf.Min(siblingIdx, transform.childCount - 1));
            }
        }

        private void BindPausePanelUI()
        {
            if (_pausePanel == null) return;
            Transform dialog = _pausePanel.transform.Find("DialogBox");
            if (dialog == null) return;

            // Upgrade legacy blue PausePanel automatically while keeping rest of scene UI intact
            if (dialog.Find("TitleBanner") != null || dialog.Find("Btn_NoAds") != null)
            {
                RebuildPausePanel();
                return;
            }

            Button backdropBtn = _pausePanel.GetComponent<Button>();
            if (backdropBtn != null)
            {
                backdropBtn.onClick.RemoveAllListeners();
                backdropBtn.onClick.AddListener(ResumeGame);
            }

            Transform closeTr = dialog.Find("Btn_Close");
            if (closeTr != null)
            {
                Image closeImg = closeTr.GetComponent<Image>();
                if (closeImg != null)
                {
                    Sprite closeSprite = GemSpriteManager.GetSettingSprite("X (1)") ?? GemSpriteManager.GetPauseSprite("X");
                    if (closeSprite != null) closeImg.sprite = closeSprite;
                    closeImg.preserveAspect = true;
                    closeImg.color = Color.white;
                }
                Button btn = closeTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ResumeGame);
                }
            }

            Transform settingsCard = dialog.Find("SettingsCard");
            if (settingsCard != null)
            {
                BindSettingRow(settingsCard.Find("SoundRow"), ref _soundIconImg, ref _soundToggleImg, ToggleSound);
                BindSettingRow(settingsCard.Find("MusicRow"), ref _musicIconImg, ref _musicToggleImg, ToggleMusic);
                BindSettingRow(settingsCard.Find("VibrationRow"), ref _vibrationIconImg, ref _vibrationToggleImg, ToggleVibration);
            }

            UpdatePauseSettingViews();
            _pausePanel.SetActive(false);
        }

        private void BindSettingRow(Transform rowTr, ref Image iconImgRef, ref Image toggleImgRef, UnityEngine.Events.UnityAction onToggle)
        {
            if (rowTr == null) return;
            Image rowBg = rowTr.GetComponent<Image>();
            if (rowBg != null)
            {
                rowBg.sprite = SpriteGenerator.GetSettingRowSprite();
                rowBg.type = Image.Type.Sliced;
                rowBg.color = Color.white;
            }

            Transform iconTr = rowTr.Find("Icon");
            if (iconTr != null)
            {
                iconImgRef = iconTr.GetComponent<Image>();
                if (iconImgRef != null) iconImgRef.preserveAspect = true;
            }

            Transform switchTr = rowTr.Find("Switch");
            if (switchTr != null)
            {
                toggleImgRef = switchTr.GetComponent<Image>();
                if (toggleImgRef != null) toggleImgRef.preserveAspect = true;
                Button btn = switchTr.GetComponent<Button>();
                if (btn != null && onToggle != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(onToggle);
                }
            }
        }

        private void BindBoosterSlot(Transform slotTr, ref Button btnRef, ref Text countTextRef, ref GameObject lockRef, UnityEngine.Events.UnityAction onClick)
        {
            if (slotTr == null) return;
            btnRef = slotTr.GetComponent<Button>();
            if (btnRef != null && onClick != null)
            {
                btnRef.onClick.RemoveAllListeners();
                btnRef.onClick.AddListener(onClick);
            }

            Transform badgeTr = slotTr.Find("Badge_Count");
            if (badgeTr != null)
            {
                countTextRef = badgeTr.GetComponentInChildren<Text>(true);
            }

            Transform lockTr = slotTr.Find("Lock");
            if (lockTr != null)
            {
                lockRef = lockTr.gameObject;
            }
        }

        private int GetCurrentLevelNumber()
        {
            LevelManager lm = LevelManager.Instance;
#if UNITY_EDITOR
            if (lm == null)
            {
#if UNITY_2023_1_OR_NEWER
                lm = Object.FindAnyObjectByType<LevelManager>();
#else
                lm = Object.FindObjectOfType<LevelManager>();
#endif
            }
#endif
            return lm != null ? (int)lm.CurrentLevel : 1;
        }

        [ContextMenu("Update Booster Unlock States")]
        public void UpdateBoosterUnlockStates()
        {
            if (_boosterBar == null)
            {
                Transform barTr = transform.Find("BoosterBar");
                if (barTr != null) _boosterBar = barTr.gameObject;
            }
            if (_boosterBar == null) return;

            int currentLevel = GetCurrentLevelNumber();
            SetBoosterSlotVisual(_boosterBar.transform.Find("Booster_Freeze"), currentLevel >= BoosterFreezeUnlockLevel, BoosterFreezeUnlockLevel, _freezeCount);
            SetBoosterSlotVisual(_boosterBar.transform.Find("Booster_Magnet"), currentLevel >= BoosterMagnetUnlockLevel, BoosterMagnetUnlockLevel, _magnetCount);
            SetBoosterSlotVisual(_boosterBar.transform.Find("Booster_Wand"), currentLevel >= BoosterWandUnlockLevel, BoosterWandUnlockLevel, _wandCount);
        }

        private void SetBoosterSlotVisual(Transform slotTr, bool isUnlocked, int unlockLevel, int currentCount)
        {
            if (slotTr == null) return;

            Image bgImg = slotTr.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = isUnlocked
                    ? Color.white
                    : new Color(0.20f, 0.22f, 0.28f, 0.56f); // Mờ + tối lại khi chưa mở
            }

            Transform iconTr = slotTr.Find("Icon");
            if (iconTr != null)
            {
                Image iconImg = iconTr.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.color = isUnlocked
                        ? Color.white
                        : new Color(0.24f, 0.26f, 0.34f, 0.38f); // Icon mờ + tối lại khi chưa mở
                }
            }

            Transform lockTr = slotTr.Find("Lock");
            if (lockTr != null)
            {
                lockTr.gameObject.SetActive(!isUnlocked);
                Image lockImg = lockTr.GetComponent<Image>();
                if (lockImg != null)
                {
                    lockImg.color = Color.white;
                }
            }

            Transform badgeTr = slotTr.Find("Badge_Count");
            if (badgeTr != null)
            {
                Image bImg = badgeTr.GetComponent<Image>();
                if (bImg != null)
                {
                    bImg.color = isUnlocked
                        ? new Color(0.10f, 0.14f, 0.28f, 0.96f)
                        : new Color(0.08f, 0.10f, 0.18f, 0.90f);
                }

                Outline outline = badgeTr.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = isUnlocked
                        ? new Color(0.35f, 0.90f, 1.0f, 0.92f)
                        : new Color(0.85f, 0.72f, 0.25f, 0.80f);
                }

                Text text = badgeTr.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = isUnlocked ? ("x" + currentCount) : ("Lv." + unlockLevel);
                    text.fontSize = isUnlocked ? 26 : 20;
                    text.color = isUnlocked
                        ? new Color(0.99f, 0.95f, 0.70f)
                        : new Color(0.95f, 0.85f, 0.45f);
                }
            }
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRemainingBlocksChanged += HandleRemainingBlocksChanged;
                LevelManager.Instance.OnLevelInitialized += UpdateLevelDisplay;
            }

            UpdateLevelDisplay();
            UpdateBoosterUnlockStates();
            StartTimer();
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRemainingBlocksChanged -= HandleRemainingBlocksChanged;
                LevelManager.Instance.OnLevelInitialized -= UpdateLevelDisplay;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (_freezeTimerRemaining > 0f)
            {
                if (!_isPaused)
                {
                    _freezeTimerRemaining -= Time.deltaTime;
                    if (_freezeTimerRemaining <= 0f)
                    {
                        _freezeTimerRemaining = 0f;
                        UpdateTimeDisplay();
                    }
                }
            }
            else if (_isTimerRunning && !_isPaused)
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

            // 1. Top Header Bar (Btn_Replay on BG1, Time 01:40 on Bn Time, Btn_Pause on BG1)
            BuildTopHeader(font);

            // 2. Bottom Booster Bar (Booster_Freeze, Booster_Magnet, Booster_Wand + Lock)
            BuildBoosterBar(font);

            // 3. Tutorial Hand & Instruction Banner (Level 1)
            BuildTutorialUI(font);

            // 3. Pause Modal
            BuildPausePanel(font);

            // 4. Victory Modal
            BuildVictoryPanel(font);

            // 5. Feedback Banner
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
            headerRect.anchoredPosition = new Vector2(0f, -50f);
            headerRect.sizeDelta = new Vector2(1000f, 190f);

            // a. Replay Button (Left: on UI_BG1 base with UI_Replay icon)
            GameObject replayObj = new GameObject("Btn_Replay");
            replayObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform replayRect = replayObj.AddComponent<RectTransform>();
            replayRect.anchorMin = new Vector2(0.5f, 0.5f);
            replayRect.anchorMax = new Vector2(0.5f, 0.5f);
            replayRect.pivot = new Vector2(0.5f, 0.5f);
            replayRect.anchoredPosition = new Vector2(-360f, 0f);
            replayRect.sizeDelta = new Vector2(150f, 166f);

            _returnButtonImg = replayObj.AddComponent<Image>();
            _returnButtonImg.sprite = GemSpriteManager.GetMainUISprite("UI_BG1");
            _returnButtonImg.preserveAspect = true;

            // Child icon UI_Replay
            GameObject replayIcon = new GameObject("Icon_Replay");
            replayIcon.transform.SetParent(replayObj.transform, false);
            RectTransform iconRect = replayIcon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(4f, 1f);
            iconRect.sizeDelta = new Vector2(98f, 110f);

            Image replayIconImg = replayIcon.AddComponent<Image>();
            replayIconImg.sprite = GemSpriteManager.GetMainUISprite("UI_Replay");
            replayIconImg.preserveAspect = true;
            replayIconImg.raycastTarget = false;

            Button replayBtn = replayObj.AddComponent<Button>();
            replayBtn.targetGraphic = _returnButtonImg;
            replayBtn.onClick.AddListener(OnReturnClicked);

            // b. Time Banner (Center: UI_Bn Time matching Image #9)
            GameObject timeObj = new GameObject("Banner_Time");
            timeObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.5f);
            timeRect.pivot = new Vector2(0.5f, 0.5f);
            timeRect.anchoredPosition = new Vector2(0f, 0f);
            timeRect.sizeDelta = new Vector2(430f, 154f);

            _timeBannerImg = timeObj.AddComponent<Image>();
            _timeBannerImg.sprite = GemSpriteManager.GetMainUISprite("UI_Bn Time");
            _timeBannerImg.preserveAspect = true;

            // Live Time Text on the right stone window of UI_Bn Time
            CreateTimeTextOnBanner(timeObj.transform, font);

            // c. Pause Button (Right: on UI_BG1 base with UI_setting icon)
            GameObject pauseObj = new GameObject("Btn_Pause");
            pauseObj.transform.SetParent(_headerPanel.transform, false);

            RectTransform pauseRect = pauseObj.AddComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(0.5f, 0.5f);
            pauseRect.anchorMax = new Vector2(0.5f, 0.5f);
            pauseRect.pivot = new Vector2(0.5f, 0.5f);
            pauseRect.anchoredPosition = new Vector2(360f, 0f);
            pauseRect.sizeDelta = new Vector2(150f, 166f);

            _pauseButtonImg = pauseObj.AddComponent<Image>();
            _pauseButtonImg.sprite = GemSpriteManager.GetMainUISprite("UI_BG1");
            _pauseButtonImg.preserveAspect = true;

            // Child icon UI_setting
            GameObject pauseIcon = new GameObject("Icon_Pause");
            pauseIcon.transform.SetParent(pauseObj.transform, false);
            RectTransform pIconRect = pauseIcon.AddComponent<RectTransform>();
            pIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            pIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            pIconRect.pivot = new Vector2(0.5f, 0.5f);
            pIconRect.anchoredPosition = new Vector2(0f, -5f);
            pIconRect.sizeDelta = new Vector2(104f, 116f);

            Image pauseIconImg = pauseIcon.AddComponent<Image>();
            pauseIconImg.sprite = GemSpriteManager.GetMainUISprite("UI_setting");
            pauseIconImg.preserveAspect = true;
            pauseIconImg.raycastTarget = false;

            Button pauseBtn = pauseObj.AddComponent<Button>();
            pauseBtn.targetGraphic = _pauseButtonImg;
            pauseBtn.onClick.AddListener(OnPauseClicked);

            EnsureLevelBanner();
        }

        private void CreateTimeTextOnBanner(Transform bannerTransform, Font font)
        {
            Transform existing = bannerTransform.Find("TimeVal");
            if (existing != null && existing.GetComponent<RectTransform>() == null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
                existing = null;
            }

            GameObject textObj = existing != null
                ? existing.gameObject
                : new GameObject("TimeVal", typeof(RectTransform));
            textObj.transform.SetParent(bannerTransform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect == null) textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(68f, 0f);
            textRect.sizeDelta = new Vector2(240f, 75f);

            _timeText = textObj.GetComponent<Text>();
            if (_timeText == null) _timeText = textObj.AddComponent<Text>();
            float duration = GetLevelDurationSeconds();
            _timeText.text = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(duration / 60f), Mathf.FloorToInt(duration % 60f));
            _timeText.font = font;
            _timeText.fontSize = 44;
            _timeText.fontStyle = FontStyle.Bold;
            _timeText.alignment = TextAnchor.MiddleCenter;
            _timeText.color = new Color(0.99f, 0.95f, 0.70f);
            _timeText.raycastTarget = false;

            Shadow shadow = textObj.GetComponent<Shadow>();
            if (shadow == null) shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.04f, 0.06f, 0.12f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        private void BuildBoosterBar(Font font)
        {
            _boosterBar = new GameObject("BoosterBar");
            _boosterBar.transform.SetParent(transform, false);

            RectTransform barRect = _boosterBar.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0f);
            barRect.anchorMax = new Vector2(0.5f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0f, 60f);
            barRect.sizeDelta = new Vector2(900f, 260f);

            CreateBoosterSlot("Booster_Freeze", new Vector2(-280f, 130f), "booster1", _freezeCount,
                OnBoosterFreezeClicked, ref _boosterFreezeBtn, ref _boosterFreezeCountText, ref _boosterFreezeLock, font);

            CreateBoosterSlot("Booster_Magnet", new Vector2(0f, 130f), "booster2", _magnetCount,
                OnBoosterMagnetClicked, ref _boosterMagnetBtn, ref _boosterMagnetCountText, ref _boosterMagnetLock, font);

            CreateBoosterSlot("Booster_Wand", new Vector2(280f, 130f), "booster3", _wandCount,
                OnBoosterWandClicked, ref _boosterWandBtn, ref _boosterWandCountText, ref _boosterWandLock, font);

            UpdateBoosterUnlockStates();
        }

        private void CreateBoosterSlot(
            string slotName,
            Vector2 anchoredPos,
            string iconSpriteName,
            int initialCount,
            UnityEngine.Events.UnityAction onClick,
            ref Button btnRef,
            ref Text countTextRef,
            ref GameObject lockRef,
            Font font)
        {
            GameObject slotObj = new GameObject(slotName);
            slotObj.transform.SetParent(_boosterBar.transform, false);

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.anchoredPosition = anchoredPos;
            slotRect.sizeDelta = new Vector2(210f, 222f);

            Image bgImg = slotObj.AddComponent<Image>();
            bgImg.sprite = GemSpriteManager.GetBoosterSprite("BG booster");
            bgImg.preserveAspect = true;

            Button btn = slotObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            if (onClick != null) btn.onClick.AddListener(onClick);
            btnRef = btn;

            // Booster Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iRect = iconObj.AddComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.5f, 0.5f);
            iRect.anchorMax = new Vector2(0.5f, 0.5f);
            iRect.pivot = new Vector2(0.5f, 0.5f);
            iRect.anchoredPosition = new Vector2(0f, 4f);
            iRect.sizeDelta = new Vector2(124f, 130f);

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GemSpriteManager.GetBoosterSprite(iconSpriteName);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Count Badge
            GameObject badgeObj = new GameObject("Badge_Count");
            badgeObj.transform.SetParent(slotObj.transform, false);
            RectTransform bRect = badgeObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.pivot = new Vector2(0.5f, 0.5f);
            bRect.anchoredPosition = new Vector2(64f, -64f);
            bRect.sizeDelta = new Vector2(64f, 64f);

            Image bImg = badgeObj.AddComponent<Image>();
            bImg.color = new Color(0.10f, 0.14f, 0.28f, 0.96f);
            bImg.raycastTarget = false;

            Outline bOutline = badgeObj.AddComponent<Outline>();
            bOutline.effectColor = new Color(0.35f, 0.90f, 1.0f, 0.92f);
            bOutline.effectDistance = new Vector2(2f, -2f);

            GameObject countObj = new GameObject("CountText");
            countObj.transform.SetParent(badgeObj.transform, false);
            RectTransform cRect = countObj.AddComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = Vector2.one;
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            Text countText = countObj.AddComponent<Text>();
            countText.text = "x" + initialCount;
            countText.font = font;
            countText.fontSize = 26;
            countText.fontStyle = FontStyle.Bold;
            countText.alignment = TextAnchor.MiddleCenter;
            countText.color = new Color(0.99f, 0.95f, 0.70f);
            countText.raycastTarget = false;

            Shadow cShadow = countObj.AddComponent<Shadow>();
            cShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            cShadow.effectDistance = new Vector2(1.5f, -1.5f);
            countTextRef = countText;

            // Lock Overlay
            GameObject lockObj = new GameObject("Lock");
            lockObj.transform.SetParent(slotObj.transform, false);
            RectTransform lRect = lockObj.AddComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 0.5f);
            lRect.anchorMax = new Vector2(0.5f, 0.5f);
            lRect.pivot = new Vector2(0.5f, 0.5f);
            lRect.anchoredPosition = new Vector2(0f, -2f);
            lRect.sizeDelta = new Vector2(88f, 96f);

            Image lockImg = lockObj.AddComponent<Image>();
            lockImg.sprite = GemSpriteManager.GetBoosterSprite("lock");
            lockImg.preserveAspect = true;
            lockImg.raycastTarget = false;
            lockRef = lockObj;
        }

        private void BuildTutorialUI(Font font)
        {
            GameObject tutRoot = new GameObject("TutorialLayer");
            tutRoot.transform.SetParent(transform, false);

            RectTransform tutRect = tutRoot.AddComponent<RectTransform>();
            tutRect.anchorMin = Vector2.zero;
            tutRect.anchorMax = Vector2.one;
            tutRect.offsetMin = Vector2.zero;
            tutRect.offsetMax = Vector2.zero;

            _tutorialGuide = tutRoot.AddComponent<TutorialHandGuide>();

            // 1. Instruction Text Banner
            GameObject bannerObj = new GameObject("InstructionBanner");
            bannerObj.transform.SetParent(tutRoot.transform, false);

            RectTransform bRect = bannerObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.pivot = new Vector2(0.5f, 0.5f);
            bRect.anchoredPosition = new Vector2(0f, 320f);
            bRect.sizeDelta = new Vector2(650f, 82f);

            Image bImg = bannerObj.AddComponent<Image>();
            bImg.color = new Color(0.06f, 0.08f, 0.16f, 0.88f); // Translucent deep slate card

            Outline bOutline = bannerObj.AddComponent<Outline>();
            bOutline.effectColor = new Color(0.96f, 0.84f, 0.38f, 0.70f); // Warm gold trim
            bOutline.effectDistance = new Vector2(2f, -2f);

            // Banner Text
            GameObject textObj = new GameObject("BannerText");
            textObj.transform.SetParent(bannerObj.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(10f, 0f);
            tRect.offsetMax = new Vector2(-10f, 0f);

            Text bText = textObj.AddComponent<Text>();
            bText.text = "Swipe block to merge";
            bText.font = font;
            bText.fontSize = 36;
            bText.fontStyle = FontStyle.Bold;
            bText.alignment = TextAnchor.MiddleCenter;
            bText.color = new Color(1f, 0.98f, 0.85f);
            bText.raycastTarget = false;

            Shadow bShadow = textObj.AddComponent<Shadow>();
            bShadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            bShadow.effectDistance = new Vector2(2f, -2f);

            _tutorialGuide.SetupComponents(bannerObj, bText);
        }

        private void BuildPausePanel(Font font)
        {
            // 1. Fullscreen dark backdrop dimming the active game view underneath
            _pausePanel = CreateUIPanel("PausePanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetImageColor(_pausePanel, new Color(0.02f, 0.015f, 0.06f, 0.80f));
            Button backdropBtn = _pausePanel.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(ResumeGame);

            // 2. Purple Crystal Settings Dialog Frame (Group 331, 1195x1027)
            Sprite settingFrame = GemSpriteManager.GetSettingSprite("Group 331");
            GameObject dialog = CreateUIPanel("DialogBox", _pausePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 756f));
            Image dialogImg = dialog.GetComponent<Image>();
            if (dialogImg != null)
            {
                dialogImg.sprite = settingFrame ?? GemSpriteManager.GetPauseSprite("panel");
                dialogImg.preserveAspect = true;
                dialogImg.color = Color.white;
            }

            // 3. Close 'X' Button on upper right corner
            GameObject closeObj = CreateUIPanel("Btn_Close", dialog.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -64f), new Vector2(88f, 88f));
            Image closeImg = closeObj.GetComponent<Image>();
            if (closeImg != null)
            {
                closeImg.sprite = GemSpriteManager.GetSettingSprite("X (1)") ?? GemSpriteManager.GetPauseSprite("X");
                closeImg.preserveAspect = true;
                closeImg.color = Color.white;
            }
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(ResumeGame);

            // 4. Settings Rows Container (3 purple rounded cards matching Image #11)
            GameObject settingsCard = CreateUIPanel("SettingsCard", dialog.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(740f, 460f));
            SetImageColor(settingsCard, Color.clear);

            // Row 1: Sound
            BuildSettingRow(settingsCard.transform, "SoundRow", 156f, "Sound", font,
                ref _soundIconImg, _soundEnabled ? "Vector 389" : "Vector 390",
                ref _soundToggleImg, _soundEnabled ? "Group 334" : "Group 333",
                ToggleSound);

            // Row 2: Music
            BuildSettingRow(settingsCard.transform, "MusicRow", 0f, "Music", font,
                ref _musicIconImg, _musicEnabled ? "Vector 392" : "Group 332",
                ref _musicToggleImg, _musicEnabled ? "Group 334" : "Group 333",
                ToggleMusic);

            // Row 3: Vibration
            BuildSettingRow(settingsCard.transform, "VibrationRow", -156f, "Vibration", font,
                ref _vibrationIconImg, _vibrationEnabled ? "Vector 394" : "Vector 395",
                ref _vibrationToggleImg, _vibrationEnabled ? "Group 334" : "Group 333",
                ToggleVibration);

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
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, posY);
            rowRect.sizeDelta = new Vector2(720f, 134f);

            Image rowBg = rowObj.AddComponent<Image>();
            rowBg.sprite = SpriteGenerator.GetSettingRowSprite();
            rowBg.type = Image.Type.Sliced;
            rowBg.color = Color.white;
            rowBg.raycastTarget = false;

            // Icon (Left)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(rowObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(78f, 0f);
            iconRect.sizeDelta = new Vector2(72f, 68f);

            iconImgRef = iconObj.AddComponent<Image>();
            iconImgRef.sprite = GemSpriteManager.GetSettingSprite(iconSpriteName) ?? GemSpriteManager.GetPauseSprite(iconSpriteName);
            iconImgRef.preserveAspect = true;
            iconImgRef.color = Color.white;
            iconImgRef.raycastTarget = false;

            // Title Label (Center-Left)
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(168f, 0f);
            labelRect.sizeDelta = new Vector2(300f, 68f);

            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = title;
            labelText.font = font;
            labelText.fontSize = 42;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = new Color(0.95f, 0.91f, 1.0f);
            labelText.raycastTarget = false;

            Shadow labelShadow = labelObj.AddComponent<Shadow>();
            labelShadow.effectColor = new Color(0.10f, 0.04f, 0.22f, 0.60f);
            labelShadow.effectDistance = new Vector2(1.5f, -1.5f);

            // Toggle Switch Button (Right)
            GameObject toggleObj = new GameObject("Switch");
            toggleObj.transform.SetParent(rowObj.transform, false);
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1f, 0.5f);
            toggleRect.anchorMax = new Vector2(1f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(-115f, 0f);
            toggleRect.sizeDelta = new Vector2(158f, 72f);

            toggleImgRef = toggleObj.AddComponent<Image>();
            toggleImgRef.sprite = GemSpriteManager.GetSettingSprite(toggleSpriteName) ?? GemSpriteManager.GetPauseSprite(toggleSpriteName);
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

        public void RefreshPauseCloseSprite()
        {
            if (_pausePanel == null) _pausePanel = transform.Find("PausePanel")?.gameObject;
            if (_pausePanel == null) return;
            Transform closeTr = _pausePanel.transform.Find("DialogBox/Btn_Close");
            if (closeTr == null) return;
            Image closeImg = closeTr.GetComponent<Image>();
            if (closeImg != null)
            {
                Sprite closeSprite = GemSpriteManager.GetSettingSprite("X (1)") ?? GemSpriteManager.GetPauseSprite("X");
                if (closeSprite != null) closeImg.sprite = closeSprite;
                closeImg.preserveAspect = true;
                closeImg.color = Color.white;
            }
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
            {
                _soundIconImg.sprite = GemSpriteManager.GetSettingSprite(_soundEnabled ? "Vector 389" : "Vector 390")
                    ?? GemSpriteManager.GetPauseSprite(_soundEnabled ? "Group 308" : "Group 314");
                _soundIconImg.preserveAspect = true;
            }
            if (_soundToggleImg != null)
            {
                _soundToggleImg.sprite = GemSpriteManager.GetSettingSprite(_soundEnabled ? "Group 334" : "Group 333")
                    ?? GemSpriteManager.GetPauseSprite(_soundEnabled ? "on" : "off");
                _soundToggleImg.preserveAspect = true;
            }

            if (_musicIconImg != null)
            {
                _musicIconImg.sprite = GemSpriteManager.GetSettingSprite(_musicEnabled ? "Vector 392" : "Group 332")
                    ?? GemSpriteManager.GetPauseSprite(_musicEnabled ? "Group 312" : "Group 315");
                _musicIconImg.preserveAspect = true;
            }
            if (_musicToggleImg != null)
            {
                _musicToggleImg.sprite = GemSpriteManager.GetSettingSprite(_musicEnabled ? "Group 334" : "Group 333")
                    ?? GemSpriteManager.GetPauseSprite(_musicEnabled ? "on" : "off");
                _musicToggleImg.preserveAspect = true;
            }

            if (_vibrationIconImg != null)
            {
                _vibrationIconImg.sprite = GemSpriteManager.GetSettingSprite(_vibrationEnabled ? "Vector 394" : "Vector 395")
                    ?? GemSpriteManager.GetPauseSprite(_vibrationEnabled ? "Group 313" : "Group 316");
                _vibrationIconImg.preserveAspect = true;
            }
            if (_vibrationToggleImg != null)
            {
                _vibrationToggleImg.sprite = GemSpriteManager.GetSettingSprite(_vibrationEnabled ? "Group 334" : "Group 333")
                    ?? GemSpriteManager.GetPauseSprite(_vibrationEnabled ? "on" : "off");
                _vibrationToggleImg.preserveAspect = true;
            }
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
            _timeRemaining = GetLevelDurationSeconds();
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
            _timeRemaining = GetLevelDurationSeconds();
            _freezeTimerRemaining = 0f;
            _isTimerRunning = true;
            _isPaused = false;
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            if (_timeText == null) return;
            int mins = Mathf.FloorToInt(_timeRemaining / 60f);
            int secs = Mathf.FloorToInt(_timeRemaining % 60f);
            _timeText.text = string.Format("{0:D2}:{1:D2}", mins, secs);

            if (_freezeTimerRemaining > 0f)
            {
                _timeText.color = new Color(0.35f, 0.95f, 1.0f);
            }
            else if (_timeRemaining < 20f && _timeRemaining > 0f)
            {
                _timeText.color = new Color(1f, 0.35f, 0.35f); // Red warning when low on time
            }
            else
            {
                _timeText.color = new Color(0.99f, 0.95f, 0.70f); // Warm digital cream yellow matching Image #9
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

        public void OnBoosterFreezeClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_boosterFreezeBtn != null) StartCoroutine(CoButtonPunch(_boosterFreezeBtn.transform));
            int lvl = GetCurrentLevelNumber();
            if (lvl < BoosterFreezeUnlockLevel)
            {
                ShowFeedbackError($"Đóng Băng mở khoá ở Level {BoosterFreezeUnlockLevel}!");
                return;
            }
            if (_freezeCount <= 0)
            {
                ShowFeedbackError("Hết lượt dùng Đóng Băng!");
                return;
            }

            _freezeCount--;
            if (_boosterFreezeCountText != null) _boosterFreezeCountText.text = "x" + _freezeCount;
            _freezeTimerRemaining = 15f;
            UpdateTimeDisplay();
            ShowFloatingText("ĐÃ ĐÓNG BĂNG 15S!", Vector3.zero, new Color(0.35f, 0.95f, 1.0f));
        }

        public void OnBoosterMagnetClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_boosterMagnetBtn != null) StartCoroutine(CoButtonPunch(_boosterMagnetBtn.transform));
            int lvl = GetCurrentLevelNumber();
            if (lvl < BoosterMagnetUnlockLevel)
            {
                ShowFeedbackError($"Nam Châm mở khoá ở Level {BoosterMagnetUnlockLevel}!");
                return;
            }
            if (_magnetCount <= 0)
            {
                ShowFeedbackError("Hết lượt dùng Nam Châm!");
                return;
            }

            _magnetCount--;
            if (_boosterMagnetCountText != null) _boosterMagnetCountText.text = "x" + _magnetCount;
            if (TryMagnetAutoMerge())
            {
                ShowFloatingText("NAM CHÂM MERGE!", Vector3.zero, new Color(1.0f, 0.85f, 0.30f));
            }
            else
            {
                ShowFloatingText("KÍCH HOẠT NAM CHÂM!", Vector3.zero, new Color(1.0f, 0.85f, 0.30f));
            }
        }

        public void OnBoosterWandClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_boosterWandBtn != null) StartCoroutine(CoButtonPunch(_boosterWandBtn.transform));
            int lvl = GetCurrentLevelNumber();
            if (lvl < BoosterWandUnlockLevel)
            {
                ShowFeedbackError($"Đũa Thần mở khoá ở Level {BoosterWandUnlockLevel}!");
                return;
            }
            if (_wandCount <= 0)
            {
                ShowFeedbackError("Hết lượt dùng Đũa Thần!");
                return;
            }

            _wandCount--;
            if (_boosterWandCountText != null) _boosterWandCountText.text = "x" + _wandCount;
            ShowFloatingText("ĐŨA THẦN PHÉP THUẬT!", Vector3.zero, new Color(1.0f, 0.60f, 1.0f));
        }

        private bool TryMagnetAutoMerge()
        {
            if (LevelManager.Instance == null) return false;
            var pieces = LevelManager.Instance.ActivePieces;
            if (pieces == null || pieces.Count < 2) return false;

            for (int i = 0; i < pieces.Count; i++)
            {
                var p1 = pieces[i];
                if (p1 == null || p1.IsSliding || p1.IsEscaping) continue;
                for (int j = i + 1; j < pieces.Count; j++)
                {
                    var p2 = pieces[j];
                    if (p2 == null || p2.IsSliding || p2.IsEscaping) continue;
                    if (p1.PieceColor == p2.PieceColor && p1.Shape == p2.Shape && p1.Value == p2.Value)
                    {
                        LevelManager.Instance.ExecuteMerge(p1, p2, 0.16f);
                        return true;
                    }
                }
            }
            return false;
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
            Time.timeScale = 1f;
            _isPaused = false;
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex >= 0 &&
                (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == LevelManager.Instance.InitialSceneLevel))
            {
                SceneManager.LoadScene(activeScene.buildIndex);
                return;
            }

            ResetTimer();
            UpdateLevelDisplay();
            UpdateBoosterUnlockStates();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.InitializeLevel(true);
            }
            if (_tutorialGuide != null)
            {
                _tutorialGuide.HandleLevelReset();
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
