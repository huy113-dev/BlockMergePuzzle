using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.UI
{
    public class WandBoosterController : MonoBehaviour
    {
        public enum WandPhase
        {
            Idle,
            SelectColor,
            Executing
        }

        private Canvas _rootCanvas;
        private RectTransform _rootRect;

        // First-time Level 16 tutorial overlay
        private GameObject _tutorialRoot;
        private Canvas _tutorialCanvas;
        private CanvasGroup _tutorialCanvasGroup;
        private RectTransform _tutorialSpotlightGlowRect;
        private Image _tutorialSpotlightGlowImg;
        private RectTransform _tutorialCardRect;
        private RectTransform _tutorialCardIconRect;
        private Image _tutorialCardIconImg;
        private GameObject _tutorialHandLayer;
        private Canvas _tutorialHandCanvas;
        private RectTransform _tutorialHandAnchorRect;
        private RectTransform _tutorialHandImageRect;
        private RectTransform _tutorialTapRippleRect;
        private Image _tutorialTapRippleImg;
        private RectTransform _spotlightButtonRect;
        private Canvas _addedSpotlightCanvas;
        private GraphicRaycaster _addedSpotlightRaycaster;
        private Coroutine _tutorialIntroCoroutine;
        private float _tutorialAnimTime = 0f;

        // Targeting prompt overlay
        private GameObject _promptRoot;
        private CanvasGroup _promptCanvasGroup;
        private RectTransform _promptCardRect;
        private Image _promptIconImg;
        private Text _promptTitleText;
        private Text _promptSubText;
        private RectTransform _buttonHaloRect;
        private Image _buttonHaloImg;
        private RectTransform _wandButtonRect;

        // UI spell effect layer
        private RectTransform _effectLayer;
        private Canvas _effectCanvas;
        private CanvasGroup _effectCanvasGroup;

        // World-space targeting & FX
        private WandPhase _phase = WandPhase.Idle;
        private Sprite _wandIconSprite;
        private Action _onConsumeCharge;
        private struct ElevatedRendererEntry
        {
            public Renderer Renderer;
            public int OriginalSortingOrder;
        }

        private GameObject _boardDimOverlay;
        private readonly List<ElevatedRendererEntry> _elevatedRenderers = new List<ElevatedRendererEntry>();
        private readonly List<GameObject> _transientWorldObjects = new List<GameObject>();
        private readonly List<GameObject> _transientUiObjects = new List<GameObject>();
        private Coroutine _executionCoroutine;
        private float _fxAnimTime = 0f;
        private bool _isDestroying = false;

        public bool IsTutorialShowing => _tutorialRoot != null && _tutorialRoot.activeSelf;
        public bool IsTargeting => _phase == WandPhase.SelectColor;
        public bool IsExecuting => _phase == WandPhase.Executing;
        public bool IsBusy => IsTutorialShowing || IsTargeting || IsExecuting;

        private void Awake()
        {
            EnsureRootReferences();
        }

        private void OnDestroy()
        {
            _isDestroying = true;
            StopImmediate();
        }

        private void EnsureRootReferences()
        {
            if (_rootCanvas == null)
            {
                _rootCanvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
            }
            if (_rootRect == null)
            {
                _rootRect = _rootCanvas != null
                    ? _rootCanvas.GetComponent<RectTransform>()
                    : GetComponent<RectTransform>();
            }
        }

        private Camera GetCanvasCamera()
        {
            EnsureRootReferences();
            if (_rootCanvas == null) return null;
            return _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;
        }

        private Vector2 WorldRectCenterToLocal(RectTransform sourceRect, RectTransform targetParent)
        {
            if (sourceRect == null || targetParent == null) return Vector2.zero;
            Camera cam = GetCanvasCamera();
            Vector3 worldCenter = sourceRect.TransformPoint(sourceRect.rect.center);
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPt, cam, out Vector2 localPt);
            return localPt;
        }

        private Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        public void ShowFirstTimeTutorial(RectTransform wandButtonRect, Sprite wandIconSprite)
        {
            if (wandButtonRect == null || IsTutorialShowing) return;

            EnsureRootReferences();
            if (wandIconSprite != null) _wandIconSprite = wandIconSprite;
            EnsureTutorialOverlay(_wandIconSprite);

            _tutorialRoot.SetActive(true);
            _tutorialRoot.transform.SetAsLastSibling();

            int rootOrder = _rootCanvas != null ? _rootCanvas.sortingOrder : 0;
            int rootLayerId = _rootCanvas != null ? _rootCanvas.sortingLayerID : 0;

            if (_tutorialCanvas != null)
            {
                _tutorialCanvas.overrideSorting = true;
                _tutorialCanvas.sortingLayerID = rootLayerId;
                _tutorialCanvas.sortingOrder = rootOrder + 20;
            }
            if (_tutorialHandCanvas != null)
            {
                _tutorialHandCanvas.overrideSorting = true;
                _tutorialHandCanvas.sortingLayerID = rootLayerId;
                _tutorialHandCanvas.sortingOrder = rootOrder + 30;
            }

            // Elevate the existing Wand booster button without changing its sprite/shape/parent
            RemoveSpotlightComponents();
            _spotlightButtonRect = wandButtonRect;
            if (_spotlightButtonRect.GetComponent<Canvas>() == null)
            {
                _addedSpotlightCanvas = _spotlightButtonRect.gameObject.AddComponent<Canvas>();
                _addedSpotlightCanvas.overrideSorting = true;
                _addedSpotlightCanvas.sortingLayerID = rootLayerId;
                _addedSpotlightCanvas.sortingOrder = rootOrder + 25;
                if (_spotlightButtonRect.GetComponent<GraphicRaycaster>() == null)
                {
                    _addedSpotlightRaycaster = _spotlightButtonRect.gameObject.AddComponent<GraphicRaycaster>();
                }
            }
            else
            {
                Canvas existingCanvas = _spotlightButtonRect.GetComponent<Canvas>();
                existingCanvas.overrideSorting = true;
                existingCanvas.sortingLayerID = rootLayerId;
                existingCanvas.sortingOrder = rootOrder + 25;
            }

            Canvas.ForceUpdateCanvases();

            RectTransform tutRect = _tutorialRoot.GetComponent<RectTransform>();
            Vector2 btnLocal = WorldRectCenterToLocal(wandButtonRect, tutRect);
            if (_tutorialSpotlightGlowRect != null) _tutorialSpotlightGlowRect.anchoredPosition = btnLocal;
            if (_tutorialHandAnchorRect != null) _tutorialHandAnchorRect.anchoredPosition = btnLocal;
            if (_tutorialCardIconImg != null && _wandIconSprite != null) _tutorialCardIconImg.sprite = _wandIconSprite;

            _tutorialAnimTime = 0f;
            if (_tutorialIntroCoroutine != null) StopCoroutine(_tutorialIntroCoroutine);
            _tutorialIntroCoroutine = StartCoroutine(CoAnimateTutorialIntro());
        }

        public void DismissTutorial()
        {
            if (_tutorialIntroCoroutine != null)
            {
                StopCoroutine(_tutorialIntroCoroutine);
                _tutorialIntroCoroutine = null;
            }

            RemoveSpotlightComponents();

            if (_tutorialRoot != null)
            {
                _tutorialRoot.SetActive(false);
            }
        }

        private void RemoveSpotlightComponents()
        {
            if (_addedSpotlightRaycaster != null)
            {
                if (_isDestroying) Destroy(_addedSpotlightRaycaster);
                else DestroyImmediate(_addedSpotlightRaycaster);
                _addedSpotlightRaycaster = null;
            }
            if (_addedSpotlightCanvas != null)
            {
                if (_isDestroying) Destroy(_addedSpotlightCanvas);
                else DestroyImmediate(_addedSpotlightCanvas);
                _addedSpotlightCanvas = null;
            }
            _spotlightButtonRect = null;
        }

        private void EnsureTutorialOverlay(Sprite boosterIconSprite)
        {
            if (_tutorialRoot != null) return;

            Font font = GetDefaultFont();

            _tutorialRoot = new GameObject("WandTutorialOverlay", typeof(RectTransform));
            _tutorialRoot.transform.SetParent(transform, false);
            StretchFull(_tutorialRoot.GetComponent<RectTransform>());

            _tutorialCanvas = _tutorialRoot.AddComponent<Canvas>();
            _tutorialCanvas.overrideSorting = true;
            _tutorialRoot.AddComponent<GraphicRaycaster>();
            _tutorialCanvasGroup = _tutorialRoot.AddComponent<CanvasGroup>();
            _tutorialCanvasGroup.alpha = 0f;

            // Dark scrim blocking board interactions while focusing on Wand booster
            GameObject scrimObj = new GameObject("Scrim", typeof(RectTransform));
            scrimObj.transform.SetParent(_tutorialRoot.transform, false);
            StretchFull(scrimObj.GetComponent<RectTransform>());
            Image scrimImg = scrimObj.AddComponent<Image>();
            scrimImg.color = new Color(0.02f, 0.03f, 0.09f, 0.74f);
            scrimImg.raycastTarget = true;

            // Radial magenta-gold spotlight glow behind the Wand button
            GameObject glowObj = new GameObject("SpotlightGlow", typeof(RectTransform));
            glowObj.transform.SetParent(_tutorialRoot.transform, false);
            _tutorialSpotlightGlowRect = glowObj.GetComponent<RectTransform>();
            _tutorialSpotlightGlowRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.sizeDelta = new Vector2(340f, 340f);
            _tutorialSpotlightGlowImg = glowObj.AddComponent<Image>();
            _tutorialSpotlightGlowImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _tutorialSpotlightGlowImg.color = new Color(0.95f, 0.48f, 1.0f, 0.85f);
            _tutorialSpotlightGlowImg.raycastTarget = false;

            // White rounded tooltip card at upper-center
            GameObject cardObj = new GameObject("TooltipCard", typeof(RectTransform));
            cardObj.transform.SetParent(_tutorialRoot.transform, false);
            _tutorialCardRect = cardObj.GetComponent<RectTransform>();
            _tutorialCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialCardRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialCardRect.anchoredPosition = new Vector2(0f, 320f);
            _tutorialCardRect.sizeDelta = new Vector2(760f, 186f);

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.sprite = SpriteGenerator.GetFreezeTooltipCardSprite();
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;
            cardImg.raycastTarget = false;

            Shadow cardShadow = cardObj.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0.04f, 0.05f, 0.16f, 0.42f);
            cardShadow.effectDistance = new Vector2(0f, -10f);

            // Booster icon inside tooltip card (preserving user's booster3 sprite)
            GameObject cardIconObj = new GameObject("BoosterPreviewIcon", typeof(RectTransform));
            cardIconObj.transform.SetParent(cardObj.transform, false);
            _tutorialCardIconRect = cardIconObj.GetComponent<RectTransform>();
            _tutorialCardIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchoredPosition = new Vector2(-268f, 0f);
            _tutorialCardIconRect.sizeDelta = new Vector2(128f, 128f);

            _tutorialCardIconImg = cardIconObj.AddComponent<Image>();
            _tutorialCardIconImg.sprite = boosterIconSprite != null ? boosterIconSprite : GemSpriteManager.GetBoosterSprite("booster3");
            _tutorialCardIconImg.preserveAspect = true;
            _tutorialCardIconImg.raycastTarget = false;

            // Tooltip instruction text
            GameObject textObj = new GameObject("InstructionText", typeof(RectTransform));
            textObj.transform.SetParent(cardObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(70f, 0f);
            textRect.sizeDelta = new Vector2(500f, 146f);

            Text txt = textObj.AddComponent<Text>();
            txt.text = "Choose a Color to Eliminate";
            txt.font = font;
            txt.fontSize = 38;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 24;
            txt.resizeTextMaxSize = 40;
            txt.color = new Color(0.24f, 0.16f, 0.50f);
            txt.raycastTarget = false;

            // Animated hand pointer layer above spotlighted Wand button (non-blocking)
            _tutorialHandLayer = new GameObject("HandLayer", typeof(RectTransform));
            _tutorialHandLayer.transform.SetParent(_tutorialRoot.transform, false);
            StretchFull(_tutorialHandLayer.GetComponent<RectTransform>());
            _tutorialHandCanvas = _tutorialHandLayer.AddComponent<Canvas>();
            _tutorialHandCanvas.overrideSorting = true;

            GameObject handAnchorObj = new GameObject("HandAnchor", typeof(RectTransform));
            handAnchorObj.transform.SetParent(_tutorialHandLayer.transform, false);
            _tutorialHandAnchorRect = handAnchorObj.GetComponent<RectTransform>();
            _tutorialHandAnchorRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialHandAnchorRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialHandAnchorRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialHandAnchorRect.sizeDelta = new Vector2(200f, 200f);

            GameObject rippleObj = new GameObject("TapRipple", typeof(RectTransform));
            rippleObj.transform.SetParent(handAnchorObj.transform, false);
            _tutorialTapRippleRect = rippleObj.GetComponent<RectTransform>();
            _tutorialTapRippleRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialTapRippleRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialTapRippleRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialTapRippleRect.anchoredPosition = Vector2.zero;
            _tutorialTapRippleRect.sizeDelta = new Vector2(120f, 120f);
            _tutorialTapRippleImg = rippleObj.AddComponent<Image>();
            _tutorialTapRippleImg.sprite = SpriteGenerator.GetCircleSprite();
            _tutorialTapRippleImg.color = new Color(1.0f, 0.72f, 0.98f, 0f);
            _tutorialTapRippleImg.raycastTarget = false;

            GameObject handImgObj = new GameObject("HandSprite", typeof(RectTransform));
            handImgObj.transform.SetParent(handAnchorObj.transform, false);
            _tutorialHandImageRect = handImgObj.GetComponent<RectTransform>();
            _tutorialHandImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialHandImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialHandImageRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialHandImageRect.anchoredPosition = new Vector2(62f, -72f);
            _tutorialHandImageRect.sizeDelta = new Vector2(156f, 156f);

            Image handImg = handImgObj.AddComponent<Image>();
            handImg.sprite = GemSpriteManager.GetMainUISprite("UI_hand");
            handImg.preserveAspect = true;
            handImg.raycastTarget = false;

            _tutorialRoot.SetActive(false);
        }

        private IEnumerator CoAnimateTutorialIntro()
        {
            if (_tutorialCanvasGroup != null) _tutorialCanvasGroup.alpha = 0f;
            if (_tutorialCardRect != null) _tutorialCardRect.localScale = Vector3.one * 0.85f;

            float duration = 0.22f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (_tutorialCanvasGroup != null) _tutorialCanvasGroup.alpha = t;
                if (_tutorialCardRect != null) _tutorialCardRect.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.0f, EaseOutBack(t));
                yield return null;
            }

            if (_tutorialCanvasGroup != null) _tutorialCanvasGroup.alpha = 1f;
            if (_tutorialCardRect != null) _tutorialCardRect.localScale = Vector3.one;
            _tutorialIntroCoroutine = null;
        }

        public void BeginTargeting(RectTransform wandButtonRect, Sprite wandIconSprite, Action onConsumeCharge)
        {
            if (IsExecuting) return;

            DismissTutorial();
            StopTargetingOnly();

            _wandButtonRect = wandButtonRect;
            _wandIconSprite = wandIconSprite != null ? wandIconSprite : GemSpriteManager.GetBoosterSprite("booster3");
            _onConsumeCharge = onConsumeCharge;
            _phase = WandPhase.SelectColor;
            _fxAnimTime = 0f;

            EnsurePromptOverlay(_wandIconSprite);
            if (_promptIconImg != null && _wandIconSprite != null) _promptIconImg.sprite = _wandIconSprite;
            if (_promptTitleText != null) _promptTitleText.text = "Magic Wand";
            if (_promptSubText != null) _promptSubText.text = "Choose a Color to Eliminate";

            if (_promptRoot != null)
            {
                _promptRoot.SetActive(true);
                PositionAboveModals(_promptRoot.transform);
                if (_promptCanvasGroup != null) _promptCanvasGroup.alpha = 1f;
            }

            if (_buttonHaloRect != null && _wandButtonRect != null && _promptRoot != null)
            {
                _buttonHaloRect.gameObject.SetActive(true);
                _buttonHaloRect.anchoredPosition = WorldRectCenterToLocal(_wandButtonRect, _promptRoot.GetComponent<RectTransform>());
            }

            ApplySelectColorDimSpotlight();
        }

        public void CancelTargeting()
        {
            if (IsExecuting) return;
            StopTargetingOnly();
        }

        public void StopTargetingOnly()
        {
            if (_executionCoroutine != null)
            {
                StopCoroutine(_executionCoroutine);
                _executionCoroutine = null;
            }

            ClearTargetHighlights();
            ClearTransientWorldObjects();
            ClearTransientUiObjects();

            if (_buttonHaloRect != null) _buttonHaloRect.gameObject.SetActive(false);
            if (_promptRoot != null) _promptRoot.SetActive(false);

            _onConsumeCharge = null;
            _phase = WandPhase.Idle;
        }

        public void StopImmediate()
        {
            DismissTutorial();
            StopTargetingOnly();
        }

        private void PositionAboveModals(Transform tr)
        {
            if (tr == null) return;
            Transform boosterBar = transform.Find("BoosterBar");
            if (boosterBar != null)
            {
                tr.SetSiblingIndex(Mathf.Min(boosterBar.GetSiblingIndex() + 2, transform.childCount - 1));
            }
            else
            {
                Transform pausePanel = transform.Find("PausePanel");
                if (pausePanel != null)
                {
                    tr.SetSiblingIndex(Mathf.Max(0, pausePanel.GetSiblingIndex()));
                }
            }
        }

        private void EnsurePromptOverlay(Sprite iconSprite)
        {
            if (_promptRoot != null) return;

            Font font = GetDefaultFont();

            _promptRoot = new GameObject("WandPromptOverlay", typeof(RectTransform));
            _promptRoot.transform.SetParent(transform, false);
            StretchFull(_promptRoot.GetComponent<RectTransform>());

            _promptCanvasGroup = _promptRoot.AddComponent<CanvasGroup>();
            _promptCanvasGroup.alpha = 1f;
            _promptCanvasGroup.blocksRaycasts = true;

            // Pulsing halo behind Booster_Wand while targeting
            GameObject haloObj = new GameObject("WandActiveHalo", typeof(RectTransform));
            haloObj.transform.SetParent(_promptRoot.transform, false);
            _buttonHaloRect = haloObj.GetComponent<RectTransform>();
            _buttonHaloRect.anchorMin = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.anchorMax = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.pivot = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.sizeDelta = new Vector2(280f, 280f);
            _buttonHaloImg = haloObj.AddComponent<Image>();
            _buttonHaloImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _buttonHaloImg.color = new Color(0.95f, 0.52f, 1.0f, 0.78f);
            _buttonHaloImg.raycastTarget = false;
            haloObj.SetActive(false);

            // Top prompt card below Banner_Level (doesn't cover board)
            GameObject cardObj = new GameObject("PromptCard", typeof(RectTransform));
            cardObj.transform.SetParent(_promptRoot.transform, false);
            _promptCardRect = cardObj.GetComponent<RectTransform>();
            _promptCardRect.anchorMin = new Vector2(0.5f, 1f);
            _promptCardRect.anchorMax = new Vector2(0.5f, 1f);
            _promptCardRect.pivot = new Vector2(0.5f, 0.5f);
            _promptCardRect.anchoredPosition = new Vector2(0f, -335f);
            _promptCardRect.sizeDelta = new Vector2(800f, 134f);

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.sprite = SpriteGenerator.GetFreezeTooltipCardSprite();
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;
            cardImg.raycastTarget = true;

            Shadow cardShadow = cardObj.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0.04f, 0.05f, 0.16f, 0.36f);
            cardShadow.effectDistance = new Vector2(0f, -8f);

            // Left Wand icon
            GameObject iconObj = new GameObject("WandIcon", typeof(RectTransform));
            iconObj.transform.SetParent(cardObj.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(68f, 0f);
            iconRect.sizeDelta = new Vector2(88f, 88f);

            _promptIconImg = iconObj.AddComponent<Image>();
            _promptIconImg.sprite = iconSprite != null ? iconSprite : GemSpriteManager.GetBoosterSprite("booster3");
            _promptIconImg.preserveAspect = true;
            _promptIconImg.raycastTarget = false;

            // Center title + subtitle
            GameObject titleObj = new GameObject("PromptTitleText", typeof(RectTransform));
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(8f, 20f);
            titleRect.sizeDelta = new Vector2(540f, 44f);

            _promptTitleText = titleObj.AddComponent<Text>();
            _promptTitleText.text = "Magic Wand";
            _promptTitleText.font = font;
            _promptTitleText.fontSize = 32;
            _promptTitleText.fontStyle = FontStyle.Bold;
            _promptTitleText.alignment = TextAnchor.MiddleLeft;
            _promptTitleText.color = new Color(0.24f, 0.16f, 0.50f);
            _promptTitleText.raycastTarget = false;

            GameObject subObj = new GameObject("PromptSubText", typeof(RectTransform));
            subObj.transform.SetParent(cardObj.transform, false);
            RectTransform subRect = subObj.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.pivot = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = new Vector2(8f, -20f);
            subRect.sizeDelta = new Vector2(540f, 42f);

            _promptSubText = subObj.AddComponent<Text>();
            _promptSubText.text = "Choose a Color to Eliminate";
            _promptSubText.font = font;
            _promptSubText.fontSize = 24;
            _promptSubText.fontStyle = FontStyle.Bold;
            _promptSubText.alignment = TextAnchor.MiddleLeft;
            _promptSubText.resizeTextForBestFit = true;
            _promptSubText.resizeTextMinSize = 18;
            _promptSubText.resizeTextMaxSize = 26;
            _promptSubText.color = new Color(0.36f, 0.28f, 0.58f);
            _promptSubText.raycastTarget = false;

            // Right Cancel X button
            GameObject closeObj = new GameObject("Btn_CancelWand", typeof(RectTransform));
            closeObj.transform.SetParent(cardObj.transform, false);
            RectTransform closeRect = closeObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-56f, 0f);
            closeRect.sizeDelta = new Vector2(68f, 68f);

            Image closeImg = closeObj.AddComponent<Image>();
            Sprite closeSp = GemSpriteManager.GetSettingSprite("X (1)") ?? GemSpriteManager.GetPauseSprite("X");
            if (closeSp != null) closeImg.sprite = closeSp;
            closeImg.preserveAspect = true;
            closeImg.color = Color.white;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                CancelTargeting();
            });

            _promptRoot.SetActive(false);
        }

        private void EnsureEffectLayer()
        {
            if (_effectLayer != null) return;

            EnsureRootReferences();
            GameObject layerObj = new GameObject("WandEffectLayer", typeof(RectTransform));
            layerObj.transform.SetParent(transform, false);
            _effectLayer = layerObj.GetComponent<RectTransform>();
            StretchFull(_effectLayer);

            _effectCanvas = layerObj.AddComponent<Canvas>();
            _effectCanvas.overrideSorting = true;
            int rootOrder = _rootCanvas != null ? _rootCanvas.sortingOrder : 0;
            int rootLayerId = _rootCanvas != null ? _rootCanvas.sortingLayerID : 0;
            _effectCanvas.sortingLayerID = rootLayerId;
            _effectCanvas.sortingOrder = rootOrder + 18;

            _effectCanvasGroup = layerObj.AddComponent<CanvasGroup>();
            _effectCanvasGroup.blocksRaycasts = false;
            _effectCanvasGroup.interactable = false;
        }

        public void HandleWorldTap(Vector3 worldPos)
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || !IsTargeting) return;

            PuzzlePiece tappedPiece = FindSelectablePieceAtWorldPos(worldPos);
            if (tappedPiece == null) return;

            PieceColor targetColor = tappedPiece.PieceColor;
            List<PuzzlePiece> matchingPieces = new List<PuzzlePiece>();
            if (lm.ActivePieces != null)
            {
                for (int i = 0; i < lm.ActivePieces.Count; i++)
                {
                    PuzzlePiece p = lm.ActivePieces[i];
                    if (p != null && !p.IsStone && !p.IsSliding && !p.IsEscaping && p.PieceColor == targetColor)
                    {
                        matchingPieces.Add(p);
                    }
                }
            }

            if (matchingPieces.Count == 0) return;

            Action consume = _onConsumeCharge;
            _onConsumeCharge = null;
            consume?.Invoke();
            GameUI.Instance?.BeginTimerOnFirstInteraction();

            HideTargetingUIBeforeExecution();
            _phase = WandPhase.Executing;
            _executionCoroutine = StartCoroutine(CoWandExecuteColorElimination(tappedPiece, matchingPieces, targetColor));
        }

        private void HideTargetingUIBeforeExecution()
        {
            ClearTargetHighlights();
            if (_buttonHaloRect != null) _buttonHaloRect.gameObject.SetActive(false);
            if (_promptRoot != null) _promptRoot.SetActive(false);
        }

        private static PuzzlePiece FindSelectablePieceAtWorldPos(Vector3 worldPos)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(worldPos.x, worldPos.y));
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                PuzzlePiece p = hits[i].GetComponentInParent<PuzzlePiece>();
                if (p != null && !p.IsStone && !p.IsSliding && !p.IsEscaping)
                {
                    return p;
                }
            }
            return null;
        }

        private void ApplySelectColorDimSpotlight()
        {
            LevelManager lm = LevelManager.Instance;
            List<PuzzlePiece> selectablePieces = new List<PuzzlePiece>();
            if (lm != null && lm.ActivePieces != null)
            {
                for (int i = 0; i < lm.ActivePieces.Count; i++)
                {
                    PuzzlePiece p = lm.ActivePieces[i];
                    if (p != null && !p.IsStone && !p.IsSliding && !p.IsEscaping)
                    {
                        selectablePieces.Add(p);
                    }
                }
            }
            ApplyDimSpotlight(selectablePieces, 42);
        }

        private void ApplyDimSpotlight(List<PuzzlePiece> elevatedPieces, int sortingOffset)
        {
            ClearTargetHighlights();

            Camera cam = Camera.main;
            Vector3 camCenter = cam != null ? cam.transform.position : Vector3.zero;
            float camH = cam != null ? cam.orthographicSize * 2f : 18f;
            float aspect = (cam != null && cam.aspect > 0.05f) ? cam.aspect : (1440f / 3200f);
            float camW = camH * aspect;

            _boardDimOverlay = new GameObject("WandBoardDimOverlay");
            _boardDimOverlay.transform.position = new Vector3(camCenter.x, camCenter.y, 0f);
            _boardDimOverlay.transform.localScale = new Vector3(camW * 1.6f, camH * 1.6f, 1f);

            SpriteRenderer dimSr = _boardDimOverlay.AddComponent<SpriteRenderer>();
            dimSr.sprite = SpriteGenerator.GetShardSprite();
            dimSr.sortingOrder = 40;
            dimSr.color = new Color(0.02f, 0.02f, 0.08f, 0.72f);

            if (elevatedPieces != null)
            {
                for (int i = 0; i < elevatedPieces.Count; i++)
                {
                    ElevateObjectRenderers(elevatedPieces[i] != null ? elevatedPieces[i].gameObject : null, sortingOffset);
                }
            }
        }

        private void ElevateObjectRenderers(GameObject go, int orderOffset)
        {
            if (go == null) return;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null) continue;
                _elevatedRenderers.Add(new ElevatedRendererEntry
                {
                    Renderer = r,
                    OriginalSortingOrder = r.sortingOrder
                });
                r.sortingOrder += orderOffset;
            }
        }

        private void ClearTargetHighlights()
        {
            for (int i = _elevatedRenderers.Count - 1; i >= 0; i--)
            {
                ElevatedRendererEntry entry = _elevatedRenderers[i];
                if (entry.Renderer != null)
                {
                    entry.Renderer.sortingOrder = entry.OriginalSortingOrder;
                }
            }
            _elevatedRenderers.Clear();

            if (_boardDimOverlay != null)
            {
                if (_isDestroying) Destroy(_boardDimOverlay);
                else DestroyImmediate(_boardDimOverlay);
                _boardDimOverlay = null;
            }
        }

        private IEnumerator CoWandExecuteColorElimination(PuzzlePiece tappedPiece, List<PuzzlePiece> matchingPieces, PieceColor targetColor)
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || matchingPieces == null || matchingPieces.Count == 0)
            {
                ClearTargetHighlights();
                _phase = WandPhase.Idle;
                _executionCoroutine = null;
                yield break;
            }

            // Highlight only the selected color's blocks while the wand casts
            ApplyDimSpotlight(matchingPieces, 45);
            EnsureEffectLayer();
            SoundManager.Instance?.PlayWandCast();

            Sprite wandSprite = _wandIconSprite != null ? _wandIconSprite : GemSpriteManager.GetBoosterSprite("booster3");
            Vector2 startLocal = _wandButtonRect != null
                ? WorldRectCenterToLocal(_wandButtonRect, _effectLayer)
                : new Vector2(280f, -680f);
            Vector2 arcMidPos = new Vector2(-110f, 165f);
            Vector2 centerCastPos = new Vector2(0f, 60f);

            GameObject wandObj = new GameObject("FlyingMagicWand", typeof(RectTransform));
            wandObj.transform.SetParent(_effectLayer, false);
            _transientUiObjects.Add(wandObj);

            RectTransform wandRect = wandObj.GetComponent<RectTransform>();
            wandRect.anchorMin = new Vector2(0.5f, 0.5f);
            wandRect.anchorMax = new Vector2(0.5f, 0.5f);
            wandRect.pivot = new Vector2(0.5f, 0.5f);
            wandRect.anchoredPosition = startLocal;
            wandRect.sizeDelta = new Vector2(138f, 138f);

            Image wandImg = wandObj.AddComponent<Image>();
            wandImg.sprite = wandSprite;
            wandImg.preserveAspect = true;
            wandImg.color = Color.white;
            wandImg.raycastTarget = false;

            // Phase 1: Curved flight from Booster_Wand button to upper-left board with star trail
            float phase1Dur = 0.36f;
            float elapsed = 0f;
            float trailTimer = 0f;
            Vector2 p1Ctrl = new Vector2(Mathf.Lerp(startLocal.x, arcMidPos.x, 0.35f) + 95f, Mathf.Lerp(startLocal.y, arcMidPos.y, 0.55f));

            while (elapsed < phase1Dur && wandRect != null)
            {
                elapsed += Time.deltaTime;
                trailTimer += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase1Dur);
                float ease = 1f - (1f - t) * (1f - t);

                Vector2 pos = QuadraticBezier(startLocal, p1Ctrl, arcMidPos, ease);
                wandRect.anchoredPosition = pos;
                wandRect.localScale = Vector3.one * Mathf.Lerp(1.0f, 2.15f, ease);
                wandRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -18f, ease));

                if (trailTimer >= 0.045f)
                {
                    trailTimer = 0f;
                    SpawnUiTrailStar(pos + new Vector2(UnityEngine.Random.Range(-18f, 18f), UnityEngine.Random.Range(-18f, 18f)));
                }
                yield return null;
            }

            // Phase 2: Swoop to board center and cast shockwave + sparkles
            float phase2Dur = 0.30f;
            elapsed = 0f;
            trailTimer = 0f;
            while (elapsed < phase2Dur && wandRect != null)
            {
                elapsed += Time.deltaTime;
                trailTimer += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase2Dur);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                Vector2 pos = Vector2.Lerp(arcMidPos, centerCastPos, ease);
                wandRect.anchoredPosition = pos;
                wandRect.localScale = Vector3.one * Mathf.Lerp(2.15f, 2.50f, Mathf.Sin(t * Mathf.PI * 0.5f));

                float rotZ = t < 0.55f
                    ? Mathf.Lerp(-18f, 22f, t / 0.55f)
                    : Mathf.Lerp(22f, -16f, (t - 0.55f) / 0.45f);
                wandRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                if (trailTimer >= 0.045f)
                {
                    trailTimer = 0f;
                    SpawnUiTrailStar(pos + new Vector2(UnityEngine.Random.Range(-22f, 22f), UnityEngine.Random.Range(-22f, 22f)));
                }
                yield return null;
            }

            // Cast radial wave, radiating golden stars, and highlight flash on every matching block
            StartCoroutine(CoMagicShockwave(centerCastPos, GameConfig.GetColorRGB(targetColor)));
            SpawnRadiatingMagicStars(centerCastPos, 14);

            for (int i = 0; i < matchingPieces.Count; i++)
            {
                PuzzlePiece p = matchingPieces[i];
                if (p == null) continue;
                p.PlayMergeEffect();
                SpawnWorldSparkBurst(p.transform.position, new Color(1.0f, 0.86f, 0.45f, 0.95f));
            }

            // Fade out the flying wand briefly while clearing the board dim overlay
            float fadeDur = 0.15f;
            elapsed = 0f;
            while (elapsed < fadeDur && wandObj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDur);
                wandImg.color = new Color(1f, 1f, 1f, 1f - t);
                wandRect.localScale = Vector3.one * Mathf.Lerp(2.50f, 2.95f, t);
                yield return null;
            }

            if (wandObj != null)
            {
                _transientUiObjects.Remove(wandObj);
                Destroy(wandObj);
            }

            ClearTargetHighlights();

            // Phase 3: Clear grid cells immediately, thaw iced matching pieces, and launch staggered flights to gate
            List<PieceGatePair> launchQueue = new List<PieceGatePair>(matchingPieces.Count);
            for (int i = 0; i < matchingPieces.Count; i++)
            {
                PuzzlePiece p = matchingPieces[i];
                if (p == null || p.IsEscaping) continue;

                lm.ClearPieceFromGrid(p);
                if (p.IsFrozen)
                {
                    SoundManager.Instance?.PlayIceCrack(0.6f, 1.25f);
                    SpawnWorldSparkBurst(p.transform.position, new Color(0.45f, 0.94f, 1.0f, 0.95f));
                    p.SetIceCount(0);
                }

                ExitGateView gate = lm.FindBestGateForColor(targetColor, p.transform.position);
                float dist = gate != null ? Vector3.Distance(p.transform.position, gate.transform.position) : 0f;
                launchQueue.Add(new PieceGatePair { Piece = p, Gate = gate, Distance = dist });
            }

            launchQueue.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            int remaining = launchQueue.Count;
            for (int i = 0; i < launchQueue.Count; i++)
            {
                PieceGatePair item = launchQueue[i];
                if (item.Piece == null)
                {
                    remaining--;
                    continue;
                }

                SpawnWorldSparkBurst(item.Piece.transform.position, new Color(1.0f, 0.78f, 0.98f, 0.92f));
                Vector3 gateBurstPos = item.Gate != null ? item.Gate.transform.position : item.Piece.transform.position;
                ExitGateView targetGate = item.Gate;

                lm.ExecuteWandJumpEscape(item.Piece, targetGate, () =>
                {
                    SpawnWorldSparkBurst(gateBurstPos, new Color(0.45f, 0.96f, 1.0f, 0.90f));
                    remaining--;
                }, 0.30f);

                if (i < launchQueue.Count - 1)
                {
                    yield return new WaitForSeconds(0.065f);
                }
            }

            float waitElapsed = 0f;
            while (remaining > 0 && waitElapsed < 3.5f)
            {
                waitElapsed += Time.deltaTime;
                yield return null;
            }

            ClearTransientWorldObjects();
            ClearTransientUiObjects();
            _phase = WandPhase.Idle;
            _executionCoroutine = null;
        }

        private struct PieceGatePair
        {
            public PuzzlePiece Piece;
            public ExitGateView Gate;
            public float Distance;
        }

        private void SpawnUiTrailStar(Vector2 pos)
        {
            if (_effectLayer == null) return;
            StartCoroutine(CoUiTrailStar(pos));
        }

        private IEnumerator CoUiTrailStar(Vector2 pos)
        {
            GameObject starObj = new GameObject("WandTrailStar", typeof(RectTransform));
            starObj.transform.SetParent(_effectLayer, false);
            _transientUiObjects.Add(starObj);

            RectTransform rt = starObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            float size = UnityEngine.Random.Range(36f, 62f);
            rt.sizeDelta = new Vector2(size, size);

            Image img = starObj.AddComponent<Image>();
            img.sprite = (UnityEngine.Random.value > 0.4f)
                ? SpriteGenerator.GetGoldenStarSprite()
                : SpriteGenerator.GetStarFlareSprite();
            Color baseCol = (UnityEngine.Random.value > 0.35f)
                ? new Color(1.0f, 0.88f, 0.32f, 0.95f)
                : new Color(1.0f, 0.55f, 0.95f, 0.95f);
            img.color = baseCol;
            img.preserveAspect = true;
            img.raycastTarget = false;

            float dur = 0.28f;
            float elapsed = 0f;
            float rotSpeed = UnityEngine.Random.Range(-180f, 180f);
            while (elapsed < dur && starObj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                rt.anchoredPosition = pos + new Vector2(0f, -24f * t);
                rt.localScale = Vector3.one * Mathf.Lerp(1.0f, 0.2f, t);
                rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
                Color c = baseCol;
                c.a = baseCol.a * (1f - t);
                img.color = c;
                yield return null;
            }

            if (starObj != null)
            {
                _transientUiObjects.Remove(starObj);
                Destroy(starObj);
            }
        }

        private IEnumerator CoMagicShockwave(Vector2 centerPos, Color gemColor)
        {
            if (_effectLayer == null) yield break;

            GameObject glowObj = new GameObject("WandMagicGlow", typeof(RectTransform));
            glowObj.transform.SetParent(_effectLayer, false);
            _transientUiObjects.Add(glowObj);

            RectTransform glowRt = glowObj.GetComponent<RectTransform>();
            glowRt.anchorMin = new Vector2(0.5f, 0.5f);
            glowRt.anchorMax = new Vector2(0.5f, 0.5f);
            glowRt.pivot = new Vector2(0.5f, 0.5f);
            glowRt.anchoredPosition = centerPos;
            glowRt.sizeDelta = new Vector2(360f, 360f);

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            Color glowCol = Color.Lerp(new Color(0.95f, 0.42f, 0.98f, 0.85f), gemColor, 0.30f);
            glowImg.color = glowCol;
            glowImg.raycastTarget = false;

            GameObject ringObj = new GameObject("WandShockwaveRing", typeof(RectTransform));
            ringObj.transform.SetParent(_effectLayer, false);
            _transientUiObjects.Add(ringObj);

            RectTransform ringRt = ringObj.GetComponent<RectTransform>();
            ringRt.anchorMin = new Vector2(0.5f, 0.5f);
            ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            ringRt.pivot = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = centerPos;
            ringRt.sizeDelta = new Vector2(220f, 220f);

            Image ringImg = ringObj.AddComponent<Image>();
            ringImg.sprite = SpriteGenerator.GetCircleSprite();
            Color ringCol = new Color(1.0f, 0.85f, 0.98f, 0.72f);
            ringImg.color = ringCol;
            ringImg.raycastTarget = false;

            float dur = 0.42f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float ease = Mathf.Sqrt(t);

                if (glowObj != null)
                {
                    glowRt.localScale = Vector3.one * Mathf.Lerp(0.6f, 3.8f, ease);
                    Color gc = glowCol;
                    gc.a = glowCol.a * (1f - t);
                    glowImg.color = gc;
                }
                if (ringObj != null)
                {
                    ringRt.localScale = Vector3.one * Mathf.Lerp(0.5f, 5.2f, ease);
                    Color rc = ringCol;
                    rc.a = ringCol.a * (1f - t) * (1f - t);
                    ringImg.color = rc;
                }
                yield return null;
            }

            if (glowObj != null)
            {
                _transientUiObjects.Remove(glowObj);
                Destroy(glowObj);
            }
            if (ringObj != null)
            {
                _transientUiObjects.Remove(ringObj);
                Destroy(ringObj);
            }
        }

        private void SpawnRadiatingMagicStars(Vector2 origin, int count)
        {
            if (_effectLayer == null) return;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.18f, 0.18f);
                float speed = UnityEngine.Random.Range(420f, 760f);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                StartCoroutine(CoRadiatingStar(origin, dir * speed));
            }
        }

        private IEnumerator CoRadiatingStar(Vector2 origin, Vector2 velocity)
        {
            GameObject obj = new GameObject("WandBurstStar", typeof(RectTransform));
            obj.transform.SetParent(_effectLayer, false);
            _transientUiObjects.Add(obj);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = origin;
            float size = UnityEngine.Random.Range(40f, 68f);
            rt.sizeDelta = new Vector2(size, size);

            Image img = obj.AddComponent<Image>();
            img.sprite = (UnityEngine.Random.value > 0.35f)
                ? SpriteGenerator.GetGoldenStarSprite()
                : SpriteGenerator.GetStarFlareSprite();
            Color baseCol = (UnityEngine.Random.value > 0.4f)
                ? new Color(1.0f, 0.88f, 0.28f, 1f)
                : new Color(0.95f, 0.65f, 1.0f, 1f);
            img.color = baseCol;
            img.preserveAspect = true;
            img.raycastTarget = false;

            float dur = 0.45f;
            float elapsed = 0f;
            float rotSpeed = UnityEngine.Random.Range(-220f, 220f);
            while (elapsed < dur && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float ease = 1f - (1f - t) * (1f - t);
                rt.anchoredPosition = origin + velocity * (ease * 0.42f);
                float s = t < 0.25f ? Mathf.Lerp(0.4f, 1.15f, t / 0.25f) : Mathf.Lerp(1.15f, 0.1f, (t - 0.25f) / 0.75f);
                rt.localScale = Vector3.one * s;
                rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

                Color c = baseCol;
                c.a = Mathf.Clamp01(1f - Mathf.Max(0f, t - 0.35f) / 0.65f);
                img.color = c;
                yield return null;
            }

            if (obj != null)
            {
                _transientUiObjects.Remove(obj);
                Destroy(obj);
            }
        }

        private void SpawnWorldSparkBurst(Vector3 worldPos, Color burstColor)
        {
            StartCoroutine(CoWorldSparkBurst(worldPos, burstColor));
        }

        private IEnumerator CoWorldSparkBurst(Vector3 worldPos, Color burstColor)
        {
            GameObject obj = new GameObject("WandWorldBurst");
            obj.transform.position = new Vector3(worldPos.x, worldPos.y, -0.7f);
            _transientWorldObjects.Add(obj);

            SpriteRenderer glowSr = obj.AddComponent<SpriteRenderer>();
            glowSr.sprite = SpriteGenerator.GetRadialGlowSprite();
            glowSr.sortingOrder = 96;
            glowSr.color = burstColor;

            GameObject starObj = new GameObject("Star");
            starObj.transform.SetParent(obj.transform, false);
            SpriteRenderer starSr = starObj.AddComponent<SpriteRenderer>();
            starSr.sprite = SpriteGenerator.GetStarFlareSprite();
            starSr.sortingOrder = 97;
            starSr.color = Color.white;

            float dur = 0.28f;
            float elapsed = 0f;
            while (elapsed < dur && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float s = Mathf.Lerp(0.55f, 2.35f, Mathf.Sqrt(t));
                obj.transform.localScale = new Vector3(s, s, 1f);
                starObj.transform.localRotation = Quaternion.Euler(0f, 0f, t * 55f);

                Color c = burstColor;
                c.a = burstColor.a * (1f - t);
                glowSr.color = c;
                starSr.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }

            if (obj != null)
            {
                _transientWorldObjects.Remove(obj);
                Destroy(obj);
            }
        }

        private void ClearTransientWorldObjects()
        {
            for (int i = _transientWorldObjects.Count - 1; i >= 0; i--)
            {
                if (_transientWorldObjects[i] != null)
                {
                    Destroy(_transientWorldObjects[i]);
                }
            }
            _transientWorldObjects.Clear();
        }

        private void ClearTransientUiObjects()
        {
            for (int i = _transientUiObjects.Count - 1; i >= 0; i--)
            {
                if (_transientUiObjects[i] != null)
                {
                    Destroy(_transientUiObjects[i]);
                }
            }
            _transientUiObjects.Clear();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (IsTutorialShowing)
            {
                UpdateTutorialIdleAnimation(Time.unscaledDeltaTime);
            }

            if (IsTargeting)
            {
                UpdateTargetingVisuals(Time.unscaledDeltaTime);
            }
        }

        private void UpdateTutorialIdleAnimation(float dt)
        {
            _tutorialAnimTime += dt;

            if (_spotlightButtonRect != null && _tutorialRoot != null)
            {
                RectTransform tutRect = _tutorialRoot.GetComponent<RectTransform>();
                Vector2 btnLocal = WorldRectCenterToLocal(_spotlightButtonRect, tutRect);
                if (_tutorialSpotlightGlowRect != null) _tutorialSpotlightGlowRect.anchoredPosition = btnLocal;
                if (_tutorialHandAnchorRect != null) _tutorialHandAnchorRect.anchoredPosition = btnLocal;
            }

            if (_tutorialSpotlightGlowRect != null && _tutorialSpotlightGlowImg != null)
            {
                float breath = 1.0f + 0.09f * Mathf.Sin(_tutorialAnimTime * 3.4f);
                _tutorialSpotlightGlowRect.localScale = new Vector3(breath, breath, 1f);
                Color c = _tutorialSpotlightGlowImg.color;
                c.a = 0.68f + 0.20f * Mathf.Sin(_tutorialAnimTime * 3.4f);
                _tutorialSpotlightGlowImg.color = c;
            }

            if (_tutorialCardIconRect != null)
            {
                float bob = Mathf.Sin(_tutorialAnimTime * 3.0f) * 5.0f;
                _tutorialCardIconRect.anchoredPosition = new Vector2(-268f, bob);
            }

            float cycle = _tutorialAnimTime % 1.15f;
            float pressPhase = cycle / 1.15f;
            Vector2 baseOffset = new Vector2(62f, -72f);

            if (_tutorialHandImageRect != null)
            {
                float tapDip;
                float handScale;
                if (pressPhase < 0.28f)
                {
                    float p = pressPhase / 0.28f;
                    tapDip = Mathf.SmoothStep(0f, 1f, p);
                    handScale = Mathf.Lerp(1.04f, 0.88f, p);
                }
                else if (pressPhase < 0.56f)
                {
                    float p = (pressPhase - 0.28f) / 0.28f;
                    tapDip = Mathf.SmoothStep(1f, 0f, p);
                    handScale = Mathf.Lerp(0.88f, 1.04f, p);
                }
                else
                {
                    tapDip = 0f;
                    handScale = 1.04f;
                }

                _tutorialHandImageRect.anchoredPosition = baseOffset + new Vector2(-10f * tapDip, 14f * tapDip);
                _tutorialHandImageRect.localScale = new Vector3(handScale, handScale, 1f);
                _tutorialHandImageRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 6f, tapDip));
            }

            if (_tutorialTapRippleRect != null && _tutorialTapRippleImg != null)
            {
                if (pressPhase >= 0.20f && pressPhase <= 0.78f)
                {
                    float rp = (pressPhase - 0.20f) / 0.58f;
                    float rs = Mathf.Lerp(0.45f, 1.85f, rp);
                    _tutorialTapRippleRect.localScale = new Vector3(rs, rs, 1f);
                    Color rc = _tutorialTapRippleImg.color;
                    rc.a = Mathf.Lerp(0.65f, 0f, rp);
                    _tutorialTapRippleImg.color = rc;
                }
                else
                {
                    Color rc = _tutorialTapRippleImg.color;
                    rc.a = 0f;
                    _tutorialTapRippleImg.color = rc;
                }
            }
        }

        private void UpdateTargetingVisuals(float dt)
        {
            _fxAnimTime += dt;

            if (_buttonHaloRect != null && _buttonHaloImg != null && _buttonHaloRect.gameObject.activeSelf)
            {
                if (_wandButtonRect != null && _promptRoot != null)
                {
                    _buttonHaloRect.anchoredPosition = WorldRectCenterToLocal(_wandButtonRect, _promptRoot.GetComponent<RectTransform>());
                }
                float s = 1.0f + 0.12f * Mathf.Sin(_fxAnimTime * 4.2f);
                _buttonHaloRect.localScale = new Vector3(s, s, 1f);
                Color c = _buttonHaloImg.color;
                c.a = 0.58f + 0.24f * Mathf.Sin(_fxAnimTime * 4.2f);
                _buttonHaloImg.color = c;
            }

            if (_boardDimOverlay != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 camCenter = cam.transform.position;
                    float camH = cam.orthographicSize * 2f;
                    float aspect = cam.aspect > 0.05f ? cam.aspect : (1440f / 3200f);
                    float camW = camH * aspect;
                    _boardDimOverlay.transform.position = new Vector3(camCenter.x, camCenter.y, 0f);
                    _boardDimOverlay.transform.localScale = new Vector3(camW * 1.6f, camH * 1.6f, 1f);
                }
            }
        }

        private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static void StretchFull(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float p = t - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }
    }
}
