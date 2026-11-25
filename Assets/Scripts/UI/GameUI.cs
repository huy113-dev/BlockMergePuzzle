using System.Collections;
using System.Collections.Generic;
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
        private const float Level1Time = 300f; // 5:00 for Level 1..5
        private const float Level6Time = 105f; // 1:45 for Level 6..7

        [Header("Tuỳ Chỉnh Thời Gian Chơi (Level Timer)")]
        [Tooltip("Nhập thời gian dạng MM:SS (VD: 05:00, 02:30, 01:45) hoặc sửa trực tiếp tại ô Text của TimeVal.")]
        [SerializeField] private string _customTimeMMSS = "";

        [Tooltip("Thời gian chơi tính bằng giây (0 = tự động lấy theo ô Text của TimeVal trên màn hình).")]
        [SerializeField] private float _customDurationSeconds = 0f;

        private float _initialInspectorTimeSeconds = 0f;
        private string _lastDisplayedTimeString = "";
        private float _timeRemaining = Level1Time;
        private bool _isTimerRunning = false;
        private bool _hasTimerStartedByInput = false;
        private bool _isPaused = false;

        public static bool TryParseTimeText(string raw, out float totalSeconds)
        {
            totalSeconds = 0f;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            string s = raw.Trim();

            if (s.Contains(":"))
            {
                string[] parts = s.Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0].Trim(), out int mins) && float.TryParse(parts[1].Trim(), out float secs))
                    {
                        totalSeconds = Mathf.Max(1f, mins * 60f + secs);
                        return true;
                    }
                }
                else if (parts.Length == 3)
                {
                    if (int.TryParse(parts[0].Trim(), out int hrs) &&
                        int.TryParse(parts[1].Trim(), out int mins) &&
                        float.TryParse(parts[2].Trim(), out float secs))
                    {
                        totalSeconds = Mathf.Max(1f, hrs * 3600f + mins * 60f + secs);
                        return true;
                    }
                }
            }
            else if (float.TryParse(s, out float rawSeconds) && rawSeconds > 0f)
            {
                totalSeconds = Mathf.Max(1f, rawSeconds);
                return true;
            }

            return false;
        }

        public static string FormatTimeMMSS(float durationSeconds)
        {
            float clamped = Mathf.Max(0f, durationSeconds);
            int mins = Mathf.FloorToInt(clamped / 60f);
            int secs = Mathf.FloorToInt(clamped % 60f);
            return string.Format("{0:D2}:{1:D2}", mins, secs);
        }

        public void SetPlayDurationSeconds(float seconds)
        {
            _customDurationSeconds = Mathf.Max(1f, seconds);
            _customTimeMMSS = FormatTimeMMSS(_customDurationSeconds);
            _initialInspectorTimeSeconds = _customDurationSeconds;
            _timeRemaining = _customDurationSeconds;
            EnsureTimeText();
            if (_timeText != null)
            {
                _timeText.text = _customTimeMMSS;
                _lastDisplayedTimeString = _timeText.text;
            }
        }

        private float GetLevelDurationSeconds()
        {
            if (_customDurationSeconds > 0f)
            {
                return _customDurationSeconds;
            }
            if (!string.IsNullOrWhiteSpace(_customTimeMMSS) && TryParseTimeText(_customTimeMMSS, out float customParsed))
            {
                return customParsed;
            }
            if (LevelManager.Instance != null && LevelManager.Instance.ActiveCustomLevelData != null && LevelManager.Instance.ActiveCustomLevelData.timeSeconds > 0)
            {
                return LevelManager.Instance.ActiveCustomLevelData.timeSeconds;
            }
            if (_initialInspectorTimeSeconds > 0f)
            {
                return _initialInspectorTimeSeconds;
            }
            if (_timeText != null && TryParseTimeText(_timeText.text, out float textParsed))
            {
                return textParsed;
            }

            int lvl = GetCurrentLevelNumber();
            if (lvl <= 5) return Level1Time;
            if (lvl == 6 || lvl == 7) return Level6Time;
            return DefaultLevelTime;
        }

        private void OnValidate()
        {
            if (_headerPanel == null)
            {
                _headerPanel = transform.Find("HeaderPanel")?.gameObject;
            }
            if (_timeText == null && _headerPanel != null)
            {
                Transform timeTr = _headerPanel.transform.Find("Banner_Time");
                if (timeTr != null) _timeText = timeTr.GetComponentInChildren<Text>(true);
            }

            float targetSec = 0f;
            if (!string.IsNullOrWhiteSpace(_customTimeMMSS) && TryParseTimeText(_customTimeMMSS, out float parsedStr))
            {
                targetSec = parsedStr;
            }
            else if (_customDurationSeconds > 0f)
            {
                targetSec = _customDurationSeconds;
            }

            if (targetSec > 0f)
            {
                _initialInspectorTimeSeconds = targetSec;
                string formatted = FormatTimeMMSS(targetSec);
                if (_timeText != null)
                {
                    _timeText.text = formatted;
                    _lastDisplayedTimeString = formatted;
                }
                if (Application.isPlaying)
                {
                    _timeRemaining = targetSec;
                }
            }
        }

        // Modals
        public const int MaxLevelNumber = 10;
        public const int BaseWinRewardCoins = 40;
        public const int AdsWinRewardCoins = 80;
        public const string PrefCurrentLevelKey = "BMP_CurrentLevel";
        public const string PrefJustWonLevelKey = "BMP_JustWonLevel";
        private const string PrefCoinsKey = "BMP_TotalCoins";
        private const string PrefLivesKey = "BMP_Lives";
        private const int DefaultLives = 3;
        private const int MaxLives = 5;
        private const int DefaultStartingCoinsIfUnset = 2000;
        public bool IsVictoryShowing => _victoryPanel != null && _victoryPanel.activeSelf;
        public bool IsTimeOutShowing => _timeOutPanel != null && _timeOutPanel.activeSelf;

        [Header("Time Out & Level Failed Settings")]
        [SerializeField] private int _reviveCoinCost = 1900;
        [SerializeField] private float _reviveExtraSeconds = 20f;
        [SerializeField] private int _reviveBundleCoins = 1400;
        [SerializeField] private string _reviveBundlePrice = "₫131,000";
        [SerializeField] private int _starterBundleCoins = 450;
        [SerializeField] private string _starterBundlePrice = "₫52,000";

        private GameObject _pausePanel;
        private GameObject _victoryPanel;
        private CanvasGroup _victoryCanvasGroup;
        private RectTransform _winTopBarRect;
        private Text _winTopLivesCountText;
        private Text _winTopLivesStatusText;
        private RectTransform _winTopCoinsPillRect;
        private RectTransform _winTopCoinTargetRect;
        private Text _winTopCoinsCountText;
        private RectTransform _winWellDoneRoot;
        private RectTransform _winWellDoneGlowRect;
        private RectTransform[] _winLetterTiles;
        private Vector2[] _winTileRestPositions;
        private RectTransform _winSparklesRoot;
        private RectTransform _winCoinsRoot;
        private RectTransform _winCoinsAuraOuter;
        private RectTransform _winCoinsSunburstRect;
        private RectTransform _winCoinsImageRect;
        private Text _winRewardAmountText;
        private RectTransform _winProgressRoot;
        private CanvasGroup _winProgressCanvasGroup;
        private RectTransform _winProgressFillRect;
        private Image[] _winProgressNodes;
        private GameObject[] _winProgressNodeGlows;
        private GameObject[] _winProgressNodeIcons;
        private Text[] _winProgressLabels;
        private RectTransform _winMilestoneBubbleRect;
        private RectTransform _winButtonsRoot;
        private CanvasGroup _winButtonsCanvasGroup;
        private Button _winGetBtn;
        private RectTransform _winGetBtnRect;
        private Vector2 _winGetBtnRestPos = new Vector2(-205f, 0f);
        private Text _winGetAmountText;
        private Button _winAdsBtn;
        private RectTransform _winAdsBtnRect;
        private Vector2 _winAdsBtnRestPos = new Vector2(212f, 0f);
        private RectTransform _winAdsShineRect;
        private Text _winAdsAmountText;
        private RectTransform _winEffectsLayer;
        private Coroutine _victoryAnimCoroutine;
        private Coroutine _winClaimCoroutine;
        private Coroutine _winAmbientConfettiCoroutine;
        private bool _isLoadingNextLevel = false;
        private bool _hasClaimedWinReward = false;
        private bool _winIntroComplete = false;
        private float _winIdleTime = 0f;

        // Time Out & Level Failed Modal
        private GameObject _timeOutPanel;
        private CanvasGroup _timeOutCanvasGroup;
        private RectTransform _timeOutTopBarRect;
        private RectTransform _timeOutLivesPillRect;
        private Image _timeOutLivesPillImg;
        private Text _timeOutLivesCountText;
        private Text _timeOutLivesTimerText;
        private RectTransform _timeOutCoinsPillRect;
        private Text _timeOutCoinsCountText;
        private RectTransform _timeOutCenterStackRect;
        private RectTransform _timeOutMainCardRect;
        private RectTransform _timeOutPlanetRingedRect;
        private RectTransform _timeOutPlanetMiniRect;
        private RectTransform _timeOutCardSparklesRoot;
        private Image _timeOutHeaderImg;
        private Text _timeOutHeaderText;

        private GameObject _timeOutStageRoot;
        private RectTransform _timeOutStopwatchRoot;
        private RectTransform _timeOutStopwatchAuraRect;
        private RectTransform _timeOutStopwatchSunburstRect;
        private RectTransform _timeOutStopwatchIconRect;
        private Text _timeOutExtraText;
        private Text _timeOutSubtitleText;
        private Button _buyTimeBtn;
        private RectTransform _buyTimeBtnRect;
        private Text _buyTimeCostText;
        private Button _adTimeBtn;
        private RectTransform _adTimeBtnRect;
        private RectTransform _adTimeShineRect;
        private Text _adTimeLabelText;

        private GameObject _levelFailedStageRoot;
        private RectTransform _failedHeartRoot;
        private RectTransform _failedAuraRect;
        private RectTransform _failedHeartLeftRect;
        private RectTransform _failedHeartRightRect;
        private RectTransform _failedCrackGlowRect;
        private Image _failedCrackGlowImg;
        private Text _failedMinusOneText;
        private Text _failedSubtitleText;
        private Button _retryBtn;
        private RectTransform _retryBtnRect;
        private RectTransform _retryShineRect;

        private RectTransform _offerCardRect;
        private GameObject _reviveOfferRoot;
        private Text _reviveOfferCoinsText;
        private Button _buyRevivePackBtn;
        private RectTransform _buyRevivePackBtnRect;
        private Text _buyRevivePackPriceText;
        private RectTransform _buyRevivePackShineRect;

        private GameObject _starterOfferRoot;
        private RectTransform _bestOfferRibbonRect;
        private Text _starterOfferCoinsText;
        private Button _buyStarterPackBtn;
        private RectTransform _buyStarterPackBtnRect;
        private Text _buyStarterPackPriceText;
        private RectTransform _buyStarterPackShineRect;

        private Button _tapToContinueBtn;
        private RectTransform _tapToContinueRect;
        private CanvasGroup _tapToContinueCanvasGroup;
        private RectTransform _timeOutEffectsLayer;

        private bool _isInLevelFailedState = false;
        private bool _timeOutIntroComplete = false;
        private bool _timeOutAllowContinue = false;
        private float _timeOutIdleTime = 0f;
        private float _timeOutLifeRegenSeconds = 28f * 60f + 16f;
        private int _timeOutLastTickSecond = -1;
        private Coroutine _timeOutAnimCoroutine;
        private Coroutine _timeOutTransitionCoroutine;
        private Coroutine _timeOutReviveCoroutine;

        private GameObject _feedbackPanel;
        private Text _feedbackText;
        private CanvasGroup _feedbackCanvasGroup;

        // Settings Toggles State (Pause Dialog matching [Image #7])
        private const string PrefSoundKey = "BMP_SoundEnabled";
        private const string PrefMusicKey = "BMP_MusicEnabled";
        private const string PrefVibrationKey = "BMP_VibrationEnabled";
        private const string PrefBoosterFreezeKey = "BMP_BoosterFreeze";
        private const string PrefBoosterMagnetKey = "BMP_BoosterMagnet";
        private const string PrefBoosterWandKey = "BMP_BoosterWand";

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
        public const int BoosterFreezeUnlockLevel = 8;
        public const int BoosterMagnetUnlockLevel = 12;
        public const int BoosterWandUnlockLevel = 16;
        public const float FreezeBoosterDuration = 15f;
        private const string PrefFreezeTutorialSeenKey = "BMP_FreezeTutorialSeen";
        private const string PrefMagnetTutorialSeenKey = "BMP_MagnetTutorialSeen";
        private const string PrefWandTutorialSeenKey = "BMP_WandTutorialSeen";

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
        private FreezeBoosterEffectController _freezeEffectController;
        private MagnetBoosterController _magnetController;
        private WandBoosterController _wandController;

        public bool IsBoosterTutorialShowing =>
            (_freezeEffectController != null && _freezeEffectController.IsTutorialShowing) ||
            (_magnetController != null && _magnetController.IsTutorialShowing) ||
            (_wandController != null && _wandController.IsTutorialShowing);

        public bool IsMagnetTargeting => _magnetController != null && _magnetController.IsTargeting;
        public bool IsMagnetBusy => _magnetController != null && _magnetController.IsBusy;
        public bool IsWandTargeting => _wandController != null && _wandController.IsTargeting;
        public bool IsWandBusy => _wandController != null && _wandController.IsBusy;

        public void HandleMagnetWorldTap(Vector3 worldPos)
        {
            _magnetController?.HandleWorldTap(worldPos);
        }

        public void HandleWandWorldTap(Vector3 worldPos)
        {
            _wandController?.HandleWorldTap(worldPos);
        }

        private FreezeBoosterEffectController EnsureFreezeEffectController()
        {
            if (_freezeEffectController == null)
            {
                _freezeEffectController = GetOrAddComponent<FreezeBoosterEffectController>(gameObject);
            }
            return _freezeEffectController;
        }

        private MagnetBoosterController EnsureMagnetController()
        {
            if (_magnetController == null)
            {
                _magnetController = GetOrAddComponent<MagnetBoosterController>(gameObject);
            }
            return _magnetController;
        }

        private WandBoosterController EnsureWandController()
        {
            if (_wandController == null)
            {
                _wandController = GetOrAddComponent<WandBoosterController>(gameObject);
            }
            return _wandController;
        }

        [ContextMenu("Reset Freeze Booster Tutorial")]
        public void ResetFreezeBoosterTutorial()
        {
            PlayerPrefs.DeleteKey(PrefFreezeTutorialSeenKey);
            _freezeCount = Mathf.Max(_freezeCount, 3);
            SaveBoosterCounts();
            UpdateBoosterUnlockStates();
        }

        [ContextMenu("Reset Magnet Booster Tutorial")]
        public void ResetMagnetBoosterTutorial()
        {
            PlayerPrefs.DeleteKey(PrefMagnetTutorialSeenKey);
            _magnetCount = Mathf.Max(_magnetCount, 3);
            SaveBoosterCounts();
            UpdateBoosterUnlockStates();
        }

        [ContextMenu("Reset Wand Booster Tutorial")]
        public void ResetWandBoosterTutorial()
        {
            PlayerPrefs.DeleteKey(PrefWandTutorialSeenKey);
            _wandCount = Mathf.Max(_wandCount, 3);
            SaveBoosterCounts();
            UpdateBoosterUnlockStates();
        }

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
            _freezeCount = PlayerPrefs.GetInt(PrefBoosterFreezeKey, _freezeCount);
            _magnetCount = PlayerPrefs.GetInt(PrefBoosterMagnetKey, _magnetCount);
            _wandCount = PlayerPrefs.GetInt(PrefBoosterWandKey, _wandCount);
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
                BindVictoryPanelUI();
            }
            else
            {
                RebuildVictoryPanel();
            }

            _timeOutPanel = transform.Find("TimeOutPanel")?.gameObject;
            if (_timeOutPanel != null)
            {
                BindTimeOutPanelUI();
            }
            else
            {
                RebuildTimeOutPanel();
            }

            _feedbackPanel = transform.Find("FeedbackBanner")?.gameObject;
            if (_feedbackPanel != null)
            {
                _feedbackCanvasGroup = _feedbackPanel.GetComponent<CanvasGroup>();
                _feedbackText = _feedbackPanel.GetComponentInChildren<Text>(true);
                if (_feedbackCanvasGroup != null) _feedbackCanvasGroup.alpha = 0f;
                if (_feedbackText != null) _feedbackText.text = string.Empty;
                _feedbackPanel.SetActive(false);
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
            else if (_initialInspectorTimeSeconds <= 0f)
            {
                // Capture whatever time string the user typed into TimeVal's Text in the Inspector (e.g., "05:00", "01:45", "02:30")
                if (TryParseTimeText(_timeText.text, out float inspectorSec))
                {
                    _initialInspectorTimeSeconds = inspectorSec;
                }
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

        [ContextMenu("Rebuild Victory Panel")]
        public void RebuildVictoryPanel()
        {
            int siblingIdx = -1;
            Transform oldVictory = transform.Find("VictoryPanel");
            if (oldVictory != null)
            {
                siblingIdx = oldVictory.GetSiblingIndex();
                if (Application.isPlaying) Destroy(oldVictory.gameObject);
                else DestroyImmediate(oldVictory.gameObject);
            }

            _victoryPanel = null;
            _victoryCanvasGroup = null;
            _winTopBarRect = null;
            _winTopLivesCountText = null;
            _winTopLivesStatusText = null;
            _winTopCoinsPillRect = null;
            _winTopCoinTargetRect = null;
            _winTopCoinsCountText = null;
            _winWellDoneRoot = null;
            _winWellDoneGlowRect = null;
            _winLetterTiles = null;
            _winTileRestPositions = null;
            _winSparklesRoot = null;
            _winCoinsRoot = null;
            _winCoinsAuraOuter = null;
            _winCoinsSunburstRect = null;
            _winCoinsImageRect = null;
            _winRewardAmountText = null;
            _winProgressRoot = null;
            _winProgressCanvasGroup = null;
            _winProgressFillRect = null;
            _winProgressNodes = null;
            _winProgressNodeGlows = null;
            _winProgressNodeIcons = null;
            _winProgressLabels = null;
            _winMilestoneBubbleRect = null;
            _winButtonsRoot = null;
            _winButtonsCanvasGroup = null;
            _winGetBtn = null;
            _winGetBtnRect = null;
            _winGetAmountText = null;
            _winAdsBtn = null;
            _winAdsBtnRect = null;
            _winAdsShineRect = null;
            _winAdsAmountText = null;
            _winEffectsLayer = null;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildVictoryPanel(font);

            if (_victoryPanel != null)
            {
                if (siblingIdx >= 0)
                {
                    _victoryPanel.transform.SetSiblingIndex(Mathf.Min(siblingIdx, transform.childCount - 1));
                }
                else
                {
                    Transform feedbackTr = transform.Find("FeedbackBanner");
                    if (feedbackTr != null)
                    {
                        _victoryPanel.transform.SetSiblingIndex(Mathf.Max(0, feedbackTr.GetSiblingIndex()));
                    }
                }
            }
        }

        private void BindVictoryPanelUI()
        {
            if (_victoryPanel == null) return;
            Transform winContent = _victoryPanel.transform.Find("WinContent");
            if (winContent == null ||
                winContent.Find("WinTopBar/CoinsPill/CoinTarget") == null ||
                winContent.Find("WellDoneRoot") == null ||
                winContent.Find("CoinsRewardRoot/CoinsSunburst") == null ||
                winContent.Find("ButtonsRow/Btn_Get") == null ||
                winContent.Find("ButtonsRow/Btn_Ads/ShineClip") == null)
            {
                // Automatically upgrade legacy VictoryPanel across all existing scenes
                RebuildVictoryPanel();
                return;
            }

            _victoryCanvasGroup = _victoryPanel.GetComponent<CanvasGroup>();
            if (_victoryCanvasGroup == null) _victoryCanvasGroup = _victoryPanel.AddComponent<CanvasGroup>();

            Transform topBarTr = winContent.Find("WinTopBar");
            _winTopBarRect = topBarTr as RectTransform;
            _winTopLivesCountText = topBarTr?.Find("LivesPill/LivesCountText")?.GetComponent<Text>();
            _winTopLivesStatusText = topBarTr?.Find("LivesPill/LivesStatusText")?.GetComponent<Text>();
            Transform coinsPillTr = topBarTr?.Find("CoinsPill");
            _winTopCoinsPillRect = coinsPillTr as RectTransform;
            _winTopCoinTargetRect = coinsPillTr?.Find("CoinTarget") as RectTransform;
            _winTopCoinsCountText = coinsPillTr?.Find("CoinsCountText")?.GetComponent<Text>();

            Transform wellDoneTr = winContent.Find("WellDoneRoot");
            _winWellDoneRoot = wellDoneTr as RectTransform;
            _winWellDoneGlowRect = wellDoneTr?.Find("WellDoneGlow") as RectTransform;
            _winSparklesRoot = wellDoneTr?.Find("WellDoneSparkles") as RectTransform;

            Transform lettersTr = wellDoneTr?.Find("LetterBlocks");
            if (lettersTr != null)
            {
                string[] tileNames = { "Tile_W", "Tile_E1", "Tile_L1", "Tile_L2", "Tile_D", "Tile_O", "Tile_N", "Tile_E2" };
                _winLetterTiles = new RectTransform[tileNames.Length];
                _winTileRestPositions = new Vector2[tileNames.Length];
                for (int i = 0; i < tileNames.Length; i++)
                {
                    RectTransform rt = lettersTr.Find(tileNames[i]) as RectTransform;
                    _winLetterTiles[i] = rt;
                    _winTileRestPositions[i] = rt != null ? rt.anchoredPosition : Vector2.zero;
                }
            }

            Transform coinsTr = winContent.Find("CoinsRewardRoot");
            _winCoinsRoot = coinsTr as RectTransform;
            _winCoinsAuraOuter = coinsTr?.Find("CoinsAuraOuter") as RectTransform;
            _winCoinsSunburstRect = coinsTr?.Find("CoinsSunburst") as RectTransform;
            _winCoinsImageRect = coinsTr?.Find("CoinsImage") as RectTransform;
            _winRewardAmountText = coinsTr?.Find("RewardAmountText")?.GetComponent<Text>();

            Transform progTr = winContent.Find("LevelProgressRoot");
            _winProgressRoot = progTr as RectTransform;
            if (progTr != null)
            {
                _winProgressCanvasGroup = progTr.GetComponent<CanvasGroup>();
                if (_winProgressCanvasGroup == null) _winProgressCanvasGroup = progTr.gameObject.AddComponent<CanvasGroup>();
                _winProgressFillRect = progTr.Find("TrackBar/TrackFill") as RectTransform;
                _winMilestoneBubbleRect = progTr.Find("MilestoneBubble") as RectTransform;

                _winProgressNodes = new Image[5];
                _winProgressNodeGlows = new GameObject[5];
                _winProgressNodeIcons = new GameObject[5];
                _winProgressLabels = new Text[5];

                for (int i = 0; i < 5; i++)
                {
                    Transform nodeTr = progTr.Find($"Node_{i}");
                    if (nodeTr == null) continue;
                    _winProgressNodeGlows[i] = nodeTr.Find("NodeGlow")?.gameObject;
                    Transform circleTr = nodeTr.Find("NodeCircle");
                    if (circleTr != null)
                    {
                        _winProgressNodes[i] = circleTr.GetComponent<Image>();
                        _winProgressNodeIcons[i] = circleTr.Find("NodeIcon")?.gameObject;
                    }
                    _winProgressLabels[i] = nodeTr.Find("LevelLabel")?.GetComponent<Text>();
                }
            }

            Transform buttonsTr = winContent.Find("ButtonsRow");
            _winButtonsRoot = buttonsTr as RectTransform;
            if (buttonsTr != null)
            {
                _winButtonsCanvasGroup = buttonsTr.GetComponent<CanvasGroup>();
                if (_winButtonsCanvasGroup == null) _winButtonsCanvasGroup = buttonsTr.gameObject.AddComponent<CanvasGroup>();
                Transform getTr = buttonsTr.Find("Btn_Get");
                if (getTr != null)
                {
                    _winGetBtn = getTr.GetComponent<Button>();
                    _winGetBtnRect = getTr as RectTransform;
                    if (_winGetBtnRect != null) _winGetBtnRestPos = _winGetBtnRect.anchoredPosition;
                    _winGetAmountText = getTr.Find("Content/AmountText")?.GetComponent<Text>();
                    if (_winGetBtn != null)
                    {
                        _winGetBtn.onClick.RemoveAllListeners();
                        _winGetBtn.onClick.AddListener(OnVictoryGetClicked);
                    }
                }

                Transform adsTr = buttonsTr.Find("Btn_Ads");
                if (adsTr != null)
                {
                    _winAdsBtn = adsTr.GetComponent<Button>();
                    _winAdsBtnRect = adsTr as RectTransform;
                    if (_winAdsBtnRect != null) _winAdsBtnRestPos = _winAdsBtnRect.anchoredPosition;
                    _winAdsShineRect = adsTr.Find("ShineClip/ShineSweep") as RectTransform;
                    _winAdsAmountText = adsTr.Find("Content/AmountText")?.GetComponent<Text>();
                    if (_winAdsBtn != null)
                    {
                        _winAdsBtn.onClick.RemoveAllListeners();
                        _winAdsBtn.onClick.AddListener(OnVictoryAdsClicked);
                    }
                }
            }

            EnsureWinProceduralSprites(winContent);
            UpdateVictoryBannerContent();
            _victoryPanel.SetActive(false);
        }

        private void EnsureWinProceduralSprites(Transform winContent)
        {
            if (winContent == null) return;

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite shardSp = GemSpriteManager.GetWinHelperSprite("win_shard") ?? SpriteGenerator.GetShardSprite();
            Sprite trackSp = GemSpriteManager.GetWinHelperSprite("win_progress_track") ?? SpriteGenerator.GetWinProgressTrackSprite();
            Sprite fillSp = GemSpriteManager.GetWinHelperSprite("win_progress_fill") ?? SpriteGenerator.GetWinProgressFillSprite();
            Sprite nodeSp = GemSpriteManager.GetWinHelperSprite("win_progress_node") ?? SpriteGenerator.GetWinProgressNodeSprite();
            Sprite bubbleSp = GemSpriteManager.GetWinHelperSprite("win_speech_bubble") ?? SpriteGenerator.GetWinSpeechBubbleSprite();
            Sprite sunburstSp = GemSpriteManager.GetWinHelperSprite("win_sunburst") ?? SpriteGenerator.GetWinSunburstSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();

            SetSpriteIfMissing(winContent.Find("WinTopBar/LivesPill"), GemSpriteManager.GetHomeSceneSprite("heart"));
            SetSpriteIfMissing(winContent.Find("WinTopBar/CoinsPill"), GemSpriteManager.GetHomeSceneSprite("coins"));
            SetSpriteIfMissing(winContent.Find("WellDoneRoot/WellDoneGlow"), glowSp);
            if (_winSparklesRoot != null)
            {
                for (int i = 0; i < _winSparklesRoot.childCount; i++)
                {
                    Transform ch = _winSparklesRoot.GetChild(i);
                    SetSpriteIfMissing(ch, ch.name.StartsWith("Confetti") ? shardSp : flareSp);
                }
            }

            if (_winLetterTiles != null)
            {
                string[] letterAssets = { "W", "E", "L", "N (2)", "D", "O", "N", "E (2)" };
                for (int i = 0; i < _winLetterTiles.Length && i < letterAssets.Length; i++)
                {
                    Transform tileTr = _winLetterTiles[i];
                    if (tileTr == null) continue;
                    Transform graphicTr = tileTr.Find("TileGraphic");
                    SetSpriteIfMissing(graphicTr != null ? graphicTr : tileTr, GemSpriteManager.GetWinSceneSprite(letterAssets[i]));
                }
            }

            SetSpriteIfMissing(winContent.Find("CoinsRewardRoot/CoinsAuraOuter"), glowSp);
            SetSpriteIfMissing(winContent.Find("CoinsRewardRoot/CoinsAuraInner"), glowSp);
            SetSpriteIfMissing(winContent.Find("CoinsRewardRoot/CoinsSunburst"), sunburstSp);
            SetSpriteIfMissing(winContent.Find("CoinsRewardRoot/CoinsImage"), GemSpriteManager.GetWinSceneSprite("Coins"));
            for (int i = 0; i < 3; i++)
            {
                SetSpriteIfMissing(winContent.Find($"CoinsRewardRoot/CoinSparkle_{i}"), flareSp);
            }

            SetSpriteIfMissing(winContent.Find("LevelProgressRoot/TrackBar"), trackSp);
            SetSpriteIfMissing(winContent.Find("LevelProgressRoot/TrackBar/TrackFill"), fillSp);
            for (int i = 0; i < 5; i++)
            {
                SetSpriteIfMissing(winContent.Find($"LevelProgressRoot/Node_{i}/NodeGlow"), glowSp);
                SetSpriteIfMissing(winContent.Find($"LevelProgressRoot/Node_{i}/NodeCircle"), nodeSp);
                SetSpriteIfMissing(winContent.Find($"LevelProgressRoot/Node_{i}/NodeCircle/NodeIcon"), flareSp);
            }

            SetSpriteIfMissing(winContent.Find("LevelProgressRoot/MilestoneBubble"), bubbleSp);
            SetSpriteIfMissing(winContent.Find("LevelProgressRoot/MilestoneBubble/BubbleIcon"), GemSpriteManager.GetNewBlockSprite("Group 343"));

            SetSpriteIfMissing(winContent.Find("ButtonsRow/Btn_Get"), GemSpriteManager.GetWinSceneSprite("Button1"));
            SetSpriteIfMissing(winContent.Find("ButtonsRow/Btn_Get/Content/CoinIcon"), GemSpriteManager.GetWinSceneSprite("Coin"));
            SetSpriteIfMissing(winContent.Find("ButtonsRow/Btn_Ads"), GemSpriteManager.GetWinSceneSprite("Button2"));
            SetSpriteIfMissing(winContent.Find("ButtonsRow/Btn_Ads/Content/AdsIcon"), GemSpriteManager.GetWinSceneSprite("ADS"));
            SetSpriteIfMissing(winContent.Find("ButtonsRow/Btn_Ads/ShineClip/ShineSweep"), shineSp);
        }

        private static void SetSpriteIfMissing(Transform tr, Sprite fallback)
        {
            if (tr == null || fallback == null) return;
            Image img = tr.GetComponent<Image>();
            if (img != null && img.sprite == null)
            {
                img.sprite = fallback;
            }
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

            Transform lockTr = slotTr.Find("LockedVisual") ?? slotTr.Find("Lock");
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
            return lm != null ? lm.CurrentLevelNumber : 1;
        }

        private int GetMaxLevelNumber()
        {
            return CustomLevelRepository.GetMaxLevelNumber();
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

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private GameObject EnsureLockedBoosterVisual(Transform slotTr, Font font)
        {
            if (slotTr == null) return null;
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Transform existing = slotTr.Find("LockedVisual");
            GameObject lockedObj = existing != null ? existing.gameObject : new GameObject("LockedVisual", typeof(RectTransform));
            if (existing == null)
            {
                lockedObj.transform.SetParent(slotTr, false);
            }

            RectTransform lRect = GetOrAddComponent<RectTransform>(lockedObj);
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            lRect.pivot = new Vector2(0.5f, 0.5f);
            lRect.offsetMin = Vector2.zero;
            lRect.offsetMax = Vector2.zero;
            lRect.localScale = Vector3.one;

            AspectRatioFitter fitter = GetOrAddComponent<AspectRatioFitter>(lockedObj);
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 393f / 416f;

            Image bgImg = GetOrAddComponent<Image>(lockedObj);
            Sprite lockedBgSp = GemSpriteManager.GetBoosterSprite("booster_locked_bg");
            if (lockedBgSp != null) bgImg.sprite = lockedBgSp;
            bgImg.color = Color.white;
            bgImg.preserveAspect = true;
            bgImg.raycastTarget = false;

            // Padlock Icon centered in cyan area
            Transform padlockTr = lockedObj.transform.Find("PadlockIcon");
            GameObject padlockObj = padlockTr != null ? padlockTr.gameObject : new GameObject("PadlockIcon", typeof(RectTransform));
            if (padlockTr == null) padlockObj.transform.SetParent(lockedObj.transform, false);

            RectTransform pRect = GetOrAddComponent<RectTransform>(padlockObj);
            pRect.anchorMin = new Vector2(0.305f, 0.355f);
            pRect.anchorMax = new Vector2(0.695f, 0.705f);
            pRect.offsetMin = Vector2.zero;
            pRect.offsetMax = Vector2.zero;

            Image padlockImg = GetOrAddComponent<Image>(padlockObj);
            Sprite lockSp = GemSpriteManager.GetBoosterSprite("lock");
            if (lockSp != null) padlockImg.sprite = lockSp;
            padlockImg.color = Color.white;
            padlockImg.preserveAspect = true;
            padlockImg.raycastTarget = false;

            Shadow pShadow = GetOrAddComponent<Shadow>(padlockObj);
            pShadow.effectColor = new Color(0.05f, 0.16f, 0.42f, 0.45f);
            pShadow.effectDistance = new Vector2(0f, -4f);

            // Unlock Level Text inside curved red cup
            Transform lvlTr = lockedObj.transform.Find("UnlockLevelText");
            GameObject lvlObj = lvlTr != null ? lvlTr.gameObject : new GameObject("UnlockLevelText", typeof(RectTransform));
            if (lvlTr == null) lvlObj.transform.SetParent(lockedObj.transform, false);

            RectTransform tRect = GetOrAddComponent<RectTransform>(lvlObj);
            tRect.anchorMin = new Vector2(0.20f, 0.215f);
            tRect.anchorMax = new Vector2(0.80f, 0.350f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            Text lvlText = GetOrAddComponent<Text>(lvlObj);
            if (font != null) lvlText.font = font;
            lvlText.fontStyle = FontStyle.Bold;
            lvlText.alignment = TextAnchor.MiddleCenter;
            lvlText.resizeTextForBestFit = true;
            lvlText.resizeTextMinSize = 14;
            lvlText.resizeTextMaxSize = 44;
            lvlText.color = new Color(1.0f, 0.96f, 0.76f);
            lvlText.raycastTarget = false;

            Outline tOutline = GetOrAddComponent<Outline>(lvlObj);
            tOutline.effectColor = new Color(0.34f, 0.06f, 0.05f, 0.98f);
            tOutline.effectDistance = new Vector2(2.2f, -2.2f);

            Shadow tShadow = GetOrAddComponent<Shadow>(lvlObj);
            tShadow.effectColor = new Color(0.22f, 0.03f, 0.03f, 0.95f);
            tShadow.effectDistance = new Vector2(0f, -3.2f);

            lockedObj.transform.SetAsLastSibling();
            return lockedObj;
        }

        private void SetBoosterSlotVisual(Transform slotTr, bool isUnlocked, int unlockLevel, int currentCount)
        {
            if (slotTr == null) return;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            for (int i = 0; i < slotTr.childCount; i++)
            {
                Transform child = slotTr.GetChild(i);
                if (child.name.StartsWith("Lock") && child.name != "LockedVisual")
                {
                    child.gameObject.SetActive(false);
                }
            }

            GameObject lockedVisual = EnsureLockedBoosterVisual(slotTr, font);
            Image bgImg = slotTr.GetComponent<Image>();
            Transform iconTr = slotTr.Find("Icon");
            Transform badgeTr = slotTr.Find("Badge_Count");

            if (!isUnlocked)
            {
                if (bgImg != null)
                {
                    bgImg.color = new Color(1f, 1f, 1f, 0f);
                }
                if (iconTr != null) iconTr.gameObject.SetActive(false);
                if (badgeTr != null) badgeTr.gameObject.SetActive(false);

                if (lockedVisual != null)
                {
                    lockedVisual.SetActive(true);
                    Text lvlText = lockedVisual.transform.Find("UnlockLevelText")?.GetComponent<Text>();
                    if (lvlText != null)
                    {
                        lvlText.text = "Lv." + unlockLevel;
                    }
                }
                return;
            }

            if (lockedVisual != null) lockedVisual.SetActive(false);

            if (bgImg != null)
            {
                bgImg.color = Color.white;
            }

            if (iconTr != null)
            {
                iconTr.gameObject.SetActive(true);
                Image iconImg = iconTr.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.color = Color.white;
                }
            }

            if (badgeTr != null)
            {
                badgeTr.gameObject.SetActive(true);
                Image bImg = badgeTr.GetComponent<Image>();
                if (bImg != null)
                {
                    bImg.color = new Color(0.10f, 0.14f, 0.28f, 0.96f);
                }

                Outline outline = badgeTr.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = new Color(0.35f, 0.90f, 1.0f, 0.92f);
                }

                Text text = badgeTr.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = "x" + currentCount;
                    text.fontSize = 26;
                    text.color = new Color(0.99f, 0.95f, 0.70f);
                }
            }
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRemainingBlocksChanged += HandleRemainingBlocksChanged;
                LevelManager.Instance.OnLevelInitialized += HandleLevelInitialized;
            }

            UpdateLevelDisplay();
            UpdateBoosterUnlockStates();
            StartTimer();
            TryShowFreezeBoosterTutorial();
            TryShowMagnetBoosterTutorial();
            TryShowWandBoosterTutorial();
        }

        private void HandleLevelInitialized()
        {
            UpdateLevelDisplay();
            UpdateBoosterUnlockStates();
            if (LevelManager.Instance != null && LevelManager.Instance.HasActiveCustomLevel)
            {
                ResetTimer();
            }
            TryShowFreezeBoosterTutorial();
            TryShowMagnetBoosterTutorial();
            TryShowWandBoosterTutorial();
        }

        private void TryShowFreezeBoosterTutorial()
        {
            if (!Application.isPlaying) return;
            if (IsVictoryShowing || IsTimeOutShowing) return;
            if (GetCurrentLevelNumber() != BoosterFreezeUnlockLevel) return;
            if (PlayerPrefs.GetInt(PrefFreezeTutorialSeenKey, 0) != 0) return;

            EnsureBoosterBar();
            if (_boosterFreezeBtn == null) return;

            if (_freezeCount < 3)
            {
                _freezeCount = 3;
                SaveBoosterCounts();
                UpdateBoosterUnlockStates();
            }

            RectTransform freezeRect = _boosterFreezeBtn.GetComponent<RectTransform>();
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster1");
            EnsureFreezeEffectController().ShowFirstTimeTutorial(freezeRect, iconSprite);
        }

        private void TryShowMagnetBoosterTutorial()
        {
            if (!Application.isPlaying) return;
            if (IsVictoryShowing || IsTimeOutShowing) return;
            if (GetCurrentLevelNumber() != BoosterMagnetUnlockLevel) return;
            if (PlayerPrefs.GetInt(PrefMagnetTutorialSeenKey, 0) != 0) return;

            EnsureBoosterBar();
            if (_boosterMagnetBtn == null) return;

            if (_magnetCount < 3)
            {
                _magnetCount = 3;
                SaveBoosterCounts();
                UpdateBoosterUnlockStates();
            }

            RectTransform magnetRect = _boosterMagnetBtn.GetComponent<RectTransform>();
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster2");
            EnsureMagnetController().ShowFirstTimeTutorial(magnetRect, iconSprite);
        }

        private void TryShowWandBoosterTutorial()
        {
            if (!Application.isPlaying) return;
            if (IsVictoryShowing || IsTimeOutShowing) return;
            if (GetCurrentLevelNumber() != BoosterWandUnlockLevel) return;
            if (PlayerPrefs.GetInt(PrefWandTutorialSeenKey, 0) != 0) return;

            EnsureBoosterBar();
            if (_boosterWandBtn == null) return;

            if (_wandCount < 3)
            {
                _wandCount = 3;
                SaveBoosterCounts();
                UpdateBoosterUnlockStates();
            }

            RectTransform wandRect = _boosterWandBtn.GetComponent<RectTransform>();
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster3");
            EnsureWandController().ShowFirstTimeTutorial(wandRect, iconSprite);
        }

        private void OnDestroy()
        {
            CleanupWinEffects();
            CleanupTimeOutEffects();
            _freezeEffectController?.StopImmediate();
            _magnetController?.StopImmediate();
            _wandController?.StopImmediate();
            if (Instance == this)
            {
                Instance = null;
            }
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnRemainingBlocksChanged -= HandleRemainingBlocksChanged;
                LevelManager.Instance.OnLevelInitialized -= HandleLevelInitialized;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (IsVictoryShowing && _winIntroComplete)
            {
                float dt = Time.unscaledDeltaTime;
                _winIdleTime += dt;
                float t = _winIdleTime;

                if (_winCoinsSunburstRect != null)
                {
                    _winCoinsSunburstRect.Rotate(0f, 0f, -24f * dt);
                }
                if (_winCoinsImageRect != null)
                {
                    _winCoinsImageRect.anchoredPosition = new Vector2(0f, 22f + Mathf.Sin(t * 2.4f) * 5.5f);
                }
                if (_winCoinsAuraOuter != null)
                {
                    float pulse = 1f + Mathf.Sin(t * 2.5f) * 0.06f;
                    _winCoinsAuraOuter.localScale = new Vector3(pulse, pulse, 1f);
                }
                if (_winSparklesRoot != null)
                {
                    for (int i = 0; i < _winSparklesRoot.childCount; i++)
                    {
                        Transform child = _winSparklesRoot.GetChild(i);
                        if (child == null) continue;
                        float s = 0.78f + 0.30f * Mathf.Sin(t * 4.2f + i * 1.1f);
                        child.localScale = new Vector3(s, s, 1f);
                    }
                }
                if (_winLetterTiles != null && _winTileRestPositions != null)
                {
                    for (int i = 0; i < _winLetterTiles.Length && i < _winTileRestPositions.Length; i++)
                    {
                        if (_winLetterTiles[i] == null) continue;
                        _winLetterTiles[i].anchoredPosition = _winTileRestPositions[i] + new Vector2(0f, Mathf.Sin(t * 2.8f + i * 0.55f) * 2.5f);
                    }
                }
                if (_winMilestoneBubbleRect != null)
                {
                    Vector2 bp = _winMilestoneBubbleRect.anchoredPosition;
                    _winMilestoneBubbleRect.anchoredPosition = new Vector2(bp.x, 76f + Mathf.Sin(t * 3.0f) * 5.0f);
                }
                if (_winAdsBtnRect != null)
                {
                    float p = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 2.4f)), 6f);
                    _winAdsBtnRect.localScale = Vector3.one * (1f + 0.040f * p);
                }
                if (_winAdsShineRect != null)
                {
                    float cycle = t % 2.6f;
                    float sx = cycle < 0.65f ? Mathf.Lerp(-280f, 280f, cycle / 0.65f) : -280f;
                    _winAdsShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
            }

            if (IsTimeOutShowing)
            {
                float dt = Time.unscaledDeltaTime;
                _timeOutLifeRegenSeconds -= dt;
                if (_timeOutLifeRegenSeconds <= 0f) _timeOutLifeRegenSeconds = 30f * 60f;
                if (_timeOutLivesTimerText != null)
                {
                    _timeOutLivesTimerText.text = FormatTimeMMSS(_timeOutLifeRegenSeconds);
                }

                if (_timeOutIntroComplete)
                {
                    _timeOutIdleTime += dt;
                    UpdateTimeOutIdleAnimations(_timeOutIdleTime, dt);
                }
            }

            // Detect live Inspector edit on TimeVal.text while in Play Mode
            if (_timeText != null && !string.IsNullOrEmpty(_lastDisplayedTimeString) && _timeText.text != _lastDisplayedTimeString)
            {
                if (TryParseTimeText(_timeText.text, out float liveEditedSec))
                {
                    _initialInspectorTimeSeconds = liveEditedSec;
                    _timeRemaining = liveEditedSec;
                    UpdateTimeDisplay();
                    return;
                }
            }

            if (_freezeEffectController != null && _freezeEffectController.IsActivating)
            {
                // Keep level timer frozen while Freeze booster flies to the timer banner
            }
            else if (_freezeTimerRemaining > 0f)
            {
                if (!_isPaused && !IsVictoryShowing && !IsTimeOutShowing)
                {
                    _freezeTimerRemaining -= Time.deltaTime;
                    _freezeEffectController?.UpdateFreezeRemaining(_freezeTimerRemaining, FreezeBoosterDuration);
                    if (_freezeTimerRemaining <= 0f)
                    {
                        _freezeTimerRemaining = 0f;
                        UpdateTimeDisplay();
                        _freezeEffectController?.PlayFreezeMeltOut();
                    }
                }
            }
            else if (_isTimerRunning && !_isPaused)
            {
                _timeRemaining -= Time.deltaTime;
                if (_timeRemaining <= 0f)
                {
                    _timeRemaining = 0f;
                    _timeOutLastTickSecond = -1;
                    _isTimerRunning = false;
                    UpdateTimeDisplay();
                    OnTimeExpired();
                }
                else
                {
                    UpdateTimeDisplay();
                    if (_timeRemaining <= 5f)
                    {
                        int wholeSec = Mathf.CeilToInt(_timeRemaining);
                        if (wholeSec != _timeOutLastTickSecond)
                        {
                            _timeOutLastTickSecond = wholeSec;
                            if (_timeText != null) StartCoroutine(CoPulseTransform(_timeText.transform, 1.20f, 0.22f));
                        }
                    }
                    else
                    {
                        _timeOutLastTickSecond = -1;
                    }
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

            // 5. Time Out & Level Failed Modal
            BuildTimeOutPanel(font);

            // 6. Feedback Banner
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

            // Locked Visual (glossy cyan-blue + curved red cup + padlock + Lv.X)
            lockRef = EnsureLockedBoosterVisual(slotObj.transform, font);
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
            SetImageColor(_victoryPanel, new Color(0.04f, 0.02f, 0.12f, 0.85f));
            _victoryCanvasGroup = _victoryPanel.AddComponent<CanvasGroup>();
            _victoryCanvasGroup.alpha = 1f;

            GameObject winContent = CreateUIContainer("WinContent", _victoryPanel.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite shardSp = GemSpriteManager.GetWinHelperSprite("win_shard") ?? SpriteGenerator.GetShardSprite();
            Sprite trackSp = GemSpriteManager.GetWinHelperSprite("win_progress_track") ?? SpriteGenerator.GetWinProgressTrackSprite();
            Sprite fillSp = GemSpriteManager.GetWinHelperSprite("win_progress_fill") ?? SpriteGenerator.GetWinProgressFillSprite();
            Sprite nodeSp = GemSpriteManager.GetWinHelperSprite("win_progress_node") ?? SpriteGenerator.GetWinProgressNodeSprite();
            Sprite bubbleSp = GemSpriteManager.GetWinHelperSprite("win_speech_bubble") ?? SpriteGenerator.GetWinSpeechBubbleSprite();
            Sprite sunburstSp = GemSpriteManager.GetWinHelperSprite("win_sunburst") ?? SpriteGenerator.GetWinSunburstSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();

            // 0. Top Resource Bar (Lives & Coins pills matching HomeScene)
            GameObject topBarObj = CreateUIContainer("WinTopBar", winContent.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(680f, 118f));
            _winTopBarRect = topBarObj.GetComponent<RectTransform>();

            Image livesPill = CreateWinImage("LivesPill", _winTopBarRect, new Vector2(-156f, 0f), new Vector2(274f, 102f),
                GemSpriteManager.GetHomeSceneSprite("heart"), Color.white, preserveAspect: true);
            _winTopLivesCountText = CreateStyledText("LivesCountText", livesPill.transform, "5", font, 38,
                Color.white, new Vector2(-86f, 7f), new Vector2(64f, 52f), true,
                new Color(0.20f, 0.02f, 0.09f, 0.90f), new Vector2(0f, -2.5f));
            Outline topLivesOutline = _winTopLivesCountText.gameObject.AddComponent<Outline>();
            topLivesOutline.effectColor = new Color(0.361f, 0.055f, 0.157f, 0.98f);
            topLivesOutline.effectDistance = new Vector2(2.2f, -2.2f);
            _winTopLivesStatusText = CreateStyledText("LivesStatusText", livesPill.transform, "Full", font, 33,
                new Color(0.153f, 0.094f, 0.329f), new Vector2(46f, 2f), new Vector2(148f, 52f), true,
                new Color(1f, 1f, 1f, 0.55f), new Vector2(0f, -1f));

            Image coinsPill = CreateWinImage("CoinsPill", _winTopBarRect, new Vector2(156f, 0f), new Vector2(284f, 112f),
                GemSpriteManager.GetHomeSceneSprite("coins"), Color.white, preserveAspect: true);
            _winTopCoinsPillRect = coinsPill.rectTransform;
            GameObject coinTargetObj = CreateUIContainer("CoinTarget", _winTopCoinsPillRect,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-95f, 0f), new Vector2(70f, 70f));
            _winTopCoinTargetRect = coinTargetObj.GetComponent<RectTransform>();
            _winTopCoinsCountText = CreateStyledText("CoinsCountText", _winTopCoinsPillRect, "0", font, 36,
                new Color(0.153f, 0.094f, 0.329f), new Vector2(46f, 3f), new Vector2(156f, 54f), true,
                new Color(1f, 1f, 1f, 0.55f), new Vector2(0f, -1f));

            // 1. WELL DONE 2x4 Lego Block Banner
            GameObject wellDoneObj = CreateUIContainer("WellDoneRoot", winContent.transform, new Vector2(0.5f, 0.785f), new Vector2(0.5f, 0.785f), Vector2.zero, new Vector2(640f, 320f));
            _winWellDoneRoot = wellDoneObj.GetComponent<RectTransform>();

            Image wellGlowImg = CreateWinImage("WellDoneGlow", wellDoneObj.transform, new Vector2(0f, 6f), new Vector2(820f, 480f),
                glowSp, new Color(1.0f, 0.50f, 0.92f, 0.72f), preserveAspect: false);
            _winWellDoneGlowRect = wellGlowImg.GetComponent<RectTransform>();

            GameObject sparklesObj = CreateUIContainer("WellDoneSparkles", wellDoneObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 380f));
            _winSparklesRoot = sparklesObj.GetComponent<RectTransform>();

            CreateWinSparkle("Sparkle_0", _winSparklesRoot, new Vector2(-280f, 128f), 46f, flareSp, new Color(0.290f, 0.957f, 1.000f, 1f));
            CreateWinSparkle("Sparkle_1", _winSparklesRoot, new Vector2(-168f, 174f), 40f, flareSp, new Color(1.000f, 0.416f, 0.910f, 1f));
            CreateWinSparkle("Sparkle_2", _winSparklesRoot, new Vector2(-42f, 182f), 50f, flareSp, new Color(1.000f, 0.914f, 0.333f, 1f));
            CreateWinSparkle("Sparkle_3", _winSparklesRoot, new Vector2(102f, 176f), 42f, flareSp, new Color(0.482f, 1.000f, 0.361f, 1f));
            CreateWinSparkle("Sparkle_4", _winSparklesRoot, new Vector2(246f, 148f), 52f, flareSp, new Color(1.000f, 0.886f, 0.302f, 1f));
            CreateWinSparkle("Sparkle_5", _winSparklesRoot, new Vector2(290f, 34f), 40f, flareSp, new Color(1.000f, 0.604f, 0.267f, 1f));
            CreateWinSparkle("Sparkle_6", _winSparklesRoot, new Vector2(-294f, -38f), 42f, flareSp, new Color(0.361f, 0.922f, 1.000f, 1f));
            CreateWinSparkle("Sparkle_7", _winSparklesRoot, new Vector2(268f, -126f), 44f, flareSp, new Color(1.000f, 0.459f, 0.710f, 1f));

            CreateWinConfetti("Confetti_0", _winSparklesRoot, new Vector2(-235f, 160f), 15f, 22f, shardSp, new Color(1.00f, 0.45f, 0.78f, 0.95f));
            CreateWinConfetti("Confetti_1", _winSparklesRoot, new Vector2(-114f, -148f), 14f, -35f, shardSp, new Color(0.35f, 0.94f, 1.00f, 0.95f));
            CreateWinConfetti("Confetti_2", _winSparklesRoot, new Vector2(20f, 168f), 15f, 45f, shardSp, new Color(1.00f, 0.89f, 0.32f, 0.95f));
            CreateWinConfetti("Confetti_3", _winSparklesRoot, new Vector2(174f, 162f), 17f, -20f, shardSp, new Color(0.45f, 1.00f, 0.45f, 0.95f));
            CreateWinConfetti("Confetti_4", _winSparklesRoot, new Vector2(244f, -44f), 15f, 60f, shardSp, new Color(1.00f, 0.56f, 0.24f, 0.95f));
            CreateWinConfetti("Confetti_5", _winSparklesRoot, new Vector2(-248f, -128f), 14f, -50f, shardSp, new Color(1.00f, 0.90f, 0.35f, 0.95f));
            CreateWinConfetti("Confetti_6", _winSparklesRoot, new Vector2(114f, -152f), 14f, 30f, shardSp, new Color(1.00f, 0.40f, 0.85f, 0.95f));
            CreateWinConfetti("Confetti_7", _winSparklesRoot, new Vector2(-10f, -154f), 15f, 15f, shardSp, new Color(0.45f, 1.00f, 0.52f, 0.95f));

            GameObject lettersObj = CreateUIContainer("LetterBlocks", wellDoneObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 300f));

            // Bottom row (DONE) created first so top row (WELL) overlaps cleanly
            RectTransform tileD = CreateWinLetterTile("Tile_D", lettersObj.transform, new Vector2(-194.6f, -79.6f), new Vector2(163.5f, 187.0f), "D");
            RectTransform tileO = CreateWinLetterTile("Tile_O", lettersObj.transform, new Vector2(-65.2f, -79.85f), new Vector2(153.6f, 185.3f), "O");
            RectTransform tileN = CreateWinLetterTile("Tile_N", lettersObj.transform, new Vector2(64.3f, -80.15f), new Vector2(144.6f, 179.8f), "N");
            RectTransform tileE2 = CreateWinLetterTile("Tile_E2", lettersObj.transform, new Vector2(193.3f, -81.05f), new Vector2(150.2f, 182.0f), "E (2)");

            // Top row (WELL): N (2).png is the yellow-lime second 'L' tile (needs backing fill for transparent cutout inside 'L')
            RectTransform tileW = CreateWinLetterTile("Tile_W", lettersObj.transform, new Vector2(-196.8f, 76.1f), new Vector2(173.0f, 176.8f), "W");
            RectTransform tileE1 = CreateWinLetterTile("Tile_E1", lettersObj.transform, new Vector2(-63.5f, 77.9f), new Vector2(154.5f, 187.9f), "E");
            RectTransform tileL1 = CreateWinLetterTile("Tile_L1", lettersObj.transform, new Vector2(64.3f, 75.6f), new Vector2(147.0f, 173.5f), "L");
            RectTransform tileL2 = CreateWinLetterTileWithBacking("Tile_L2", lettersObj.transform, new Vector2(193.3f, 76.2f), new Vector2(145.7f, 173.3f), "N (2)",
                new Vector2(8.75f, 4.77f), new Vector2(54f, 94f), new Color(0.996f, 0.992f, 0.965f, 1f));

            _winLetterTiles = new RectTransform[] { tileW, tileE1, tileL1, tileL2, tileD, tileO, tileN, tileE2 };
            _winTileRestPositions = new Vector2[_winLetterTiles.Length];
            for (int i = 0; i < _winLetterTiles.Length; i++)
            {
                _winTileRestPositions[i] = _winLetterTiles[i] != null ? _winLetterTiles[i].anchoredPosition : Vector2.zero;
            }

            // 2. Center Coins Reward & "+40"
            GameObject coinsRootObj = CreateUIContainer("CoinsRewardRoot", winContent.transform, new Vector2(0.5f, 0.525f), new Vector2(0.5f, 0.525f), Vector2.zero, new Vector2(480f, 420f));
            _winCoinsRoot = coinsRootObj.GetComponent<RectTransform>();

            Image outerAura = CreateWinImage("CoinsAuraOuter", _winCoinsRoot, new Vector2(0f, 14f), new Vector2(580f, 580f),
                glowSp, new Color(1.00f, 0.42f, 0.92f, 0.88f), preserveAspect: false);
            _winCoinsAuraOuter = outerAura.GetComponent<RectTransform>();

            CreateWinImage("CoinsAuraInner", _winCoinsRoot, new Vector2(0f, 18f), new Vector2(400f, 400f),
                glowSp, new Color(1.0f, 0.92f, 0.68f, 0.78f), preserveAspect: false);

            Image sunburstImg = CreateWinImage("CoinsSunburst", _winCoinsRoot, new Vector2(0f, 22f), new Vector2(520f, 520f),
                sunburstSp, new Color(1.0f, 0.88f, 0.36f, 0.55f), preserveAspect: true);
            _winCoinsSunburstRect = sunburstImg.GetComponent<RectTransform>();

            Image coinsImg = CreateWinImage("CoinsImage", _winCoinsRoot, new Vector2(0f, 22f), new Vector2(340f, 344f),
                GemSpriteManager.GetWinSceneSprite("Coins"), Color.white, preserveAspect: true);
            _winCoinsImageRect = coinsImg.GetComponent<RectTransform>();

            CreateWinSparkle("CoinSparkle_0", _winCoinsRoot, new Vector2(-142f, 108f), 42f, flareSp, new Color(1.00f, 0.96f, 0.80f, 1f));
            CreateWinSparkle("CoinSparkle_1", _winCoinsRoot, new Vector2(148f, 76f), 38f, flareSp, new Color(1.00f, 0.96f, 0.80f, 1f));
            CreateWinSparkle("CoinSparkle_2", _winCoinsRoot, new Vector2(116f, -42f), 34f, flareSp, new Color(1.00f, 0.96f, 0.80f, 1f));

            _winRewardAmountText = CreateStyledText("RewardAmountText", _winCoinsRoot, "+" + BaseWinRewardCoins, font, 90,
                Color.white, new Vector2(0f, -124f), new Vector2(340f, 114f), true,
                new Color(0.06f, 0.01f, 0.14f, 0.92f), new Vector2(0f, -6f));
            Outline rewardOutline = _winRewardAmountText.gameObject.AddComponent<Outline>();
            rewardOutline.effectColor = new Color(0.141f, 0.031f, 0.243f, 0.98f); // #24083E
            rewardOutline.effectDistance = new Vector2(4f, -4f);

            // 3. Level Progress Bar & Milestone Bubble
            GameObject progObj = CreateUIContainer("LevelProgressRoot", winContent.transform, new Vector2(0.5f, 0.345f), new Vector2(0.5f, 0.345f), Vector2.zero, new Vector2(860f, 200f));
            _winProgressRoot = progObj.GetComponent<RectTransform>();
            _winProgressCanvasGroup = progObj.AddComponent<CanvasGroup>();
            _winProgressCanvasGroup.alpha = 1f;

            Image trackImg = CreateWinImage("TrackBar", _winProgressRoot, Vector2.zero, new Vector2(820f, 28f), trackSp, Color.white, preserveAspect: false);
            GameObject fillObj = CreateUIPanel("TrackFill", trackImg.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(410f, 28f));
            _winProgressFillRect = fillObj.GetComponent<RectTransform>();
            _winProgressFillRect.pivot = new Vector2(0f, 0.5f);
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.sprite = fillSp;
            fillImg.color = Color.white;
            fillImg.raycastTarget = false;

            _winProgressNodes = new Image[5];
            _winProgressNodeGlows = new GameObject[5];
            _winProgressNodeIcons = new GameObject[5];
            _winProgressLabels = new Text[5];

            for (int i = 0; i < 5; i++)
            {
                float nodeX = -300f + i * 150f;
                GameObject nodeContainer = CreateUIContainer($"Node_{i}", _winProgressRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(nodeX, 0f), new Vector2(120f, 140f));

                Image nodeGlow = CreateWinImage("NodeGlow", nodeContainer.transform, Vector2.zero, new Vector2(116f, 116f),
                    glowSp, new Color(1.0f, 0.48f, 0.96f, 0.85f), preserveAspect: false);
                _winProgressNodeGlows[i] = nodeGlow.gameObject;

                Image nodeCircle = CreateWinImage("NodeCircle", nodeContainer.transform, Vector2.zero, new Vector2(52f, 52f),
                    nodeSp, Color.white, preserveAspect: true);
                _winProgressNodes[i] = nodeCircle;

                Image nodeIcon = CreateWinImage("NodeIcon", nodeCircle.transform, Vector2.zero, new Vector2(32f, 32f),
                    flareSp, new Color(1.0f, 0.97f, 0.88f, 1f), preserveAspect: true);
                _winProgressNodeIcons[i] = nodeIcon.gameObject;

                Text lvlLabel = CreateStyledText("LevelLabel", nodeContainer.transform, "Lv" + (i + 1), font, 30,
                    Color.white, new Vector2(0f, -48f), new Vector2(120f, 44f), true,
                    new Color(0.05f, 0.01f, 0.12f, 0.88f), new Vector2(0f, -3f));
                Outline lblOutline = lvlLabel.gameObject.AddComponent<Outline>();
                lblOutline.effectColor = new Color(0.082f, 0.024f, 0.157f, 0.95f); // #150628
                lblOutline.effectDistance = new Vector2(2f, -2f);
                _winProgressLabels[i] = lvlLabel;
            }

            Image bubbleImg = CreateWinImage("MilestoneBubble", _winProgressRoot, new Vector2(0f, 76f), new Vector2(92f, 98f),
                bubbleSp, Color.white, preserveAspect: true);
            _winMilestoneBubbleRect = bubbleImg.GetComponent<RectTransform>();
            CreateWinImage("BubbleIcon", bubbleImg.transform, new Vector2(0f, 7f), new Vector2(54f, 54f),
                GemSpriteManager.GetNewBlockSprite("Group 343"), Color.white, preserveAspect: true);

            // 4. Bottom Action Buttons (Get 40 & AD 80)
            GameObject buttonsObj = CreateUIContainer("ButtonsRow", winContent.transform, new Vector2(0.5f, 0.195f), new Vector2(0.5f, 0.195f), Vector2.zero, new Vector2(880f, 250f));
            _winButtonsRoot = buttonsObj.GetComponent<RectTransform>();
            _winButtonsCanvasGroup = buttonsObj.AddComponent<CanvasGroup>();
            _winButtonsCanvasGroup.alpha = 1f;

            // Left Button: Get [Coin] 40
            GameObject getObj = CreateUIPanel("Btn_Get", _winButtonsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-205f, 0f), new Vector2(412f, 236f));
            _winGetBtnRect = getObj.GetComponent<RectTransform>();
            _winGetBtnRestPos = _winGetBtnRect.anchoredPosition;
            Image getImg = getObj.GetComponent<Image>();
            getImg.sprite = GemSpriteManager.GetWinSceneSprite("Button1");
            getImg.preserveAspect = true;
            getImg.color = Color.white;
            _winGetBtn = getObj.AddComponent<Button>();
            _winGetBtn.targetGraphic = getImg;
            _winGetBtn.onClick.AddListener(OnVictoryGetClicked);

            GameObject getContent = CreateUIContainer("Content", getObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-7f, 0f), new Vector2(360f, 190f));
            Text getLabel = CreateStyledText("GetLabel", getContent.transform, "Get", font, 46,
                Color.white, new Vector2(0f, 40f), new Vector2(300f, 64f), true,
                new Color(0.03f, 0.08f, 0.18f, 0.90f), new Vector2(0f, -4f));
            Outline getLblOutline = getLabel.gameObject.AddComponent<Outline>();
            getLblOutline.effectColor = new Color(0.039f, 0.098f, 0.208f, 0.96f); // #0A1935
            getLblOutline.effectDistance = new Vector2(2.5f, -2.5f);

            CreateWinImage("CoinIcon", getContent.transform, new Vector2(-58f, -32f), new Vector2(80f, 80f),
                GemSpriteManager.GetWinSceneSprite("Coin"), Color.white, preserveAspect: true);

            _winGetAmountText = CreateStyledText("AmountText", getContent.transform, BaseWinRewardCoins.ToString(), font, 64,
                Color.white, new Vector2(48f, -34f), new Vector2(140f, 74f), true,
                new Color(0.03f, 0.08f, 0.18f, 0.92f), new Vector2(0f, -4f));
            _winGetAmountText.alignment = TextAnchor.MiddleLeft;
            Outline getAmtOutline = _winGetAmountText.gameObject.AddComponent<Outline>();
            getAmtOutline.effectColor = new Color(0.039f, 0.098f, 0.208f, 0.96f);
            getAmtOutline.effectDistance = new Vector2(3f, -3f);

            // Right Button: [AD] 80
            GameObject adsObj = CreateUIPanel("Btn_Ads", _winButtonsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(212f, 0f), new Vector2(420f, 234f));
            _winAdsBtnRect = adsObj.GetComponent<RectTransform>();
            _winAdsBtnRestPos = _winAdsBtnRect.anchoredPosition;
            Image adsImg = adsObj.GetComponent<Image>();
            adsImg.sprite = GemSpriteManager.GetWinSceneSprite("Button2");
            adsImg.preserveAspect = true;
            adsImg.color = Color.white;
            _winAdsBtn = adsObj.AddComponent<Button>();
            _winAdsBtn.targetGraphic = adsImg;
            _winAdsBtn.onClick.AddListener(OnVictoryAdsClicked);

            GameObject adsContent = CreateUIContainer("Content", adsObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 190f));
            CreateWinImage("AdsIcon", adsContent.transform, new Vector2(-68f, 4f), new Vector2(122f, 142f),
                GemSpriteManager.GetWinSceneSprite("ADS"), Color.white, preserveAspect: true);

            _winAdsAmountText = CreateStyledText("AmountText", adsContent.transform, AdsWinRewardCoins.ToString(), font, 76,
                Color.white, new Vector2(64f, -2f), new Vector2(160f, 88f), true,
                new Color(0.18f, 0.02f, 0.12f, 0.92f), new Vector2(0f, -5f));
            Outline adsAmtOutline = _winAdsAmountText.gameObject.AddComponent<Outline>();
            adsAmtOutline.effectColor = new Color(0.290f, 0.051f, 0.196f, 0.98f); // #4A0D32
            adsAmtOutline.effectDistance = new Vector2(3.5f, -3.5f);

            GameObject shineClip = CreateUIContainer("ShineClip", adsObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(372f, 180f));
            shineClip.AddComponent<RectMask2D>();
            Image shineImg = CreateWinImage("ShineSweep", shineClip.transform, new Vector2(-280f, 0f), new Vector2(130f, 220f),
                shineSp, new Color(1f, 1f, 1f, 0.42f), preserveAspect: false);
            _winAdsShineRect = shineImg.GetComponent<RectTransform>();

            UpdateVictoryBannerContent();
            _victoryPanel.SetActive(false);
        }

        private RectTransform CreateWinLetterTileWithBacking(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, string spriteName, Vector2 backingOffset, Vector2 backingSize, Color backingColor)
        {
            GameObject root = CreateUIContainer(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
            CreateWinImage("FillBacking", root.transform, backingOffset, backingSize, null, backingColor, preserveAspect: false);
            GameObject graphicObj = CreateUIPanel("TileGraphic", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image gImg = graphicObj.GetComponent<Image>();
            gImg.sprite = GemSpriteManager.GetWinSceneSprite(spriteName);
            gImg.color = Color.white;
            gImg.preserveAspect = true;
            gImg.raycastTarget = false;
            return root.GetComponent<RectTransform>();
        }

        private GameObject CreateUIContainer(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            return obj;
        }

        private Image CreateWinImage(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite, Color color, bool preserveAspect)
        {
            GameObject obj = CreateUIPanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return img;
        }

        private RectTransform CreateWinLetterTile(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, string spriteName)
        {
            Image img = CreateWinImage(name, parent, anchoredPos, sizeDelta, GemSpriteManager.GetWinSceneSprite(spriteName), Color.white, preserveAspect: true);
            return img.GetComponent<RectTransform>();
        }

        private void CreateWinSparkle(string name, Transform parent, Vector2 anchoredPos, float size, Sprite flareSprite, Color color)
        {
            CreateWinImage(name, parent, anchoredPos, new Vector2(size, size), flareSprite, color, preserveAspect: true);
        }

        private void CreateWinConfetti(string name, Transform parent, Vector2 anchoredPos, float size, float rotZ, Sprite shardSprite, Color color)
        {
            Image img = CreateWinImage(name, parent, anchoredPos, new Vector2(size, size), shardSprite, color, preserveAspect: false);
            img.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, rotZ);
        }

        private Text CreateStyledText(string name, Transform parent, string content, Font font, int fontSize,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, bool centeredRect, Color shadowColor, Vector2 shadowDist)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            if (centeredRect)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPos;
                rect.sizeDelta = sizeDelta;
            }
            else
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = anchoredPos;
                rect.sizeDelta = sizeDelta;
            }

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.raycastTarget = false;

            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDist;
            return txt;
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
            _feedbackPanel.SetActive(false);
        }

        public void StartTimer()
        {
            _timeRemaining = GetLevelDurationSeconds();
            _freezeTimerRemaining = 0f;
            _freezeEffectController?.StopFreezeEffectsOnly();
            _magnetController?.StopTargetingOnly();
            _wandController?.StopTargetingOnly();
            _hasTimerStartedByInput = false;
            _isTimerRunning = false;
            _isPaused = false;
            UpdateTimeDisplay();
        }

        public void StopTimer()
        {
            _isTimerRunning = false;
        }

        public void BeginTimerOnFirstInteraction()
        {
            if (_hasTimerStartedByInput || _isTimerRunning) return;
            if (IsVictoryShowing || IsTimeOutShowing) return;
            if (_timeRemaining <= 0f) return;
            _hasTimerStartedByInput = true;
            _isTimerRunning = true;
        }

        public void ResetTimer()
        {
            _timeRemaining = GetLevelDurationSeconds();
            _freezeTimerRemaining = 0f;
            _freezeEffectController?.StopFreezeEffectsOnly();
            _magnetController?.StopTargetingOnly();
            _wandController?.StopTargetingOnly();
            _hasTimerStartedByInput = false;
            _isTimerRunning = false;
            _isPaused = false;
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            if (_timeText == null) return;
            int mins = Mathf.FloorToInt(_timeRemaining / 60f);
            int secs = Mathf.FloorToInt(_timeRemaining % 60f);
            _timeText.text = string.Format("{0:D2}:{1:D2}", mins, secs);
            _lastDisplayedTimeString = _timeText.text;

            bool isFrozenState = _freezeTimerRemaining > 0f || (_freezeEffectController != null && _freezeEffectController.IsActivating);
            if (isFrozenState)
            {
                _timeText.color = new Color(0.75f, 0.97f, 1.0f);
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
            ShowTimeOutPopup();
        }

        public void OnReturnClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_returnButtonImg != null) StartCoroutine(CoButtonPunch(_returnButtonImg.transform));
            RestartLevel();
        }

        public void OnPauseClicked()
        {
            if (IsVictoryShowing || IsTimeOutShowing) return;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_pauseButtonImg != null) StartCoroutine(CoButtonPunch(_pauseButtonImg.transform));
            TogglePause();
        }

        public void OnBoosterFreezeClicked()
        {
            if (IsVictoryShowing || IsTimeOutShowing || _isPaused) return;
            if (_magnetController != null && (_magnetController.IsTutorialShowing || _magnetController.IsExecuting)) return;
            if (_wandController != null && (_wandController.IsTutorialShowing || _wandController.IsExecuting)) return;
            FreezeBoosterEffectController fx = EnsureFreezeEffectController();
            if (fx.IsActivating) return;

            if (_magnetController != null && _magnetController.IsTargeting)
            {
                _magnetController.CancelTargeting();
            }
            if (_wandController != null && _wandController.IsTargeting)
            {
                _wandController.CancelTargeting();
            }

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

            if (fx.IsTutorialShowing)
            {
                fx.DismissTutorial();
                PlayerPrefs.SetInt(PrefFreezeTutorialSeenKey, 1);
                PlayerPrefs.Save();
            }

            _freezeCount--;
            SaveBoosterCounts();
            if (_boosterFreezeCountText != null) _boosterFreezeCountText.text = "x" + _freezeCount;

            _freezeTimerRemaining = FreezeBoosterDuration;
            UpdateTimeDisplay();

            RectTransform freezeBtnRect = _boosterFreezeBtn != null ? _boosterFreezeBtn.GetComponent<RectTransform>() : null;
            RectTransform timerBannerRect = _timeBannerImg != null
                ? _timeBannerImg.rectTransform
                : (_headerPanel != null ? _headerPanel.transform.Find("Banner_Time") as RectTransform : null);
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster1");

            fx.PlayFreezeActivation(freezeBtnRect, timerBannerRect, iconSprite, () =>
            {
                _freezeTimerRemaining = FreezeBoosterDuration;
                UpdateTimeDisplay();
            });
        }

        public void OnBoosterMagnetClicked()
        {
            if (IsVictoryShowing || IsTimeOutShowing || _isPaused) return;
            if (_freezeEffectController != null && (_freezeEffectController.IsTutorialShowing || _freezeEffectController.IsActivating)) return;
            if (_wandController != null && (_wandController.IsTutorialShowing || _wandController.IsExecuting)) return;

            if (_wandController != null && _wandController.IsTargeting)
            {
                _wandController.CancelTargeting();
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_boosterMagnetBtn != null) StartCoroutine(CoButtonPunch(_boosterMagnetBtn.transform));

            int lvl = GetCurrentLevelNumber();
            if (lvl < BoosterMagnetUnlockLevel)
            {
                ShowFeedbackError($"Nam Châm mở khoá ở Level {BoosterMagnetUnlockLevel}!");
                return;
            }

            MagnetBoosterController magnetCtrl = EnsureMagnetController();
            if (magnetCtrl.IsExecuting) return;
            if (magnetCtrl.IsTargeting)
            {
                magnetCtrl.CancelTargeting();
                return;
            }

            if (_magnetCount <= 0)
            {
                ShowFeedbackError("Hết lượt dùng Nam Châm!");
                return;
            }

            if (magnetCtrl.IsTutorialShowing)
            {
                magnetCtrl.DismissTutorial();
                PlayerPrefs.SetInt(PrefMagnetTutorialSeenKey, 1);
                PlayerPrefs.Save();
            }

            Gameplay.PlayerPieceController.Instance?.CancelActiveDragIfNeeded();

            RectTransform magnetBtnRect = _boosterMagnetBtn != null ? _boosterMagnetBtn.GetComponent<RectTransform>() : null;
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster2");

            magnetCtrl.BeginTargeting(magnetBtnRect, iconSprite, () =>
            {
                _magnetCount = Mathf.Max(0, _magnetCount - 1);
                SaveBoosterCounts();
                if (_boosterMagnetCountText != null) _boosterMagnetCountText.text = "x" + _magnetCount;
            });
        }

        public void OnBoosterWandClicked()
        {
            if (IsVictoryShowing || IsTimeOutShowing || _isPaused) return;
            if (_freezeEffectController != null && (_freezeEffectController.IsTutorialShowing || _freezeEffectController.IsActivating)) return;
            if (_magnetController != null && (_magnetController.IsTutorialShowing || _magnetController.IsExecuting)) return;

            if (_magnetController != null && _magnetController.IsTargeting)
            {
                _magnetController.CancelTargeting();
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_boosterWandBtn != null) StartCoroutine(CoButtonPunch(_boosterWandBtn.transform));

            int lvl = GetCurrentLevelNumber();
            if (lvl < BoosterWandUnlockLevel)
            {
                ShowFeedbackError($"Đũa Thần mở khoá ở Level {BoosterWandUnlockLevel}!");
                return;
            }

            WandBoosterController wandCtrl = EnsureWandController();
            if (wandCtrl.IsExecuting) return;
            if (wandCtrl.IsTargeting)
            {
                wandCtrl.CancelTargeting();
                return;
            }

            if (_wandCount <= 0)
            {
                ShowFeedbackError("Hết lượt dùng Đũa Thần!");
                return;
            }

            if (wandCtrl.IsTutorialShowing)
            {
                wandCtrl.DismissTutorial();
                PlayerPrefs.SetInt(PrefWandTutorialSeenKey, 1);
                PlayerPrefs.Save();
            }

            Gameplay.PlayerPieceController.Instance?.CancelActiveDragIfNeeded();

            RectTransform wandBtnRect = _boosterWandBtn != null ? _boosterWandBtn.GetComponent<RectTransform>() : null;
            Sprite iconSprite = GemSpriteManager.GetBoosterSprite("booster3");

            wandCtrl.BeginTargeting(wandBtnRect, iconSprite, () =>
            {
                _wandCount = Mathf.Max(0, _wandCount - 1);
                SaveBoosterCounts();
                if (_boosterWandCountText != null) _boosterWandCountText.text = "x" + _wandCount;
            });
        }

        private void SaveBoosterCounts()
        {
            PlayerPrefs.SetInt(PrefBoosterFreezeKey, Mathf.Max(0, _freezeCount));
            PlayerPrefs.SetInt(PrefBoosterMagnetKey, Mathf.Max(0, _magnetCount));
            PlayerPrefs.SetInt(PrefBoosterWandKey, Mathf.Max(0, _wandCount));
            PlayerPrefs.Save();
        }

        private bool TryMagnetAutoMerge()
        {
            if (LevelManager.Instance == null) return false;
            var pieces = LevelManager.Instance.ActivePieces;
            if (pieces == null || pieces.Count < 2) return false;

            for (int i = 0; i < pieces.Count; i++)
            {
                var p1 = pieces[i];
                if (p1 == null || p1.IsSliding || p1.IsEscaping || p1.IsFrozen || p1.IsStone) continue;
                for (int j = i + 1; j < pieces.Count; j++)
                {
                    var p2 = pieces[j];
                    if (p2 == null || p2.IsSliding || p2.IsEscaping || p2.IsFrozen || p2.IsStone) continue;
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
            if (IsVictoryShowing || IsTimeOutShowing || IsBoosterTutorialShowing) return;
            _isPaused = !_isPaused;
            if (_pausePanel != null)
            {
                if (_isPaused) _pausePanel.transform.SetAsLastSibling();
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

        public void OnVictoryNextClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            NextLevel();
        }

        public void OnVictoryGetClicked()
        {
            ClaimWinRewardAndAdvance(BaseWinRewardCoins, _winGetBtnRect);
        }

        public void OnVictoryAdsClicked()
        {
            ClaimWinRewardAndAdvance(AdsWinRewardCoins, _winAdsBtnRect);
        }

        private void ClaimWinRewardAndAdvance(int coinAmount, RectTransform clickedBtnRect)
        {
            if (_hasClaimedWinReward) return;
            _hasClaimedWinReward = true;

            int startCoins = PlayerPrefs.GetInt(PrefCoinsKey, 0);
            if (!CustomLevelRepository.IsEditorPlayTestActive)
            {
                int targetCoins = startCoins + coinAmount;
                PlayerPrefs.SetInt(PrefCoinsKey, targetCoins);

                int currentLevel = GetCurrentLevelNumber();
                int maxLvl = GetMaxLevelNumber();
                int nextLevel = currentLevel < maxLvl ? (currentLevel + 1) : 1;
                PlayerPrefs.SetInt(PrefCurrentLevelKey, nextLevel);
                PlayerPrefs.SetInt(PrefJustWonLevelKey, 1);
                PlayerPrefs.Save();
            }

            _winIntroComplete = false;
            if (_victoryAnimCoroutine != null)
            {
                StopCoroutine(_victoryAnimCoroutine);
                _victoryAnimCoroutine = null;
            }
            if (_winTopBarRect != null)
            {
                _winTopBarRect.anchoredPosition = new Vector2(0f, -130f);
            }
            if (_winTopCoinsCountText != null)
            {
                _winTopCoinsCountText.text = startCoins.ToString();
            }

            if (_winGetBtn != null) _winGetBtn.interactable = false;
            if (_winAdsBtn != null) _winAdsBtn.interactable = false;
            if (_winButtonsCanvasGroup != null)
            {
                _winButtonsCanvasGroup.interactable = false;
                _winButtonsCanvasGroup.blocksRaycasts = false;
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();

            if (Application.isPlaying)
            {
                if (_winClaimCoroutine != null) StopCoroutine(_winClaimCoroutine);
                _winClaimCoroutine = StartCoroutine(CoClaimRewardAndAdvance(startCoins, coinAmount, clickedBtnRect));
            }
            else
            {
                ReturnToHomeAfterVictory();
            }
        }

        public void OnVictoryReplayClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            HideGameOver();
            RestartLevel();
        }

        public void NextLevel()
        {
            if (_isLoadingNextLevel) return;

            Time.timeScale = 1f;
            _isPaused = false;
            if (_pausePanel != null) _pausePanel.SetActive(false);
            HideGameOver();
            HideTimeOutPopup();
            CleanupWinEffects();
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            _hasClaimedWinReward = false;

            int currentLevel = GetCurrentLevelNumber();
            int maxLvl = GetMaxLevelNumber();
            int nextLevel = currentLevel < maxLvl ? (currentLevel + 1) : 1;
            if (!CustomLevelRepository.IsEditorPlayTestActive)
            {
                PlayerPrefs.SetInt(PrefCurrentLevelKey, nextLevel);
                PlayerPrefs.Save();
            }
            string nextSceneName = "Level" + nextLevel;

            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                _isLoadingNextLevel = true;
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            _isLoadingNextLevel = false;
            if (LevelManager.Instance != null)
            {
                if (CustomLevelRepository.TryGetLevel(nextLevel, out CustomLevelData nextData))
                {
                    _customDurationSeconds = 0f;
                    _customTimeMMSS = "";
                    LevelManager.Instance.LoadCustomLevel(nextData, true);
                }
                else
                {
                    LevelManager.Instance.SwitchLevel((LevelId)nextLevel);
                }
            }
            ResetTimer();
            UpdateLevelDisplay();
            UpdateBoosterUnlockStates();
            if (_tutorialGuide != null)
            {
                _tutorialGuide.HandleLevelReset();
            }
        }

        public void RestartLevel()
        {
            _isLoadingNextLevel = false;
            Time.timeScale = 1f;
            _isPaused = false;
            if (_pausePanel != null) _pausePanel.SetActive(false);
            HideGameOver();
            HideTimeOutPopup();
            CleanupWinEffects();
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            _hasClaimedWinReward = false;

            if (LevelManager.Instance != null && LevelManager.Instance.HasActiveCustomLevel)
            {
                LevelManager.Instance.InitializeLevel(true);
                ResetTimer();
                UpdateLevelDisplay();
                UpdateBoosterUnlockStates();
                if (_tutorialGuide != null) _tutorialGuide.HandleLevelReset();
                return;
            }

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
            if (_feedbackCanvasGroup != null) _feedbackCanvasGroup.alpha = 0f;
            if (_feedbackText != null) _feedbackText.text = string.Empty;
            if (_feedbackPanel != null) _feedbackPanel.SetActive(false);
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

        public void ReplayVictoryAnimation()
        {
            HideGameOver();
            _hasClaimedWinReward = false;
            ShowVictoryPopup();
        }

        private void CleanupWinEffects()
        {
            if (_victoryAnimCoroutine != null)
            {
                StopCoroutine(_victoryAnimCoroutine);
                _victoryAnimCoroutine = null;
            }
            if (_winClaimCoroutine != null)
            {
                StopCoroutine(_winClaimCoroutine);
                _winClaimCoroutine = null;
            }
            if (_winAmbientConfettiCoroutine != null)
            {
                StopCoroutine(_winAmbientConfettiCoroutine);
                _winAmbientConfettiCoroutine = null;
            }
            _winIntroComplete = false;
            if (_winEffectsLayer != null)
            {
                if (Application.isPlaying) Destroy(_winEffectsLayer.gameObject);
                else DestroyImmediate(_winEffectsLayer.gameObject);
                _winEffectsLayer = null;
            }
        }

        public void ShowVictoryPopup()
        {
            if (_victoryPanel == null)
            {
                RebuildVictoryPanel();
            }
            if (_victoryPanel == null || _victoryPanel.activeSelf) return;

            HideTimeOutPopup();
            _hasClaimedWinReward = false;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_pausePanel != null) _pausePanel.SetActive(false);

            StopTimer();
            _freezeTimerRemaining = 0f;
            _freezeEffectController?.StopImmediate();
            _magnetController?.StopImmediate();
            _wandController?.StopImmediate();
            Gameplay.PlayerPieceController.Instance?.ResetSelection();

            UpdateVictoryBannerContent();
            _victoryPanel.transform.SetAsLastSibling();
            _victoryPanel.SetActive(true);

            if (Application.isPlaying)
            {
                CleanupWinEffects();
                _victoryAnimCoroutine = StartCoroutine(CoAnimateVictoryBanner());
            }
            else
            {
                if (_victoryCanvasGroup != null) _victoryCanvasGroup.alpha = 1f;
                ResetWinAnimatedTransforms();
            }
        }

        public void SetVictoryPreviewActive(bool active)
        {
            if (active)
            {
                if (_victoryPanel == null ||
                    _victoryPanel.transform.Find("WinContent/WinTopBar/CoinsPill/CoinTarget") == null ||
                    _victoryPanel.transform.Find("WinContent/WellDoneRoot") == null ||
                    _victoryPanel.transform.Find("WinContent/CoinsRewardRoot/CoinsSunburst") == null ||
                    _victoryPanel.transform.Find("WinContent/ButtonsRow/Btn_Ads/ShineClip") == null)
                {
                    RebuildVictoryPanel();
                }
                if (_victoryPanel == null) return;

                _hasClaimedWinReward = false;
                UpdateVictoryBannerContent();
                _victoryPanel.transform.SetAsLastSibling();
                _victoryPanel.SetActive(true);
                if (_victoryCanvasGroup != null) _victoryCanvasGroup.alpha = 1f;
                ResetWinAnimatedTransforms();
            }
            else if (_victoryPanel != null)
            {
                CleanupWinEffects();
                _victoryPanel.SetActive(false);
            }
        }

        private void ResetWinAnimatedTransforms()
        {
            if (_winWellDoneRoot != null) _winWellDoneRoot.localScale = Vector3.one;
            if (_winWellDoneGlowRect != null) _winWellDoneGlowRect.localScale = Vector3.one;
            if (_winLetterTiles != null)
            {
                for (int i = 0; i < _winLetterTiles.Length; i++)
                {
                    if (_winLetterTiles[i] == null) continue;
                    _winLetterTiles[i].localScale = Vector3.one;
                    _winLetterTiles[i].localRotation = Quaternion.identity;
                    if (_winTileRestPositions != null && i < _winTileRestPositions.Length)
                    {
                        _winLetterTiles[i].anchoredPosition = _winTileRestPositions[i];
                    }
                }
            }
            if (_winSparklesRoot != null)
            {
                for (int i = 0; i < _winSparklesRoot.childCount; i++)
                {
                    Transform ch = _winSparklesRoot.GetChild(i);
                    if (ch != null) ch.localScale = Vector3.one;
                }
            }
            if (_winCoinsRoot != null) _winCoinsRoot.localScale = Vector3.one;
            if (_winCoinsAuraOuter != null) _winCoinsAuraOuter.localScale = Vector3.one;
            if (_winCoinsSunburstRect != null) _winCoinsSunburstRect.localScale = Vector3.one;
            if (_winCoinsImageRect != null) _winCoinsImageRect.anchoredPosition = new Vector2(0f, 22f);
            if (_winRewardAmountText != null)
            {
                _winRewardAmountText.rectTransform.localScale = Vector3.one;
                _winRewardAmountText.text = "+" + BaseWinRewardCoins;
            }
            if (_winProgressRoot != null)
            {
                _winProgressRoot.localScale = Vector3.one;
                _winProgressRoot.anchoredPosition = Vector2.zero;
            }
            if (_winProgressCanvasGroup != null) _winProgressCanvasGroup.alpha = 1f;
            if (_winProgressNodes != null)
            {
                for (int i = 0; i < _winProgressNodes.Length; i++)
                {
                    if (_winProgressNodes[i] != null) _winProgressNodes[i].rectTransform.localScale = Vector3.one;
                }
            }
            if (_winMilestoneBubbleRect != null)
            {
                _winMilestoneBubbleRect.localScale = Vector3.one;
                _winMilestoneBubbleRect.localRotation = Quaternion.identity;
                Vector2 bp = _winMilestoneBubbleRect.anchoredPosition;
                _winMilestoneBubbleRect.anchoredPosition = new Vector2(bp.x, 76f);
            }
            if (_winTopBarRect != null)
            {
                _winTopBarRect.anchoredPosition = new Vector2(0f, -130f);
                _winTopBarRect.localScale = Vector3.one;
            }
            if (_winTopCoinsPillRect != null) _winTopCoinsPillRect.localScale = Vector3.one;
            if (_winButtonsRoot != null) _winButtonsRoot.localScale = Vector3.one;
            if (_winButtonsCanvasGroup != null)
            {
                _winButtonsCanvasGroup.alpha = 1f;
                _winButtonsCanvasGroup.interactable = true;
                _winButtonsCanvasGroup.blocksRaycasts = true;
            }
            if (_winGetBtn != null) _winGetBtn.interactable = true;
            if (_winAdsBtn != null) _winAdsBtn.interactable = true;
            if (_winGetBtnRect != null)
            {
                _winGetBtnRect.localScale = Vector3.one;
                _winGetBtnRect.anchoredPosition = _winGetBtnRestPos;
            }
            if (_winAdsBtnRect != null)
            {
                _winAdsBtnRect.localScale = Vector3.one;
                _winAdsBtnRect.anchoredPosition = _winAdsBtnRestPos;
            }
            if (_winAdsShineRect != null)
            {
                _winAdsShineRect.anchoredPosition = new Vector2(-280f, 0f);
            }
        }

        private void UpdateVictoryBannerContent()
        {
            int currentLevel = GetCurrentLevelNumber();
            int startLvl = ((Mathf.Max(1, currentLevel) - 1) / 5) * 5 + 1;
            int activeIdx = Mathf.Clamp(currentLevel - startLvl, 0, 4);

            Color completedPurple = new Color(0.62f, 0.28f, 0.96f, 1f);
            Color activeMagenta = new Color(0.94f, 0.24f, 0.84f, 1f);
            Color upcomingCyan = new Color(0.14f, 0.84f, 0.94f, 1f);

            for (int i = 0; i < 5; i++)
            {
                if (_winProgressLabels != null && i < _winProgressLabels.Length && _winProgressLabels[i] != null)
                {
                    _winProgressLabels[i].text = "Lv" + (startLvl + i);
                }

                bool isActive = (i == activeIdx);
                bool isCompleted = (i < activeIdx);

                if (_winProgressNodes != null && i < _winProgressNodes.Length && _winProgressNodes[i] != null)
                {
                    _winProgressNodes[i].color = isActive ? activeMagenta : (isCompleted ? completedPurple : upcomingCyan);
                    RectTransform nodeRect = _winProgressNodes[i].rectTransform;
                    nodeRect.sizeDelta = isActive ? new Vector2(62f, 62f) : new Vector2(50f, 50f);
                }

                if (_winProgressNodeGlows != null && i < _winProgressNodeGlows.Length && _winProgressNodeGlows[i] != null)
                {
                    _winProgressNodeGlows[i].SetActive(isActive);
                }

                if (_winProgressNodeIcons != null && i < _winProgressNodeIcons.Length && _winProgressNodeIcons[i] != null)
                {
                    _winProgressNodeIcons[i].SetActive(isActive);
                }
            }

            if (_winProgressFillRect != null)
            {
                _winProgressFillRect.sizeDelta = new Vector2(110f + activeIdx * 150f, 28f);
            }

            if (_winMilestoneBubbleRect != null)
            {
                _winMilestoneBubbleRect.anchoredPosition = new Vector2(-300f + activeIdx * 150f, 76f);
            }

            int lives = PlayerPrefs.HasKey(PrefLivesKey) ? PlayerPrefs.GetInt(PrefLivesKey, DefaultLives) : MaxLives;
            if (_winTopLivesCountText != null) _winTopLivesCountText.text = Mathf.Clamp(lives, 0, MaxLives).ToString();
            if (_winTopLivesStatusText != null) _winTopLivesStatusText.text = lives >= MaxLives ? "Full" : FormatTimeMMSS(_timeOutLifeRegenSeconds);
            int coins = PlayerPrefs.GetInt(PrefCoinsKey, 0);
            if (_winTopCoinsCountText != null) _winTopCoinsCountText.text = coins.ToString();

            if (_winRewardAmountText != null) _winRewardAmountText.text = "+" + BaseWinRewardCoins;
            if (_winGetAmountText != null) _winGetAmountText.text = BaseWinRewardCoins.ToString();
            if (_winAdsAmountText != null) _winAdsAmountText.text = AdsWinRewardCoins.ToString();
        }

        private IEnumerator CoAnimateVictoryBanner()
        {
            Transform winContent = _victoryPanel.transform.Find("WinContent") ?? _victoryPanel.transform;
            GameObject fxObj = CreateUIContainer("WinEffectsLayer", winContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _winEffectsLayer = fxObj.GetComponent<RectTransform>();
            _winEffectsLayer.SetAsLastSibling();

            RectTransform cometAnchor = CreateUIContainer("CometAnchor", _winEffectsLayer, new Vector2(0.5f, 0.525f), new Vector2(0.5f, 0.525f), Vector2.zero, new Vector2(800f, 800f)).GetComponent<RectTransform>();
            RectTransform confettiAnchor = CreateUIContainer("ConfettiAnchor", _winEffectsLayer, new Vector2(0.5f, 0.785f), new Vector2(0.5f, 0.785f), Vector2.zero, new Vector2(900f, 900f)).GetComponent<RectTransform>();
            RectTransform nodeAnchor = CreateUIContainer("NodeAnchor", _winEffectsLayer, new Vector2(0.5f, 0.345f), new Vector2(0.5f, 0.345f), Vector2.zero, new Vector2(860f, 200f)).GetComponent<RectTransform>();

            // Initial states at t=0
            if (_victoryCanvasGroup != null) _victoryCanvasGroup.alpha = 0f;
            if (_winTopBarRect != null) _winTopBarRect.anchoredPosition = new Vector2(0f, 80f);
            if (_winTopCoinsPillRect != null) _winTopCoinsPillRect.localScale = Vector3.one;
            if (_winButtonsCanvasGroup != null)
            {
                _winButtonsCanvasGroup.alpha = 1f;
                _winButtonsCanvasGroup.interactable = true;
                _winButtonsCanvasGroup.blocksRaycasts = true;
            }
            if (_winGetBtn != null) _winGetBtn.interactable = true;
            if (_winAdsBtn != null) _winAdsBtn.interactable = true;
            if (_winWellDoneGlowRect != null) _winWellDoneGlowRect.localScale = Vector3.zero;
            if (_winLetterTiles != null)
            {
                for (int i = 0; i < _winLetterTiles.Length; i++)
                {
                    if (_winLetterTiles[i] != null) _winLetterTiles[i].localScale = Vector3.zero;
                }
            }
            if (_winSparklesRoot != null)
            {
                for (int i = 0; i < _winSparklesRoot.childCount; i++)
                {
                    Transform ch = _winSparklesRoot.GetChild(i);
                    if (ch != null) ch.localScale = Vector3.zero;
                }
            }
            if (_winCoinsRoot != null) _winCoinsRoot.localScale = Vector3.zero;
            if (_winCoinsSunburstRect != null) _winCoinsSunburstRect.localScale = Vector3.zero;
            if (_winRewardAmountText != null)
            {
                _winRewardAmountText.text = "+0";
                _winRewardAmountText.rectTransform.localScale = Vector3.zero;
            }
            if (_winProgressCanvasGroup != null) _winProgressCanvasGroup.alpha = 0f;
            if (_winProgressRoot != null) _winProgressRoot.anchoredPosition = new Vector2(0f, -32f);

            int currentLevel = GetCurrentLevelNumber();
            int startLvl = ((Mathf.Max(1, currentLevel) - 1) / 5) * 5 + 1;
            int activeIdx = Mathf.Clamp(currentLevel - startLvl, 0, 4);
            float prevFillW = activeIdx > 0 ? (110f + (activeIdx - 1) * 150f) : 14f;
            float targetFillW = 110f + activeIdx * 150f;

            if (_winProgressFillRect != null) _winProgressFillRect.sizeDelta = new Vector2(prevFillW, 28f);
            if (_winProgressNodes != null && activeIdx < _winProgressNodes.Length && _winProgressNodes[activeIdx] != null)
            {
                _winProgressNodes[activeIdx].color = new Color(0.14f, 0.84f, 0.94f, 1f);
                _winProgressNodes[activeIdx].rectTransform.sizeDelta = new Vector2(50f, 50f);
            }
            if (_winProgressNodeGlows != null && activeIdx < _winProgressNodeGlows.Length && _winProgressNodeGlows[activeIdx] != null)
            {
                _winProgressNodeGlows[activeIdx].SetActive(false);
            }
            if (_winProgressNodeIcons != null && activeIdx < _winProgressNodeIcons.Length && _winProgressNodeIcons[activeIdx] != null)
            {
                _winProgressNodeIcons[activeIdx].SetActive(false);
            }
            if (_winMilestoneBubbleRect != null)
            {
                _winMilestoneBubbleRect.anchoredPosition = new Vector2(-300f + activeIdx * 150f, 34f);
                _winMilestoneBubbleRect.localScale = Vector3.zero;
            }

            if (_winGetBtnRect != null)
            {
                _winGetBtnRect.localScale = Vector3.zero;
                _winGetBtnRect.anchoredPosition = _winGetBtnRestPos + new Vector2(0f, -48f);
            }
            if (_winAdsBtnRect != null)
            {
                _winAdsBtnRect.localScale = Vector3.zero;
                _winAdsBtnRect.anchoredPosition = _winAdsBtnRestPos + new Vector2(0f, -48f);
            }

            // Phase 1: Launch golden coin comet arc
            const float cometDur = 0.42f;
            StartCoroutine(CoSpawnCoinCometAndTrail(cometAnchor, cometDur));

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            RectTransform fillSpark = null;

            bool impactFired = false;
            bool fillStarted = false;
            bool nodeBurstFired = false;

            const float totalDur = 1.72f;
            float elapsed = 0f;

            while (elapsed < totalDur)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;

                // Backdrop fade in & top resource bar slide down
                if (_victoryCanvasGroup != null)
                {
                    _victoryCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.20f);
                }
                if (_winTopBarRect != null)
                {
                    float tbT = Mathf.Clamp01(elapsed / 0.28f);
                    float easeOut = 1f - (1f - tbT) * (1f - tbT);
                    _winTopBarRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(80f, -130f, easeOut));
                }

                // Phase 2 trigger at t = 0.42s
                if (!impactFired && elapsed >= cometDur)
                {
                    impactFired = true;
                    StartCoroutine(CoSpawnFlashBurst(cometAnchor, new Vector2(0f, 22f), glowSp, new Color(1.0f, 0.91f, 0.44f, 0.95f), 0.30f, 2.2f, 0.28f));
                    StartCoroutine(CoRunConfettiBurst(confettiAnchor, 42));
                    SoundManager.Instance?.PlayPickup();
                }

                // Coins pop + Sunburst + Rolling "+40" (0.42s -> 0.76s)
                if (elapsed >= cometDur)
                {
                    float ct = Mathf.Clamp01((elapsed - cometDur) / 0.34f);
                    if (_winCoinsRoot != null)
                    {
                        _winCoinsRoot.localScale = Vector3.one * EvaluateOvershootScale(ct, 0.25f, 1.18f, 1f, 0.62f);
                    }
                    if (_winCoinsSunburstRect != null)
                    {
                        _winCoinsSunburstRect.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, ct);
                        _winCoinsSunburstRect.Rotate(0f, 0f, -24f * dt);
                    }
                    if (_winRewardAmountText != null)
                    {
                        float rt = Mathf.Clamp01((elapsed - 0.45f) / 0.31f);
                        float easeR = 1f - (1f - rt) * (1f - rt);
                        _winRewardAmountText.text = "+" + Mathf.RoundToInt(Mathf.Lerp(0f, BaseWinRewardCoins, easeR));
                        _winRewardAmountText.rectTransform.localScale = Vector3.one * EvaluateOvershootScale(rt, 0f, 1.20f, 1f, 0.65f);
                    }
                }

                // WELL DONE spread/tilted drop -> magnetic Lego snap (0.42s -> 1.02s)
                if (elapsed >= cometDur && _winLetterTiles != null && _winTileRestPositions != null)
                {
                    for (int i = 0; i < _winLetterTiles.Length && i < _winTileRestPositions.Length; i++)
                    {
                        RectTransform tile = _winLetterTiles[i];
                        if (tile == null) continue;

                        int col = i % 4;
                        bool isTop = i < 4;
                        Vector2 restPos = _winTileRestPositions[i];
                        Vector2 spreadOff = new Vector2((col - 1.5f) * 24f, isTop ? 34f : -30f);
                        float spreadTilt = ((i % 2 == 0) ? 1f : -1f) * (isTop ? 9.5f : -8.5f);
                        float tileAppearStart = cometDur + i * 0.024f;

                        if (elapsed < tileAppearStart)
                        {
                            tile.localScale = Vector3.zero;
                        }
                        else if (elapsed < 0.78f)
                        {
                            float popT = Mathf.Clamp01((elapsed - tileAppearStart) / 0.20f);
                            float popScale = EvaluateOvershootScale(popT, 0f, 1.16f, 1.02f, 0.65f);
                            float easeOut = 1f - (1f - popT) * (1f - popT);
                            tile.localScale = Vector3.one * popScale;
                            tile.anchoredPosition = Vector2.Lerp(restPos + spreadOff * 1.8f, restPos + spreadOff, easeOut);
                            tile.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(spreadTilt * 1.4f, spreadTilt, easeOut));
                        }
                        else if (elapsed < 1.02f)
                        {
                            float snapT = Mathf.Clamp01((elapsed - 0.78f) / 0.24f);
                            float smoothSnap = snapT < 0.5f ? 4f * snapT * snapT * snapT : 1f - Mathf.Pow(-2f * snapT + 2f, 3f) / 2f;
                            tile.anchoredPosition = Vector2.Lerp(restPos + spreadOff, restPos, smoothSnap);
                            tile.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(spreadTilt, 0f, smoothSnap));
                            tile.localScale = Vector3.one * Mathf.Lerp(1.02f, 1f, smoothSnap);
                        }
                        else
                        {
                            tile.anchoredPosition = restPos;
                            tile.localRotation = Quaternion.identity;
                            tile.localScale = Vector3.one;
                        }
                    }
                }

                // WELL DONE squash & stretch + perimeter glow/sparkles flare (0.94s -> 1.18s)
                if (elapsed >= 0.94f)
                {
                    if (_winWellDoneRoot != null)
                    {
                        float sqT = Mathf.Clamp01((elapsed - 0.96f) / 0.18f);
                        if (sqT > 0f && sqT < 1f)
                        {
                            float wave = Mathf.Sin(sqT * Mathf.PI * 2f) * (1f - sqT);
                            _winWellDoneRoot.localScale = new Vector3(1f + 0.08f * wave, 1f - 0.09f * wave, 1f);
                        }
                        else if (sqT >= 1f)
                        {
                            _winWellDoneRoot.localScale = Vector3.one;
                        }
                    }
                    if (_winWellDoneGlowRect != null)
                    {
                        float gt = Mathf.Clamp01((elapsed - 0.94f) / 0.24f);
                        _winWellDoneGlowRect.localScale = Vector3.one * EvaluateOvershootScale(gt, 0f, 1.25f, 1f, 0.6f);
                    }
                    if (_winSparklesRoot != null)
                    {
                        for (int i = 0; i < _winSparklesRoot.childCount; i++)
                        {
                            Transform ch = _winSparklesRoot.GetChild(i);
                            if (ch == null) continue;
                            float st = Mathf.Clamp01((elapsed - (0.94f + i * 0.01f)) / 0.20f);
                            ch.localScale = Vector3.one * EvaluateOvershootScale(st, 0f, 1.25f, 1f, 0.6f);
                        }
                    }
                }

                // Buttons staggered elastic entrance (0.62s -> 1.04s)
                if (elapsed >= 0.62f && _winGetBtnRect != null)
                {
                    float gt = Mathf.Clamp01((elapsed - 0.62f) / 0.30f);
                    float ease = 1f - (1f - gt) * (1f - gt);
                    _winGetBtnRect.localScale = Vector3.one * EvaluateOvershootScale(gt, 0f, 1.10f, 1f, 0.65f);
                    _winGetBtnRect.anchoredPosition = _winGetBtnRestPos + new Vector2(0f, Mathf.Lerp(-48f, 0f, ease));
                }
                if (elapsed >= 0.72f && _winAdsBtnRect != null)
                {
                    float at = Mathf.Clamp01((elapsed - 0.72f) / 0.32f);
                    float ease = 1f - (1f - at) * (1f - at);
                    _winAdsBtnRect.localScale = Vector3.one * EvaluateOvershootScale(at, 0f, 1.14f, 1f, 0.65f);
                    _winAdsBtnRect.anchoredPosition = _winAdsBtnRestPos + new Vector2(0f, Mathf.Lerp(-48f, 0f, ease));
                }

                // Phase 3: Progress bar slide-in (0.80s -> 1.04s)
                if (elapsed >= 0.80f)
                {
                    float pt = Mathf.Clamp01((elapsed - 0.80f) / 0.24f);
                    float easeP = 1f - (1f - pt) * (1f - pt);
                    if (_winProgressCanvasGroup != null) _winProgressCanvasGroup.alpha = pt;
                    if (_winProgressRoot != null) _winProgressRoot.anchoredPosition = new Vector2(0f, Mathf.Lerp(-32f, 0f, easeP));
                }

                // Smooth neon fill sweep (1.04s -> 1.40s)
                if (elapsed >= 1.04f && elapsed < 1.40f && _winProgressFillRect != null)
                {
                    if (!fillStarted)
                    {
                        fillStarted = true;
                        fillSpark = SpawnFloatingEffectImage("FillSpark", _winProgressFillRect, flareSp, Vector2.zero, new Vector2(46f, 46f), Color.white, true);
                        fillSpark.anchorMin = new Vector2(1f, 0.5f);
                        fillSpark.anchorMax = new Vector2(1f, 0.5f);
                    }
                    float ft = Mathf.Clamp01((elapsed - 1.04f) / 0.36f);
                    float easeOutCubic = 1f - Mathf.Pow(1f - ft, 3f);
                    _winProgressFillRect.sizeDelta = new Vector2(Mathf.Lerp(prevFillW, targetFillW, easeOutCubic), 28f);
                    if (fillSpark != null) fillSpark.Rotate(0f, 0f, 180f * dt);
                }

                // Active node burst & floating milestone bubble spring (1.40s -> 1.72s)
                if (elapsed >= 1.40f)
                {
                    if (!nodeBurstFired)
                    {
                        nodeBurstFired = true;
                        if (fillSpark != null)
                        {
                            Destroy(fillSpark.gameObject);
                            fillSpark = null;
                        }
                        if (_winProgressFillRect != null) _winProgressFillRect.sizeDelta = new Vector2(targetFillW, 28f);

                        if (_winProgressNodes != null && activeIdx < _winProgressNodes.Length && _winProgressNodes[activeIdx] != null)
                        {
                            _winProgressNodes[activeIdx].color = new Color(0.94f, 0.24f, 0.84f, 1f);
                            _winProgressNodes[activeIdx].rectTransform.sizeDelta = new Vector2(62f, 62f);
                        }
                        if (_winProgressNodeGlows != null && activeIdx < _winProgressNodeGlows.Length && _winProgressNodeGlows[activeIdx] != null)
                        {
                            _winProgressNodeGlows[activeIdx].SetActive(true);
                        }
                        if (_winProgressNodeIcons != null && activeIdx < _winProgressNodeIcons.Length && _winProgressNodeIcons[activeIdx] != null)
                        {
                            _winProgressNodeIcons[activeIdx].SetActive(true);
                        }

                        StartCoroutine(CoSpawnFlashBurst(nodeAnchor, new Vector2(-300f + activeIdx * 150f, 0f), glowSp, new Color(1.0f, 0.45f, 0.96f, 0.95f), 0.35f, 2.0f, 0.28f));
                        SoundManager.Instance?.PlayPickup();
                    }

                    float nt = Mathf.Clamp01((elapsed - 1.40f) / 0.28f);
                    if (_winProgressNodes != null && activeIdx < _winProgressNodes.Length && _winProgressNodes[activeIdx] != null)
                    {
                        _winProgressNodes[activeIdx].rectTransform.localScale = Vector3.one * EvaluateOvershootScale(nt, 0.70f, 1.40f, 1f, 0.55f);
                    }

                    if (_winMilestoneBubbleRect != null)
                    {
                        float bt = Mathf.Clamp01((elapsed - 1.42f) / 0.30f);
                        float bScale = EvaluateOvershootScale(bt, 0f, 1.24f, 1f, 0.58f);
                        float bY = EvaluateOvershootScale(bt, 34f, 88f, 76f, 0.60f);
                        float rotZ = Mathf.Sin(bt * Mathf.PI * 2f) * (1f - bt) * -12f;
                        _winMilestoneBubbleRect.localScale = Vector3.one * bScale;
                        _winMilestoneBubbleRect.anchoredPosition = new Vector2(-300f + activeIdx * 150f, bY);
                        _winMilestoneBubbleRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);
                    }
                }

                yield return null;
            }

            if (fillSpark != null) Destroy(fillSpark.gameObject);
            if (_victoryCanvasGroup != null) _victoryCanvasGroup.alpha = 1f;
            UpdateVictoryBannerContent();
            ResetWinAnimatedTransforms();

            _winIntroComplete = true;
            _winIdleTime = 0f;
            _victoryAnimCoroutine = null;
            _winAmbientConfettiCoroutine = StartCoroutine(CoRunAmbientConfetti(confettiAnchor));
        }

        private IEnumerator CoSpawnCoinCometAndTrail(RectTransform anchor, float duration)
        {
            if (anchor == null) yield break;

            Sprite coinSp = GemSpriteManager.GetWinSceneSprite("Coin");
            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();

            Vector2 p0 = new Vector2(-390f, -60f);
            Vector2 p1 = new Vector2(-200f, -320f);
            Vector2 p2 = new Vector2(0f, 22f);

            RectTransform cometGlow = SpawnFloatingEffectImage("CometGlow", anchor, glowSp, p0, new Vector2(180f, 180f), new Color(1.0f, 0.86f, 0.25f, 0.85f), false);
            RectTransform cometCoin = SpawnFloatingEffectImage("CometCoin", anchor, coinSp, p0, new Vector2(128f, 128f), Color.white, true);

            float elapsed = 0f;
            float trailAccum = 0f;

            while (elapsed < duration && anchor != null && _winEffectsLayer != null && IsVictoryShowing)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;
                trailAccum += dt;

                float t = Mathf.Clamp01(elapsed / duration);
                float ease = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
                float u = 1f - ease;
                Vector2 pos = (u * u) * p0 + (2f * u * ease) * p1 + (ease * ease) * p2;

                cometCoin.anchoredPosition = pos;
                cometGlow.anchoredPosition = pos;
                cometCoin.localScale = Vector3.one * Mathf.Lerp(1.15f, 1.45f, Mathf.Sin(t * Mathf.PI));
                cometCoin.Rotate(0f, 0f, -540f * dt);

                if (trailAccum >= 0.022f)
                {
                    trailAccum = 0f;
                    StartCoroutine(CoAnimateTrailDot(anchor, pos, glowSp, flareSp));
                }

                yield return null;
            }

            if (cometCoin != null) Destroy(cometCoin.gameObject);
            if (cometGlow != null) Destroy(cometGlow.gameObject);
        }

        private IEnumerator CoAnimateTrailDot(RectTransform anchor, Vector2 pos, Sprite glowSp, Sprite flareSp)
        {
            if (anchor == null) yield break;

            bool useFlare = UnityEngine.Random.value > 0.55f;
            float baseSize = useFlare ? UnityEngine.Random.Range(24f, 38f) : UnityEngine.Random.Range(28f, 44f);
            Vector2 jitter = new Vector2(UnityEngine.Random.Range(-14f, 14f), UnityEngine.Random.Range(-14f, 14f));
            RectTransform rt = SpawnFloatingEffectImage("CometTrail", anchor, useFlare ? flareSp : glowSp, pos + jitter, new Vector2(baseSize, baseSize), new Color(1.0f, 0.90f, 0.36f, 0.95f), true);
            Image img = rt.GetComponent<Image>();

            float life = useFlare ? 0.42f : 0.32f;
            float age = 0f;
            Vector2 drift = new Vector2(UnityEngine.Random.Range(-22f, 22f), UnityEngine.Random.Range(15f, 48f));

            while (age < life && rt != null && _winEffectsLayer != null && IsVictoryShowing)
            {
                float dt = Time.unscaledDeltaTime;
                age += dt;
                float t = Mathf.Clamp01(age / life);
                rt.anchoredPosition += drift * dt;
                rt.localScale = Vector3.one * (1f - t * 0.85f);
                if (img != null) img.color = new Color(1.0f, 0.90f, 0.36f, (1f - t) * 0.92f);
                yield return null;
            }

            if (rt != null) Destroy(rt.gameObject);
        }

        private IEnumerator CoSpawnFlashBurst(RectTransform parent, Vector2 pos, Sprite sprite, Color color, float startScale, float endScale, float duration)
        {
            if (parent == null || sprite == null) yield break;
            RectTransform rt = SpawnFloatingEffectImage("FlashBurst", parent, sprite, pos, new Vector2(220f, 220f), color, false);
            Image img = rt.GetComponent<Image>();

            float elapsed = 0f;
            while (elapsed < duration && rt != null && _winEffectsLayer != null && IsVictoryShowing)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - (1f - t) * (1f - t);
                rt.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, ease);
                if (img != null)
                {
                    Color c = color;
                    c.a = color.a * (1f - t * t);
                    img.color = c;
                }
                yield return null;
            }

            if (rt != null) Destroy(rt.gameObject);
        }

        private struct ConfettiParticle
        {
            public RectTransform Rect;
            public Image Img;
            public Vector2 Vel;
            public float Spin;
            public float FlutterFreq;
            public float FlutterPhase;
            public float Lifetime;
            public float Age;
            public Color BaseColor;
        }

        private static readonly Color[] WinConfettiPalette = new Color[]
        {
            new Color(1.00f, 0.25f, 0.38f, 1f),
            new Color(1.00f, 0.86f, 0.18f, 1f),
            new Color(0.22f, 0.92f, 1.00f, 1f),
            new Color(0.32f, 0.96f, 0.42f, 1f),
            new Color(1.00f, 0.36f, 0.86f, 1f),
            new Color(1.00f, 0.56f, 0.16f, 1f),
            new Color(0.35f, 0.56f, 1.00f, 1f)
        };

        private IEnumerator CoRunConfettiBurst(RectTransform anchor, int count)
        {
            if (anchor == null) yield break;
            Sprite shardSp = GemSpriteManager.GetWinHelperSprite("win_shard") ?? SpriteGenerator.GetShardSprite();
            List<ConfettiParticle> particles = new List<ConfettiParticle>(count);

            for (int i = 0; i < count; i++)
            {
                float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float spd = UnityEngine.Random.Range(260f, 760f);
                Vector2 vel = new Vector2(Mathf.Cos(ang) * spd, Mathf.Sin(ang) * spd + UnityEngine.Random.Range(160f, 420f));
                Vector2 spawnPos = new Vector2(UnityEngine.Random.Range(-90f, 90f), UnityEngine.Random.Range(-50f, 50f));
                float w = UnityEngine.Random.Range(16f, 28f);
                float h = UnityEngine.Random.Range(8f, 14f);
                Color col = WinConfettiPalette[i % WinConfettiPalette.Length];

                RectTransform rt = SpawnFloatingEffectImage("ConfettiBurst_" + i, anchor, shardSp, spawnPos, new Vector2(w, h), col, false);
                rt.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

                particles.Add(new ConfettiParticle
                {
                    Rect = rt,
                    Img = rt.GetComponent<Image>(),
                    Vel = vel,
                    Spin = UnityEngine.Random.Range(-420f, 420f),
                    FlutterFreq = UnityEngine.Random.Range(8f, 16f),
                    FlutterPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                    Lifetime = UnityEngine.Random.Range(1.8f, 2.8f),
                    Age = 0f,
                    BaseColor = col
                });
            }

            while (particles.Count > 0 && anchor != null && _winEffectsLayer != null && IsVictoryShowing)
            {
                float dt = Time.unscaledDeltaTime;
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    ConfettiParticle p = particles[i];
                    p.Age += dt;
                    if (p.Age >= p.Lifetime || p.Rect == null)
                    {
                        if (p.Rect != null) Destroy(p.Rect.gameObject);
                        particles.RemoveAt(i);
                        continue;
                    }

                    p.Vel.x *= Mathf.Max(0f, 1f - 1.35f * dt);
                    p.Vel.y -= 620f * dt;
                    p.Rect.anchoredPosition += p.Vel * dt;
                    p.Rect.Rotate(0f, 0f, p.Spin * dt);
                    p.Rect.localScale = new Vector3(1f, Mathf.Cos(p.Age * p.FlutterFreq + p.FlutterPhase), 1f);

                    if (p.Img != null)
                    {
                        float remain = p.Lifetime - p.Age;
                        float alpha = remain < 0.5f ? Mathf.Clamp01(remain / 0.5f) : 1f;
                        p.Img.color = new Color(p.BaseColor.r, p.BaseColor.g, p.BaseColor.b, alpha);
                    }
                    particles[i] = p;
                }
                yield return null;
            }

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].Rect != null) Destroy(particles[i].Rect.gameObject);
            }
        }

        private IEnumerator CoRunAmbientConfetti(RectTransform anchor)
        {
            if (anchor == null) yield break;
            Sprite shardSp = GemSpriteManager.GetWinHelperSprite("win_shard") ?? SpriteGenerator.GetShardSprite();
            List<ConfettiParticle> active = new List<ConfettiParticle>(12);
            float spawnTimer = 0.25f;

            while (anchor != null && _winEffectsLayer != null && IsVictoryShowing)
            {
                float dt = Time.unscaledDeltaTime;
                spawnTimer += dt;
                if (spawnTimer >= 0.55f)
                {
                    spawnTimer = 0f;
                    float w = UnityEngine.Random.Range(14f, 22f);
                    float h = UnityEngine.Random.Range(7f, 12f);
                    Color col = WinConfettiPalette[UnityEngine.Random.Range(0, WinConfettiPalette.Length)];
                    Vector2 pos = new Vector2(UnityEngine.Random.Range(-420f, 420f), 420f);
                    RectTransform rt = SpawnFloatingEffectImage("ConfettiAmbient", anchor, shardSp, pos, new Vector2(w, h), col, false);
                    active.Add(new ConfettiParticle
                    {
                        Rect = rt,
                        Img = rt.GetComponent<Image>(),
                        Vel = new Vector2(UnityEngine.Random.Range(-45f, 45f), UnityEngine.Random.Range(-220f, -150f)),
                        Spin = UnityEngine.Random.Range(-220f, 220f),
                        FlutterFreq = UnityEngine.Random.Range(6f, 11f),
                        FlutterPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                        Lifetime = 3.2f,
                        Age = 0f,
                        BaseColor = col
                    });
                }

                for (int i = active.Count - 1; i >= 0; i--)
                {
                    ConfettiParticle p = active[i];
                    p.Age += dt;
                    if (p.Age >= p.Lifetime || p.Rect == null)
                    {
                        if (p.Rect != null) Destroy(p.Rect.gameObject);
                        active.RemoveAt(i);
                        continue;
                    }

                    p.Rect.anchoredPosition += p.Vel * dt;
                    p.Rect.Rotate(0f, 0f, p.Spin * dt);
                    p.Rect.localScale = new Vector3(1f, Mathf.Cos(p.Age * p.FlutterFreq + p.FlutterPhase), 1f);
                    if (p.Img != null)
                    {
                        float alpha = p.Age < 0.35f ? (p.Age / 0.35f) : (p.Lifetime - p.Age < 0.6f ? Mathf.Clamp01((p.Lifetime - p.Age) / 0.6f) : 0.9f);
                        p.Img.color = new Color(p.BaseColor.r, p.BaseColor.g, p.BaseColor.b, alpha);
                    }
                    active[i] = p;
                }

                yield return null;
            }

            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].Rect != null) Destroy(active[i].Rect.gameObject);
            }
            _winAmbientConfettiCoroutine = null;
        }

        private struct FlyingClaimCoin
        {
            public RectTransform Rect;
            public Image Img;
            public Vector2 StartPos;
            public Vector2 ScatterPos;
            public Vector2 ControlPos;
            public float LaunchDelay;
            public float FlightDur;
            public float Spin;
            public bool Arrived;
        }

        private static Vector2 WorldToLocalInRect(RectTransform source, RectTransform targetParent)
        {
            if (source == null || targetParent == null) return Vector2.zero;
            Vector3 worldCenter = source.TransformPoint(source.rect.center);
            Vector3 localPos = targetParent.InverseTransformPoint(worldCenter);
            return new Vector2(localPos.x, localPos.y);
        }

        private void ReturnToHomeAfterVictory()
        {
            if (_isLoadingNextLevel) return;

            Time.timeScale = 1f;
            _isPaused = false;
            if (_pausePanel != null) _pausePanel.SetActive(false);

            if (CustomLevelRepository.IsEditorPlayTestActive)
            {
                _isLoadingNextLevel = false;
                RestartLevel();
                return;
            }

            if (Application.CanStreamedLevelBeLoaded("HomeScene"))
            {
                _isLoadingNextLevel = true;
                SceneManager.LoadScene("HomeScene");
                return;
            }

            _isLoadingNextLevel = false;
            NextLevel();
        }

        private IEnumerator CoClaimRewardAndAdvance(int startCoins, int coinAmount, RectTransform clickedBtnRect)
        {
            if (_victoryCanvasGroup != null) _victoryCanvasGroup.alpha = 1f;
            if (clickedBtnRect != null) StartCoroutine(CoButtonPunch(clickedBtnRect));

            Transform winContent = _victoryPanel != null ? (_victoryPanel.transform.Find("WinContent") ?? _victoryPanel.transform) : transform;
            if (_winEffectsLayer == null)
            {
                _winEffectsLayer = CreateUIContainer("WinEffectsLayer", winContent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
            }
            _winEffectsLayer.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();

            Sprite coinSp = GemSpriteManager.GetWinSceneSprite("Coin");
            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();

            RectTransform originRect = clickedBtnRect != null
                ? clickedBtnRect
                : (_winCoinsImageRect != null ? _winCoinsImageRect : _winButtonsRoot);
            Vector2 spawnOrigin = originRect != null ? WorldToLocalInRect(originRect, _winEffectsLayer) : new Vector2(0f, -320f);
            Vector2 targetPos = _winTopCoinTargetRect != null
                ? WorldToLocalInRect(_winTopCoinTargetRect, _winEffectsLayer)
                : new Vector2(156f, 820f);

            StartCoroutine(CoSpawnFlashBurst(_winEffectsLayer, spawnOrigin, glowSp, new Color(1.0f, 0.90f, 0.42f, 0.85f), 0.35f, 1.9f, 0.26f));

            int count = coinAmount >= AdsWinRewardCoins ? 22 : 16;
            List<FlyingClaimCoin> coins = new List<FlyingClaimCoin>(count);
            for (int i = 0; i < count; i++)
            {
                float baseAngle = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.22f, 0.22f);
                float radius = UnityEngine.Random.Range(85f, 225f);
                Vector2 scatterPos = spawnOrigin + new Vector2(
                    Mathf.Cos(baseAngle) * radius * 1.15f,
                    Mathf.Sin(baseAngle) * radius * 0.85f + 70f);
                scatterPos.x = Mathf.Clamp(scatterPos.x, -380f, 380f);

                float launchDelay = 0.24f + i * 0.026f;
                float flightDur = UnityEngine.Random.Range(0.38f, 0.48f);
                Vector2 mid = Vector2.Lerp(scatterPos, targetPos, 0.45f);
                Vector2 controlPos = mid + new Vector2(
                    UnityEngine.Random.Range(-220f, 220f),
                    UnityEngine.Random.Range(-60f, 120f));

                RectTransform rt = SpawnFloatingEffectImage("ClaimCoin_" + i, _winEffectsLayer, coinSp, spawnOrigin, new Vector2(68f, 68f), Color.white, true);
                coins.Add(new FlyingClaimCoin
                {
                    Rect = rt,
                    Img = rt.GetComponent<Image>(),
                    StartPos = spawnOrigin,
                    ScatterPos = scatterPos,
                    ControlPos = controlPos,
                    LaunchDelay = launchDelay,
                    FlightDur = flightDur,
                    Spin = UnityEngine.Random.Range(-360f, 360f),
                    Arrived = false
                });
            }

            float elapsed = 0f;
            float pillPunchTimer = 0f;
            int arrivedCount = 0;
            const float btnFadeDur = 0.18f;
            const float safetyTimeout = 2.5f;

            while (arrivedCount < count && elapsed < safetyTimeout && _winEffectsLayer != null)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;

                // Fade & shrink buttons out as coins burst
                float btnT = Mathf.Clamp01(elapsed / btnFadeDur);
                if (_winButtonsCanvasGroup != null)
                {
                    _winButtonsCanvasGroup.alpha = 1f - btnT;
                }
                if (_winButtonsRoot != null)
                {
                    _winButtonsRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.92f, btnT);
                }

                // Top CoinsPill punch pulse when coins land
                if (pillPunchTimer > 0f && _winTopCoinsPillRect != null)
                {
                    pillPunchTimer = Mathf.Max(0f, pillPunchTimer - dt);
                    float pt = 1f - (pillPunchTimer / 0.14f);
                    float s = 1f + Mathf.Sin(pt * Mathf.PI) * 0.13f;
                    _winTopCoinsPillRect.localScale = Vector3.one * s;
                }
                else if (_winTopCoinsPillRect != null)
                {
                    _winTopCoinsPillRect.localScale = Vector3.one;
                }

                for (int i = 0; i < coins.Count; i++)
                {
                    FlyingClaimCoin c = coins[i];
                    if (c.Arrived || c.Rect == null) continue;

                    if (elapsed < c.LaunchDelay)
                    {
                        float sT = Mathf.Clamp01(elapsed / 0.24f);
                        float sEase = 1f - Mathf.Pow(1f - sT, 3f);
                        c.Rect.anchoredPosition = Vector2.Lerp(c.StartPos, c.ScatterPos, sEase);
                        c.Rect.localScale = Vector3.one * EvaluateOvershootScale(sT, 0.25f, 1.18f, 1.0f, 0.65f);
                        c.Rect.Rotate(0f, 0f, c.Spin * 0.35f * dt);
                    }
                    else
                    {
                        float fT = Mathf.Clamp01((elapsed - c.LaunchDelay) / c.FlightDur);
                        float e = Mathf.Lerp(fT * fT, fT * fT * (3f - 2f * fT), 0.45f);
                        float u = 1f - e;
                        c.Rect.anchoredPosition = (u * u) * c.ScatterPos + (2f * u * e) * c.ControlPos + (e * e) * targetPos;
                        c.Rect.localScale = Vector3.one * Mathf.Lerp(1.02f, 0.68f, e);
                        c.Rect.Rotate(0f, 0f, c.Spin * dt);

                        if (fT >= 1f)
                        {
                            c.Arrived = true;
                            Destroy(c.Rect.gameObject);
                            c.Rect = null;
                            arrivedCount++;

                            float progress = arrivedCount / (float)count;
                            if (_winTopCoinsCountText != null)
                            {
                                _winTopCoinsCountText.text = Mathf.RoundToInt(Mathf.Lerp(startCoins, startCoins + coinAmount, progress)).ToString();
                            }
                            pillPunchTimer = 0.14f;

                            if (arrivedCount == 1 || arrivedCount == count || arrivedCount % 2 == 0)
                            {
                                StartCoroutine(CoSpawnFlashBurst(_winEffectsLayer, targetPos, flareSp,
                                    new Color(1.0f, 0.92f, 0.42f, 0.95f), 0.25f, 1.15f, 0.20f));
                                SoundManager.Instance?.PlayCoinTick(0.94f + 0.34f * progress);
                            }
                        }
                    }

                    coins[i] = c;
                }

                yield return null;
            }

            for (int i = 0; i < coins.Count; i++)
            {
                if (coins[i].Rect != null) Destroy(coins[i].Rect.gameObject);
            }

            if (_winTopCoinsCountText != null)
            {
                _winTopCoinsCountText.text = (startCoins + coinAmount).ToString();
            }
            if (_winTopCoinsPillRect != null)
            {
                _winTopCoinsPillRect.localScale = Vector3.one;
            }
            if (_winEffectsLayer != null)
            {
                StartCoroutine(CoSpawnFlashBurst(_winEffectsLayer, targetPos, glowSp,
                    new Color(1.0f, 0.92f, 0.48f, 0.95f), 0.35f, 1.55f, 0.26f));
            }
            SoundManager.Instance?.PlayPickup();

            float hold = 0f;
            while (hold < 0.26f)
            {
                hold += Time.unscaledDeltaTime;
                yield return null;
            }

            float fadeOut = 0f;
            const float fadeDur = 0.18f;
            while (fadeOut < fadeDur)
            {
                fadeOut += Time.unscaledDeltaTime;
                if (_victoryCanvasGroup != null)
                {
                    _victoryCanvasGroup.alpha = 1f - Mathf.Clamp01(fadeOut / fadeDur);
                }
                yield return null;
            }

            _winClaimCoroutine = null;
            ReturnToHomeAfterVictory();
        }

        private static RectTransform SpawnFloatingEffectImage(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, Color color, bool preserveAspect = true)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return rt;
        }

        private static float EvaluateOvershootScale(float t, float startScale, float peakScale, float endScale, float peakT)
        {
            if (t < peakT)
            {
                float k = Mathf.Sin((t / peakT) * Mathf.PI * 0.5f);
                return Mathf.Lerp(startScale, peakScale, k);
            }
            float k2 = (t - peakT) / Mathf.Max(0.0001f, 1f - peakT);
            return Mathf.Lerp(peakScale, endScale, k2);
        }

        public void ShowGameOver(int score, int bestScore)
        {
            ShowVictoryPopup();
        }

        public void HideGameOver()
        {
            CleanupWinEffects();
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            HideTimeOutPopup();
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

        // =========================================================================
        // TIME OUT -> LEVEL FAILED (COSMIC GEMSTONE PURPLE + PLANET THEME)
        // =========================================================================

        private static readonly Color TimeOutHeaderTint = Color.white;
        private static readonly Color FailedHeaderTint = new Color(1.00f, 0.78f, 0.90f, 1.0f);
        private static readonly Color[] LoseHeartShardPalette = new Color[]
        {
            new Color(1.00f, 0.25f, 0.50f, 1f),
            new Color(0.88f, 0.14f, 0.42f, 1f),
            new Color(0.74f, 0.38f, 1.00f, 1f),
            new Color(1.00f, 0.72f, 0.90f, 1f)
        };

        [ContextMenu("Rebuild Time Out Panel")]
        public void RebuildTimeOutPanel()
        {
            int siblingIdx = -1;
            Transform oldPanel = transform.Find("TimeOutPanel");
            if (oldPanel != null)
            {
                siblingIdx = oldPanel.GetSiblingIndex();
                if (Application.isPlaying) Destroy(oldPanel.gameObject);
                else DestroyImmediate(oldPanel.gameObject);
            }

            _timeOutPanel = null;
            _timeOutCanvasGroup = null;
            _timeOutTopBarRect = null;
            _timeOutLivesPillRect = null;
            _timeOutLivesPillImg = null;
            _timeOutLivesCountText = null;
            _timeOutLivesTimerText = null;
            _timeOutCoinsPillRect = null;
            _timeOutCoinsCountText = null;
            _timeOutCenterStackRect = null;
            _timeOutMainCardRect = null;
            _timeOutPlanetRingedRect = null;
            _timeOutPlanetMiniRect = null;
            _timeOutCardSparklesRoot = null;
            _timeOutHeaderImg = null;
            _timeOutHeaderText = null;
            _timeOutStageRoot = null;
            _timeOutStopwatchRoot = null;
            _timeOutStopwatchAuraRect = null;
            _timeOutStopwatchSunburstRect = null;
            _timeOutStopwatchIconRect = null;
            _timeOutExtraText = null;
            _timeOutSubtitleText = null;
            _buyTimeBtn = null;
            _buyTimeBtnRect = null;
            _buyTimeCostText = null;
            _adTimeBtn = null;
            _adTimeBtnRect = null;
            _adTimeShineRect = null;
            _adTimeLabelText = null;
            _levelFailedStageRoot = null;
            _failedHeartRoot = null;
            _failedAuraRect = null;
            _failedHeartLeftRect = null;
            _failedHeartRightRect = null;
            _failedCrackGlowRect = null;
            _failedCrackGlowImg = null;
            _failedMinusOneText = null;
            _failedSubtitleText = null;
            _retryBtn = null;
            _retryBtnRect = null;
            _retryShineRect = null;
            _offerCardRect = null;
            _reviveOfferRoot = null;
            _reviveOfferCoinsText = null;
            _buyRevivePackBtn = null;
            _buyRevivePackBtnRect = null;
            _buyRevivePackPriceText = null;
            _buyRevivePackShineRect = null;
            _starterOfferRoot = null;
            _bestOfferRibbonRect = null;
            _starterOfferCoinsText = null;
            _buyStarterPackBtn = null;
            _buyStarterPackBtnRect = null;
            _buyStarterPackPriceText = null;
            _buyStarterPackShineRect = null;
            _tapToContinueBtn = null;
            _tapToContinueRect = null;
            _tapToContinueCanvasGroup = null;
            _timeOutEffectsLayer = null;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildTimeOutPanel(font);

            if (_timeOutPanel != null)
            {
                if (siblingIdx >= 0)
                {
                    _timeOutPanel.transform.SetSiblingIndex(Mathf.Min(siblingIdx, transform.childCount - 1));
                }
                Transform feedbackTr = transform.Find("FeedbackBanner");
                if (feedbackTr != null)
                {
                    feedbackTr.SetAsLastSibling();
                }
            }
        }

        private void BindTimeOutPanelUI()
        {
            if (_timeOutPanel == null) return;

            Transform topBar = _timeOutPanel.transform.Find("TopResourceBar");
            Transform centerStack = _timeOutPanel.transform.Find("CenterStack");
            Transform mainCard = centerStack?.Find("MainCard");
            Transform offerCard = centerStack?.Find("OfferBannerCard");
            Transform tapContinue = centerStack?.Find("Btn_TapToContinue");

            if (topBar == null || centerStack == null || mainCard == null || offerCard == null || tapContinue == null ||
                mainCard.Find("TimeOutContent/ButtonsRow/Btn_BuyTime") == null ||
                mainCard.Find("TimeOutContent/ButtonsRow/Btn_AdTime") == null ||
                mainCard.Find("LevelFailedContent/BrokenHeartRoot/HeartLeft") == null ||
                offerCard.Find("ReviveOfferContent/BoostersCluster/BoosterFreeze") == null ||
                offerCard.Find("StarterOfferContent/BoostersCluster/BoosterFreeze") == null)
            {
                RebuildTimeOutPanel();
                return;
            }

            _timeOutCanvasGroup = _timeOutPanel.GetComponent<CanvasGroup>();
            if (_timeOutCanvasGroup == null) _timeOutCanvasGroup = _timeOutPanel.AddComponent<CanvasGroup>();

            Button backdropBtn = _timeOutPanel.GetComponent<Button>();
            if (backdropBtn != null)
            {
                backdropBtn.onClick.RemoveAllListeners();
                backdropBtn.onClick.AddListener(OnTimeOutContinueClicked);
            }

            _timeOutTopBarRect = topBar as RectTransform;
            Transform livesPillTr = topBar.Find("LivesPill");
            if (livesPillTr != null)
            {
                _timeOutLivesPillRect = livesPillTr as RectTransform;
                _timeOutLivesPillImg = livesPillTr.GetComponent<Image>();
                _timeOutLivesCountText = livesPillTr.Find("HeartIcon/LivesCountText")?.GetComponent<Text>();
                _timeOutLivesTimerText = livesPillTr.Find("LivesTimerText")?.GetComponent<Text>();
                Button addLivesBtn = livesPillTr.Find("Btn_AddLives")?.GetComponent<Button>();
                if (addLivesBtn != null)
                {
                    addLivesBtn.onClick.RemoveAllListeners();
                    addLivesBtn.onClick.AddListener(OnTimeOutAddLivesClicked);
                }
            }

            Transform coinsPillTr = topBar.Find("CoinsPill");
            if (coinsPillTr != null)
            {
                _timeOutCoinsPillRect = coinsPillTr as RectTransform;
                _timeOutCoinsCountText = coinsPillTr.Find("CoinsCountText")?.GetComponent<Text>();
                Button addCoinsBtn = coinsPillTr.Find("Btn_AddCoins")?.GetComponent<Button>();
                if (addCoinsBtn != null)
                {
                    addCoinsBtn.onClick.RemoveAllListeners();
                    addCoinsBtn.onClick.AddListener(OnTimeOutAddCoinsClicked);
                }
            }

            _timeOutCenterStackRect = centerStack as RectTransform;
            _timeOutMainCardRect = mainCard as RectTransform;
            _timeOutPlanetRingedRect = mainCard.Find("PlanetRinged") as RectTransform;
            _timeOutPlanetMiniRect = mainCard.Find("PlanetMini") as RectTransform;
            _timeOutCardSparklesRoot = mainCard.Find("CardSparkles") as RectTransform;

            Transform headerTr = mainCard.Find("HeaderBanner");
            if (headerTr != null)
            {
                _timeOutHeaderImg = headerTr.GetComponent<Image>();
                _timeOutHeaderText = headerTr.Find("HeaderTitleText")?.GetComponent<Text>();
            }

            Transform timeOutTr = mainCard.Find("TimeOutContent");
            if (timeOutTr != null)
            {
                _timeOutStageRoot = timeOutTr.gameObject;
                Transform swTr = timeOutTr.Find("StopwatchRoot");
                _timeOutStopwatchRoot = swTr as RectTransform;
                _timeOutStopwatchAuraRect = swTr?.Find("StopwatchAura") as RectTransform;
                _timeOutStopwatchSunburstRect = swTr?.Find("StopwatchSunburst") as RectTransform;
                _timeOutStopwatchIconRect = swTr?.Find("StopwatchIcon") as RectTransform;
                _timeOutExtraText = timeOutTr.Find("ExtraTimeText")?.GetComponent<Text>();
                _timeOutSubtitleText = timeOutTr.Find("TimeOutSubtitle")?.GetComponent<Text>();

                Transform buyTr = timeOutTr.Find("ButtonsRow/Btn_BuyTime");
                if (buyTr != null)
                {
                    _buyTimeBtn = buyTr.GetComponent<Button>();
                    _buyTimeBtnRect = buyTr as RectTransform;
                    _buyTimeCostText = buyTr.Find("CostText")?.GetComponent<Text>();
                    if (_buyTimeBtn != null)
                    {
                        _buyTimeBtn.onClick.RemoveAllListeners();
                        _buyTimeBtn.onClick.AddListener(OnBuyExtraTimeClicked);
                    }
                }

                Transform adTr = timeOutTr.Find("ButtonsRow/Btn_AdTime");
                if (adTr != null)
                {
                    _adTimeBtn = adTr.GetComponent<Button>();
                    _adTimeBtnRect = adTr as RectTransform;
                    _adTimeLabelText = adTr.Find("AdLabel")?.GetComponent<Text>();
                    _adTimeShineRect = adTr.Find("ShineClip/ShineSweep") as RectTransform;
                    if (_adTimeBtn != null)
                    {
                        _adTimeBtn.onClick.RemoveAllListeners();
                        _adTimeBtn.onClick.AddListener(OnWatchAdExtraTimeClicked);
                    }
                }
            }

            Transform failedTr = mainCard.Find("LevelFailedContent");
            if (failedTr != null)
            {
                _levelFailedStageRoot = failedTr.gameObject;
                Transform heartRootTr = failedTr.Find("BrokenHeartRoot");
                _failedHeartRoot = heartRootTr as RectTransform;
                _failedAuraRect = heartRootTr?.Find("FailAura") as RectTransform;
                _failedHeartLeftRect = heartRootTr?.Find("HeartLeft") as RectTransform;
                _failedHeartRightRect = heartRootTr?.Find("HeartRight") as RectTransform;
                Transform crackGlowTr = heartRootTr?.Find("CrackGlow");
                _failedCrackGlowRect = crackGlowTr as RectTransform;
                _failedCrackGlowImg = crackGlowTr?.GetComponent<Image>();
                _failedMinusOneText = heartRootTr?.Find("MinusOneText")?.GetComponent<Text>();
                _failedSubtitleText = failedTr.Find("FailedSubtitle")?.GetComponent<Text>();

                Transform retryTr = failedTr.Find("Btn_Retry");
                if (retryTr != null)
                {
                    _retryBtn = retryTr.GetComponent<Button>();
                    _retryBtnRect = retryTr as RectTransform;
                    _retryShineRect = retryTr.Find("ShineClip/ShineSweep") as RectTransform;
                    if (_retryBtn != null)
                    {
                        _retryBtn.onClick.RemoveAllListeners();
                        _retryBtn.onClick.AddListener(OnRetryLevelClicked);
                    }
                }
            }

            _offerCardRect = offerCard as RectTransform;
            Transform reviveOfferTr = offerCard.Find("ReviveOfferContent");
            if (reviveOfferTr != null)
            {
                _reviveOfferRoot = reviveOfferTr.gameObject;
                _reviveOfferCoinsText = reviveOfferTr.Find("CoinsBundle/CoinsText")?.GetComponent<Text>();
                Transform buyPackTr = reviveOfferTr.Find("FooterRow/Btn_BuyRevivePack");
                if (buyPackTr != null)
                {
                    _buyRevivePackBtn = buyPackTr.GetComponent<Button>();
                    _buyRevivePackBtnRect = buyPackTr as RectTransform;
                    _buyRevivePackPriceText = buyPackTr.Find("PriceText")?.GetComponent<Text>();
                    _buyRevivePackShineRect = buyPackTr.Find("ShineClip/ShineSweep") as RectTransform;
                    if (_buyRevivePackBtn != null)
                    {
                        _buyRevivePackBtn.onClick.RemoveAllListeners();
                        _buyRevivePackBtn.onClick.AddListener(OnBuyReviveBundleClicked);
                    }
                }
            }

            Transform starterOfferTr = offerCard.Find("StarterOfferContent");
            if (starterOfferTr != null)
            {
                _starterOfferRoot = starterOfferTr.gameObject;
                _bestOfferRibbonRect = starterOfferTr.Find("BestOfferRibbon") as RectTransform;
                _starterOfferCoinsText = starterOfferTr.Find("CoinsBundle/CoinsText")?.GetComponent<Text>();
                Transform buyStarterTr = starterOfferTr.Find("FooterRow/Btn_BuyStarterPack");
                if (buyStarterTr != null)
                {
                    _buyStarterPackBtn = buyStarterTr.GetComponent<Button>();
                    _buyStarterPackBtnRect = buyStarterTr as RectTransform;
                    _buyStarterPackPriceText = buyStarterTr.Find("PriceText")?.GetComponent<Text>();
                    _buyStarterPackShineRect = buyStarterTr.Find("ShineClip/ShineSweep") as RectTransform;
                    if (_buyStarterPackBtn != null)
                    {
                        _buyStarterPackBtn.onClick.RemoveAllListeners();
                        _buyStarterPackBtn.onClick.AddListener(OnBuyStarterBundleClicked);
                    }
                }
            }

            _tapToContinueBtn = tapContinue.GetComponent<Button>();
            _tapToContinueRect = tapContinue as RectTransform;
            _tapToContinueCanvasGroup = tapContinue.GetComponent<CanvasGroup>();
            if (_tapToContinueCanvasGroup == null) _tapToContinueCanvasGroup = tapContinue.gameObject.AddComponent<CanvasGroup>();
            if (_tapToContinueBtn != null)
            {
                _tapToContinueBtn.onClick.RemoveAllListeners();
                _tapToContinueBtn.onClick.AddListener(OnTimeOutContinueClicked);
            }

            EnsureTimeOutSprites();
            SyncTimeOutResourceTexts();
            ApplyTimeOutVisualState(showFailed: false);
            _timeOutPanel.SetActive(false);
        }

        private void EnsureTimeOutSprites()
        {
            if (_timeOutPanel == null) return;

            Sprite cardFrameSp = GemSpriteManager.GetLoseHelperSprite("lose_card_frame") ?? SpriteGenerator.GetLoseCardFrameSprite();
            Sprite headerSp = GemSpriteManager.GetLoseHelperSprite("lose_header_banner") ?? SpriteGenerator.GetLoseHeaderBannerSprite();
            Sprite offerCardSp = GemSpriteManager.GetLoseHelperSprite("lose_offer_card") ?? SpriteGenerator.GetLoseOfferCardSprite();
            Sprite ribbonSp = GemSpriteManager.GetLoseHelperSprite("lose_best_offer_ribbon") ?? SpriteGenerator.GetLoseBestOfferRibbonSprite();
            Sprite greenBtnSp = GemSpriteManager.GetLoseHelperSprite("lose_btn_green") ?? SpriteGenerator.GetLoseGreenButtonSprite();
            Sprite pillSp = GemSpriteManager.GetLoseHelperSprite("lose_resource_pill") ?? SpriteGenerator.GetLoseResourcePillSprite();
            Sprite plusSp = GemSpriteManager.GetLoseHelperSprite("lose_plus_badge") ?? SpriteGenerator.GetLosePlusBadgeSprite();
            Sprite stopwatchSp = GemSpriteManager.GetLoseHelperSprite("lose_stopwatch") ?? SpriteGenerator.GetLoseStopwatchSprite();
            Sprite heartLeftSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_left") ?? SpriteGenerator.GetLoseHeartLeftSprite();
            Sprite heartRightSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_right") ?? SpriteGenerator.GetLoseHeartRightSprite();
            Sprite heartSmallSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_small") ?? SpriteGenerator.GetLoseHeartSmallSprite();
            Sprite wingsSp = GemSpriteManager.GetLoseHelperSprite("lose_revive_wings") ?? SpriteGenerator.GetLoseReviveWingsSprite();
            Sprite planetRingedSp = GemSpriteManager.GetLoseHelperSprite("lose_planet_ringed") ?? SpriteGenerator.GetLosePlanetRingedSprite();
            Sprite planetMiniSp = GemSpriteManager.GetLoseHelperSprite("lose_planet_mini") ?? SpriteGenerator.GetLosePlanetMiniSprite();

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite sunburstSp = GemSpriteManager.GetWinHelperSprite("win_sunburst") ?? SpriteGenerator.GetWinSunburstSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();

            Sprite coinSp = GemSpriteManager.GetWinSceneSprite("Coin");
            Sprite coinsSp = GemSpriteManager.GetWinSceneSprite("Coins");
            Sprite adsSp = GemSpriteManager.GetWinSceneSprite("ADS");
            Sprite adBtnSp = GemSpriteManager.GetWinSceneSprite("Button2");

            Sprite freezeSp = GemSpriteManager.GetBoosterSprite("booster1");
            Sprite magnetSp = GemSpriteManager.GetBoosterSprite("booster2");
            Sprite wandSp = GemSpriteManager.GetBoosterSprite("booster3");

            Transform topBar = _timeOutPanel.transform.Find("TopResourceBar");
            if (topBar != null)
            {
                SetImageSpriteForce(topBar.Find("LivesPill"), pillSp, Image.Type.Sliced);
                SetImageSpriteForce(topBar.Find("LivesPill/HeartIcon"), heartSmallSp, Image.Type.Simple);
                SetImageSpriteForce(topBar.Find("LivesPill/Btn_AddLives"), plusSp, Image.Type.Simple);
                SetImageSpriteForce(topBar.Find("CoinsPill"), pillSp, Image.Type.Sliced);
                SetImageSpriteForce(topBar.Find("CoinsPill/CoinIcon"), coinSp, Image.Type.Simple);
                SetImageSpriteForce(topBar.Find("CoinsPill/Btn_AddCoins"), plusSp, Image.Type.Simple);
            }

            Transform mainCard = _timeOutPanel.transform.Find("CenterStack/MainCard");
            if (mainCard != null)
            {
                SetImageSpriteForce(mainCard, cardFrameSp, Image.Type.Sliced);
                SetImageSpriteForce(mainCard.Find("PlanetRinged"), planetRingedSp, Image.Type.Simple);
                SetImageSpriteForce(mainCard.Find("PlanetMini"), planetMiniSp, Image.Type.Simple);
                SetImageSpriteForce(mainCard.Find("HeaderBanner"), headerSp, Image.Type.Sliced);

                if (_timeOutCardSparklesRoot != null)
                {
                    for (int i = 0; i < _timeOutCardSparklesRoot.childCount; i++)
                    {
                        SetImageSpriteForce(_timeOutCardSparklesRoot.GetChild(i), flareSp, Image.Type.Simple);
                    }
                }

                Transform swTr = mainCard.Find("TimeOutContent/StopwatchRoot");
                if (swTr != null)
                {
                    SetImageSpriteForce(swTr.Find("StopwatchAura"), glowSp, Image.Type.Simple);
                    SetImageSpriteForce(swTr.Find("StopwatchSunburst"), sunburstSp, Image.Type.Simple);
                    SetImageSpriteForce(swTr.Find("StopwatchIcon"), stopwatchSp, Image.Type.Simple);
                    for (int i = 0; i < 3; i++)
                    {
                        SetImageSpriteForce(swTr.Find("SwSparkle_" + i), flareSp, Image.Type.Simple);
                    }
                }

                SetImageSpriteForce(mainCard.Find("TimeOutContent/ButtonsRow/Btn_BuyTime"), greenBtnSp, Image.Type.Sliced);
                SetImageSpriteForce(mainCard.Find("TimeOutContent/ButtonsRow/Btn_BuyTime/CoinIcon"), coinSp, Image.Type.Simple);
                SetImageSpriteForce(mainCard.Find("TimeOutContent/ButtonsRow/Btn_AdTime"), adBtnSp, Image.Type.Simple);
                SetImageSpriteForce(mainCard.Find("TimeOutContent/ButtonsRow/Btn_AdTime/AdIcon"), adsSp, Image.Type.Simple);
                SetImageSpriteForce(mainCard.Find("TimeOutContent/ButtonsRow/Btn_AdTime/ShineClip/ShineSweep"), shineSp, Image.Type.Simple);

                Transform heartTr = mainCard.Find("LevelFailedContent/BrokenHeartRoot");
                if (heartTr != null)
                {
                    SetImageSpriteForce(heartTr.Find("FailAura"), glowSp, Image.Type.Simple);
                    SetImageSpriteForce(heartTr.Find("HeartLeft"), heartLeftSp, Image.Type.Simple);
                    SetImageSpriteForce(heartTr.Find("HeartRight"), heartRightSp, Image.Type.Simple);
                    SetImageSpriteForce(heartTr.Find("CrackGlow"), flareSp, Image.Type.Simple);
                }

                SetImageSpriteForce(mainCard.Find("LevelFailedContent/Btn_Retry"), greenBtnSp, Image.Type.Sliced);
                SetImageSpriteForce(mainCard.Find("LevelFailedContent/Btn_Retry/ShineClip/ShineSweep"), shineSp, Image.Type.Simple);
            }

            Transform offerCard = _timeOutPanel.transform.Find("CenterStack/OfferBannerCard");
            if (offerCard != null)
            {
                SetImageSpriteForce(offerCard, offerCardSp, Image.Type.Sliced);
                SetImageSpriteForce(offerCard.Find("WatermarkPlanet"), planetRingedSp, Image.Type.Simple);

                Transform reviveTr = offerCard.Find("ReviveOfferContent");
                if (reviveTr != null)
                {
                    SetImageSpriteForce(reviveTr.Find("CoinsBundle/CoinsImg"), coinsSp, Image.Type.Simple);
                    SetImageSpriteForce(reviveTr.Find("BoostersCluster/BoosterWand"), wandSp, Image.Type.Simple);
                    SetImageSpriteForce(reviveTr.Find("BoostersCluster/BoosterFreeze"), freezeSp, Image.Type.Simple);
                    SetImageSpriteForce(reviveTr.Find("BoostersCluster/BoosterMagnet"), magnetSp, Image.Type.Simple);
                    SetImageSpriteForce(reviveTr.Find("ReviveWingsItem/WingsImg"), wingsSp, Image.Type.Simple);
                    SetImageSpriteForce(reviveTr.Find("FooterRow/Btn_BuyRevivePack"), greenBtnSp, Image.Type.Sliced);
                    SetImageSpriteForce(reviveTr.Find("FooterRow/Btn_BuyRevivePack/ShineClip/ShineSweep"), shineSp, Image.Type.Simple);
                }

                Transform starterTr = offerCard.Find("StarterOfferContent");
                if (starterTr != null)
                {
                    SetImageSpriteForce(starterTr.Find("BestOfferRibbon"), ribbonSp, Image.Type.Sliced);
                    SetImageSpriteForce(starterTr.Find("CoinsBundle/CoinsImg"), coinsSp, Image.Type.Simple);
                    SetImageSpriteForce(starterTr.Find("BoostersCluster/BoosterWand"), wandSp, Image.Type.Simple);
                    SetImageSpriteForce(starterTr.Find("BoostersCluster/BoosterFreeze"), freezeSp, Image.Type.Simple);
                    SetImageSpriteForce(starterTr.Find("BoostersCluster/BoosterMagnet"), magnetSp, Image.Type.Simple);
                    SetImageSpriteForce(starterTr.Find("FooterRow/Btn_BuyStarterPack"), greenBtnSp, Image.Type.Sliced);
                    SetImageSpriteForce(starterTr.Find("FooterRow/Btn_BuyStarterPack/ShineClip/ShineSweep"), shineSp, Image.Type.Simple);
                }
            }
        }

        private static void SetImageSpriteForce(Transform tr, Sprite sprite, Image.Type imgType)
        {
            if (tr == null || sprite == null) return;
            Image img = tr.GetComponent<Image>();
            if (img == null) return;
            img.sprite = sprite;
            img.type = imgType;
        }

        private void BuildTimeOutPanel(Font font)
        {
            _timeOutPanel = CreateUIPanel("TimeOutPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetImageColor(_timeOutPanel, new Color(0.04f, 0.02f, 0.12f, 0.84f));
            _timeOutCanvasGroup = _timeOutPanel.AddComponent<CanvasGroup>();
            _timeOutCanvasGroup.alpha = 1f;

            Button backdropBtn = _timeOutPanel.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(OnTimeOutContinueClicked);

            Sprite cardFrameSp = GemSpriteManager.GetLoseHelperSprite("lose_card_frame") ?? SpriteGenerator.GetLoseCardFrameSprite();
            Sprite headerSp = GemSpriteManager.GetLoseHelperSprite("lose_header_banner") ?? SpriteGenerator.GetLoseHeaderBannerSprite();
            Sprite offerCardSp = GemSpriteManager.GetLoseHelperSprite("lose_offer_card") ?? SpriteGenerator.GetLoseOfferCardSprite();
            Sprite ribbonSp = GemSpriteManager.GetLoseHelperSprite("lose_best_offer_ribbon") ?? SpriteGenerator.GetLoseBestOfferRibbonSprite();
            Sprite greenBtnSp = GemSpriteManager.GetLoseHelperSprite("lose_btn_green") ?? SpriteGenerator.GetLoseGreenButtonSprite();
            Sprite pillSp = GemSpriteManager.GetLoseHelperSprite("lose_resource_pill") ?? SpriteGenerator.GetLoseResourcePillSprite();
            Sprite plusSp = GemSpriteManager.GetLoseHelperSprite("lose_plus_badge") ?? SpriteGenerator.GetLosePlusBadgeSprite();
            Sprite stopwatchSp = GemSpriteManager.GetLoseHelperSprite("lose_stopwatch") ?? SpriteGenerator.GetLoseStopwatchSprite();
            Sprite heartLeftSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_left") ?? SpriteGenerator.GetLoseHeartLeftSprite();
            Sprite heartRightSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_right") ?? SpriteGenerator.GetLoseHeartRightSprite();
            Sprite heartSmallSp = GemSpriteManager.GetLoseHelperSprite("lose_heart_small") ?? SpriteGenerator.GetLoseHeartSmallSprite();
            Sprite wingsSp = GemSpriteManager.GetLoseHelperSprite("lose_revive_wings") ?? SpriteGenerator.GetLoseReviveWingsSprite();
            Sprite planetRingedSp = GemSpriteManager.GetLoseHelperSprite("lose_planet_ringed") ?? SpriteGenerator.GetLosePlanetRingedSprite();
            Sprite planetMiniSp = GemSpriteManager.GetLoseHelperSprite("lose_planet_mini") ?? SpriteGenerator.GetLosePlanetMiniSprite();

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite sunburstSp = GemSpriteManager.GetWinHelperSprite("win_sunburst") ?? SpriteGenerator.GetWinSunburstSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();

            Sprite coinSp = GemSpriteManager.GetWinSceneSprite("Coin");
            Sprite coinsSp = GemSpriteManager.GetWinSceneSprite("Coins");
            Sprite adsSp = GemSpriteManager.GetWinSceneSprite("ADS");
            Sprite adBtnSp = GemSpriteManager.GetWinSceneSprite("Button2");

            Sprite freezeSp = GemSpriteManager.GetBoosterSprite("booster1");
            Sprite magnetSp = GemSpriteManager.GetBoosterSprite("booster2");
            Sprite wandSp = GemSpriteManager.GetBoosterSprite("booster3");

            // 1. Top Resource Bar (Lives & Coins)
            GameObject topBarObj = CreateUIContainer("TopResourceBar", _timeOutPanel.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(840f, 88f));
            _timeOutTopBarRect = topBarObj.GetComponent<RectTransform>();

            // Lives Pill
            GameObject livesPillObj = CreateUIPanel("LivesPill", topBarObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-168f, 0f), new Vector2(330f, 82f));
            _timeOutLivesPillRect = livesPillObj.GetComponent<RectTransform>();
            _timeOutLivesPillImg = livesPillObj.GetComponent<Image>();
            _timeOutLivesPillImg.sprite = pillSp;
            _timeOutLivesPillImg.type = Image.Type.Sliced;
            _timeOutLivesPillImg.color = Color.white;
            _timeOutLivesPillImg.raycastTarget = true;

            Image heartIcon = CreateWinImage("HeartIcon", livesPillObj.transform, new Vector2(-146f, 0f), new Vector2(80f, 80f),
                heartSmallSp, Color.white, preserveAspect: true);
            _timeOutLivesCountText = CreateOutlinedText("LivesCountText", heartIcon.transform, DefaultLives.ToString(), font, 34,
                Color.white, new Vector2(0f, 2f), new Vector2(60f, 52f),
                new Color(0.227f, 0.020f, 0.125f, 0.98f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);

            _timeOutLivesTimerText = CreateOutlinedText("LivesTimerText", livesPillObj.transform, FormatTimeMMSS(_timeOutLifeRegenSeconds), font, 34,
                Color.white, new Vector2(6f, 0f), new Vector2(160f, 56f),
                new Color(0.05f, 0.02f, 0.14f, 0.95f), new Vector2(1.5f, -1.5f), TextAnchor.MiddleCenter);

            GameObject addLivesObj = CreateUIPanel("Btn_AddLives", livesPillObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(136f, 0f), new Vector2(48f, 48f));
            Image addLivesImg = addLivesObj.GetComponent<Image>();
            addLivesImg.sprite = plusSp;
            addLivesImg.preserveAspect = true;
            addLivesImg.color = Color.white;
            Button addLivesBtn = addLivesObj.AddComponent<Button>();
            addLivesBtn.targetGraphic = addLivesImg;
            addLivesBtn.onClick.AddListener(OnTimeOutAddLivesClicked);

            // Coins Pill
            GameObject coinsPillObj = CreateUIPanel("CoinsPill", topBarObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(182f, 0f), new Vector2(300f, 82f));
            _timeOutCoinsPillRect = coinsPillObj.GetComponent<RectTransform>();
            Image coinsPillImg = coinsPillObj.GetComponent<Image>();
            coinsPillImg.sprite = pillSp;
            coinsPillImg.type = Image.Type.Sliced;
            coinsPillImg.color = Color.white;
            coinsPillImg.raycastTarget = true;

            CreateWinImage("CoinIcon", coinsPillObj.transform, new Vector2(-128f, 0f), new Vector2(76f, 76f),
                coinSp, Color.white, preserveAspect: true);

            _timeOutCoinsCountText = CreateOutlinedText("CoinsCountText", coinsPillObj.transform, "2000", font, 36,
                new Color(1.00f, 0.97f, 0.78f), new Vector2(8f, 0f), new Vector2(165f, 56f),
                new Color(0.05f, 0.02f, 0.14f, 0.95f), new Vector2(1.5f, -1.5f), TextAnchor.MiddleCenter);

            GameObject addCoinsObj = CreateUIPanel("Btn_AddCoins", coinsPillObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(122f, 0f), new Vector2(48f, 48f));
            Image addCoinsImg = addCoinsObj.GetComponent<Image>();
            addCoinsImg.sprite = plusSp;
            addCoinsImg.preserveAspect = true;
            addCoinsImg.color = Color.white;
            Button addCoinsBtn = addCoinsObj.AddComponent<Button>();
            addCoinsBtn.targetGraphic = addCoinsImg;
            addCoinsBtn.onClick.AddListener(OnTimeOutAddCoinsClicked);

            // 2. Center Stack
            GameObject centerStackObj = CreateUIContainer("CenterStack", _timeOutPanel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -25f), new Vector2(860f, 1460f));
            _timeOutCenterStackRect = centerStackObj.GetComponent<RectTransform>();

            // 2A. Main Gemstone Card
            GameObject mainCardObj = CreateUIPanel("MainCard", centerStackObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(820f, 680f));
            _timeOutMainCardRect = mainCardObj.GetComponent<RectTransform>();
            Image mainCardImg = mainCardObj.GetComponent<Image>();
            mainCardImg.sprite = cardFrameSp;
            mainCardImg.type = Image.Type.Sliced;
            mainCardImg.color = Color.white;
            mainCardImg.raycastTarget = true;

            // Protruding Header Gemstone Banner
            GameObject headerObj = CreateUIPanel("HeaderBanner", mainCardObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 328f), new Vector2(670f, 124f));
            _timeOutHeaderImg = headerObj.GetComponent<Image>();
            _timeOutHeaderImg.sprite = headerSp;
            _timeOutHeaderImg.type = Image.Type.Sliced;
            _timeOutHeaderImg.color = TimeOutHeaderTint;
            _timeOutHeaderImg.raycastTarget = false;

            _timeOutHeaderText = CreateOutlinedText("HeaderTitleText", headerObj.transform, "Time Out!", font, 54,
                Color.white, new Vector2(0f, 2f), new Vector2(560f, 92f),
                new Color(0.125f, 0.031f, 0.259f, 0.98f), new Vector2(3.5f, -3.5f), TextAnchor.MiddleCenter);

            // Cosmic Corner Planets & Sparkles (rendered after header so corner planet perches cleanly)
            Image ringedPlanet = CreateWinImage("PlanetRinged", mainCardObj.transform, new Vector2(386f, 260f), new Vector2(158f, 124f),
                planetRingedSp, Color.white, preserveAspect: true);
            _timeOutPlanetRingedRect = ringedPlanet.GetComponent<RectTransform>();
            _timeOutPlanetRingedRect.localEulerAngles = new Vector3(0f, 0f, -10f);

            Image miniPlanet = CreateWinImage("PlanetMini", mainCardObj.transform, new Vector2(-368f, -284f), new Vector2(86f, 86f),
                planetMiniSp, Color.white, preserveAspect: true);
            _timeOutPlanetMiniRect = miniPlanet.GetComponent<RectTransform>();

            GameObject sparklesObj = CreateUIContainer("CardSparkles", mainCardObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 680f));
            _timeOutCardSparklesRoot = sparklesObj.GetComponent<RectTransform>();
            CreateWinSparkle("CardSparkle_0", _timeOutCardSparklesRoot, new Vector2(-356f, 278f), 38f, flareSp, new Color(0.84f, 0.65f, 1.00f, 1f));
            CreateWinSparkle("CardSparkle_1", _timeOutCardSparklesRoot, new Vector2(-282f, 316f), 26f, flareSp, new Color(0.45f, 0.94f, 1.00f, 1f));
            CreateWinSparkle("CardSparkle_2", _timeOutCardSparklesRoot, new Vector2(280f, 344f), 32f, flareSp, new Color(1.00f, 0.88f, 0.40f, 1f));
            CreateWinSparkle("CardSparkle_3", _timeOutCardSparklesRoot, new Vector2(382f, 180f), 28f, flareSp, new Color(1.00f, 0.45f, 0.92f, 1f));
            CreateWinSparkle("CardSparkle_4", _timeOutCardSparklesRoot, new Vector2(-380f, -210f), 30f, flareSp, new Color(0.45f, 0.94f, 1.00f, 1f));
            CreateWinSparkle("CardSparkle_5", _timeOutCardSparklesRoot, new Vector2(362f, -290f), 36f, flareSp, new Color(0.84f, 0.65f, 1.00f, 1f));

            // Stage 1: TimeOutContent
            GameObject timeOutContent = CreateUIContainer("TimeOutContent", mainCardObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(760f, 560f));
            _timeOutStageRoot = timeOutContent;

            GameObject swRootObj = CreateUIContainer("StopwatchRoot", timeOutContent.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 138f), new Vector2(260f, 260f));
            _timeOutStopwatchRoot = swRootObj.GetComponent<RectTransform>();

            Image swAura = CreateWinImage("StopwatchAura", _timeOutStopwatchRoot, Vector2.zero, new Vector2(360f, 360f),
                glowSp, new Color(0.75f, 0.32f, 1.0f, 0.65f), preserveAspect: false);
            _timeOutStopwatchAuraRect = swAura.GetComponent<RectTransform>();

            Image swSunburst = CreateWinImage("StopwatchSunburst", _timeOutStopwatchRoot, Vector2.zero, new Vector2(300f, 300f),
                sunburstSp, new Color(0.95f, 0.80f, 1.0f, 0.45f), preserveAspect: true);
            _timeOutStopwatchSunburstRect = swSunburst.GetComponent<RectTransform>();

            Image swIcon = CreateWinImage("StopwatchIcon", _timeOutStopwatchRoot, Vector2.zero, new Vector2(196f, 196f),
                stopwatchSp, Color.white, preserveAspect: true);
            _timeOutStopwatchIconRect = swIcon.GetComponent<RectTransform>();

            CreateWinSparkle("SwSparkle_0", _timeOutStopwatchRoot, new Vector2(-106f, 68f), 32f, flareSp, new Color(1.00f, 0.92f, 0.42f, 1f));
            CreateWinSparkle("SwSparkle_1", _timeOutStopwatchRoot, new Vector2(114f, 56f), 28f, flareSp, new Color(0.45f, 0.95f, 1.00f, 1f));
            CreateWinSparkle("SwSparkle_2", _timeOutStopwatchRoot, new Vector2(92f, -72f), 26f, flareSp, new Color(1.00f, 0.45f, 0.90f, 1f));

            _timeOutExtraText = CreateOutlinedText("ExtraTimeText", timeOutContent.transform, "+" + Mathf.RoundToInt(_reviveExtraSeconds) + "s", font, 54,
                new Color(1.00f, 0.96f, 0.706f), new Vector2(0f, 18f), new Vector2(320f, 68f),
                new Color(0.157f, 0.043f, 0.322f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleCenter);

            _timeOutSubtitleText = CreateOutlinedText("TimeOutSubtitle", timeOutContent.transform,
                $"Add {Mathf.RoundToInt(_reviveExtraSeconds)} more seconds to keep merging!", font, 29,
                new Color(0.925f, 0.886f, 1.00f), new Vector2(0f, -48f), new Vector2(710f, 56f),
                new Color(0.08f, 0.03f, 0.22f, 0.92f), new Vector2(1.5f, -1.5f), TextAnchor.MiddleCenter);

            GameObject buttonsRowObj = CreateUIContainer("ButtonsRow", timeOutContent.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -172f), new Vector2(730f, 166f));

            // Left CTA: Buy 1900
            GameObject buyBtnObj = CreateUIPanel("Btn_BuyTime", buttonsRowObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-178f, 0f), new Vector2(334f, 148f));
            _buyTimeBtnRect = buyBtnObj.GetComponent<RectTransform>();
            Image buyBtnImg = buyBtnObj.GetComponent<Image>();
            buyBtnImg.sprite = greenBtnSp;
            buyBtnImg.type = Image.Type.Sliced;
            buyBtnImg.color = Color.white;
            _buyTimeBtn = buyBtnObj.AddComponent<Button>();
            _buyTimeBtn.targetGraphic = buyBtnImg;
            _buyTimeBtn.onClick.AddListener(OnBuyExtraTimeClicked);

            CreateOutlinedText("BuyLabel", buyBtnObj.transform, "Buy", font, 38,
                Color.white, new Vector2(0f, 32f), new Vector2(260f, 54f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);
            CreateWinImage("CoinIcon", buyBtnObj.transform, new Vector2(-68f, -28f), new Vector2(50f, 50f),
                coinSp, Color.white, preserveAspect: true);
            _buyTimeCostText = CreateOutlinedText("CostText", buyBtnObj.transform, _reviveCoinCost.ToString(), font, 46,
                new Color(1.00f, 0.976f, 0.784f), new Vector2(34f, -28f), new Vector2(148f, 60f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            // Right CTA: AD +20s
            GameObject adBtnObj = CreateUIPanel("Btn_AdTime", buttonsRowObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(178f, 0f), new Vector2(334f, 148f));
            _adTimeBtnRect = adBtnObj.GetComponent<RectTransform>();
            Image adBtnImg = adBtnObj.GetComponent<Image>();
            adBtnImg.sprite = adBtnSp;
            adBtnImg.type = Image.Type.Simple;
            adBtnImg.preserveAspect = false;
            adBtnImg.color = Color.white;
            _adTimeBtn = adBtnObj.AddComponent<Button>();
            _adTimeBtn.targetGraphic = adBtnImg;
            _adTimeBtn.onClick.AddListener(OnWatchAdExtraTimeClicked);

            CreateWinImage("AdIcon", adBtnObj.transform, new Vector2(-82f, 2f), new Vector2(84f, 84f),
                adsSp, Color.white, preserveAspect: true);
            _adTimeLabelText = CreateOutlinedText("AdLabel", adBtnObj.transform, "+" + Mathf.RoundToInt(_reviveExtraSeconds) + "s", font, 50,
                Color.white, new Vector2(48f, -2f), new Vector2(150f, 70f),
                new Color(0.243f, 0.043f, 0.173f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleCenter);
            _adTimeShineRect = CreateShineButtonOverlay(adBtnObj.transform, new Vector2(300f, 124f), new Vector2(110f, 180f), shineSp);

            // Stage 2: LevelFailedContent
            GameObject failedContent = CreateUIContainer("LevelFailedContent", mainCardObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(760f, 560f));
            _levelFailedStageRoot = failedContent;

            GameObject heartRootObj = CreateUIContainer("BrokenHeartRoot", failedContent.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 116f), new Vector2(260f, 260f));
            _failedHeartRoot = heartRootObj.GetComponent<RectTransform>();

            Image failAura = CreateWinImage("FailAura", _failedHeartRoot, Vector2.zero, new Vector2(360f, 360f),
                glowSp, new Color(0.95f, 0.20f, 0.52f, 0.72f), preserveAspect: false);
            _failedAuraRect = failAura.GetComponent<RectTransform>();

            Image heartLeftImg = CreateWinImage("HeartLeft", _failedHeartRoot, Vector2.zero, new Vector2(208f, 208f),
                heartLeftSp, Color.white, preserveAspect: true);
            _failedHeartLeftRect = heartLeftImg.GetComponent<RectTransform>();

            Image heartRightImg = CreateWinImage("HeartRight", _failedHeartRoot, Vector2.zero, new Vector2(208f, 208f),
                heartRightSp, Color.white, preserveAspect: true);
            _failedHeartRightRect = heartRightImg.GetComponent<RectTransform>();

            Image crackGlowImg = CreateWinImage("CrackGlow", _failedHeartRoot, Vector2.zero, new Vector2(110f, 180f),
                flareSp, new Color(1.0f, 0.72f, 0.90f, 0f), preserveAspect: false);
            _failedCrackGlowRect = crackGlowImg.GetComponent<RectTransform>();
            _failedCrackGlowImg = crackGlowImg;

            _failedMinusOneText = CreateOutlinedText("MinusOneText", _failedHeartRoot, "-1", font, 72,
                Color.white, new Vector2(0f, 4f), new Vector2(180f, 96f),
                new Color(0.227f, 0.020f, 0.125f, 0.98f), new Vector2(3.5f, -3.5f), TextAnchor.MiddleCenter);

            _failedSubtitleText = CreateOutlinedText("FailedSubtitle", failedContent.transform, "You failed!", font, 36,
                new Color(0.945f, 0.902f, 1.00f), new Vector2(0f, -26f), new Vector2(560f, 56f),
                new Color(0.08f, 0.03f, 0.22f, 0.92f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);

            GameObject retryBtnObj = CreateUIPanel("Btn_Retry", failedContent.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -158f), new Vector2(420f, 144f));
            _retryBtnRect = retryBtnObj.GetComponent<RectTransform>();
            Image retryImg = retryBtnObj.GetComponent<Image>();
            retryImg.sprite = greenBtnSp;
            retryImg.type = Image.Type.Sliced;
            retryImg.color = Color.white;
            _retryBtn = retryBtnObj.AddComponent<Button>();
            _retryBtn.targetGraphic = retryImg;
            _retryBtn.onClick.AddListener(OnRetryLevelClicked);

            CreateOutlinedText("RetryText", retryBtnObj.transform, "RETRY", font, 54,
                Color.white, Vector2.zero, new Vector2(340f, 90f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleCenter);
            _retryShineRect = CreateShineButtonOverlay(retryBtnObj.transform, new Vector2(384f, 120f), new Vector2(120f, 180f), shineSp);

            // 2B. Offer Banner Card
            GameObject offerCardObj = CreateUIPanel("OfferBannerCard", centerStackObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -322f), new Vector2(820f, 312f));
            _offerCardRect = offerCardObj.GetComponent<RectTransform>();
            Image offerCardImg = offerCardObj.GetComponent<Image>();
            offerCardImg.sprite = offerCardSp;
            offerCardImg.type = Image.Type.Sliced;
            offerCardImg.color = Color.white;
            offerCardImg.raycastTarget = true;

            CreateWinImage("WatermarkPlanet", offerCardObj.transform, new Vector2(290f, 68f), new Vector2(160f, 124f),
                planetRingedSp, new Color(1f, 1f, 1f, 0.16f), preserveAspect: true);

            // Offer Stage 1: ReviveOfferContent (with user's actual 3 boosters!)
            GameObject reviveOfferObj = CreateUIContainer("ReviveOfferContent", offerCardObj.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _reviveOfferRoot = reviveOfferObj;

            GameObject revCoinsObj = CreateUIContainer("CoinsBundle", reviveOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-258f, 52f), new Vector2(210f, 170f));
            CreateWinImage("CoinsImg", revCoinsObj.transform, new Vector2(0f, 18f), new Vector2(148f, 124f),
                coinsSp, Color.white, preserveAspect: true);
            _reviveOfferCoinsText = CreateOutlinedText("CoinsText", revCoinsObj.transform, _reviveBundleCoins.ToString(), font, 42,
                new Color(1.00f, 0.965f, 0.722f), new Vector2(0f, -54f), new Vector2(180f, 56f),
                new Color(0.114f, 0.039f, 0.231f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            CreateOfferBoosterCluster(reviveOfferObj.transform, new Vector2(-10f, 52f), new Vector2(260f, 174f), 72f,
                wandSp, freezeSp, magnetSp, font);

            GameObject wingsItemObj = CreateUIContainer("ReviveWingsItem", reviveOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(255f, 52f), new Vector2(210f, 170f));
            CreateWinImage("WingsImg", wingsItemObj.transform, new Vector2(0f, 18f), new Vector2(166f, 124f),
                wingsSp, Color.white, preserveAspect: true);
            CreateOutlinedText("ReviveLabel", wingsItemObj.transform, "Revive", font, 34,
                new Color(0.949f, 0.910f, 1.00f), new Vector2(0f, -54f), new Vector2(180f, 50f),
                new Color(0.114f, 0.039f, 0.231f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            GameObject revFooterObj = CreateUIContainer("FooterRow", reviveOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -98f), new Vector2(780f, 96f));
            CreateOutlinedText("OfferTitle", revFooterObj.transform, "Revive!", font, 48,
                Color.white, new Vector2(-205f, 0f), new Vector2(320f, 74f),
                new Color(0.102f, 0.031f, 0.212f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleLeft);

            GameObject buyReviveObj = CreateUIPanel("Btn_BuyRevivePack", revFooterObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(198f, 0f), new Vector2(334f, 84f));
            _buyRevivePackBtnRect = buyReviveObj.GetComponent<RectTransform>();
            Image buyReviveImg = buyReviveObj.GetComponent<Image>();
            buyReviveImg.sprite = greenBtnSp;
            buyReviveImg.type = Image.Type.Sliced;
            buyReviveImg.color = Color.white;
            _buyRevivePackBtn = buyReviveObj.AddComponent<Button>();
            _buyRevivePackBtn.targetGraphic = buyReviveImg;
            _buyRevivePackBtn.onClick.AddListener(OnBuyReviveBundleClicked);

            _buyRevivePackPriceText = CreateOutlinedText("PriceText", buyReviveObj.transform, _reviveBundlePrice, font, 38,
                Color.white, Vector2.zero, new Vector2(300f, 64f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);
            _buyRevivePackShineRect = CreateShineButtonOverlay(buyReviveObj.transform, new Vector2(306f, 68f), new Vector2(96f, 120f), shineSp);

            // Offer Stage 2: StarterOfferContent (Best Offer + Cosmic Pack! with user's actual 3 boosters!)
            GameObject starterOfferObj = CreateUIContainer("StarterOfferContent", offerCardObj.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _starterOfferRoot = starterOfferObj;

            GameObject ribbonObj = CreateUIPanel("BestOfferRibbon", starterOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-272f, 148f), new Vector2(276f, 62f));
            _bestOfferRibbonRect = ribbonObj.GetComponent<RectTransform>();
            Image ribbonImg = ribbonObj.GetComponent<Image>();
            ribbonImg.sprite = ribbonSp;
            ribbonImg.type = Image.Type.Sliced;
            ribbonImg.color = Color.white;
            ribbonImg.raycastTarget = false;
            CreateOutlinedText("RibbonText", ribbonObj.transform, "Best Offer", font, 30,
                Color.white, new Vector2(-8f, 1f), new Vector2(240f, 48f),
                new Color(0.227f, 0.031f, 0.212f, 0.98f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);

            GameObject stCoinsObj = CreateUIContainer("CoinsBundle", starterOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-248f, 48f), new Vector2(200f, 168f));
            CreateWinImage("CoinsImg", stCoinsObj.transform, new Vector2(0f, 16f), new Vector2(140f, 116f),
                coinsSp, Color.white, preserveAspect: true);
            _starterOfferCoinsText = CreateOutlinedText("CoinsText", stCoinsObj.transform, _starterBundleCoins.ToString(), font, 42,
                new Color(1.00f, 0.965f, 0.722f), new Vector2(0f, -54f), new Vector2(180f, 56f),
                new Color(0.114f, 0.039f, 0.231f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            CreateOutlinedText("PlusSymbol", starterOfferObj.transform, "+", font, 58,
                new Color(0.957f, 0.808f, 1.00f), new Vector2(-88f, 46f), new Vector2(70f, 70f),
                new Color(0.149f, 0.047f, 0.290f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            CreateOfferBoosterCluster(starterOfferObj.transform, new Vector2(148f, 52f), new Vector2(280f, 174f), 76f,
                wandSp, freezeSp, magnetSp, font);

            GameObject stFooterObj = CreateUIContainer("FooterRow", starterOfferObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -98f), new Vector2(780f, 96f));
            CreateOutlinedText("OfferTitle", stFooterObj.transform, "Cosmic Pack!", font, 44,
                Color.white, new Vector2(-190f, 0f), new Vector2(350f, 74f),
                new Color(0.102f, 0.031f, 0.212f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleLeft);

            GameObject buyStarterObj = CreateUIPanel("Btn_BuyStarterPack", stFooterObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(198f, 0f), new Vector2(334f, 84f));
            _buyStarterPackBtnRect = buyStarterObj.GetComponent<RectTransform>();
            Image buyStarterImg = buyStarterObj.GetComponent<Image>();
            buyStarterImg.sprite = greenBtnSp;
            buyStarterImg.type = Image.Type.Sliced;
            buyStarterImg.color = Color.white;
            _buyStarterPackBtn = buyStarterObj.AddComponent<Button>();
            _buyStarterPackBtn.targetGraphic = buyStarterImg;
            _buyStarterPackBtn.onClick.AddListener(OnBuyStarterBundleClicked);

            _buyStarterPackPriceText = CreateOutlinedText("PriceText", buyStarterObj.transform, _starterBundlePrice, font, 38,
                Color.white, Vector2.zero, new Vector2(300f, 64f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);
            _buyStarterPackShineRect = CreateShineButtonOverlay(buyStarterObj.transform, new Vector2(306f, 68f), new Vector2(96f, 120f), shineSp);

            // 2C. Tap To Continue
            GameObject tapObj = CreateUIPanel("Btn_TapToContinue", centerStackObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -612f), new Vector2(640f, 96f));
            _tapToContinueRect = tapObj.GetComponent<RectTransform>();
            Image tapBg = tapObj.GetComponent<Image>();
            tapBg.color = new Color(1f, 1f, 1f, 0.001f); // Transparent raycast target
            _tapToContinueCanvasGroup = tapObj.AddComponent<CanvasGroup>();
            _tapToContinueCanvasGroup.alpha = 1f;
            _tapToContinueBtn = tapObj.AddComponent<Button>();
            _tapToContinueBtn.transition = Selectable.Transition.None;
            _tapToContinueBtn.onClick.AddListener(OnTimeOutContinueClicked);

            CreateOutlinedText("ContinueText", tapObj.transform, "TAP TO CONTINUE", font, 40,
                new Color(0.918f, 0.878f, 1.00f), Vector2.zero, new Vector2(600f, 70f),
                new Color(0.086f, 0.027f, 0.188f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);

            SyncTimeOutResourceTexts();
            ApplyTimeOutVisualState(showFailed: false);
            _timeOutPanel.SetActive(false);
        }

        private Text CreateOutlinedText(string name, Transform parent, string content, Font font, int fontSize,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, Color outlineColor, Vector2 outlineDist, TextAnchor alignment)
        {
            Text txt = CreateStyledText(name, parent, content, font, fontSize, color, anchoredPos, sizeDelta, true,
                new Color(outlineColor.r * 0.5f, outlineColor.g * 0.5f, outlineColor.b * 0.5f, 0.90f),
                new Vector2(0f, -Mathf.Max(2f, Mathf.Abs(outlineDist.y) * 1.4f)));
            txt.alignment = alignment;

            Outline outline = txt.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDist;
            return txt;
        }

        private RectTransform CreateShineButtonOverlay(Transform parent, Vector2 clipSize, Vector2 sweepSize, Sprite shineSp)
        {
            GameObject shineClip = CreateUIContainer("ShineClip", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, clipSize);
            shineClip.AddComponent<RectMask2D>();
            Image shineImg = CreateWinImage("ShineSweep", shineClip.transform, new Vector2(-260f, 0f), sweepSize,
                shineSp, new Color(1f, 1f, 1f, 0.40f), preserveAspect: false);
            return shineImg.GetComponent<RectTransform>();
        }

        private void CreateOfferBoosterCluster(Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, float spreadX,
            Sprite wandSp, Sprite freezeSp, Sprite magnetSp, Font font)
        {
            GameObject cluster = CreateUIContainer("BoostersCluster", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);

            CreateOfferBoosterItem("BoosterWand", cluster.transform, new Vector2(0f, 46f), 90f, 8f, wandSp,
                new Vector2(38f, -26f), font);
            CreateOfferBoosterItem("BoosterFreeze", cluster.transform, new Vector2(-spreadX, -24f), 94f, -8f, freezeSp,
                new Vector2(36f, -32f), font);
            CreateOfferBoosterItem("BoosterMagnet", cluster.transform, new Vector2(spreadX, -24f), 92f, 10f, magnetSp,
                new Vector2(36f, -32f), font);
        }

        private void CreateOfferBoosterItem(string name, Transform parent, Vector2 anchoredPos, float size, float rotZ,
            Sprite sprite, Vector2 badgePos, Font font)
        {
            Image icon = CreateWinImage(name, parent, anchoredPos, new Vector2(size, size), sprite, Color.white, preserveAspect: true);
            icon.rectTransform.localEulerAngles = new Vector3(0f, 0f, rotZ);

            Text badge = CreateOutlinedText("QtyBadge", icon.transform, "x1", font, 26,
                new Color(1.00f, 0.957f, 0.651f), badgePos, new Vector2(64f, 40f),
                new Color(0.102f, 0.031f, 0.212f, 0.98f), new Vector2(2.2f, -2.2f), TextAnchor.MiddleCenter);
            badge.rectTransform.localEulerAngles = new Vector3(0f, 0f, -rotZ);
        }

        private void SyncTimeOutResourceTexts()
        {
            int lives = PlayerPrefs.HasKey(PrefLivesKey) ? PlayerPrefs.GetInt(PrefLivesKey, DefaultLives) : DefaultLives;
            int coins = PlayerPrefs.HasKey(PrefCoinsKey) ? PlayerPrefs.GetInt(PrefCoinsKey, DefaultStartingCoinsIfUnset) : DefaultStartingCoinsIfUnset;

            if (_timeOutLivesCountText != null) _timeOutLivesCountText.text = Mathf.Clamp(lives, 0, MaxLives).ToString();
            if (_timeOutLivesTimerText != null) _timeOutLivesTimerText.text = FormatTimeMMSS(_timeOutLifeRegenSeconds);
            if (_timeOutCoinsCountText != null) _timeOutCoinsCountText.text = coins.ToString();

            if (_timeOutExtraText != null) _timeOutExtraText.text = "+" + Mathf.RoundToInt(_reviveExtraSeconds) + "s";
            if (_timeOutSubtitleText != null) _timeOutSubtitleText.text = $"Add {Mathf.RoundToInt(_reviveExtraSeconds)} more seconds to keep merging!";
            if (_buyTimeCostText != null) _buyTimeCostText.text = _reviveCoinCost.ToString();
            if (_adTimeLabelText != null) _adTimeLabelText.text = "+" + Mathf.RoundToInt(_reviveExtraSeconds) + "s";

            if (_reviveOfferCoinsText != null) _reviveOfferCoinsText.text = _reviveBundleCoins.ToString();
            if (_buyRevivePackPriceText != null) _buyRevivePackPriceText.text = _reviveBundlePrice;
            if (_starterOfferCoinsText != null) _starterOfferCoinsText.text = _starterBundleCoins.ToString();
            if (_buyStarterPackPriceText != null) _buyStarterPackPriceText.text = _starterBundlePrice;
        }

        private void ApplyTimeOutVisualState(bool showFailed)
        {
            _isInLevelFailedState = showFailed;

            if (_timeOutStageRoot != null) _timeOutStageRoot.SetActive(!showFailed);
            if (_levelFailedStageRoot != null) _levelFailedStageRoot.SetActive(showFailed);
            if (_reviveOfferRoot != null) _reviveOfferRoot.SetActive(!showFailed);
            if (_starterOfferRoot != null) _starterOfferRoot.SetActive(showFailed);

            if (_timeOutHeaderText != null) _timeOutHeaderText.text = showFailed ? "Level Failed" : "Time Out!";
            if (_timeOutHeaderImg != null) _timeOutHeaderImg.color = showFailed ? FailedHeaderTint : TimeOutHeaderTint;

            if (_timeOutTopBarRect != null) _timeOutTopBarRect.anchoredPosition = new Vector2(0f, -145f);
            if (_timeOutMainCardRect != null) _timeOutMainCardRect.localScale = Vector3.one;
            if (_offerCardRect != null)
            {
                _offerCardRect.anchoredPosition = new Vector2(0f, -322f);
                _offerCardRect.localScale = Vector3.one;
            }
            if (_timeOutPlanetRingedRect != null)
            {
                _timeOutPlanetRingedRect.anchoredPosition = new Vector2(386f, 260f);
                _timeOutPlanetRingedRect.localScale = Vector3.one;
                _timeOutPlanetRingedRect.localEulerAngles = new Vector3(0f, 0f, -10f);
            }
            if (_timeOutPlanetMiniRect != null)
            {
                _timeOutPlanetMiniRect.anchoredPosition = new Vector2(-368f, -284f);
                _timeOutPlanetMiniRect.localScale = Vector3.one;
            }

            if (_timeOutStopwatchRoot != null) _timeOutStopwatchRoot.localScale = Vector3.one;
            if (_timeOutStopwatchIconRect != null)
            {
                _timeOutStopwatchIconRect.localScale = Vector3.one;
                _timeOutStopwatchIconRect.localRotation = Quaternion.identity;
            }
            if (_buyTimeBtnRect != null) _buyTimeBtnRect.localScale = Vector3.one;
            if (_adTimeBtnRect != null) _adTimeBtnRect.localScale = Vector3.one;
            if (_adTimeShineRect != null) _adTimeShineRect.anchoredPosition = new Vector2(-260f, 0f);

            if (_failedHeartRoot != null) _failedHeartRoot.localScale = Vector3.one;
            if (_failedHeartLeftRect != null)
            {
                _failedHeartLeftRect.anchoredPosition = showFailed ? new Vector2(-12f, -3f) : Vector2.zero;
                _failedHeartLeftRect.localEulerAngles = new Vector3(0f, 0f, showFailed ? 9f : 0f);
            }
            if (_failedHeartRightRect != null)
            {
                _failedHeartRightRect.anchoredPosition = showFailed ? new Vector2(12f, -3f) : Vector2.zero;
                _failedHeartRightRect.localEulerAngles = new Vector3(0f, 0f, showFailed ? -9f : 0f);
            }
            if (_failedCrackGlowRect != null) _failedCrackGlowRect.localScale = Vector3.one;
            if (_failedCrackGlowImg != null) _failedCrackGlowImg.color = new Color(1.0f, 0.72f, 0.90f, 0f);
            if (_failedMinusOneText != null) _failedMinusOneText.rectTransform.localScale = Vector3.one;
            if (_retryBtnRect != null) _retryBtnRect.localScale = Vector3.one;
            if (_retryShineRect != null) _retryShineRect.anchoredPosition = new Vector2(-260f, 0f);

            if (_bestOfferRibbonRect != null) _bestOfferRibbonRect.localScale = Vector3.one;
            if (_buyRevivePackShineRect != null) _buyRevivePackShineRect.anchoredPosition = new Vector2(-260f, 0f);
            if (_buyStarterPackShineRect != null) _buyStarterPackShineRect.anchoredPosition = new Vector2(-260f, 0f);
            if (_tapToContinueCanvasGroup != null) _tapToContinueCanvasGroup.alpha = 1f;
        }

        private void CleanupTimeOutEffects()
        {
            if (_timeOutAnimCoroutine != null)
            {
                StopCoroutine(_timeOutAnimCoroutine);
                _timeOutAnimCoroutine = null;
            }
            if (_timeOutTransitionCoroutine != null)
            {
                StopCoroutine(_timeOutTransitionCoroutine);
                _timeOutTransitionCoroutine = null;
            }
            if (_timeOutReviveCoroutine != null)
            {
                StopCoroutine(_timeOutReviveCoroutine);
                _timeOutReviveCoroutine = null;
            }
            _timeOutIntroComplete = false;
            _timeOutAllowContinue = false;
            if (_timeOutEffectsLayer != null)
            {
                if (Application.isPlaying) Destroy(_timeOutEffectsLayer.gameObject);
                else DestroyImmediate(_timeOutEffectsLayer.gameObject);
                _timeOutEffectsLayer = null;
            }
        }

        public void ShowTimeOutPopup()
        {
            if (_timeOutPanel == null)
            {
                RebuildTimeOutPanel();
            }
            if (_timeOutPanel == null || IsVictoryShowing || IsTimeOutShowing) return;

            StopTimer();
            _freezeTimerRemaining = 0f;
            _freezeEffectController?.StopImmediate();
            _magnetController?.StopImmediate();
            _wandController?.StopImmediate();
            _isPaused = false;
            Time.timeScale = 1f;
            if (_pausePanel != null) _pausePanel.SetActive(false);

            Gameplay.PlayerPieceController.Instance?.CancelActiveDragIfNeeded();

            if (!PlayerPrefs.HasKey(PrefLivesKey))
            {
                PlayerPrefs.SetInt(PrefLivesKey, DefaultLives);
            }
            if (!PlayerPrefs.HasKey(PrefCoinsKey))
            {
                PlayerPrefs.SetInt(PrefCoinsKey, DefaultStartingCoinsIfUnset);
            }
            PlayerPrefs.Save();

            EnsureTimeOutSprites();
            SyncTimeOutResourceTexts();
            ApplyTimeOutVisualState(showFailed: false);

            _timeOutPanel.transform.SetAsLastSibling();
            Transform feedbackTr = transform.Find("FeedbackBanner");
            if (feedbackTr != null) feedbackTr.SetAsLastSibling();

            _timeOutPanel.SetActive(true);
            SoundManager.Instance?.PlayTimeOut();

            if (Application.isPlaying)
            {
                CleanupTimeOutEffects();
                _timeOutAnimCoroutine = StartCoroutine(CoAnimateTimeOutIntro());
            }
            else
            {
                if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 1f;
            }
        }

        public void HideTimeOutPopup()
        {
            CleanupTimeOutEffects();
            _isInLevelFailedState = false;
            if (_timeOutPanel != null)
            {
                _timeOutPanel.SetActive(false);
            }
        }

        public void SetTimeOutPreviewActive(bool active, bool showFailedState = false)
        {
            if (active)
            {
                if (_timeOutPanel == null ||
                    _timeOutPanel.transform.Find("CenterStack/MainCard/TimeOutContent") == null ||
                    _timeOutPanel.transform.Find("CenterStack/OfferBannerCard/StarterOfferContent") == null)
                {
                    RebuildTimeOutPanel();
                }
                if (_timeOutPanel == null) return;

                CleanupTimeOutEffects();
                EnsureTimeOutSprites();
                SyncTimeOutResourceTexts();
                ApplyTimeOutVisualState(showFailedState);

                _timeOutPanel.transform.SetAsLastSibling();
                Transform feedbackTr = transform.Find("FeedbackBanner");
                if (feedbackTr != null) feedbackTr.SetAsLastSibling();

                if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 1f;
                _timeOutPanel.SetActive(true);
            }
            else
            {
                HideTimeOutPopup();
            }
        }

        public void ReplayTimeOutFlow()
        {
            HideGameOver();
            HideTimeOutPopup();
            ShowTimeOutPopup();
        }

        public void OnTimeOutContinueClicked()
        {
            if (!IsTimeOutShowing || !_timeOutAllowContinue ||
                _timeOutTransitionCoroutine != null || _timeOutReviveCoroutine != null)
            {
                return;
            }

            if (!_isInLevelFailedState)
            {
                SoundManager.Instance?.PlayButton();
                _timeOutTransitionCoroutine = StartCoroutine(CoTransitionToLevelFailed());
            }
            else
            {
                SoundManager.Instance?.PlayButton();
                HideTimeOutPopup();
                if (!CustomLevelRepository.IsEditorPlayTestActive && Application.CanStreamedLevelBeLoaded("HomeScene"))
                {
                    SceneManager.LoadScene("HomeScene");
                }
                else
                {
                    RestartLevel();
                }
            }
        }

        public void OnBuyExtraTimeClicked()
        {
            if (!IsTimeOutShowing || _isInLevelFailedState ||
                _timeOutTransitionCoroutine != null || _timeOutReviveCoroutine != null)
            {
                return;
            }

            int coins = PlayerPrefs.GetInt(PrefCoinsKey, DefaultStartingCoinsIfUnset);
            if (coins < _reviveCoinCost)
            {
                SoundManager.Instance?.PlayError();
                if (_buyTimeBtnRect != null) StartCoroutine(CoButtonShake(_buyTimeBtnRect));
                ShowFeedbackError($"Không đủ xu! Cần {_reviveCoinCost} xu");
                return;
            }

            coins -= _reviveCoinCost;
            PlayerPrefs.SetInt(PrefCoinsKey, coins);
            PlayerPrefs.Save();
            if (_timeOutCoinsCountText != null) _timeOutCoinsCountText.text = coins.ToString();

            _timeOutReviveCoroutine = StartCoroutine(CoReviveWithExtraTime(_reviveExtraSeconds, _buyTimeBtnRect));
        }

        public void OnWatchAdExtraTimeClicked()
        {
            if (!IsTimeOutShowing || _isInLevelFailedState ||
                _timeOutTransitionCoroutine != null || _timeOutReviveCoroutine != null)
            {
                return;
            }

            _timeOutReviveCoroutine = StartCoroutine(CoReviveWithExtraTime(_reviveExtraSeconds, _adTimeBtnRect));
        }

        public void OnBuyReviveBundleClicked()
        {
            if (!IsTimeOutShowing || _isInLevelFailedState ||
                _timeOutTransitionCoroutine != null || _timeOutReviveCoroutine != null)
            {
                return;
            }

            int coins = PlayerPrefs.GetInt(PrefCoinsKey, DefaultStartingCoinsIfUnset) + _reviveBundleCoins;
            PlayerPrefs.SetInt(PrefCoinsKey, coins);
            PlayerPrefs.Save();

            _freezeCount++;
            _magnetCount++;
            _wandCount++;
            SaveBoosterCounts();
            UpdateBoosterUnlockStates();
            if (_timeOutCoinsCountText != null) _timeOutCoinsCountText.text = coins.ToString();

            _timeOutReviveCoroutine = StartCoroutine(CoReviveWithExtraTime(_reviveExtraSeconds, _buyRevivePackBtnRect));
        }

        public void OnBuyStarterBundleClicked()
        {
            if (!IsTimeOutShowing || _timeOutReviveCoroutine != null) return;

            if (_buyStarterPackBtnRect != null) StartCoroutine(CoButtonPunch(_buyStarterPackBtnRect));
            SoundManager.Instance?.PlayRevive();

            int coins = PlayerPrefs.GetInt(PrefCoinsKey, DefaultStartingCoinsIfUnset) + _starterBundleCoins;
            PlayerPrefs.SetInt(PrefCoinsKey, coins);
            PlayerPrefs.Save();

            _freezeCount++;
            _magnetCount++;
            _wandCount++;
            SaveBoosterCounts();
            UpdateBoosterUnlockStates();
            if (_timeOutCoinsCountText != null) _timeOutCoinsCountText.text = coins.ToString();
            if (_timeOutCoinsPillRect != null) StartCoroutine(CoPulseTransform(_timeOutCoinsPillRect, 1.14f, 0.28f));

            ShowFloatingText($"+{_starterBundleCoins} XU & +1 BOOSTER!", Vector3.zero, new Color(1.0f, 0.88f, 0.35f));
        }

        public void OnRetryLevelClicked()
        {
            if (!IsTimeOutShowing || _timeOutTransitionCoroutine != null || _timeOutReviveCoroutine != null) return;
            SoundManager.Instance?.PlayButton();
            if (_retryBtnRect != null) StartCoroutine(CoButtonPunch(_retryBtnRect));
            HideTimeOutPopup();
            RestartLevel();
        }

        private void OnTimeOutAddLivesClicked()
        {
            SoundManager.Instance?.PlayButton();
            if (_timeOutLivesPillRect != null) StartCoroutine(CoPulseTransform(_timeOutLivesPillRect, 1.08f, 0.18f));
            ShowFeedbackError("Tim tự hồi theo thời gian hoặc mua gói Revive!");
        }

        private void OnTimeOutAddCoinsClicked()
        {
            SoundManager.Instance?.PlayButton();
            if (_timeOutCoinsPillRect != null) StartCoroutine(CoPulseTransform(_timeOutCoinsPillRect, 1.08f, 0.18f));
            ShowFeedbackError("Chọn gói ưu đãi bên dưới để nhận thêm Xu!");
        }

        private RectTransform EnsureTimeOutEffectsLayer()
        {
            if (_timeOutEffectsLayer == null && _timeOutPanel != null)
            {
                _timeOutEffectsLayer = CreateUIContainer("TimeOutEffectsLayer", _timeOutPanel.transform,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).GetComponent<RectTransform>();
                _timeOutEffectsLayer.SetAsLastSibling();
            }
            return _timeOutEffectsLayer;
        }

        private IEnumerator CoAnimateTimeOutIntro()
        {
            _timeOutIntroComplete = false;
            _timeOutAllowContinue = false;
            EnsureTimeOutEffectsLayer();

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();

            if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 0f;
            if (_timeOutTopBarRect != null) _timeOutTopBarRect.anchoredPosition = new Vector2(0f, 60f);
            if (_timeOutMainCardRect != null) _timeOutMainCardRect.localScale = new Vector3(0.70f, 0.70f, 1f);
            if (_timeOutPlanetRingedRect != null) _timeOutPlanetRingedRect.localScale = Vector3.zero;
            if (_timeOutPlanetMiniRect != null) _timeOutPlanetMiniRect.localScale = Vector3.zero;
            if (_timeOutStopwatchRoot != null) _timeOutStopwatchRoot.localScale = Vector3.zero;
            if (_buyTimeBtnRect != null) _buyTimeBtnRect.localScale = Vector3.zero;
            if (_adTimeBtnRect != null) _adTimeBtnRect.localScale = Vector3.zero;
            if (_offerCardRect != null) _offerCardRect.anchoredPosition = new Vector2(0f, -680f);
            if (_tapToContinueCanvasGroup != null) _tapToContinueCanvasGroup.alpha = 0f;

            bool swBurstFired = false;
            const float totalDur = 0.78f;
            float elapsed = 0f;

            while (elapsed < totalDur && IsTimeOutShowing)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;

                if (_timeOutCanvasGroup != null)
                {
                    _timeOutCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.16f);
                }

                if (_timeOutTopBarRect != null)
                {
                    float bt = Mathf.Clamp01(elapsed / 0.28f);
                    float ease = 1f - (1f - bt) * (1f - bt);
                    _timeOutTopBarRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(60f, -145f, ease));
                }

                if (_timeOutMainCardRect != null)
                {
                    float ct = Mathf.Clamp01((elapsed - 0.04f) / 0.34f);
                    _timeOutMainCardRect.localScale = Vector3.one * EvaluateOvershootScale(ct, 0.70f, 1.07f, 1f, 0.62f);
                }

                if (elapsed >= 0.16f)
                {
                    float pt = Mathf.Clamp01((elapsed - 0.16f) / 0.30f);
                    float pScale = EvaluateOvershootScale(pt, 0f, 1.18f, 1f, 0.60f);
                    if (_timeOutPlanetRingedRect != null) _timeOutPlanetRingedRect.localScale = Vector3.one * pScale;
                    if (_timeOutPlanetMiniRect != null) _timeOutPlanetMiniRect.localScale = Vector3.one * pScale;
                }

                if (elapsed >= 0.18f)
                {
                    if (!swBurstFired)
                    {
                        swBurstFired = true;
                        if (_timeOutStopwatchRoot != null && glowSp != null)
                        {
                            StartCoroutine(CoSpawnTimeOutFlashBurst(_timeOutStopwatchRoot, Vector2.zero, glowSp,
                                new Color(0.84f, 0.45f, 1.0f, 0.90f), 0.35f, 1.95f, 0.26f));
                        }
                    }
                    float st = Mathf.Clamp01((elapsed - 0.18f) / 0.30f);
                    if (_timeOutStopwatchRoot != null)
                    {
                        _timeOutStopwatchRoot.localScale = Vector3.one * EvaluateOvershootScale(st, 0f, 1.18f, 1f, 0.60f);
                    }
                }

                if (elapsed >= 0.26f && _buyTimeBtnRect != null)
                {
                    float b1 = Mathf.Clamp01((elapsed - 0.26f) / 0.28f);
                    _buyTimeBtnRect.localScale = Vector3.one * EvaluateOvershootScale(b1, 0f, 1.10f, 1f, 0.62f);
                }
                if (elapsed >= 0.34f && _adTimeBtnRect != null)
                {
                    float b2 = Mathf.Clamp01((elapsed - 0.34f) / 0.28f);
                    _adTimeBtnRect.localScale = Vector3.one * EvaluateOvershootScale(b2, 0f, 1.12f, 1f, 0.62f);
                }

                if (elapsed >= 0.26f && _offerCardRect != null)
                {
                    float ot = Mathf.Clamp01((elapsed - 0.26f) / 0.38f);
                    float easeOut = 1f - Mathf.Pow(1f - ot, 3f);
                    _offerCardRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-680f, -322f, easeOut));
                }

                if (elapsed >= 0.44f && _tapToContinueCanvasGroup != null)
                {
                    _tapToContinueCanvasGroup.alpha = Mathf.Clamp01((elapsed - 0.44f) / 0.24f);
                    if (!_timeOutAllowContinue) _timeOutAllowContinue = true;
                }

                if (_timeOutStopwatchSunburstRect != null)
                {
                    _timeOutStopwatchSunburstRect.Rotate(0f, 0f, -22f * dt);
                }

                yield return null;
            }

            if (IsTimeOutShowing)
            {
                if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 1f;
                ApplyTimeOutVisualState(showFailed: false);
                _timeOutAllowContinue = true;
                _timeOutIntroComplete = true;
                _timeOutIdleTime = 0f;
            }
            _timeOutAnimCoroutine = null;
        }

        private IEnumerator CoTransitionToLevelFailed()
        {
            _timeOutAllowContinue = false;
            _timeOutIntroComplete = false;
            EnsureTimeOutEffectsLayer();

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();

            // 1. Deduct 1 life immediately with visual punch on top-left Lives pill
            int lives = PlayerPrefs.HasKey(PrefLivesKey) ? PlayerPrefs.GetInt(PrefLivesKey, DefaultLives) : DefaultLives;
            lives = Mathf.Max(0, lives - 1);
            PlayerPrefs.SetInt(PrefLivesKey, lives);
            PlayerPrefs.Save();
            if (_timeOutLivesCountText != null) _timeOutLivesCountText.text = lives.ToString();

            if (_timeOutLivesPillRect != null)
            {
                StartCoroutine(CoPulseTransform(_timeOutLivesPillRect, 1.18f, 0.28f));
                StartCoroutine(CoFloatingLoseBadge(_timeOutLivesPillRect, "-1",
                    new Vector2(-146f, -18f), new Vector2(-146f, -72f), new Color(1.0f, 0.32f, 0.56f), 32, 0.65f));
            }
            if (_timeOutLivesPillImg != null)
            {
                StartCoroutine(CoFlashImageColor(_timeOutLivesPillImg, new Color(1.0f, 0.45f, 0.68f, 1f), Color.white, 0.32f));
            }

            // 2. Morph MainCard & OfferBannerCard into Level Failed
            float morphElapsed = 0f;
            const float shrinkDur = 0.14f;
            while (morphElapsed < shrinkDur && IsTimeOutShowing)
            {
                morphElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(morphElapsed / shrinkDur);
                if (_timeOutMainCardRect != null) _timeOutMainCardRect.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.04f, 0.90f, 1f), t);
                if (_offerCardRect != null) _offerCardRect.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.06f, t), 1f);
                yield return null;
            }

            // Swap to Level Failed content while heart starts intact & joined at center
            ApplyTimeOutVisualState(showFailed: true);
            if (_failedHeartRoot != null) _failedHeartRoot.localScale = Vector3.zero;
            if (_failedHeartLeftRect != null)
            {
                _failedHeartLeftRect.anchoredPosition = Vector2.zero;
                _failedHeartLeftRect.localRotation = Quaternion.identity;
            }
            if (_failedHeartRightRect != null)
            {
                _failedHeartRightRect.anchoredPosition = Vector2.zero;
                _failedHeartRightRect.localRotation = Quaternion.identity;
            }
            if (_failedMinusOneText != null) _failedMinusOneText.rectTransform.localScale = Vector3.zero;
            if (_retryBtnRect != null) _retryBtnRect.localScale = Vector3.zero;
            if (_bestOfferRibbonRect != null) _bestOfferRibbonRect.localScale = Vector3.zero;
            if (_tapToContinueCanvasGroup != null) _tapToContinueCanvasGroup.alpha = 0f;

            // Expand MainCard & OfferCard back
            float expandElapsed = 0f;
            const float expandDur = 0.22f;
            while (expandElapsed < expandDur && IsTimeOutShowing)
            {
                expandElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(expandElapsed / expandDur);
                if (_timeOutMainCardRect != null)
                {
                    _timeOutMainCardRect.localScale = Vector3.one * EvaluateOvershootScale(t, 0.94f, 1.05f, 1f, 0.55f);
                }
                if (_offerCardRect != null)
                {
                    _offerCardRect.localScale = new Vector3(1f, EvaluateOvershootScale(t, 0.06f, 1.06f, 1f, 0.60f), 1f);
                }
                if (_bestOfferRibbonRect != null)
                {
                    _bestOfferRibbonRect.localScale = Vector3.one * EvaluateOvershootScale(t, 0f, 1.24f, 1f, 0.62f);
                }
                if (_failedHeartRoot != null)
                {
                    _failedHeartRoot.localScale = Vector3.one * EvaluateOvershootScale(t, 0f, 1.18f, 1f, 0.62f);
                }
                yield return null;
            }

            if (_timeOutMainCardRect != null) _timeOutMainCardRect.localScale = Vector3.one;
            if (_offerCardRect != null) _offerCardRect.localScale = Vector3.one;
            if (_bestOfferRibbonRect != null) _bestOfferRibbonRect.localScale = Vector3.one;
            if (_failedHeartRoot != null) _failedHeartRoot.localScale = Vector3.one;

            // 3. Rapid heart tremor before crack
            float shakeElapsed = 0f;
            const float shakeDur = 0.14f;
            while (shakeElapsed < shakeDur && IsTimeOutShowing)
            {
                shakeElapsed += Time.unscaledDeltaTime;
                float amp = 8f * (1f - shakeElapsed / shakeDur);
                if (_failedHeartRoot != null)
                {
                    _failedHeartRoot.anchoredPosition = new Vector2(Mathf.Sin(shakeElapsed * 90f) * amp, 116f + Mathf.Cos(shakeElapsed * 70f) * amp * 0.4f);
                }
                yield return null;
            }
            if (_failedHeartRoot != null) _failedHeartRoot.anchoredPosition = new Vector2(0f, 116f);

            // 4. Crack & split animation!
            SoundManager.Instance?.PlayHeartBreak();
            if (_failedHeartRoot != null)
            {
                if (glowSp != null)
                {
                    StartCoroutine(CoSpawnTimeOutFlashBurst(_failedHeartRoot, Vector2.zero, glowSp,
                        new Color(1.0f, 0.35f, 0.65f, 0.92f), 0.25f, 1.85f, 0.26f));
                }
                StartCoroutine(CoSpawnLoseShards(_failedHeartRoot, Vector2.zero, 14, LoseHeartShardPalette));
            }

            float crackElapsed = 0f;
            const float crackDur = 0.42f;
            while (crackElapsed < crackDur && IsTimeOutShowing)
            {
                crackElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(crackElapsed / crackDur);
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);

                if (_failedHeartLeftRect != null)
                {
                    _failedHeartLeftRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-12f, -3f), easeOut);
                    _failedHeartLeftRect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 9f, easeOut));
                }
                if (_failedHeartRightRect != null)
                {
                    _failedHeartRightRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(12f, -3f), easeOut);
                    _failedHeartRightRect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, -9f, easeOut));
                }

                if (_failedCrackGlowImg != null && _failedCrackGlowRect != null)
                {
                    float flashA = t < 0.25f ? (t / 0.25f) : (1f - (t - 0.25f) / 0.75f);
                    _failedCrackGlowImg.color = new Color(1.0f, 0.72f, 0.90f, Mathf.Clamp01(flashA) * 0.90f);
                    _failedCrackGlowRect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.45f, easeOut);
                }

                if (t >= 0.16f && _failedMinusOneText != null)
                {
                    float mt = Mathf.Clamp01((t - 0.16f) / 0.65f);
                    float mScale = mt < 0.4f
                        ? Mathf.Lerp(1.65f, 0.90f, mt / 0.4f)
                        : Mathf.Lerp(0.90f, 1.0f, (mt - 0.4f) / 0.6f);
                    _failedMinusOneText.rectTransform.localScale = Vector3.one * mScale;
                }

                if (t >= 0.26f && _retryBtnRect != null)
                {
                    float rt = Mathf.Clamp01((t - 0.26f) / 0.74f);
                    _retryBtnRect.localScale = Vector3.one * EvaluateOvershootScale(rt, 0f, 1.14f, 1f, 0.60f);
                }

                if (t >= 0.50f && _tapToContinueCanvasGroup != null)
                {
                    _tapToContinueCanvasGroup.alpha = Mathf.Clamp01((t - 0.50f) / 0.50f);
                }

                yield return null;
            }

            if (IsTimeOutShowing)
            {
                ApplyTimeOutVisualState(showFailed: true);
                _timeOutAllowContinue = true;
                _timeOutIntroComplete = true;
                _timeOutIdleTime = 0f;
            }
            _timeOutTransitionCoroutine = null;
        }

        private IEnumerator CoReviveWithExtraTime(float extraSec, RectTransform clickedBtnRect)
        {
            _timeOutAllowContinue = false;
            _timeOutIntroComplete = false;
            if (clickedBtnRect != null) StartCoroutine(CoButtonPunch(clickedBtnRect));
            SoundManager.Instance?.PlayRevive();

            RectTransform fxLayer = EnsureTimeOutEffectsLayer();
            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            if (fxLayer != null)
            {
                if (glowSp != null)
                {
                    StartCoroutine(CoSpawnTimeOutFlashBurst(fxLayer, new Vector2(0f, 190f), glowSp,
                        new Color(0.55f, 0.96f, 1.0f, 0.92f), 0.35f, 2.3f, 0.30f));
                }
                StartCoroutine(CoFloatingLoseBadge(fxLayer, "+" + Mathf.RoundToInt(extraSec) + "s",
                    new Vector2(0f, 220f), new Vector2(0f, 780f), new Color(0.99f, 0.96f, 0.62f), 64, 0.42f));
            }

            float elapsed = 0f;
            const float closeDur = 0.28f;
            while (elapsed < closeDur && IsTimeOutShowing)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / closeDur);
                if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 1f - t;
                if (_timeOutCenterStackRect != null) _timeOutCenterStackRect.localScale = Vector3.one * Mathf.Lerp(1f, 0.88f, t);
                yield return null;
            }

            HideTimeOutPopup();
            if (_timeOutCenterStackRect != null) _timeOutCenterStackRect.localScale = Vector3.one;
            if (_timeOutCanvasGroup != null) _timeOutCanvasGroup.alpha = 1f;

            _timeRemaining = Mathf.Max(0f, _timeRemaining) + extraSec;
            _timeOutLastTickSecond = -1;
            _hasTimerStartedByInput = true;
            _isTimerRunning = true;
            _isPaused = false;
            UpdateTimeDisplay();

            if (_timeBannerImg != null)
            {
                StartCoroutine(CoPulseTransform(_timeBannerImg.transform, 1.18f, 0.30f));
            }
            ShowFloatingText($"+{Mathf.RoundToInt(extraSec)} GIÂY!", Vector3.zero, new Color(0.35f, 0.95f, 1.0f));
            _timeOutReviveCoroutine = null;
        }

        private void UpdateTimeOutIdleAnimations(float t, float dt)
        {
            if (_timeOutPlanetRingedRect != null)
            {
                _timeOutPlanetRingedRect.anchoredPosition = new Vector2(386f, 260f + Mathf.Sin(t * 1.9f) * 5.5f);
                _timeOutPlanetRingedRect.localEulerAngles = new Vector3(0f, 0f, -10f + Mathf.Sin(t * 1.4f) * 3.0f);
            }
            if (_timeOutPlanetMiniRect != null)
            {
                _timeOutPlanetMiniRect.anchoredPosition = new Vector2(-368f, -284f + Mathf.Cos(t * 2.1f) * 4.5f);
            }
            if (_timeOutCardSparklesRoot != null)
            {
                for (int i = 0; i < _timeOutCardSparklesRoot.childCount; i++)
                {
                    Transform ch = _timeOutCardSparklesRoot.GetChild(i);
                    if (ch == null) continue;
                    float s = 0.72f + 0.34f * Mathf.Sin(t * 3.8f + i * 1.15f);
                    ch.localScale = new Vector3(s, s, 1f);
                }
            }
            if (_tapToContinueCanvasGroup != null)
            {
                _tapToContinueCanvasGroup.alpha = 0.55f + 0.45f * (0.5f + 0.5f * Mathf.Sin(t * 3.2f));
            }

            if (!_isInLevelFailedState)
            {
                if (_timeOutStopwatchSunburstRect != null)
                {
                    _timeOutStopwatchSunburstRect.Rotate(0f, 0f, -22f * dt);
                }
                if (_timeOutStopwatchAuraRect != null)
                {
                    float pulse = 1f + Mathf.Sin(t * 2.6f) * 0.06f;
                    _timeOutStopwatchAuraRect.localScale = new Vector3(pulse, pulse, 1f);
                }
                if (_timeOutStopwatchIconRect != null)
                {
                    float cycle = t % 2.1f;
                    float rotZ = cycle < 0.42f ? Mathf.Sin(cycle * 36f) * 8.5f * (1f - cycle / 0.42f) : 0f;
                    _timeOutStopwatchIconRect.localEulerAngles = new Vector3(0f, 0f, rotZ);
                }
                if (_adTimeBtnRect != null)
                {
                    float p = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 2.4f)), 6f);
                    _adTimeBtnRect.localScale = Vector3.one * (1f + 0.040f * p);
                }
                if (_adTimeShineRect != null)
                {
                    float cycle = t % 2.5f;
                    float sx = cycle < 0.62f ? Mathf.Lerp(-260f, 260f, cycle / 0.62f) : -260f;
                    _adTimeShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
                if (_buyRevivePackShineRect != null)
                {
                    float cycle = (t + 0.8f) % 2.8f;
                    float sx = cycle < 0.62f ? Mathf.Lerp(-260f, 260f, cycle / 0.62f) : -260f;
                    _buyRevivePackShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
            }
            else
            {
                float bob = Mathf.Sin(t * 2.0f) * 3.5f;
                if (_failedHeartLeftRect != null) _failedHeartLeftRect.anchoredPosition = new Vector2(-12f, -3f + bob);
                if (_failedHeartRightRect != null) _failedHeartRightRect.anchoredPosition = new Vector2(12f, -3f - bob * 0.6f);
                if (_failedAuraRect != null)
                {
                    float pulse = 1f + Mathf.Sin(t * 2.2f) * 0.05f;
                    _failedAuraRect.localScale = new Vector3(pulse, pulse, 1f);
                }
                if (_retryBtnRect != null)
                {
                    float p = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * 2.4f)), 6f);
                    _retryBtnRect.localScale = Vector3.one * (1f + 0.038f * p);
                }
                if (_retryShineRect != null)
                {
                    float cycle = t % 2.5f;
                    float sx = cycle < 0.62f ? Mathf.Lerp(-260f, 260f, cycle / 0.62f) : -260f;
                    _retryShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
                if (_buyStarterPackShineRect != null)
                {
                    float cycle = (t + 0.8f) % 2.8f;
                    float sx = cycle < 0.62f ? Mathf.Lerp(-260f, 260f, cycle / 0.62f) : -260f;
                    _buyStarterPackShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
            }
        }

        private IEnumerator CoButtonShake(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 orig = rt.anchoredPosition;
            float elapsed = 0f;
            const float dur = 0.28f;
            while (elapsed < dur && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / dur;
                float offsetX = Mathf.Sin(t * Mathf.PI * 6f) * 14f * (1f - t);
                rt.anchoredPosition = orig + new Vector2(offsetX, 0f);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = orig;
        }

        private IEnumerator CoPulseTransform(Transform tr, float peakScale, float dur)
        {
            if (tr == null) yield break;
            float elapsed = 0f;
            while (elapsed < dur && tr != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = Mathf.Lerp(1f, peakScale, Mathf.Sin(t * Mathf.PI));
                tr.localScale = Vector3.one * s;
                yield return null;
            }
            if (tr != null) tr.localScale = Vector3.one;
        }

        private IEnumerator CoFlashImageColor(Image img, Color flashCol, Color restCol, float dur)
        {
            if (img == null) yield break;
            float elapsed = 0f;
            while (elapsed < dur && img != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                img.color = Color.Lerp(flashCol, restCol, t);
                yield return null;
            }
            if (img != null) img.color = restCol;
        }

        private IEnumerator CoSpawnTimeOutFlashBurst(RectTransform parent, Vector2 pos, Sprite sprite, Color color,
            float startScale, float endScale, float duration)
        {
            if (parent == null || sprite == null) yield break;
            RectTransform rt = SpawnFloatingEffectImage("TimeOutFlashBurst", parent, sprite, pos, new Vector2(220f, 220f), color, false);
            Image img = rt.GetComponent<Image>();

            float elapsed = 0f;
            while (elapsed < duration && rt != null && IsTimeOutShowing)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - (1f - t) * (1f - t);
                rt.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, ease);
                if (img != null)
                {
                    img.color = new Color(color.r, color.g, color.b, color.a * (1f - t * t));
                }
                yield return null;
            }

            if (rt != null) Destroy(rt.gameObject);
        }

        private IEnumerator CoSpawnLoseShards(RectTransform anchor, Vector2 origin, int count, Color[] palette)
        {
            if (anchor == null) yield break;
            Sprite shardSp = GemSpriteManager.GetWinHelperSprite("win_shard") ?? SpriteGenerator.GetShardSprite();
            List<ConfettiParticle> shards = new List<ConfettiParticle>(count);

            for (int i = 0; i < count; i++)
            {
                float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float spd = UnityEngine.Random.Range(180f, 480f);
                Vector2 vel = new Vector2(Mathf.Cos(ang) * spd, Mathf.Sin(ang) * spd + 110f);
                float sz = UnityEngine.Random.Range(12f, 22f);
                Color col = palette[i % palette.Length];

                RectTransform rt = SpawnFloatingEffectImage("HeartShard_" + i, anchor, shardSp, origin, new Vector2(sz, sz), col, false);
                rt.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, 360f));

                shards.Add(new ConfettiParticle
                {
                    Rect = rt,
                    Img = rt.GetComponent<Image>(),
                    Vel = vel,
                    Spin = UnityEngine.Random.Range(-480f, 480f),
                    Lifetime = UnityEngine.Random.Range(0.45f, 0.72f),
                    Age = 0f,
                    BaseColor = col
                });
            }

            while (shards.Count > 0 && anchor != null && IsTimeOutShowing)
            {
                float dt = Time.unscaledDeltaTime;
                for (int i = shards.Count - 1; i >= 0; i--)
                {
                    ConfettiParticle p = shards[i];
                    p.Age += dt;
                    if (p.Age >= p.Lifetime || p.Rect == null)
                    {
                        if (p.Rect != null) Destroy(p.Rect.gameObject);
                        shards.RemoveAt(i);
                        continue;
                    }
                    p.Vel.y -= 720f * dt;
                    p.Rect.anchoredPosition += p.Vel * dt;
                    p.Rect.Rotate(0f, 0f, p.Spin * dt);
                    float t = p.Age / p.Lifetime;
                    p.Rect.localScale = Vector3.one * (1f - t * 0.5f);
                    if (p.Img != null)
                    {
                        p.Img.color = new Color(p.BaseColor.r, p.BaseColor.g, p.BaseColor.b, 1f - t * t);
                    }
                    shards[i] = p;
                }
                yield return null;
            }

            for (int i = 0; i < shards.Count; i++)
            {
                if (shards[i].Rect != null) Destroy(shards[i].Rect.gameObject);
            }
        }

        private IEnumerator CoFloatingLoseBadge(RectTransform anchor, string text, Vector2 startPos, Vector2 endPos,
            Color color, int fontSize, float duration)
        {
            if (anchor == null) yield break;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Text badge = CreateOutlinedText("FloatingBadge", anchor, text, font, fontSize, color, startPos,
                new Vector2(220f, 84f), new Color(0.12f, 0.02f, 0.22f, 0.98f), new Vector2(2.5f, -2.5f), TextAnchor.MiddleCenter);
            RectTransform rt = badge.rectTransform;

            float elapsed = 0f;
            while (elapsed < duration && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - (1f - t) * (1f - t);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
                rt.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.15f, Mathf.Sin(t * Mathf.PI));
                badge.color = new Color(color.r, color.g, color.b, t > 0.65f ? Mathf.Clamp01((1f - t) / 0.35f) : 1f);
                yield return null;
            }

            if (rt != null) Destroy(rt.gameObject);
        }
    }
}
