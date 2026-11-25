using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.UI
{
    public enum HomeLevelDifficulty
    {
        Easy,
        Hard,
        SuperHard
    }

    public class HomeUI : MonoBehaviour
    {
        public static HomeUI Instance { get; private set; }

        [Header("Home Scene Settings")]
        [Tooltip("0 = tự đọc từ BMP_CurrentLevel; nhập > 0 để xem thử độ khó từng level (VD: 1=Easy, 3=Hard, 5=SuperHard).")]
        [SerializeField] private int _previewLevelOverride = 0;

        [Header("Cỡ Số Các Level (Chỉnh trực tiếp tại đây)")]
        [Tooltip("Font tùy chỉnh cho số level (để trống = tự dùng Assets/Resources/Fonts/HomeLevelFont.ttf).")]
        [SerializeField] private Font _levelNumberFontOverride;

        [Tooltip("Kiểu chữ khi dùng font tùy chỉnh / HomeLevelFont (mặc định Normal vì HomeLevelFont đã là nét Black dày sẵn).")]
        [SerializeField] private FontStyle _levelNumberFontStyle = FontStyle.Normal;

        [Tooltip("Tỉ lệ phóng to / thu nhỏ đồng loạt TẤT CẢ các cỡ số level.")]
        [Range(0.5f, 2.0f)]
        [SerializeField] private float _allLevelNumbersScale = 1.0f;

        [Tooltip("Cỡ số ô đang chọn (Node_0 - ô lớn nằm ngay trên nút PLAY).")]
        [Range(24, 140)]
        [SerializeField] private int _activeNodeFontSize = 72;

        [Tooltip("Cỡ số ô level kế tiếp thứ 1 (Node_1).")]
        [Range(24, 140)]
        [SerializeField] private int _node1FontSize = 58;

        [Tooltip("Cỡ số ô level kế tiếp thứ 2 (Node_2).")]
        [Range(24, 140)]
        [SerializeField] private int _node2FontSize = 58;

        [Tooltip("Cỡ số ô level mờ trên cùng (Node_3).")]
        [Range(24, 140)]
        [SerializeField] private int _node3FontSize = 58;

        [Tooltip("Cỡ số các ô level trong bảng chọn Levels (Tab Collection / Cards).")]
        [Range(24, 140)]
        [SerializeField] private int _collectionLevelFontSize = 54;

        [Tooltip("Thu nhỏ nhẹ khi số level có từ 2 chữ số trở lên (VD: 0.90 để số 33, 100 gọn hơn; để 1.0 nếu giữ nguyên cỡ).")]
        [Range(0.6f, 1.0f)]
        [SerializeField] private float _multiDigitScale = 1.0f;

        [Tooltip("Dịch vị trí dọc (Y) của chữ số trên các ô thang level (Node_0..Node_3).")]
        [Range(-25f, 35f)]
        [SerializeField] private float _ladderNumberOffsetY = 8f;

        [Tooltip("Dịch vị trí dọc (Y) của chữ số trên các ô trong bảng chọn Levels.")]
        [Range(-25f, 35f)]
        [SerializeField] private float _collectionNumberOffsetY = 6f;

        [Header("Remove Ads Popup Settings")]
        [SerializeField] private string _removeAdsPriceText = "₫264,000";
        [SerializeField] private int _removeAdsBonusCoins = 600;
        [SerializeField] private int _removeAdsBonusBoosterCount = 3;

        private const string PrefCurrentLevelKey = "BMP_CurrentLevel";
        private const string PrefCoinsKey = "BMP_TotalCoins";
        private const string PrefLivesKey = "BMP_Lives";
        private const string PrefFbClaimedKey = "BMP_FbRewardClaimed";
        private const string PrefNoAdsKey = "BMP_NoAdsPurchased";
        private const string PrefSoundKey = "BMP_SoundEnabled";
        private const string PrefMusicKey = "BMP_MusicEnabled";
        private const string PrefVibrationKey = "BMP_VibrationEnabled";
        private const string PrefJustWonLevelKey = "BMP_JustWonLevel";
        private const string PrefBoosterFreezeKey = "BMP_BoosterFreeze";
        private const string PrefBoosterMagnetKey = "BMP_BoosterMagnet";
        private const string PrefBoosterWandKey = "BMP_BoosterWand";

        private const int DefaultLives = 5;
        private const int MaxLives = 5;
        private const int DefaultCoins = 356;
        private const int MaxPlayableLevel = 10;

        private Canvas _canvas;
        private CanvasScaler _scaler;

        private RectTransform _ambientStarsRoot;
        private Text _livesCountText;
        private Text _livesStatusText;
        private Text _coinsCountText;
        private Button _livesBtn;
        private Button _coinsBtn;
        private Button _settingsBtn;

        private GameObject _homeContentRoot;
        private Button _fbBtn;
        private GameObject _fbBadgeObj;
        private Button _removeAdsBtn;

        private RectTransform[] _nodeRects = new RectTransform[4];
        private Image[] _nodeBgImgs = new Image[4];
        private Text[] _nodeNumberTexts = new Text[4];
        private Shadow[] _nodeNumberShadows = new Shadow[4];
        private CanvasGroup[] _nodeGroups = new CanvasGroup[4];
        private Image _activeNodeGlowImg;
        private RectTransform _activeNodeGlowRect;

        private RectTransform _playContainerRect;
        private Image _playAuraImg;
        private RectTransform _playAuraRect;
        private Button _playBtn;
        private Image _playBtnImg;
        private RectTransform _playBtnRect;
        private RectTransform _playShineRect;

        private GameObject _shopPanel;
        private GameObject _collectionPanel;
        private RectTransform[] _collectionTileRects;
        private Image[] _collectionTileImgs;
        private Text[] _collectionNumberTexts;

        private RectTransform[] _tabRects = new RectTransform[3];
        private Image[] _tabBgImgs = new Image[3];
        private RectTransform[] _tabIconRects = new RectTransform[3];
        private Image[] _tabIconImgs = new Image[3];
        private Text[] _tabLabelTexts = new Text[3];
        private Image[] _tabGlowImgs = new Image[3];
        private int _activeTabIndex = 1; // 0 = Shop, 1 = Home, 2 = Collection

        private GameObject _settingsPanel;
        private Image _soundIconImg;
        private Image _soundToggleImg;
        private Image _musicIconImg;
        private Image _musicToggleImg;
        private Image _vibrationIconImg;
        private Image _vibrationToggleImg;
        private bool _soundEnabled = true;
        private bool _musicEnabled = true;
        private bool _vibrationEnabled = true;

        private GameObject _removeAdsPanel;
        private CanvasGroup _removeAdsCanvasGroup;
        private RectTransform _removeAdsDialogRect;
        private RectTransform _removeAdsBuyBtnRect;
        private RectTransform _removeAdsBuyShineRect;
        private Text _removeAdsPriceLabel;
        private CanvasGroup _removeAdsTapContinueGroup;
        private RectTransform _removeAdsHeroFreezeRect;
        private RectTransform _removeAdsHeroWandRect;
        private RectTransform _removeAdsHeroMagnetRect;
        private RectTransform _removeAdsBottomGlowRect;
        private Coroutine _removeAdsAnimCoroutine;

        private GameObject _feedbackBanner;
        private CanvasGroup _feedbackCanvasGroup;
        private Text _feedbackText;
        private Coroutine _feedbackCoroutine;

        private float _idleTime = 0f;
        private float _lifeRegenTimer = 28f * 60f + 15f;
        private bool _isLoadingLevel = false;
#if UNITY_EDITOR
        private bool _editorRefreshPending = false;
#endif

        private static readonly Color EasyGlowColor = new Color(0.32f, 0.96f, 0.24f, 0.78f);
        private static readonly Color HardGlowColor = new Color(1.00f, 0.44f, 0.14f, 0.86f);
        private static readonly Color SuperHardGlowColor = new Color(0.76f, 0.30f, 1.00f, 0.88f);

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
                BuildHomeUIHierarchy();
            }
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (!PlayerPrefs.HasKey(PrefLivesKey)) PlayerPrefs.SetInt(PrefLivesKey, DefaultLives);
            if (!PlayerPrefs.HasKey(PrefCoinsKey)) PlayerPrefs.SetInt(PrefCoinsKey, DefaultCoins);
            if (!PlayerPrefs.HasKey(PrefCurrentLevelKey)) PlayerPrefs.SetInt(PrefCurrentLevelKey, 1);
            PlayerPrefs.Save();

            RefreshHomeDisplay();
            SetActiveTab(1, playSound: false);

            if (PlayerPrefs.GetInt(PrefJustWonLevelKey, 0) == 1)
            {
                PlayerPrefs.SetInt(PrefJustWonLevelKey, 0);
                PlayerPrefs.Save();
                StartCoroutine(CoCelebrateReturnedFromWin());
            }
        }

        private IEnumerator CoCelebrateReturnedFromWin()
        {
            yield return new WaitForSecondsRealtime(0.08f);

            if (_coinsBtn != null)
            {
                StartCoroutine(CoPunch(_coinsBtn.transform));
            }

            RectTransform activeNode = _nodeRects != null && _nodeRects.Length > 0 ? _nodeRects[0] : null;
            if (activeNode == null) yield break;

            float elapsed = 0f;
            const float dur = 0.36f;
            while (elapsed < dur && activeNode != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s;
                if (t < 0.55f)
                {
                    float k = Mathf.Sin((t / 0.55f) * Mathf.PI * 0.5f);
                    s = Mathf.Lerp(0.85f, 1.14f, k);
                }
                else
                {
                    float k2 = (t - 0.55f) / 0.45f;
                    s = Mathf.Lerp(1.14f, 1.0f, k2);
                }
                activeNode.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            if (activeNode != null) activeNode.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnValidate()
        {
            _previewLevelOverride = Mathf.Max(0, _previewLevelOverride);
            _allLevelNumbersScale = Mathf.Clamp(_allLevelNumbersScale, 0.5f, 2.0f);
            _activeNodeFontSize = Mathf.Clamp(_activeNodeFontSize, 16, 180);
            _node1FontSize = Mathf.Clamp(_node1FontSize, 16, 180);
            _node2FontSize = Mathf.Clamp(_node2FontSize, 16, 180);
            _node3FontSize = Mathf.Clamp(_node3FontSize, 16, 180);
            _collectionLevelFontSize = Mathf.Clamp(_collectionLevelFontSize, 16, 180);
            _multiDigitScale = Mathf.Clamp(_multiDigitScale, 0.5f, 1.0f);

            if (transform.childCount == 0) return;

            if (Application.isPlaying)
            {
                if (_nodeBgImgs[0] == null) BindExistingUI();
                RefreshHomeDisplay();
                return;
            }

#if UNITY_EDITOR
            if (!_editorRefreshPending)
            {
                _editorRefreshPending = true;
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    _editorRefreshPending = false;
                    if (this == null || transform == null || transform.childCount == 0) return;
                    if (_nodeBgImgs[0] == null) BindExistingUI();
                    RefreshHomeDisplay();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                };
            }
#endif
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            float dt = Time.unscaledDeltaTime;
            _idleTime += dt;

            int lives = PlayerPrefs.GetInt(PrefLivesKey, DefaultLives);
            if (lives < MaxLives)
            {
                _lifeRegenTimer -= dt;
                if (_lifeRegenTimer <= 0f)
                {
                    _lifeRegenTimer = 30f * 60f;
                    lives = Mathf.Min(MaxLives, lives + 1);
                    PlayerPrefs.SetInt(PrefLivesKey, lives);
                    PlayerPrefs.Save();
                    if (_livesCountText != null) _livesCountText.text = lives.ToString();
                }
                if (_livesStatusText != null) _livesStatusText.text = FormatTimeMMSS(_lifeRegenTimer);
            }

            if (_ambientStarsRoot != null)
            {
                for (int i = 0; i < _ambientStarsRoot.childCount; i++)
                {
                    Transform st = _ambientStarsRoot.GetChild(i);
                    if (st == null) continue;
                    float s = 0.65f + 0.40f * Mathf.Sin(_idleTime * 2.8f + i * 1.15f);
                    st.localScale = new Vector3(s, s, 1f);
                }
            }

            if (_homeContentRoot != null && _homeContentRoot.activeSelf)
            {
                if (_activeNodeGlowRect != null)
                {
                    float gPulse = 1.0f + Mathf.Sin(_idleTime * 2.6f) * 0.06f;
                    _activeNodeGlowRect.localScale = new Vector3(gPulse, gPulse, 1f);
                }
                if (_playContainerRect != null)
                {
                    float pPulse = 1.0f + Mathf.Pow(Mathf.Max(0f, Mathf.Sin(_idleTime * 2.2f)), 4f) * 0.032f;
                    _playContainerRect.localScale = new Vector3(pPulse, pPulse, 1f);
                }
                if (_playAuraRect != null)
                {
                    float aPulse = 1.0f + Mathf.Sin(_idleTime * 2.2f) * 0.05f;
                    _playAuraRect.localScale = new Vector3(aPulse, aPulse, 1f);
                }
                if (_playShineRect != null)
                {
                    float cycle = _idleTime % 2.8f;
                    float sx = cycle < 0.65f ? Mathf.Lerp(-360f, 360f, cycle / 0.65f) : -360f;
                    _playShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
            }

            if (_tabGlowImgs != null && _activeTabIndex >= 0 && _activeTabIndex < _tabGlowImgs.Length && _tabGlowImgs[_activeTabIndex] != null)
            {
                float s = 1.0f + Mathf.Sin(_idleTime * 2.5f) * 0.04f;
                _tabGlowImgs[_activeTabIndex].rectTransform.localScale = new Vector3(s, s, 1f);
            }

            if (_removeAdsPanel != null && _removeAdsPanel.activeSelf)
            {
                if (_removeAdsHeroFreezeRect != null)
                {
                    _removeAdsHeroFreezeRect.anchoredPosition = new Vector2(-186f, 88f + Mathf.Sin(_idleTime * 2.2f) * 3.5f);
                }
                if (_removeAdsHeroWandRect != null)
                {
                    _removeAdsHeroWandRect.anchoredPosition = new Vector2(204f, 242f + Mathf.Sin(_idleTime * 2.2f + 1.4f) * 3.5f);
                }
                if (_removeAdsHeroMagnetRect != null)
                {
                    _removeAdsHeroMagnetRect.anchoredPosition = new Vector2(184f, 88f + Mathf.Sin(_idleTime * 2.2f + 2.8f) * 3.5f);
                }
                if (_removeAdsBottomGlowRect != null)
                {
                    float gs = 1.0f + Mathf.Sin(_idleTime * 2.6f) * 0.06f;
                    _removeAdsBottomGlowRect.localScale = new Vector3(gs, gs, 1f);
                }
                if (_removeAdsBuyShineRect != null)
                {
                    float cycle = _idleTime % 2.6f;
                    float sx = cycle < 0.65f ? Mathf.Lerp(-340f, 340f, cycle / 0.65f) : -340f;
                    _removeAdsBuyShineRect.anchoredPosition = new Vector2(sx, 0f);
                }
                if (_removeAdsTapContinueGroup != null)
                {
                    _removeAdsTapContinueGroup.alpha = 0.62f + 0.38f * (0.5f + 0.5f * Mathf.Sin(_idleTime * 3.0f));
                }
            }
        }

        public static HomeLevelDifficulty GetLevelDifficulty(int levelNumber)
        {
            if (levelNumber <= 0) return HomeLevelDifficulty.Easy;
            if (CustomLevelRepository.TryGetLevel(levelNumber, out CustomLevelData customData) &&
                !string.IsNullOrEmpty(customData.difficulty))
            {
                if (Enum.TryParse(customData.difficulty, true, out HomeLevelDifficulty customDiff))
                {
                    return customDiff;
                }
            }
            if (levelNumber <= 10)
            {
                return (levelNumber == 6 || levelNumber == 10)
                    ? HomeLevelDifficulty.Hard
                    : HomeLevelDifficulty.Easy;
            }

            int mod = levelNumber % 5;
            if (mod == 0) return HomeLevelDifficulty.SuperHard;
            if (mod == 3) return HomeLevelDifficulty.Hard;
            return HomeLevelDifficulty.Easy;
        }

        public void SetPreviewLevel(int levelNumber)
        {
            _previewLevelOverride = Mathf.Max(0, levelNumber);
            if (transform.childCount == 0)
            {
                BuildHomeUIHierarchy();
            }
            else if (_nodeBgImgs[0] == null)
            {
                BindExistingUI();
            }
            RefreshHomeDisplay();
        }

        public int GetActiveLevelNumber()
        {
            if (_previewLevelOverride > 0) return _previewLevelOverride;
            return Mathf.Max(1, PlayerPrefs.GetInt(PrefCurrentLevelKey, 1));
        }

        [ContextMenu("Áp Dụng Cỡ Số Level Ngay")]
        public void ApplyLevelNumberSizesNow()
        {
            if (transform.childCount == 0) return;
            if (_nodeBgImgs[0] == null) BindExistingUI();
            RefreshHomeDisplay();
        }

        [ContextMenu("Khôi Phục Cỡ Số Level Mặc Định")]
        public void ResetLevelNumberSizesToDefault()
        {
            _allLevelNumbersScale = 1.0f;
            _activeNodeFontSize = 72;
            _node1FontSize = 58;
            _node2FontSize = 58;
            _node3FontSize = 58;
            _collectionLevelFontSize = 54;
            _multiDigitScale = 1.0f;
            _ladderNumberOffsetY = 8f;
            _collectionNumberOffsetY = 6f;
            _levelNumberFontStyle = FontStyle.Normal;
            if (transform.childCount > 0)
            {
                if (_nodeBgImgs[0] == null) BindExistingUI();
                RefreshHomeDisplay();
            }
        }

        [ContextMenu("Rebuild Home UI")]
        public void RebuildHomeUI()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }

            BuildHomeUIHierarchy();
            RefreshHomeDisplay();
            SetActiveTab(1, playSound: false);
        }

        private void BindExistingUI()
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
            _scaler.referenceResolution = new Vector2(1080f, 1920f);
            _scaler.matchWidthOrHeight = 0.5f;

            Transform bgLayer = transform.Find("BackgroundLayer");
            Transform topBar = transform.Find("TopBar");
            Transform homeContent = transform.Find("HomeContentRoot");
            Transform navBar = transform.Find("BottomNavBar");
            Transform settingsTr = transform.Find("SettingsPanel");
            Transform feedbackTr = transform.Find("FeedbackBanner");

            if (bgLayer == null || topBar == null || homeContent == null || navBar == null ||
                homeContent.Find("LadderGroup/Node_0") == null ||
                homeContent.Find("LadderGroup/PlayButtonContainer/Btn_Play") == null)
            {
                RebuildHomeUI();
                return;
            }

            _ambientStarsRoot = bgLayer.Find("AmbientStars") as RectTransform;

            // TopBar
            Transform livesTr = topBar.Find("Btn_Lives");
            if (livesTr != null)
            {
                _livesBtn = livesTr.GetComponent<Button>();
                _livesCountText = livesTr.Find("LivesCountText")?.GetComponent<Text>();
                _livesStatusText = livesTr.Find("LivesStatusText")?.GetComponent<Text>();
                RebindButton(_livesBtn, OnLivesPillClicked);
            }

            Transform coinsTr = topBar.Find("Btn_Coins");
            if (coinsTr != null)
            {
                _coinsBtn = coinsTr.GetComponent<Button>();
                _coinsCountText = coinsTr.Find("CoinsCountText")?.GetComponent<Text>();
                RebindButton(_coinsBtn, OnCoinsPillClicked);
            }

            Transform gearTr = topBar.Find("Btn_Settings");
            if (gearTr != null)
            {
                _settingsBtn = gearTr.GetComponent<Button>();
                RebindButton(_settingsBtn, OnSettingsClicked);
            }

            // HomeContentRoot
            _homeContentRoot = homeContent.gameObject;
            Transform fbTr = homeContent.Find("Btn_Facebook");
            if (fbTr != null)
            {
                _fbBtn = fbTr.GetComponent<Button>();
                _fbBadgeObj = fbTr.Find("RewardPill")?.gameObject;
                RebindButton(_fbBtn, OnFacebookClicked);
            }

            Transform noAdsTr = homeContent.Find("Btn_RemoveAds");
            if (noAdsTr != null)
            {
                _removeAdsBtn = noAdsTr.GetComponent<Button>();
                RebindButton(_removeAdsBtn, OnRemoveAdsClicked);
            }

            Transform ladderTr = homeContent.Find("LadderGroup");
            if (ladderTr != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Transform nodeTr = ladderTr.Find("Node_" + i);
                    if (nodeTr == null) continue;
                    _nodeRects[i] = nodeTr as RectTransform;
                    _nodeBgImgs[i] = nodeTr.GetComponent<Image>();
                    _nodeGroups[i] = nodeTr.GetComponent<CanvasGroup>();
                    _nodeNumberTexts[i] = nodeTr.Find("LevelNumText")?.GetComponent<Text>();
                    _nodeNumberShadows[i] = _nodeNumberTexts[i]?.GetComponent<Shadow>();
                    if (i == 0)
                    {
                        Transform glowTr = nodeTr.Find("NodeGlow");
                        _activeNodeGlowRect = glowTr as RectTransform;
                        _activeNodeGlowImg = glowTr?.GetComponent<Image>();
                    }
                    int idx = i;
                    Button nBtn = nodeTr.GetComponent<Button>();
                    RebindButton(nBtn, () => OnLadderNodeClicked(idx));
                }

                Transform playContTr = ladderTr.Find("PlayButtonContainer");
                if (playContTr != null)
                {
                    _playContainerRect = playContTr as RectTransform;
                    Transform auraTr = playContTr.Find("PlayAura");
                    _playAuraRect = auraTr as RectTransform;
                    _playAuraImg = auraTr?.GetComponent<Image>();
                    Transform playBtnTr = playContTr.Find("Btn_Play");
                    if (playBtnTr != null)
                    {
                        _playBtnRect = playBtnTr as RectTransform;
                        _playBtnImg = playBtnTr.GetComponent<Image>();
                        _playBtn = playBtnTr.GetComponent<Button>();
                        _playShineRect = playBtnTr.Find("ShineClip/ShineSweep") as RectTransform;
                        RebindButton(_playBtn, OnPlayClicked);
                    }
                }
            }

            // ShopPanel & CollectionPanel
            _shopPanel = transform.Find("ShopPanel")?.gameObject;
            if (_shopPanel != null) BindShopPanel(_shopPanel.transform);

            _collectionPanel = transform.Find("CollectionPanel")?.gameObject;
            if (_collectionPanel != null) BindCollectionPanel(_collectionPanel.transform);

            // BottomNavBar
            string[] tabNames = { "Tab_Shop", "Tab_Home", "Tab_Collection" };
            for (int i = 0; i < 3; i++)
            {
                Transform tTr = navBar.Find(tabNames[i]);
                if (tTr == null) continue;
                _tabRects[i] = tTr as RectTransform;
                _tabBgImgs[i] = tTr.GetComponent<Image>();
                _tabGlowImgs[i] = tTr.Find("TabGlow")?.GetComponent<Image>();
                Transform iconTr = tTr.Find("TabIcon");
                _tabIconRects[i] = iconTr as RectTransform;
                _tabIconImgs[i] = iconTr?.GetComponent<Image>();
                _tabLabelTexts[i] = tTr.Find("TabLabel")?.GetComponent<Text>();
                int tabIdx = i;
                RebindButton(tTr.GetComponent<Button>(), () => SetActiveTab(tabIdx, playSound: true));
            }

            if (settingsTr != null)
            {
                _settingsPanel = settingsTr.gameObject;
                BindSettingsPanel(_settingsPanel.transform);
            }

            Transform removeAdsTr = transform.Find("RemoveAdsPanel");
            if (removeAdsTr != null)
            {
                _removeAdsPanel = removeAdsTr.gameObject;
                BindRemoveAdsPanel(removeAdsTr);
            }
            else
            {
                Font fallbackFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (fallbackFont == null) fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                BuildRemoveAdsPanel(fallbackFont);
                if (_removeAdsPanel != null && feedbackTr != null)
                {
                    _removeAdsPanel.transform.SetSiblingIndex(Mathf.Max(0, feedbackTr.GetSiblingIndex()));
                }
            }

            if (feedbackTr != null)
            {
                _feedbackBanner = feedbackTr.gameObject;
                _feedbackCanvasGroup = _feedbackBanner.GetComponent<CanvasGroup>();
                _feedbackText = _feedbackBanner.GetComponentInChildren<Text>(true);
                _feedbackBanner.transform.SetAsLastSibling();
            }

            EnsureHomeSprites();
            RefreshHomeDisplay();
        }

        private void EnsureHomeSprites()
        {
            Sprite bgSp = GemSpriteManager.GetBackgroundSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();

            Sprite heartSp = GemSpriteManager.GetHomeSceneSprite("heart");
            Sprite coinsSp = GemSpriteManager.GetHomeSceneSprite("coins");
            Sprite uiBg1Sp = GemSpriteManager.GetHomeSceneSprite("UI_BG1");
            Sprite settingIconSp = GemSpriteManager.GetHomeSceneSprite("UI_setting");
            Sprite noAdsSp = GemSpriteManager.GetHomeSceneSprite("No ads");
            Sprite fbIconSp = GemSpriteManager.GetHomeHelperSprite("home_fb_icon") ?? SpriteGenerator.GetHomeFacebookIconSprite();
            Sprite badgePillSp = GemSpriteManager.GetHomeHelperSprite("home_badge_pill") ?? SpriteGenerator.GetHomeBadgePillSprite();
            Sprite beamSp = GemSpriteManager.GetHomeHelperSprite("home_ladder_beam") ?? SpriteGenerator.GetHomeLadderBeamSprite();
            Sprite coinSp = GemSpriteManager.GetWinSceneSprite("Coin");

            SetImageSprite(transform.Find("BackgroundLayer/BgImage"), bgSp);
            if (_ambientStarsRoot != null)
            {
                for (int i = 0; i < _ambientStarsRoot.childCount; i++)
                {
                    SetImageSprite(_ambientStarsRoot.GetChild(i), flareSp);
                }
            }

            SetImageSprite(transform.Find("TopBar/Btn_Lives"), heartSp);
            SetImageSprite(transform.Find("TopBar/Btn_Coins"), coinsSp);
            SetImageSprite(transform.Find("TopBar/Btn_Settings"), uiBg1Sp);
            SetImageSprite(transform.Find("TopBar/Btn_Settings/SettingsIcon"), settingIconSp);

            Transform homeContent = transform.Find("HomeContentRoot");
            if (homeContent != null)
            {
                SetImageSprite(homeContent.Find("Btn_Facebook"), uiBg1Sp);
                SetImageSprite(homeContent.Find("Btn_Facebook/FbIcon"), fbIconSp);
                SetImageSprite(homeContent.Find("Btn_Facebook/RewardPill"), badgePillSp, Image.Type.Sliced);
                SetImageSprite(homeContent.Find("Btn_Facebook/RewardPill/CoinIcon"), coinSp);
                SetImageSprite(homeContent.Find("Btn_RemoveAds"), noAdsSp);
                SetImageSprite(homeContent.Find("LadderGroup/LadderBeam"), beamSp);
                SetImageSprite(homeContent.Find("LadderGroup/Node_0/NodeGlow"), glowSp);
                SetImageSprite(homeContent.Find("LadderGroup/PlayButtonContainer/PlayAura"), glowSp);
                SetImageSprite(homeContent.Find("LadderGroup/PlayButtonContainer/Btn_Play/ShineClip/ShineSweep"), shineSp);
            }

            string[] tabNames = { "Tab_Shop", "Tab_Home", "Tab_Collection" };
            string[] tabIcons = { "shop", "home", "collection" };
            Transform navBar = transform.Find("BottomNavBar");
            if (navBar != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Transform tTr = navBar.Find(tabNames[i]);
                    if (tTr == null) continue;
                    SetImageSprite(tTr.Find("TabGlow"), glowSp);
                    SetImageSprite(tTr.Find("TabIcon"), GemSpriteManager.GetHomeSceneSprite(tabIcons[i]));
                }
            }

            Font levelFont = GetResolvedLevelNumberFont();
            if (levelFont != null)
            {
                FontStyle style = GetResolvedLevelNumberFontStyle(levelFont);
                for (int i = 0; i < 4; i++)
                {
                    if (_nodeNumberTexts[i] != null)
                    {
                        _nodeNumberTexts[i].font = levelFont;
                        _nodeNumberTexts[i].fontStyle = style;
                    }
                }
                if (_collectionNumberTexts != null)
                {
                    for (int i = 0; i < _collectionNumberTexts.Length; i++)
                    {
                        if (_collectionNumberTexts[i] != null)
                        {
                            _collectionNumberTexts[i].font = levelFont;
                            _collectionNumberTexts[i].fontStyle = style;
                        }
                    }
                }
            }

            EnsureRemoveAdsPanelSprites();
        }

        private void BuildHomeUIHierarchy()
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
            _scaler.referenceResolution = new Vector2(1080f, 1920f);
            _scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            BuildBackgroundLayer();
            BuildTopBar(font);
            BuildHomeContent(font);
            BuildShopPanel(font);
            BuildCollectionPanel(font);
            BuildBottomNavBar(font);
            BuildSettingsPanel(font);
            BuildRemoveAdsPanel(font);
            BuildFeedbackBanner(font);

            RefreshHomeDisplay();
        }

        private void BuildBackgroundLayer()
        {
            GameObject bgLayer = CreateContainer("BackgroundLayer", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject bgImgObj = CreateContainer("BgImage", bgLayer.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImg = bgImgObj.AddComponent<Image>();
            bgImg.sprite = GemSpriteManager.GetBackgroundSprite();
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            AspectRatioFitter fitter = bgImgObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            if (bgImg.sprite != null && bgImg.sprite.rect.height > 0f)
            {
                fitter.aspectRatio = bgImg.sprite.rect.width / bgImg.sprite.rect.height;
            }
            else
            {
                fitter.aspectRatio = 0.45f;
            }

            GameObject starsObj = CreateContainer("AmbientStars", bgLayer.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _ambientStarsRoot = starsObj.GetComponent<RectTransform>();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();

            Vector2[] starAnchors = {
                new Vector2(0.18f, 0.72f), new Vector2(0.84f, 0.78f),
                new Vector2(0.14f, 0.44f), new Vector2(0.88f, 0.50f),
                new Vector2(0.24f, 0.28f), new Vector2(0.79f, 0.30f),
                new Vector2(0.32f, 0.85f), new Vector2(0.72f, 0.65f)
            };
            float[] starSizes = { 22f, 26f, 20f, 24f, 18f, 22f, 20f, 18f };
            for (int i = 0; i < starAnchors.Length; i++)
            {
                GameObject sObj = CreateContainer("Star_" + i, _ambientStarsRoot,
                    starAnchors[i], starAnchors[i], Vector2.zero, new Vector2(starSizes[i], starSizes[i]));
                Image sImg = sObj.AddComponent<Image>();
                sImg.sprite = flareSp;
                sImg.color = new Color(0.88f, 0.80f, 1.00f, 0.68f);
                sImg.preserveAspect = true;
                sImg.raycastTarget = false;
            }
        }

        private void BuildTopBar(Font font)
        {
            GameObject topBar = CreateContainer("TopBar", transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(920f, 118f));

            // Lives Pill (heart.png: 305x114)
            GameObject livesObj = CreatePanel("Btn_Lives", topBar.transform, new Vector2(-118f, 0f), new Vector2(274f, 102f),
                GemSpriteManager.GetHomeSceneSprite("heart"), preserveAspect: true);
            _livesBtn = livesObj.AddComponent<Button>();
            _livesBtn.targetGraphic = livesObj.GetComponent<Image>();
            _livesBtn.onClick.AddListener(OnLivesPillClicked);

            _livesCountText = CreateOutlinedText("LivesCountText", livesObj.transform, DefaultLives.ToString(), font, 38,
                Color.white, new Vector2(-86f, 7f), new Vector2(64f, 52f),
                new Color(0.361f, 0.055f, 0.157f, 0.98f), new Vector2(2.2f, -2.2f), TextAnchor.MiddleCenter);

            _livesStatusText = CreateStyledText("LivesStatusText", livesObj.transform, "Full", font, 33,
                new Color(0.153f, 0.094f, 0.329f), new Vector2(46f, 2f), new Vector2(148f, 52f),
                new Color(1f, 1f, 1f, 0.55f), new Vector2(0f, -1f));

            // Coins Pill (coins.png: 333x131)
            GameObject coinsObj = CreatePanel("Btn_Coins", topBar.transform, new Vector2(178f, 0f), new Vector2(284f, 112f),
                GemSpriteManager.GetHomeSceneSprite("coins"), preserveAspect: true);
            _coinsBtn = coinsObj.AddComponent<Button>();
            _coinsBtn.targetGraphic = coinsObj.GetComponent<Image>();
            _coinsBtn.onClick.AddListener(OnCoinsPillClicked);

            _coinsCountText = CreateStyledText("CoinsCountText", coinsObj.transform, DefaultCoins.ToString(), font, 36,
                new Color(0.153f, 0.094f, 0.329f), new Vector2(46f, 3f), new Vector2(156f, 54f),
                new Color(1f, 1f, 1f, 0.55f), new Vector2(0f, -1f));

            // Top-right Settings Button (UI_BG1 + UI_setting)
            GameObject gearObj = CreatePanel("Btn_Settings", topBar.transform, new Vector2(390f, 2f), new Vector2(98f, 108f),
                GemSpriteManager.GetHomeSceneSprite("UI_BG1"), preserveAspect: true);
            _settingsBtn = gearObj.AddComponent<Button>();
            _settingsBtn.targetGraphic = gearObj.GetComponent<Image>();
            _settingsBtn.onClick.AddListener(OnSettingsClicked);

            CreateImage("SettingsIcon", gearObj.transform, Vector2.zero, new Vector2(88f, 98f),
                GemSpriteManager.GetHomeSceneSprite("UI_setting"), Color.white, preserveAspect: true);
        }

        private void BuildHomeContent(Font font)
        {
            GameObject root = CreateContainer("HomeContentRoot", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _homeContentRoot = root;

            // Left Quick Action: Facebook +20
            GameObject fbObj = CreatePanel("Btn_Facebook", root.transform, new Vector2(-364f, -342f), new Vector2(174f, 190f),
                GemSpriteManager.GetHomeSceneSprite("UI_BG1"), preserveAspect: true,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f));
            _fbBtn = fbObj.AddComponent<Button>();
            _fbBtn.targetGraphic = fbObj.GetComponent<Image>();
            _fbBtn.onClick.AddListener(OnFacebookClicked);

            Sprite fbIconSp = GemSpriteManager.GetHomeHelperSprite("home_fb_icon") ?? SpriteGenerator.GetHomeFacebookIconSprite();
            CreateImage("FbIcon", fbObj.transform, new Vector2(0f, 26f), new Vector2(86f, 86f), fbIconSp, Color.white, preserveAspect: true);

            Sprite badgePillSp = GemSpriteManager.GetHomeHelperSprite("home_badge_pill") ?? SpriteGenerator.GetHomeBadgePillSprite();
            GameObject fbPill = CreatePanel("RewardPill", fbObj.transform, new Vector2(0f, -48f), new Vector2(142f, 44f),
                badgePillSp, preserveAspect: false);
            Image fbPillImg = fbPill.GetComponent<Image>();
            fbPillImg.type = Image.Type.Sliced;
            fbPillImg.raycastTarget = false;
            _fbBadgeObj = fbPill;

            CreateImage("CoinIcon", fbPill.transform, new Vector2(-36f, 0f), new Vector2(32f, 32f),
                GemSpriteManager.GetWinSceneSprite("Coin"), Color.white, preserveAspect: true);
            CreateStyledText("AmountText", fbPill.transform, "+20", font, 26,
                new Color(0.153f, 0.094f, 0.329f), new Vector2(18f, 0f), new Vector2(78f, 36f),
                new Color(1f, 1f, 1f, 0.45f), new Vector2(0f, -1f));

            // Right Quick Action: Remove Ads (No ads.png)
            GameObject noAdsObj = CreatePanel("Btn_RemoveAds", root.transform, new Vector2(364f, -342f), new Vector2(186f, 186f),
                GemSpriteManager.GetHomeSceneSprite("No ads"), preserveAspect: true,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f));
            _removeAdsBtn = noAdsObj.AddComponent<Button>();
            _removeAdsBtn.targetGraphic = noAdsObj.GetComponent<Image>();
            _removeAdsBtn.onClick.AddListener(OnRemoveAdsClicked);

            // Center Level Ladder + PLAY button
            GameObject ladderGroup = CreateContainer("LadderGroup", root.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 1400f));

            Sprite beamSp = GemSpriteManager.GetHomeHelperSprite("home_ladder_beam") ?? SpriteGenerator.GetHomeLadderBeamSprite();
            CreateImage("LadderBeam", ladderGroup.transform, Vector2.zero, new Vector2(44f, 940f),
                beamSp, Color.white, preserveAspect: false);

            // 4 Level Nodes from top (Node_3) to bottom (Node_0)
            Font levelFont = GetResolvedLevelNumberFont();
            FontStyle levelStyle = GetResolvedLevelNumberFontStyle(levelFont);
            float[] nodeYPositions = { -172f, 54f, 260f, 466f };
            for (int i = 3; i >= 0; i--)
            {
                int idx = i;
                HomeLevelDifficulty nodeDiff = i == 0 ? HomeLevelDifficulty.Easy : HomeLevelDifficulty.Easy;
                Vector2 nodeSize = i == 0 ? new Vector2(222f, 210f) : new Vector2(168f, 144f);
                GameObject nodeObj = CreatePanel("Node_" + i, ladderGroup.transform,
                    new Vector2(0f, nodeYPositions[i]), nodeSize,
                    GemSpriteManager.GetHomeSceneSprite(i == 0 ? "o easy" : "nomal"), preserveAspect: true);

                _nodeRects[i] = nodeObj.GetComponent<RectTransform>();
                _nodeBgImgs[i] = nodeObj.GetComponent<Image>();
                _nodeGroups[i] = nodeObj.AddComponent<CanvasGroup>();
                _nodeGroups[i].alpha = (i == 3) ? 0.76f : 1.0f;

                if (i == 0)
                {
                    Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
                    Image glowImg = CreateImage("NodeGlow", nodeObj.transform, Vector2.zero, new Vector2(310f, 285f),
                        glowSp, EasyGlowColor, preserveAspect: false);
                    glowImg.transform.SetAsFirstSibling();
                    _activeNodeGlowImg = glowImg;
                    _activeNodeGlowRect = glowImg.rectTransform;
                }

                Button nodeBtn = nodeObj.AddComponent<Button>();
                nodeBtn.targetGraphic = _nodeBgImgs[i];
                nodeBtn.onClick.AddListener(() => OnLadderNodeClicked(idx));

                int fontSize = ComputeLevelFontSize(GetLadderNodeBaseFontSize(i), i + 1);
                Vector2 numSize = i == 0 ? new Vector2(180f, 120f) : new Vector2(148f, 100f);
                Vector2 shadowDist = i == 0 ? new Vector2(0f, -4.5f) : new Vector2(0f, -3.5f);
                _nodeNumberTexts[i] = CreateLevelNumberText("LevelNumText", nodeObj.transform, (i + 1).ToString(), levelFont, fontSize,
                    new Vector2(0f, _ladderNumberOffsetY), numSize, GetLevelNumberShadowColor(nodeDiff, i == 0), shadowDist, out _nodeNumberShadows[i]);
                _nodeNumberTexts[i].fontStyle = levelStyle;
            }

            // Dynamic PLAY Button below Node_0
            GameObject playContainer = CreateContainer("PlayButtonContainer", ladderGroup.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -476f), new Vector2(620f, 340f));
            _playContainerRect = playContainer.GetComponent<RectTransform>();

            Sprite playGlowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Image playAura = CreateImage("PlayAura", playContainer.transform, Vector2.zero, new Vector2(700f, 360f),
                playGlowSp, EasyGlowColor, preserveAspect: false);
            _playAuraImg = playAura;
            _playAuraRect = playAura.rectTransform;

            GameObject playBtnObj = CreatePanel("Btn_Play", playContainer.transform, new Vector2(0f, 16f), new Vector2(560f, 209f),
                GemSpriteManager.GetHomeSceneSprite("play easy"), preserveAspect: true);
            _playBtnRect = playBtnObj.GetComponent<RectTransform>();
            _playBtnImg = playBtnObj.GetComponent<Image>();
            _playBtn = playBtnObj.AddComponent<Button>();
            _playBtn.targetGraphic = _playBtnImg;
            _playBtn.onClick.AddListener(OnPlayClicked);

            GameObject shineClip = CreateContainer("ShineClip", playBtnObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 184f));
            shineClip.AddComponent<RectMask2D>();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();
            Image shineImg = CreateImage("ShineSweep", shineClip.transform, new Vector2(-360f, 0f), new Vector2(140f, 260f),
                shineSp, new Color(1f, 1f, 1f, 0.34f), preserveAspect: false);
            _playShineRect = shineImg.rectTransform;
        }

        private void BuildShopPanel(Font font)
        {
            GameObject shopRoot = CreateContainer("ShopPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _shopPanel = shopRoot;

            GameObject cardObj = CreatePanel("ShopCard", shopRoot.transform, new Vector2(0f, 32f), new Vector2(820f, 940f),
                GemSpriteManager.GetLoseHelperSprite("lose_card_frame") ?? SpriteGenerator.GetLoseCardFrameSprite(), preserveAspect: false);
            cardObj.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject headerObj = CreatePanel("ShopHeader", cardObj.transform, new Vector2(0f, 456f), new Vector2(640f, 120f),
                GemSpriteManager.GetLoseHelperSprite("lose_header_banner") ?? SpriteGenerator.GetLoseHeaderBannerSprite(), preserveAspect: false);
            headerObj.GetComponent<Image>().type = Image.Type.Sliced;
            CreateOutlinedText("Title", headerObj.transform, "COSMIC SHOP", font, 48,
                Color.white, new Vector2(0f, 2f), new Vector2(540f, 84f),
                new Color(0.125f, 0.031f, 0.259f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleCenter);

            Sprite offerBgSp = GemSpriteManager.GetLoseHelperSprite("lose_offer_card") ?? SpriteGenerator.GetLoseOfferCardSprite();
            Sprite btnGreenSp = GemSpriteManager.GetLoseHelperSprite("lose_btn_green") ?? SpriteGenerator.GetLoseGreenButtonSprite();
            Sprite coinsSp = GemSpriteManager.GetWinSceneSprite("Coins");
            Sprite freezeSp = GemSpriteManager.GetBoosterSprite("booster1");
            Sprite magnetSp = GemSpriteManager.GetBoosterSprite("booster2");
            Sprite wandSp = GemSpriteManager.GetBoosterSprite("booster3");

            BuildShopRow(cardObj.transform, "Row_Free", 240f, "Daily Gift", "+50 Coins", "FREE", 50,
                offerBgSp, btnGreenSp, coinsSp, null, null, null, font);
            BuildShopRow(cardObj.transform, "Row_Starter", -18f, "Cosmic Pack", "450 Coins + Boosters", "₫52,000", 450,
                offerBgSp, btnGreenSp, coinsSp, freezeSp, magnetSp, wandSp, font);
            BuildShopRow(cardObj.transform, "Row_Mega", -276f, "Galaxy Mega", "1400 Coins + Boosters", "₫131,000", 1400,
                offerBgSp, btnGreenSp, coinsSp, freezeSp, magnetSp, wandSp, font);

            shopRoot.SetActive(false);
        }

        private void BuildShopRow(Transform parent, string rowName, float posY, string title, string subtitle, string priceText, int coinGrant,
            Sprite rowBgSp, Sprite btnSp, Sprite coinsSp, Sprite b1, Sprite b2, Sprite b3, Font font)
        {
            GameObject row = CreatePanel(rowName, parent, new Vector2(0f, posY), new Vector2(740f, 224f), rowBgSp, preserveAspect: false);
            row.GetComponent<Image>().type = Image.Type.Sliced;

            CreateImage("CoinsIcon", row.transform, new Vector2(-260f, 16f), new Vector2(130f, 112f), coinsSp, Color.white, preserveAspect: true);

            if (b1 != null && b2 != null && b3 != null)
            {
                CreateImage("B1", row.transform, new Vector2(-120f, -14f), new Vector2(68f, 68f), b1, Color.white, preserveAspect: true);
                CreateImage("B2", row.transform, new Vector2(-46f, -14f), new Vector2(68f, 68f), b2, Color.white, preserveAspect: true);
                CreateImage("B3", row.transform, new Vector2(-84f, 44f), new Vector2(68f, 68f), b3, Color.white, preserveAspect: true);
            }

            CreateOutlinedText("RowTitle", row.transform, title, font, 38,
                Color.white, new Vector2(150f, 52f), new Vector2(360f, 54f),
                new Color(0.10f, 0.03f, 0.22f, 0.96f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);
            CreateStyledText("RowSub", row.transform, subtitle, font, 24,
                new Color(0.95f, 0.90f, 1.00f), new Vector2(150f, 10f), new Vector2(380f, 40f),
                new Color(0.08f, 0.02f, 0.18f, 0.9f), new Vector2(0f, -1.5f));

            GameObject buyBtnObj = CreatePanel("Btn_Buy", row.transform, new Vector2(150f, -54f), new Vector2(280f, 74f), btnSp, preserveAspect: false);
            buyBtnObj.GetComponent<Image>().type = Image.Type.Sliced;
            Button btn = buyBtnObj.AddComponent<Button>();
            btn.targetGraphic = buyBtnObj.GetComponent<Image>();
            btn.onClick.AddListener(() => OnShopOfferClicked(title, coinGrant, buyBtnObj.transform));

            CreateOutlinedText("PriceLabel", buyBtnObj.transform, priceText, font, 32,
                Color.white, Vector2.zero, new Vector2(240f, 52f),
                new Color(0.039f, 0.180f, 0.102f, 0.98f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);
        }

        private void BindShopPanel(Transform shopRoot)
        {
            Transform cardTr = shopRoot.Find("ShopCard");
            if (cardTr == null) return;
            BindShopRowButton(cardTr.Find("Row_Free"), "Daily Gift", 50);
            BindShopRowButton(cardTr.Find("Row_Starter"), "Cosmic Pack", 450);
            BindShopRowButton(cardTr.Find("Row_Mega"), "Galaxy Mega", 1400);
        }

        private void BindShopRowButton(Transform rowTr, string title, int coinGrant)
        {
            if (rowTr == null) return;
            Transform btnTr = rowTr.Find("Btn_Buy");
            Button btn = btnTr?.GetComponent<Button>();
            RebindButton(btn, () => OnShopOfferClicked(title, coinGrant, btnTr));
        }

        private void BuildCollectionPanel(Font font)
        {
            GameObject colRoot = CreateContainer("CollectionPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _collectionPanel = colRoot;

            GameObject cardObj = CreatePanel("CollectionCard", colRoot.transform, new Vector2(0f, 32f), new Vector2(820f, 960f),
                GemSpriteManager.GetLoseHelperSprite("lose_card_frame") ?? SpriteGenerator.GetLoseCardFrameSprite(), preserveAspect: false);
            cardObj.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject headerObj = CreatePanel("CollectionHeader", cardObj.transform, new Vector2(0f, 466f), new Vector2(640f, 120f),
                GemSpriteManager.GetLoseHelperSprite("lose_header_banner") ?? SpriteGenerator.GetLoseHeaderBannerSprite(), preserveAspect: false);
            headerObj.GetComponent<Image>().type = Image.Type.Sliced;
            CreateOutlinedText("Title", headerObj.transform, "LEVELS", font, 48,
                Color.white, new Vector2(0f, 2f), new Vector2(540f, 84f),
                new Color(0.125f, 0.031f, 0.259f, 0.98f), new Vector2(3f, -3f), TextAnchor.MiddleCenter);

            CreateStyledText("Legend", cardObj.transform, "Green: Easy  |  Orange: Hard  |  Purple: Super Hard", font, 23,
                new Color(0.92f, 0.87f, 1.00f), new Vector2(0f, 356f), new Vector2(740f, 42f),
                new Color(0.08f, 0.02f, 0.18f, 0.9f), new Vector2(0f, -1.5f));

            EnsureCollectionTiles(cardObj.transform);

            colRoot.SetActive(false);
        }

        private void BindCollectionPanel(Transform colRoot)
        {
            Transform cardTr = colRoot.Find("CollectionCard");
            if (cardTr == null) return;
            EnsureCollectionTiles(cardTr);
        }

        private void EnsureCollectionTiles(Transform cardTr)
        {
            if (cardTr == null) return;
            int totalLevels = Mathf.Max(MaxPlayableLevel, CustomLevelRepository.GetMaxLevelNumber());

            if (totalLevels <= MaxPlayableLevel && cardTr.Find("LevelsScrollView") == null)
            {
                bool allExist = true;
                for (int lvl = 1; lvl <= totalLevels; lvl++)
                {
                    if (cardTr.Find("ColLevel_" + lvl) == null)
                    {
                        allExist = false;
                        break;
                    }
                }

                if (allExist)
                {
                    _collectionTileRects = new RectTransform[totalLevels];
                    _collectionTileImgs = new Image[totalLevels];
                    _collectionNumberTexts = new Text[totalLevels];
                    for (int lvl = 1; lvl <= totalLevels; lvl++)
                    {
                        Transform tTr = cardTr.Find("ColLevel_" + lvl);
                        int idx = lvl - 1;
                        _collectionTileRects[idx] = tTr as RectTransform;
                        _collectionTileImgs[idx] = tTr.GetComponent<Image>();
                        _collectionNumberTexts[idx] = tTr.Find("NumText")?.GetComponent<Text>();
                        int capturedLvl = lvl;
                        RebindButton(tTr.GetComponent<Button>(), () => OnCollectionLevelSelected(capturedLvl));
                    }
                    return;
                }
            }

            // Clear existing static tiles or previous scroll view before rebuilding dynamically
            for (int i = cardTr.childCount - 1; i >= 0; i--)
            {
                Transform child = cardTr.GetChild(i);
                if (child.name.StartsWith("ColLevel_") || child.name == "LevelsScrollView")
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            _collectionTileRects = new RectTransform[totalLevels];
            _collectionTileImgs = new Image[totalLevels];
            _collectionNumberTexts = new Text[totalLevels];

            Font levelFont = GetResolvedLevelNumberFont();
            FontStyle levelStyle = GetResolvedLevelNumberFontStyle(levelFont);

            // ScrollRect container inside CollectionCard below the legend
            GameObject scrollObj = CreateContainer("LevelsScrollView", cardTr,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -66f), new Vector2(760f, 740f));
            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 36f;

            GameObject viewportObj = CreateContainer("Viewport", scrollObj.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewportObj.AddComponent<RectMask2D>();
            Image vpImg = viewportObj.AddComponent<Image>();
            vpImg.color = new Color(0f, 0f, 0f, 0f);
            scrollRect.viewport = viewportObj.GetComponent<RectTransform>();

            int totalRows = Mathf.CeilToInt(totalLevels / 3f);
            float contentH = Mathf.Max(740f, totalRows * 172f + 40f);

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(760f, contentH);
            scrollRect.content = contentRect;

            for (int lvl = 1; lvl <= totalLevels; lvl++)
            {
                int idx = lvl - 1;
                int row = idx / 3;
                int col = idx % 3;
                bool isLastAlone = (lvl == totalLevels) && (totalLevels % 3 == 1);
                float x = isLastAlone ? 0f : (col - 1) * 224f;
                float y = -94f - row * 172f;

                HomeLevelDifficulty diff = GetLevelDifficulty(lvl);
                Sprite tileSp = GetNodeDifficultySprite(diff, isActive: true);

                GameObject tileObj = CreatePanel("ColLevel_" + lvl, contentObj.transform, new Vector2(x, y), new Vector2(168f, 146f),
                    tileSp, preserveAspect: true,
                    anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f));
                _collectionTileRects[idx] = tileObj.GetComponent<RectTransform>();
                _collectionTileImgs[idx] = tileObj.GetComponent<Image>();

                Button btn = tileObj.AddComponent<Button>();
                btn.targetGraphic = _collectionTileImgs[idx];
                int capturedLvl = lvl;
                btn.onClick.AddListener(() => OnCollectionLevelSelected(capturedLvl));

                int colFontSize = ComputeLevelFontSize(_collectionLevelFontSize, lvl);
                _collectionNumberTexts[idx] = CreateLevelNumberText("NumText", tileObj.transform, lvl.ToString(), levelFont, colFontSize,
                    new Vector2(0f, _collectionNumberOffsetY), new Vector2(140f, 88f),
                    GetLevelNumberShadowColor(diff, isActive: true), new Vector2(0f, -3.5f), out _);
                _collectionNumberTexts[idx].fontStyle = levelStyle;
            }
        }

        private void BuildBottomNavBar(Font font)
        {
            GameObject navBar = CreateContainer("BottomNavBar", transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 142f), new Vector2(936f, 250f));

            string[] tabNames = { "Tab_Shop", "Tab_Home", "Tab_Collection" };
            string[] tabIcons = { "shop", "home", "collection" };
            string[] tabLabels = { "SHOP", "HOME", "CARDS" };
            float[] tabX = { -304f, 0f, 304f };

            Sprite activeSp = GemSpriteManager.GetHomeSceneSprite("Group 320");
            Sprite inactiveSp = GemSpriteManager.GetHomeSceneSprite("banner");
            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();

            for (int i = 0; i < 3; i++)
            {
                int tabIdx = i;
                bool isHome = (i == 1);
                GameObject tabObj = CreatePanel(tabNames[i], navBar.transform,
                    new Vector2(tabX[i], isHome ? 6f : -18f),
                    isHome ? new Vector2(316f, 278f) : new Vector2(290f, 212f),
                    isHome ? activeSp : inactiveSp, preserveAspect: true);

                _tabRects[i] = tabObj.GetComponent<RectTransform>();
                _tabBgImgs[i] = tabObj.GetComponent<Image>();

                Image glowImg = CreateImage("TabGlow", tabObj.transform, Vector2.zero, new Vector2(360f, 310f),
                    glowSp, new Color(0.68f, 0.42f, 1.00f, 0.55f), preserveAspect: false);
                glowImg.transform.SetAsFirstSibling();
                glowImg.enabled = isHome;
                _tabGlowImgs[i] = glowImg;

                Image iconImg = CreateImage("TabIcon", tabObj.transform,
                    isHome ? new Vector2(0f, 26f) : new Vector2(0f, 4f),
                    isHome ? new Vector2(138f, 124f) : new Vector2(130f, 114f),
                    GemSpriteManager.GetHomeSceneSprite(tabIcons[i]), Color.white, preserveAspect: true);
                _tabIconRects[i] = iconImg.rectTransform;
                _tabIconImgs[i] = iconImg;

                Text lbl = CreateOutlinedText("TabLabel", tabObj.transform, tabLabels[i], font, 34,
                    Color.white, new Vector2(0f, -74f), new Vector2(230f, 48f),
                    new Color(0.10f, 0.03f, 0.24f, 0.96f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);
                lbl.gameObject.SetActive(isHome);
                _tabLabelTexts[i] = lbl;

                Button btn = tabObj.AddComponent<Button>();
                btn.targetGraphic = _tabBgImgs[i];
                btn.onClick.AddListener(() => SetActiveTab(tabIdx, playSound: true));
            }
        }

        private void BuildSettingsPanel(Font font)
        {
            _settingsPanel = CreateContainer("SettingsPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgOverlay = _settingsPanel.AddComponent<Image>();
            bgOverlay.color = new Color(0.02f, 0.015f, 0.06f, 0.80f);
            Button backdropBtn = _settingsPanel.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(() => SetSettingsOpen(false));

            Sprite settingFrame = GemSpriteManager.GetSettingSprite("Group 331");
            GameObject dialog = CreatePanel("DialogBox", _settingsPanel.transform, Vector2.zero, new Vector2(880f, 756f),
                settingFrame, preserveAspect: true);

            GameObject closeObj = CreatePanel("Btn_Close", dialog.transform, new Vector2(-34f, -64f), new Vector2(88f, 88f),
                GemSpriteManager.GetSettingSprite("X (1)"), preserveAspect: true,
                anchorMin: new Vector2(1f, 1f), anchorMax: new Vector2(1f, 1f));
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeObj.GetComponent<Image>();
            closeBtn.onClick.AddListener(() => SetSettingsOpen(false));

            GameObject cardObj = CreateContainer("SettingsCard", dialog.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -44f), new Vector2(740f, 460f));

            BuildSettingRow(cardObj.transform, "SoundRow", 156f, "Sound", font, ref _soundIconImg, ref _soundToggleImg, ToggleSound);
            BuildSettingRow(cardObj.transform, "MusicRow", 0f, "Music", font, ref _musicIconImg, ref _musicToggleImg, ToggleMusic);
            BuildSettingRow(cardObj.transform, "VibrationRow", -156f, "Vibration", font, ref _vibrationIconImg, ref _vibrationToggleImg, ToggleVibration);

            UpdateSettingViews();
            _settingsPanel.SetActive(false);
        }

        private void BuildSettingRow(Transform parent, string rowName, float posY, string title, Font font,
            ref Image iconImgRef, ref Image toggleImgRef, UnityEngine.Events.UnityAction onToggle)
        {
            GameObject rowObj = CreatePanel(rowName, parent, new Vector2(0f, posY), new Vector2(720f, 134f),
                SpriteGenerator.GetSettingRowSprite(), preserveAspect: false);
            Image rowBg = rowObj.GetComponent<Image>();
            rowBg.type = Image.Type.Sliced;
            rowBg.raycastTarget = false;

            GameObject iconObj = CreatePanel("Icon", rowObj.transform, new Vector2(78f, 0f), new Vector2(72f, 68f),
                null, preserveAspect: true, anchorMin: new Vector2(0f, 0.5f), anchorMax: new Vector2(0f, 0.5f));
            iconImgRef = iconObj.GetComponent<Image>();
            iconImgRef.raycastTarget = false;

            Text lbl = CreateStyledText("Label", rowObj.transform, title, font, 42,
                new Color(0.95f, 0.91f, 1.0f), new Vector2(318f, 0f), new Vector2(300f, 68f),
                new Color(0.10f, 0.04f, 0.22f, 0.60f), new Vector2(1.5f, -1.5f));
            lbl.alignment = TextAnchor.MiddleLeft;

            GameObject swObj = CreatePanel("Switch", rowObj.transform, new Vector2(-115f, 0f), new Vector2(158f, 72f),
                null, preserveAspect: true, anchorMin: new Vector2(1f, 0.5f), anchorMax: new Vector2(1f, 0.5f));
            toggleImgRef = swObj.GetComponent<Image>();
            Button swBtn = swObj.AddComponent<Button>();
            swBtn.targetGraphic = toggleImgRef;
            swBtn.onClick.AddListener(onToggle);
        }

        private void BindSettingsPanel(Transform settingsTr)
        {
            Button backdropBtn = settingsTr.GetComponent<Button>();
            RebindButton(backdropBtn, () => SetSettingsOpen(false));

            Transform dialog = settingsTr.Find("DialogBox");
            if (dialog == null) return;
            RebindButton(dialog.Find("Btn_Close")?.GetComponent<Button>(), () => SetSettingsOpen(false));

            Transform card = dialog.Find("SettingsCard");
            if (card != null)
            {
                BindSettingRow(card.Find("SoundRow"), ref _soundIconImg, ref _soundToggleImg, ToggleSound);
                BindSettingRow(card.Find("MusicRow"), ref _musicIconImg, ref _musicToggleImg, ToggleMusic);
                BindSettingRow(card.Find("VibrationRow"), ref _vibrationIconImg, ref _vibrationToggleImg, ToggleVibration);
            }
            UpdateSettingViews();
            _settingsPanel.SetActive(false);
        }

        private void BindSettingRow(Transform rowTr, ref Image iconRef, ref Image toggleRef, UnityEngine.Events.UnityAction onToggle)
        {
            if (rowTr == null) return;
            iconRef = rowTr.Find("Icon")?.GetComponent<Image>();
            Transform swTr = rowTr.Find("Switch");
            toggleRef = swTr?.GetComponent<Image>();
            RebindButton(swTr?.GetComponent<Button>(), onToggle);
        }

        private void BuildFeedbackBanner(Font font)
        {
            GameObject fbObj = CreatePanel("FeedbackBanner", transform, new Vector2(0f, 292f), new Vector2(820f, 92f),
                GemSpriteManager.GetLoseHelperSprite("lose_resource_pill") ?? SpriteGenerator.GetLoseResourcePillSprite(),
                preserveAspect: false, anchorMin: new Vector2(0.5f, 0f), anchorMax: new Vector2(0.5f, 0f));
            Image fbImg = fbObj.GetComponent<Image>();
            fbImg.type = Image.Type.Sliced;
            fbImg.raycastTarget = false;

            _feedbackCanvasGroup = fbObj.AddComponent<CanvasGroup>();
            _feedbackCanvasGroup.alpha = 0f;
            _feedbackCanvasGroup.blocksRaycasts = false;

            _feedbackText = CreateOutlinedText("FeedbackText", fbObj.transform, "", font, 30,
                new Color(1.0f, 0.96f, 0.78f), Vector2.zero, new Vector2(760f, 68f),
                new Color(0.08f, 0.02f, 0.22f, 0.96f), new Vector2(2f, -2f), TextAnchor.MiddleCenter);

            _feedbackBanner = fbObj;
            _feedbackBanner.transform.SetAsLastSibling();
        }

        public void RefreshHomeDisplay()
        {
            int activeLevel = GetActiveLevelNumber();
            int lives = PlayerPrefs.HasKey(PrefLivesKey) ? PlayerPrefs.GetInt(PrefLivesKey, DefaultLives) : DefaultLives;
            int coins = PlayerPrefs.HasKey(PrefCoinsKey) ? PlayerPrefs.GetInt(PrefCoinsKey, DefaultCoins) : DefaultCoins;

            if (_livesCountText != null) _livesCountText.text = Mathf.Clamp(lives, 0, MaxLives).ToString();
            if (_livesStatusText != null)
            {
                _livesStatusText.text = lives >= MaxLives ? "Full" : FormatTimeMMSS(_lifeRegenTimer);
            }
            if (_coinsCountText != null) _coinsCountText.text = coins.ToString();

            if (_fbBadgeObj != null)
            {
                bool fbClaimed = PlayerPrefs.GetInt(PrefFbClaimedKey, 0) == 1;
                _fbBadgeObj.SetActive(!fbClaimed);
            }
            if (_removeAdsBtn != null)
            {
                _removeAdsBtn.interactable = true;
                Image noAdsImg = _removeAdsBtn.GetComponent<Image>();
                if (noAdsImg != null)
                {
                    noAdsImg.color = Color.white;
                }
            }

            if (_removeAdsPriceLabel != null)
            {
                _removeAdsPriceLabel.text = _removeAdsPriceText;
            }

            // Update 4 Ladder Nodes
            Font levelFont = GetResolvedLevelNumberFont();
            FontStyle levelStyle = GetResolvedLevelNumberFontStyle(levelFont);

            for (int i = 0; i < 4; i++)
            {
                int nodeLevel = activeLevel + i;
                bool isActiveNode = (i == 0);
                bool isTopFaded = (i == 3);
                HomeLevelDifficulty diff = GetLevelDifficulty(nodeLevel);

                if (_nodeNumberTexts[i] != null)
                {
                    if (levelFont != null) _nodeNumberTexts[i].font = levelFont;
                    _nodeNumberTexts[i].fontStyle = levelStyle;
                    _nodeNumberTexts[i].fontSize = ComputeLevelFontSize(GetLadderNodeBaseFontSize(i), nodeLevel);
                    _nodeNumberTexts[i].rectTransform.anchoredPosition = new Vector2(0f, _ladderNumberOffsetY);
                    _nodeNumberTexts[i].text = nodeLevel.ToString();
                    _nodeNumberTexts[i].color = isTopFaded
                        ? GetTopFadedNumberColor(diff)
                        : new Color(1.0f, 0.996f, 0.988f, 1.0f);

                    Shadow sh = _nodeNumberShadows[i] ?? _nodeNumberTexts[i].GetComponent<Shadow>();
                    _nodeNumberShadows[i] = sh;
                    if (sh != null)
                    {
                        if (isTopFaded)
                        {
                            sh.effectColor = new Color(0.72f, 0.68f, 0.96f, 0.24f);
                            sh.effectDistance = new Vector2(0f, -2.0f);
                        }
                        else
                        {
                            sh.effectColor = GetLevelNumberShadowColor(diff, isActiveNode);
                            sh.effectDistance = isActiveNode ? new Vector2(0f, -4.5f) : new Vector2(0f, -3.5f);
                        }
                    }
                }

                if (_nodeBgImgs[i] != null)
                {
                    _nodeBgImgs[i].sprite = GetNodeDifficultySprite(diff, isActiveNode);
                    _nodeBgImgs[i].preserveAspect = true;
                    _nodeBgImgs[i].color = isTopFaded ? GetTopFadedTileTint(diff) : Color.white;
                }

                if (_nodeRects[i] != null)
                {
                    if (isActiveNode)
                    {
                        _nodeRects[i].sizeDelta = diff == HomeLevelDifficulty.Easy
                            ? new Vector2(222f, 210f)
                            : new Vector2(214f, 186f);
                    }
                    else
                    {
                        _nodeRects[i].sizeDelta = new Vector2(168f, 144f);
                    }
                }

                if (_nodeGroups[i] != null)
                {
                    _nodeGroups[i].alpha = isTopFaded ? 0.76f : 1.0f;
                }
            }

            // Update Collection level tiles font sizes & difficulty visuals
            if (_collectionNumberTexts != null)
            {
                for (int idx = 0; idx < _collectionNumberTexts.Length; idx++)
                {
                    int lvl = idx + 1;
                    HomeLevelDifficulty colDiff = GetLevelDifficulty(lvl);
                    if (_collectionTileImgs != null && idx < _collectionTileImgs.Length && _collectionTileImgs[idx] != null)
                    {
                        _collectionTileImgs[idx].sprite = GetNodeDifficultySprite(colDiff, isActive: true);
                        _collectionTileImgs[idx].preserveAspect = true;
                    }
                    Text colTxt = _collectionNumberTexts[idx];
                    if (colTxt == null) continue;
                    if (levelFont != null) colTxt.font = levelFont;
                    colTxt.fontStyle = levelStyle;
                    colTxt.fontSize = ComputeLevelFontSize(_collectionLevelFontSize, lvl);
                    colTxt.rectTransform.anchoredPosition = new Vector2(0f, _collectionNumberOffsetY);
                    Shadow colShadow = colTxt.GetComponent<Shadow>();
                    if (colShadow != null) colShadow.effectColor = GetLevelNumberShadowColor(colDiff, isActive: true);
                }
            }

            // Update active difficulty aura & dynamic PLAY button
            HomeLevelDifficulty activeDiff = GetLevelDifficulty(activeLevel);
            Color glowCol = GetDifficultyGlowColor(activeDiff);

            if (_activeNodeGlowImg != null) _activeNodeGlowImg.color = glowCol;
            if (_playAuraImg != null) _playAuraImg.color = glowCol;

            if (_playBtnImg != null && _playBtnRect != null)
            {
                switch (activeDiff)
                {
                    case HomeLevelDifficulty.Hard:
                        _playBtnImg.sprite = GemSpriteManager.GetHomeSceneSprite("Play hard");
                        _playBtnRect.sizeDelta = new Vector2(566f, 290f);
                        _playBtnRect.anchoredPosition = new Vector2(0f, 42f);
                        break;
                    case HomeLevelDifficulty.SuperHard:
                        _playBtnImg.sprite = GemSpriteManager.GetHomeSceneSprite("play super hard");
                        _playBtnRect.sizeDelta = new Vector2(552f, 310f);
                        _playBtnRect.anchoredPosition = new Vector2(0f, 44f);
                        break;
                    default:
                        _playBtnImg.sprite = GemSpriteManager.GetHomeSceneSprite("play easy");
                        _playBtnRect.sizeDelta = new Vector2(560f, 209f);
                        _playBtnRect.anchoredPosition = new Vector2(0f, 16f);
                        break;
                }
                _playBtnImg.preserveAspect = true;
            }
        }

        private Sprite GetNodeDifficultySprite(HomeLevelDifficulty diff, bool isActive)
        {
            switch (diff)
            {
                case HomeLevelDifficulty.Hard:
                    return GemSpriteManager.GetHomeSceneSprite("o hard");
                case HomeLevelDifficulty.SuperHard:
                    return GemSpriteManager.GetHomeSceneSprite("o super hard");
                default:
                    return GemSpriteManager.GetHomeSceneSprite(isActive ? "o easy" : "nomal");
            }
        }

        private static Color GetDifficultyGlowColor(HomeLevelDifficulty diff)
        {
            switch (diff)
            {
                case HomeLevelDifficulty.Hard: return HardGlowColor;
                case HomeLevelDifficulty.SuperHard: return SuperHardGlowColor;
                default: return EasyGlowColor;
            }
        }

        public void SetActiveTab(int tabIndex, bool playSound = true)
        {
            tabIndex = Mathf.Clamp(tabIndex, 0, 2);
            if (playSound && SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            _activeTabIndex = tabIndex;

            if (_homeContentRoot != null) _homeContentRoot.SetActive(tabIndex == 1);
            if (_shopPanel != null) _shopPanel.SetActive(tabIndex == 0);
            if (_collectionPanel != null) _collectionPanel.SetActive(tabIndex == 2);

            Sprite activeSp = GemSpriteManager.GetHomeSceneSprite("Group 320");
            Sprite inactiveSp = GemSpriteManager.GetHomeSceneSprite("banner");
            float[] tabX = { -304f, 0f, 304f };

            for (int i = 0; i < 3; i++)
            {
                bool isActive = (i == tabIndex);
                if (_tabBgImgs[i] != null)
                {
                    _tabBgImgs[i].sprite = isActive ? activeSp : inactiveSp;
                    _tabBgImgs[i].preserveAspect = true;
                }
                if (_tabRects[i] != null)
                {
                    _tabRects[i].anchoredPosition = new Vector2(tabX[i], isActive ? 6f : -18f);
                    _tabRects[i].sizeDelta = isActive ? new Vector2(316f, 278f) : new Vector2(290f, 212f);
                }
                if (_tabGlowImgs[i] != null)
                {
                    _tabGlowImgs[i].enabled = isActive;
                }
                if (_tabIconRects[i] != null)
                {
                    _tabIconRects[i].anchoredPosition = isActive ? new Vector2(0f, 26f) : new Vector2(0f, 4f);
                    _tabIconRects[i].sizeDelta = isActive ? new Vector2(138f, 124f) : new Vector2(130f, 114f);
                }
                if (_tabLabelTexts[i] != null)
                {
                    _tabLabelTexts[i].gameObject.SetActive(isActive);
                }
            }

            if (Application.isPlaying && _tabRects[tabIndex] != null)
            {
                StartCoroutine(CoPunch(_tabRects[tabIndex]));
            }
        }

        public void OnPlayClicked()
        {
            if (_isLoadingLevel) return;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_playBtnRect != null && Application.isPlaying) StartCoroutine(CoPunch(_playBtnRect));

            int activeLevel = GetActiveLevelNumber();
            int maxPlayable = CustomLevelRepository.GetMaxLevelNumber();
            int targetLevel = activeLevel > maxPlayable ? (((activeLevel - 1) % maxPlayable) + 1) : activeLevel;
            PlayerPrefs.SetInt(PrefCurrentLevelKey, activeLevel);
            PlayerPrefs.Save();

            string targetScene = "Level" + targetLevel;
            if (Application.CanStreamedLevelBeLoaded(targetScene))
            {
                CustomLevelRepository.PendingRuntimeLevelNumber = 0;
                _isLoadingLevel = true;
                SceneManager.LoadScene(targetScene);
            }
            else if (CustomLevelRepository.HasLevel(targetLevel) && Application.CanStreamedLevelBeLoaded("MainScene"))
            {
                CustomLevelRepository.PendingRuntimeLevelNumber = targetLevel;
                _isLoadingLevel = true;
                SceneManager.LoadScene("MainScene");
            }
            else if (Application.CanStreamedLevelBeLoaded("MainScene"))
            {
                CustomLevelRepository.PendingRuntimeLevelNumber = 0;
                _isLoadingLevel = true;
                SceneManager.LoadScene("MainScene");
            }
            else
            {
                ShowToast($"Level {targetLevel} chưa có trong Build Settings!");
            }
        }

        private void OnLadderNodeClicked(int nodeIndex)
        {
            if (nodeIndex == 0)
            {
                OnPlayClicked();
                return;
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlayError();
            if (_nodeRects[nodeIndex] != null && Application.isPlaying)
            {
                StartCoroutine(CoShake(_nodeRects[nodeIndex]));
            }
            int activeLevel = GetActiveLevelNumber();
            ShowToast($"Hoàn thành Level {activeLevel} để mở khóa!");
        }

        private void OnLivesPillClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            int lives = PlayerPrefs.GetInt(PrefLivesKey, DefaultLives);
            if (lives >= MaxLives)
            {
                ShowToast("Tim đã đầy!");
            }
            else
            {
                SetActiveTab(0, playSound: false);
                ShowToast("Mua gói Cosmic trong Shop để nhận thêm tài nguyên!");
            }
        }

        private void OnCoinsPillClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            SetActiveTab(0, playSound: false);
        }

        private void OnFacebookClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_fbBtn != null && Application.isPlaying) StartCoroutine(CoPunch(_fbBtn.transform));

            if (PlayerPrefs.GetInt(PrefFbClaimedKey, 0) == 1)
            {
                ShowToast("Bạn đã nhận phần thưởng +20 Xu rồi!");
                return;
            }

            int coins = PlayerPrefs.GetInt(PrefCoinsKey, DefaultCoins) + 20;
            PlayerPrefs.SetInt(PrefCoinsKey, coins);
            PlayerPrefs.SetInt(PrefFbClaimedKey, 1);
            PlayerPrefs.Save();
            RefreshHomeDisplay();
            ShowToast("Đã nhận +20 Xu thưởng kết nối!");
        }

        private void OnRemoveAdsClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_removeAdsBtn != null && Application.isPlaying) StartCoroutine(CoPunch(_removeAdsBtn.transform));
            SetRemoveAdsOpen(true, animate: Application.isPlaying);
        }

        public void SetRemoveAdsOpen(bool open, bool animate = true)
        {
            if (open)
            {
                if (_removeAdsPanel == null)
                {
                    Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    BuildRemoveAdsPanel(font);
                }
                if (_removeAdsPanel == null) return;

                EnsureRemoveAdsPanelSprites();
                if (_removeAdsPriceLabel != null) _removeAdsPriceLabel.text = _removeAdsPriceText;
                Transform dialog = _removeAdsPanel.transform.Find("DialogRoot");
                if (dialog != null)
                {
                    UpdateRemoveAdsRewardLabels(dialog.Find("RewardBar"));
                }

                if (_removeAdsAnimCoroutine != null)
                {
                    StopCoroutine(_removeAdsAnimCoroutine);
                    _removeAdsAnimCoroutine = null;
                }

                _removeAdsPanel.SetActive(true);
                _removeAdsPanel.transform.SetAsLastSibling();
                if (_feedbackBanner != null) _feedbackBanner.transform.SetAsLastSibling();

                if (animate && Application.isPlaying)
                {
                    _removeAdsAnimCoroutine = StartCoroutine(CoAnimateRemoveAdsOpen());
                }
                else
                {
                    if (_removeAdsCanvasGroup != null) _removeAdsCanvasGroup.alpha = 1f;
                    if (_removeAdsDialogRect != null) _removeAdsDialogRect.localScale = Vector3.one;
                    if (_removeAdsTapContinueGroup != null) _removeAdsTapContinueGroup.alpha = 1f;
                }
            }
            else
            {
                if (_removeAdsAnimCoroutine != null)
                {
                    StopCoroutine(_removeAdsAnimCoroutine);
                    _removeAdsAnimCoroutine = null;
                }
                if (_removeAdsPanel != null) _removeAdsPanel.SetActive(false);
            }
        }

        private IEnumerator CoAnimateRemoveAdsOpen()
        {
            if (_removeAdsCanvasGroup != null) _removeAdsCanvasGroup.alpha = 0f;
            if (_removeAdsDialogRect != null) _removeAdsDialogRect.localScale = new Vector3(0.76f, 0.76f, 1f);
            if (_removeAdsTapContinueGroup != null) _removeAdsTapContinueGroup.alpha = 0f;

            float elapsed = 0f;
            const float dur = 0.28f;
            while (elapsed < dur && _removeAdsPanel != null && _removeAdsPanel.activeSelf)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                if (_removeAdsCanvasGroup != null)
                {
                    _removeAdsCanvasGroup.alpha = Mathf.Clamp01(t / 0.55f);
                }
                if (_removeAdsDialogRect != null)
                {
                    float s = t < 0.65f
                        ? Mathf.Lerp(0.76f, 1.05f, Mathf.Sin((t / 0.65f) * Mathf.PI * 0.5f))
                        : Mathf.Lerp(1.05f, 1.0f, (t - 0.65f) / 0.35f);
                    _removeAdsDialogRect.localScale = new Vector3(s, s, 1f);
                }
                if (_removeAdsTapContinueGroup != null && t >= 0.45f)
                {
                    _removeAdsTapContinueGroup.alpha = Mathf.Clamp01((t - 0.45f) / 0.55f);
                }
                yield return null;
            }

            if (_removeAdsCanvasGroup != null) _removeAdsCanvasGroup.alpha = 1f;
            if (_removeAdsDialogRect != null) _removeAdsDialogRect.localScale = Vector3.one;
            if (_removeAdsTapContinueGroup != null) _removeAdsTapContinueGroup.alpha = 1f;
            _removeAdsAnimCoroutine = null;
        }

        public void OnConfirmBuyRemoveAdsClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayRevive();
            if (_removeAdsBuyBtnRect != null && Application.isPlaying) StartCoroutine(CoPunch(_removeAdsBuyBtnRect));

            PlayerPrefs.SetInt(PrefNoAdsKey, 1);

            int newCoins = (PlayerPrefs.HasKey(PrefCoinsKey) ? PlayerPrefs.GetInt(PrefCoinsKey, DefaultCoins) : DefaultCoins) + _removeAdsBonusCoins;
            PlayerPrefs.SetInt(PrefCoinsKey, newCoins);

            int freeze = (PlayerPrefs.HasKey(PrefBoosterFreezeKey) ? PlayerPrefs.GetInt(PrefBoosterFreezeKey, 3) : 3) + _removeAdsBonusBoosterCount;
            int magnet = (PlayerPrefs.HasKey(PrefBoosterMagnetKey) ? PlayerPrefs.GetInt(PrefBoosterMagnetKey, 2) : 2) + _removeAdsBonusBoosterCount;
            int wand = (PlayerPrefs.HasKey(PrefBoosterWandKey) ? PlayerPrefs.GetInt(PrefBoosterWandKey, 1) : 1) + _removeAdsBonusBoosterCount;
            PlayerPrefs.SetInt(PrefBoosterFreezeKey, freeze);
            PlayerPrefs.SetInt(PrefBoosterMagnetKey, magnet);
            PlayerPrefs.SetInt(PrefBoosterWandKey, wand);
            PlayerPrefs.Save();

            RefreshHomeDisplay();
            SetRemoveAdsOpen(false, animate: false);
            ShowToast($"Đã mở khóa Remove Ads! +{_removeAdsBonusCoins} Xu & +{_removeAdsBonusBoosterCount} mỗi Booster!");
        }

        private void BuildRemoveAdsPanel(Font fallbackFont)
        {
            Font displayFont = GetResolvedLevelNumberFont() ?? fallbackFont;
            FontStyle displayStyle = GetResolvedLevelNumberFontStyle(displayFont);

            _removeAdsPanel = CreateContainer("RemoveAdsPanel", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _removeAdsCanvasGroup = _removeAdsPanel.AddComponent<CanvasGroup>();
            _removeAdsCanvasGroup.alpha = 1f;

            Image bgOverlay = _removeAdsPanel.AddComponent<Image>();
            bgOverlay.color = new Color(0.03f, 0.02f, 0.12f, 0.78f);
            Button backdropBtn = _removeAdsPanel.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(() => SetRemoveAdsOpen(false));

            GameObject dialog = CreateContainer("DialogRoot", _removeAdsPanel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 44f), new Vector2(860f, 1146f));
            _removeAdsDialogRect = dialog.GetComponent<RectTransform>();

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();
            Sprite cardSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_card_bg")
                ?? GemSpriteManager.GetLoseHelperSprite("lose_card_frame")
                ?? SpriteGenerator.GetLoseCardFrameSprite();
            Sprite shadowSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_shadow_blob") ?? SpriteGenerator.GetRemoveAdsShadowBlobSprite();
            Sprite rewardBarSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_reward_bar") ?? SpriteGenerator.GetRemoveAdsRewardBarSprite();
            Sprite miniBadgeSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_badge") ?? SpriteGenerator.GetRemoveAdsMiniBadgeSprite();
            Sprite goldBtnSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_gold_btn") ?? SpriteGenerator.GetRemoveAdsGoldButtonSprite();

            Sprite freezeSp = GemSpriteManager.GetBoosterSprite("booster1");
            Sprite magnetSp = GemSpriteManager.GetBoosterSprite("booster2");
            Sprite wandSp = GemSpriteManager.GetBoosterSprite("booster3");
            Sprite coinsSp = GemSpriteManager.GetWinSceneSprite("Coins");

            Image bottomGlow = CreateImage("BottomGlow", dialog.transform, new Vector2(0f, -536f), new Vector2(380f, 120f),
                glowSp, new Color(0.84f, 0.36f, 1.0f, 0.65f), preserveAspect: false);
            _removeAdsBottomGlowRect = bottomGlow.rectTransform;

            GameObject cardBgObj = CreatePanel("CardBg", dialog.transform, Vector2.zero, Vector2.zero, cardSp, preserveAspect: true,
                anchorMin: Vector2.zero, anchorMax: Vector2.one);
            Image cardBgImg = cardBgObj.GetComponent<Image>();
            cardBgImg.raycastTarget = true;

            // Top-right close X button
            GameObject closeObj = CreatePanel("Btn_Close", dialog.transform, new Vector2(354f, 412f), new Vector2(96f, 96f),
                GemSpriteManager.GetSettingSprite("X (1)"), preserveAspect: true);
            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeObj.GetComponent<Image>();
            closeBtn.onClick.AddListener(() => SetRemoveAdsOpen(false));

            // Hero Boosters standing on the rock pedestal (replacing the 3 old boosters)
            GameObject heroRoot = CreateContainer("HeroBoostersRoot", dialog.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 1146f));

            CreateImage("Shadow_Freeze", heroRoot.transform, new Vector2(-186f, 8f), new Vector2(142f, 42f), shadowSp, Color.white, preserveAspect: false);
            Image heroFreeze = CreateImage("HeroBooster_Freeze", heroRoot.transform, new Vector2(-186f, 88f), new Vector2(158f, 158f), freezeSp, Color.white, preserveAspect: true);
            heroFreeze.rectTransform.localEulerAngles = new Vector3(0f, 0f, 8f);
            _removeAdsHeroFreezeRect = heroFreeze.rectTransform;

            CreateImage("Shadow_Wand", heroRoot.transform, new Vector2(204f, 174f), new Vector2(110f, 34f), shadowSp, Color.white, preserveAspect: false);
            Image heroWand = CreateImage("HeroBooster_Wand", heroRoot.transform, new Vector2(204f, 242f), new Vector2(146f, 146f), wandSp, Color.white, preserveAspect: true);
            heroWand.rectTransform.localEulerAngles = new Vector3(0f, 0f, -14f);
            _removeAdsHeroWandRect = heroWand.rectTransform;

            CreateImage("Shadow_Magnet", heroRoot.transform, new Vector2(184f, 6f), new Vector2(148f, 44f), shadowSp, Color.white, preserveAspect: false);
            Image heroMagnet = CreateImage("HeroBooster_Magnet", heroRoot.transform, new Vector2(184f, 88f), new Vector2(156f, 156f), magnetSp, Color.white, preserveAspect: true);
            heroMagnet.rectTransform.localEulerAngles = new Vector3(0f, 0f, -4f);
            _removeAdsHeroMagnetRect = heroMagnet.rectTransform;

            CreateImage("Sparkle_0", heroRoot.transform, new Vector2(-210f, 250f), new Vector2(30f, 30f), flareSp, new Color(1.0f, 0.84f, 1.0f, 0.90f), preserveAspect: true);
            CreateImage("Sparkle_1", heroRoot.transform, new Vector2(165f, 305f), new Vector2(28f, 28f), flareSp, new Color(1.0f, 0.92f, 0.72f, 0.90f), preserveAspect: true);
            CreateImage("Sparkle_2", heroRoot.transform, new Vector2(-285f, -385f), new Vector2(32f, 32f), flareSp, new Color(1.0f, 0.65f, 0.98f, 0.85f), preserveAspect: true);
            CreateImage("Sparkle_3", heroRoot.transform, new Vector2(285f, -385f), new Vector2(32f, 32f), flareSp, new Color(1.0f, 0.65f, 0.98f, 0.85f), preserveAspect: true);

            // Dark-indigo Reward Capsule Bar
            GameObject rewardBarObj = CreatePanel("RewardBar", dialog.transform, new Vector2(0f, -124f), new Vector2(676f, 174f),
                rewardBarSp, preserveAspect: false);
            Image rewardBarImg = rewardBarObj.GetComponent<Image>();
            rewardBarImg.type = Image.Type.Sliced;
            rewardBarImg.raycastTarget = false;

            Image miniBadge = CreateImage("NoAdsMiniBadge", rewardBarObj.transform, new Vector2(-252f, 0f), new Vector2(108f, 108f),
                miniBadgeSp, Color.white, preserveAspect: true);
            Text miniAdsTxt = CreateStyledText("AdsText", miniBadge.transform, "ADS", displayFont, 32,
                Color.white, new Vector2(0f, -1f), new Vector2(100f, 52f),
                new Color(0.18f, 0.02f, 0.08f, 0.94f), new Vector2(0f, -2f));
            miniAdsTxt.fontStyle = displayStyle;
            miniAdsTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            miniAdsTxt.verticalOverflow = VerticalWrapMode.Overflow;

            Image divider = CreateImage("VerticalDivider", rewardBarObj.transform, new Vector2(-176f, 0f), new Vector2(3f, 106f),
                null, new Color(0.58f, 0.50f, 0.90f, 0.55f), preserveAspect: false);
            divider.raycastTarget = false;

            string qtyBooster = "x" + _removeAdsBonusBoosterCount;
            string qtyCoins = "x" + _removeAdsBonusCoins;
            BuildRemoveAdsRewardSlot(rewardBarObj.transform, "Item_Wand", -104f, wandSp, new Vector2(82f, 82f), qtyBooster, displayFont, displayStyle);
            BuildRemoveAdsRewardSlot(rewardBarObj.transform, "Item_Freeze", 10f, freezeSp, new Vector2(80f, 80f), qtyBooster, displayFont, displayStyle);
            BuildRemoveAdsRewardSlot(rewardBarObj.transform, "Item_Magnet", 124f, magnetSp, new Vector2(80f, 80f), qtyBooster, displayFont, displayStyle);
            BuildRemoveAdsRewardSlot(rewardBarObj.transform, "Item_Coins", 248f, coinsSp, new Vector2(100f, 82f), qtyCoins, displayFont, displayStyle);

            // "No forced Ads Forever" subtitle row with thin flanking lines
            GameObject subRow = CreateContainer("SubtitleRow", dialog.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -242f), new Vector2(650f, 46f));
            CreateImage("LineLeft", subRow.transform, new Vector2(-238f, 0f), new Vector2(84f, 2.5f),
                null, new Color(0.56f, 0.50f, 0.88f, 0.58f), preserveAspect: false);
            CreateImage("LineRight", subRow.transform, new Vector2(238f, 0f), new Vector2(84f, 2.5f),
                null, new Color(0.56f, 0.50f, 0.88f, 0.58f), preserveAspect: false);
            Text subTxt = CreateStyledText("SubtitleText", subRow.transform, "No forced Ads <color=#FFD63E>Forever</color>", displayFont, 29,
                Color.white, Vector2.zero, new Vector2(460f, 48f),
                new Color(0.06f, 0.02f, 0.18f, 0.92f), new Vector2(0f, -2f));
            subTxt.fontStyle = displayStyle;
            subTxt.supportRichText = true;
            subTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            subTxt.verticalOverflow = VerticalWrapMode.Overflow;

            // Golden octagonal Buy button: ₫264,000
            GameObject buyObj = CreatePanel("Btn_BuyRemoveAds", dialog.transform, new Vector2(0f, -346f), new Vector2(512f, 134f),
                goldBtnSp, preserveAspect: false);
            Image buyImg = buyObj.GetComponent<Image>();
            buyImg.type = Image.Type.Sliced;
            _removeAdsBuyBtnRect = buyObj.GetComponent<RectTransform>();
            Button buyBtn = buyObj.AddComponent<Button>();
            buyBtn.targetGraphic = buyImg;
            buyBtn.onClick.AddListener(OnConfirmBuyRemoveAdsClicked);

            GameObject buyShineClip = CreateContainer("ShineClip", buyObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470f, 120f));
            buyShineClip.AddComponent<RectMask2D>();
            Image buyShine = CreateImage("ShineSweep", buyShineClip.transform, new Vector2(-340f, 0f), new Vector2(140f, 200f),
                shineSp, new Color(1f, 1f, 1f, 0.36f), preserveAspect: false);
            _removeAdsBuyShineRect = buyShine.rectTransform;

            _removeAdsPriceLabel = CreateStyledText("PriceText", buyObj.transform, _removeAdsPriceText, displayFont, 54,
                new Color(0.357f, 0.165f, 0.016f, 1f), new Vector2(0f, 5f), new Vector2(450f, 88f),
                new Color(1f, 0.90f, 0.45f, 0.55f), new Vector2(0f, -2f));
            _removeAdsPriceLabel.fontStyle = displayStyle;
            _removeAdsPriceLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            _removeAdsPriceLabel.verticalOverflow = VerticalWrapMode.Overflow;

            // TAP TO CONTINUE below modal frame
            GameObject tapObj = CreateContainer("Btn_TapToContinue", _removeAdsPanel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -622f), new Vector2(660f, 64f));
            Image tapHit = tapObj.AddComponent<Image>();
            tapHit.color = new Color(0f, 0f, 0f, 0f);
            Button tapBtn = tapObj.AddComponent<Button>();
            tapBtn.transition = Selectable.Transition.None;
            tapBtn.onClick.AddListener(() => SetRemoveAdsOpen(false));
            _removeAdsTapContinueGroup = tapObj.AddComponent<CanvasGroup>();

            CreateImage("LineLeft", tapObj.transform, new Vector2(-242f, 0f), new Vector2(96f, 2.5f),
                null, new Color(0.55f, 0.50f, 0.88f, 0.50f), preserveAspect: false);
            CreateImage("LineRight", tapObj.transform, new Vector2(242f, 0f), new Vector2(96f, 2.5f),
                null, new Color(0.55f, 0.50f, 0.88f, 0.50f), preserveAspect: false);
            Text tapTxt = CreateStyledText("TapLabel", tapObj.transform, "TAP TO CONTINUE", displayFont, 31,
                new Color(0.635f, 0.600f, 0.910f, 1f), Vector2.zero, new Vector2(360f, 52f),
                new Color(0.04f, 0.01f, 0.14f, 0.85f), new Vector2(0f, -2f));
            tapTxt.fontStyle = displayStyle;
            tapTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            tapTxt.verticalOverflow = VerticalWrapMode.Overflow;

            _removeAdsPanel.SetActive(false);
        }

        private static void BuildRemoveAdsRewardSlot(Transform barParent, string name, float posX, Sprite iconSp, Vector2 iconSize,
            string qtyText, Font displayFont, FontStyle displayStyle)
        {
            GameObject slot = CreateContainer(name, barParent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, 0f), new Vector2(114f, 160f));
            CreateImage("Icon", slot.transform, new Vector2(0f, 22f), iconSize, iconSp, Color.white, preserveAspect: true);
            Text qty = CreateStyledText("QtyText", slot.transform, qtyText, displayFont, 32,
                Color.white, new Vector2(0f, -48f), new Vector2(124f, 56f),
                new Color(0.06f, 0.02f, 0.16f, 0.92f), new Vector2(0f, -2.5f));
            qty.fontStyle = displayStyle;
            qty.horizontalOverflow = HorizontalWrapMode.Overflow;
            qty.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void UpdateRemoveAdsRewardLabels(Transform rewardBar)
        {
            if (rewardBar == null) return;
            string qtyB = "x" + _removeAdsBonusBoosterCount;
            Text wandQty = rewardBar.Find("Item_Wand/QtyText")?.GetComponent<Text>();
            if (wandQty != null) wandQty.text = qtyB;
            Text freezeQty = rewardBar.Find("Item_Freeze/QtyText")?.GetComponent<Text>();
            if (freezeQty != null) freezeQty.text = qtyB;
            Text magnetQty = rewardBar.Find("Item_Magnet/QtyText")?.GetComponent<Text>();
            if (magnetQty != null) magnetQty.text = qtyB;
            Text coinsQty = rewardBar.Find("Item_Coins/QtyText")?.GetComponent<Text>();
            if (coinsQty != null) coinsQty.text = "x" + _removeAdsBonusCoins;
        }

        private void BindRemoveAdsPanel(Transform panelTr)
        {
            if (panelTr == null) return;
            _removeAdsCanvasGroup = panelTr.GetComponent<CanvasGroup>();
            RebindButton(panelTr.GetComponent<Button>(), () => SetRemoveAdsOpen(false));

            Transform dialog = panelTr.Find("DialogRoot");
            _removeAdsDialogRect = dialog as RectTransform;
            if (dialog != null)
            {
                _removeAdsBottomGlowRect = dialog.Find("BottomGlow") as RectTransform;
                RebindButton(dialog.Find("Btn_Close")?.GetComponent<Button>(), () => SetRemoveAdsOpen(false));

                Transform heroRoot = dialog.Find("HeroBoostersRoot");
                if (heroRoot != null)
                {
                    _removeAdsHeroFreezeRect = heroRoot.Find("HeroBooster_Freeze") as RectTransform;
                    _removeAdsHeroWandRect = heroRoot.Find("HeroBooster_Wand") as RectTransform;
                    _removeAdsHeroMagnetRect = heroRoot.Find("HeroBooster_Magnet") as RectTransform;
                }

                Transform buyTr = dialog.Find("Btn_BuyRemoveAds");
                _removeAdsBuyBtnRect = buyTr as RectTransform;
                _removeAdsBuyShineRect = buyTr?.Find("ShineClip/ShineSweep") as RectTransform;
                _removeAdsPriceLabel = buyTr?.Find("PriceText")?.GetComponent<Text>();
                RebindButton(buyTr?.GetComponent<Button>(), OnConfirmBuyRemoveAdsClicked);
            }

            Transform tapTr = panelTr.Find("Btn_TapToContinue");
            _removeAdsTapContinueGroup = tapTr?.GetComponent<CanvasGroup>();
            RebindButton(tapTr?.GetComponent<Button>(), () => SetRemoveAdsOpen(false));

            EnsureRemoveAdsPanelSprites();
            _removeAdsPanel.SetActive(false);
        }

        private void EnsureRemoveAdsPanelSprites()
        {
            if (_removeAdsPanel == null) return;
            Transform dialog = _removeAdsPanel.transform.Find("DialogRoot");
            if (dialog == null) return;

            Sprite glowSp = GemSpriteManager.GetWinHelperSprite("win_radial_glow") ?? SpriteGenerator.GetRadialGlowSprite();
            Sprite flareSp = GemSpriteManager.GetWinHelperSprite("win_star_flare") ?? SpriteGenerator.GetStarFlareSprite();
            Sprite shineSp = GemSpriteManager.GetWinHelperSprite("win_shine_sweep") ?? SpriteGenerator.GetWinShineSweepSprite();
            Sprite cardSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_card_bg")
                ?? GemSpriteManager.GetLoseHelperSprite("lose_card_frame")
                ?? SpriteGenerator.GetLoseCardFrameSprite();
            Sprite shadowSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_shadow_blob") ?? SpriteGenerator.GetRemoveAdsShadowBlobSprite();
            Sprite rewardBarSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_reward_bar") ?? SpriteGenerator.GetRemoveAdsRewardBarSprite();
            Sprite miniBadgeSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_badge") ?? SpriteGenerator.GetRemoveAdsMiniBadgeSprite();
            Sprite goldBtnSp = GemSpriteManager.GetRemoveAdsSprite("remove_ads_gold_btn") ?? SpriteGenerator.GetRemoveAdsGoldButtonSprite();

            Sprite freezeSp = GemSpriteManager.GetBoosterSprite("booster1");
            Sprite magnetSp = GemSpriteManager.GetBoosterSprite("booster2");
            Sprite wandSp = GemSpriteManager.GetBoosterSprite("booster3");
            Sprite coinsSp = GemSpriteManager.GetWinSceneSprite("Coins");

            SetImageSprite(dialog.Find("BottomGlow"), glowSp);
            SetImageSprite(dialog.Find("CardBg"), cardSp);
            SetImageSprite(dialog.Find("Btn_Close"), GemSpriteManager.GetSettingSprite("X (1)"));

            Transform heroRoot = dialog.Find("HeroBoostersRoot");
            if (heroRoot != null)
            {
                SetImageSprite(heroRoot.Find("Shadow_Freeze"), shadowSp);
                SetImageSprite(heroRoot.Find("HeroBooster_Freeze"), freezeSp);
                SetImageSprite(heroRoot.Find("Shadow_Wand"), shadowSp);
                SetImageSprite(heroRoot.Find("HeroBooster_Wand"), wandSp);
                SetImageSprite(heroRoot.Find("Shadow_Magnet"), shadowSp);
                SetImageSprite(heroRoot.Find("HeroBooster_Magnet"), magnetSp);
                for (int i = 0; i < 4; i++)
                {
                    SetImageSprite(heroRoot.Find("Sparkle_" + i), flareSp);
                }
            }

            Transform rewardBar = dialog.Find("RewardBar");
            if (rewardBar != null)
            {
                SetImageSprite(rewardBar, rewardBarSp, Image.Type.Sliced);
                SetImageSprite(rewardBar.Find("NoAdsMiniBadge"), miniBadgeSp);
                SetImageSprite(rewardBar.Find("Item_Wand/Icon"), wandSp);
                SetImageSprite(rewardBar.Find("Item_Freeze/Icon"), freezeSp);
                SetImageSprite(rewardBar.Find("Item_Magnet/Icon"), magnetSp);
                SetImageSprite(rewardBar.Find("Item_Coins/Icon"), coinsSp);
            }

            SetImageSprite(dialog.Find("Btn_BuyRemoveAds"), goldBtnSp, Image.Type.Sliced);
            SetImageSprite(dialog.Find("Btn_BuyRemoveAds/ShineClip/ShineSweep"), shineSp);
        }

        private void OnShopOfferClicked(string title, int coinGrant, Transform btnTr)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayRevive();
            if (btnTr != null && Application.isPlaying) StartCoroutine(CoPunch(btnTr));

            int coins = PlayerPrefs.GetInt(PrefCoinsKey, DefaultCoins) + coinGrant;
            PlayerPrefs.SetInt(PrefCoinsKey, coins);
            PlayerPrefs.Save();
            RefreshHomeDisplay();
            ShowToast($"{title}: +{coinGrant} Xu!");
        }

        private void OnCollectionLevelSelected(int levelNumber)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            _previewLevelOverride = 0;
            PlayerPrefs.SetInt(PrefCurrentLevelKey, levelNumber);
            PlayerPrefs.Save();
            SetActiveTab(1, playSound: false);
            RefreshHomeDisplay();
            ShowToast($"Đã chọn Level {levelNumber}!");
        }

        private void OnSettingsClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButton();
            if (_settingsBtn != null && Application.isPlaying) StartCoroutine(CoPunch(_settingsBtn.transform));
            SetSettingsOpen(true);
        }

        private void SetSettingsOpen(bool open)
        {
            if (_settingsPanel != null) _settingsPanel.SetActive(open);
        }

        private void ToggleSound()
        {
            _soundEnabled = !_soundEnabled;
            PlayerPrefs.SetInt(PrefSoundKey, _soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SoundEnabled = _soundEnabled;
                if (_soundEnabled) SoundManager.Instance.PlayButton();
            }
            UpdateSettingViews();
        }

        private void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;
            PlayerPrefs.SetInt(PrefMusicKey, _musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null && _soundEnabled) SoundManager.Instance.PlayButton();
            UpdateSettingViews();
        }

        private void ToggleVibration()
        {
            _vibrationEnabled = !_vibrationEnabled;
            PlayerPrefs.SetInt(PrefVibrationKey, _vibrationEnabled ? 1 : 0);
            PlayerPrefs.Save();
            if (SoundManager.Instance != null && _soundEnabled) SoundManager.Instance.PlayButton();
            UpdateSettingViews();
        }

        private void UpdateSettingViews()
        {
            if (_soundIconImg != null)
            {
                _soundIconImg.sprite = GemSpriteManager.GetSettingSprite(_soundEnabled ? "Vector 389" : "Vector 390");
                _soundIconImg.preserveAspect = true;
            }
            if (_soundToggleImg != null)
            {
                _soundToggleImg.sprite = GemSpriteManager.GetSettingSprite(_soundEnabled ? "Group 334" : "Group 333");
                _soundToggleImg.preserveAspect = true;
            }
            if (_musicIconImg != null)
            {
                _musicIconImg.sprite = GemSpriteManager.GetSettingSprite(_musicEnabled ? "Vector 392" : "Group 332");
                _musicIconImg.preserveAspect = true;
            }
            if (_musicToggleImg != null)
            {
                _musicToggleImg.sprite = GemSpriteManager.GetSettingSprite(_musicEnabled ? "Group 334" : "Group 333");
                _musicToggleImg.preserveAspect = true;
            }
            if (_vibrationIconImg != null)
            {
                _vibrationIconImg.sprite = GemSpriteManager.GetSettingSprite(_vibrationEnabled ? "Vector 394" : "Vector 395");
                _vibrationIconImg.preserveAspect = true;
            }
            if (_vibrationToggleImg != null)
            {
                _vibrationToggleImg.sprite = GemSpriteManager.GetSettingSprite(_vibrationEnabled ? "Group 334" : "Group 333");
                _vibrationToggleImg.preserveAspect = true;
            }
        }

        public void ShowToast(string message)
        {
            if (_feedbackBanner == null || _feedbackText == null || _feedbackCanvasGroup == null) return;
            _feedbackText.text = message;
            if (!Application.isPlaying) return;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(CoToast());
        }

        private IEnumerator CoToast()
        {
            _feedbackBanner.transform.SetAsLastSibling();
            _feedbackCanvasGroup.alpha = 1f;
            float elapsed = 0f;
            while (elapsed < 0.20f)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin((elapsed / 0.20f) * Mathf.PI) * 0.08f;
                _feedbackBanner.transform.localScale = Vector3.one * s;
                yield return null;
            }
            _feedbackBanner.transform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(1.8f);

            float fade = 0f;
            while (fade < 0.30f)
            {
                fade += Time.unscaledDeltaTime;
                _feedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fade / 0.30f);
                yield return null;
            }
            _feedbackCanvasGroup.alpha = 0f;
            _feedbackCoroutine = null;
        }

        private IEnumerator CoPunch(Transform tr)
        {
            if (tr == null) yield break;
            float elapsed = 0f;
            const float dur = 0.14f;
            while (elapsed < dur && tr != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = 1f - Mathf.Sin((elapsed / dur) * Mathf.PI) * 0.10f;
                tr.localScale = Vector3.one * s;
                yield return null;
            }
            if (tr != null) tr.localScale = Vector3.one;
        }

        private IEnumerator CoShake(RectTransform rt)
        {
            if (rt == null) yield break;
            Vector2 orig = rt.anchoredPosition;
            float elapsed = 0f;
            const float dur = 0.26f;
            while (elapsed < dur && rt != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / dur;
                rt.anchoredPosition = orig + new Vector2(Mathf.Sin(t * Mathf.PI * 6f) * 14f * (1f - t), 0f);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = orig;
        }

        private static string FormatTimeMMSS(float seconds)
        {
            float clamped = Mathf.Max(0f, seconds);
            int mins = Mathf.FloorToInt(clamped / 60f);
            int secs = Mathf.FloorToInt(clamped % 60f);
            return string.Format("{0:D2}:{1:D2}", mins, secs);
        }

        private static void RebindButton(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null || action == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        private static void SetImageSprite(Transform tr, Sprite sprite, Image.Type imgType = Image.Type.Simple)
        {
            if (tr == null || sprite == null) return;
            Image img = tr.GetComponent<Image>();
            if (img == null) return;
            img.sprite = sprite;
            img.type = imgType;
        }

        private static GameObject CreateContainer(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return obj;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta,
            Sprite sprite, bool preserveAspect,
            Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin ?? new Vector2(0.5f, 0.5f);
            rt.anchorMax = anchorMax ?? new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = preserveAspect;
            return obj;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta,
            Sprite sprite, Color color, bool preserveAspect)
        {
            GameObject obj = CreatePanel(name, parent, anchoredPos, sizeDelta, sprite, preserveAspect);
            Image img = obj.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private static Text CreateStyledText(string name, Transform parent, string content, Font font, int fontSize,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, Color shadowColor, Vector2 shadowDist)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;
            txt.raycastTarget = false;

            Shadow sh = obj.AddComponent<Shadow>();
            sh.effectColor = shadowColor;
            sh.effectDistance = shadowDist;
            return txt;
        }

        private static Text CreateOutlinedText(string name, Transform parent, string content, Font font, int fontSize,
            Color color, Vector2 anchoredPos, Vector2 sizeDelta, Color outlineColor, Vector2 outlineDist, TextAnchor alignment)
        {
            Text txt = CreateStyledText(name, parent, content, font, fontSize, color, anchoredPos, sizeDelta,
                new Color(outlineColor.r * 0.5f, outlineColor.g * 0.5f, outlineColor.b * 0.5f, 0.90f),
                new Vector2(0f, -Mathf.Max(2f, Mathf.Abs(outlineDist.y) * 1.4f)));
            txt.alignment = alignment;

            Outline ol = txt.gameObject.AddComponent<Outline>();
            ol.effectColor = outlineColor;
            ol.effectDistance = outlineDist;
            return txt;
        }

        private Font GetResolvedLevelNumberFont()
        {
            return _levelNumberFontOverride != null ? _levelNumberFontOverride : GetLevelNumberFont();
        }

        private FontStyle GetResolvedLevelNumberFontStyle(Font font)
        {
            if (font != null &&
                (font.name.IndexOf("LegacyRuntime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 font.name.IndexOf("Arial", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return FontStyle.Bold;
            }
            return _levelNumberFontStyle;
        }

        private int GetLadderNodeBaseFontSize(int nodeIndex)
        {
            switch (nodeIndex)
            {
                case 0: return _activeNodeFontSize;
                case 1: return _node1FontSize;
                case 2: return _node2FontSize;
                default: return _node3FontSize;
            }
        }

        private int ComputeLevelFontSize(int baseSize, int levelNumber)
        {
            float digitFactor = 1f;
            if (levelNumber >= 100) digitFactor = _multiDigitScale * _multiDigitScale;
            else if (levelNumber >= 10) digitFactor = _multiDigitScale;
            return Mathf.Clamp(Mathf.RoundToInt(baseSize * _allLevelNumbersScale * digitFactor), 12, 220);
        }

        private static Font GetLevelNumberFont()
        {
            Font custom = Resources.Load<Font>("Fonts/HomeLevelFont");
#if UNITY_EDITOR
            if (custom == null)
            {
                custom = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/HomeLevelFont.ttf");
            }
#endif
            if (custom != null) return custom;

            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return fallback != null ? fallback : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Text CreateLevelNumberText(string name, Transform parent, string content, Font font, int fontSize,
            Vector2 anchoredPos, Vector2 sizeDelta, Color shadowColor, Vector2 shadowDist, out Shadow shadowOut)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = fontSize;
            bool isCustomDisplay = font != null && font.name.IndexOf("HomeLevelFont", StringComparison.OrdinalIgnoreCase) >= 0;
            txt.fontStyle = isCustomDisplay ? FontStyle.Normal : FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.color = new Color(1.0f, 0.996f, 0.988f, 1.0f);
            txt.raycastTarget = false;

            Shadow sh = obj.AddComponent<Shadow>();
            sh.effectColor = shadowColor;
            sh.effectDistance = shadowDist;
            sh.useGraphicAlpha = true;
            shadowOut = sh;
            return txt;
        }

        private static Color GetLevelNumberShadowColor(HomeLevelDifficulty diff, bool isActive)
        {
            switch (diff)
            {
                case HomeLevelDifficulty.Hard:
                    return new Color(0.55f, 0.14f, 0.04f, 0.72f);
                case HomeLevelDifficulty.SuperHard:
                    return new Color(0.24f, 0.06f, 0.46f, 0.74f);
                default:
                    return isActive
                        ? new Color(0.11f, 0.40f, 0.06f, 0.72f)
                        : new Color(0.22f, 0.17f, 0.48f, 0.65f);
            }
        }

        private static Color GetTopFadedNumberColor(HomeLevelDifficulty diff)
        {
            switch (diff)
            {
                case HomeLevelDifficulty.Hard:
                    return new Color(0.38f, 0.12f, 0.09f, 0.88f);
                case HomeLevelDifficulty.SuperHard:
                    return new Color(0.22f, 0.08f, 0.38f, 0.88f);
                default:
                    return new Color(0.20f, 0.15f, 0.42f, 0.92f);
            }
        }

        private static Color GetTopFadedTileTint(HomeLevelDifficulty diff)
        {
            switch (diff)
            {
                case HomeLevelDifficulty.Hard:
                    return new Color(0.76f, 0.62f, 0.72f, 1.0f);
                case HomeLevelDifficulty.SuperHard:
                    return new Color(0.68f, 0.56f, 0.86f, 1.0f);
                default:
                    return new Color(0.68f, 0.62f, 0.86f, 1.0f);
            }
        }
    }
}
