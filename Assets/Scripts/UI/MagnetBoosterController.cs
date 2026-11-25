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
    public class MagnetBoosterController : MonoBehaviour
    {
        public enum MagnetPhase
        {
            Idle,
            SelectFirst,
            SelectTarget,
            Executing
        }

        private Canvas _rootCanvas;
        private RectTransform _rootRect;

        // First-time Level 12 tutorial overlay
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
        private Text _promptText;
        private RectTransform _buttonHaloRect;
        private Image _buttonHaloImg;
        private RectTransform _magnetButtonRect;

        // World-space targeting & FX
        private MagnetPhase _phase = MagnetPhase.Idle;
        private PuzzlePiece _selectedSourcePiece;
        private Sprite _magnetIconSprite;
        private Action _onConsumeCharge;
        private struct ElevatedRendererEntry
        {
            public Renderer Renderer;
            public int OriginalSortingOrder;
        }

        private GameObject _boardDimOverlay;
        private readonly List<ElevatedRendererEntry> _elevatedRenderers = new List<ElevatedRendererEntry>();
        private readonly List<GameObject> _transientWorldObjects = new List<GameObject>();
        private GameObject _floatingMagnetObj;
        private Coroutine _executionCoroutine;
        private float _fxAnimTime = 0f;
        private bool _isDestroying = false;

        public bool IsTutorialShowing => _tutorialRoot != null && _tutorialRoot.activeSelf;
        public bool IsTargeting => _phase == MagnetPhase.SelectFirst || _phase == MagnetPhase.SelectTarget;
        public bool IsExecuting => _phase == MagnetPhase.Executing;
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

        public void ShowFirstTimeTutorial(RectTransform magnetButtonRect, Sprite magnetIconSprite)
        {
            if (magnetButtonRect == null || IsTutorialShowing) return;

            EnsureRootReferences();
            if (magnetIconSprite != null) _magnetIconSprite = magnetIconSprite;
            EnsureTutorialOverlay(_magnetIconSprite);

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

            // Elevate the existing Magnet booster button without changing its sprite/shape/parent
            RemoveSpotlightComponents();
            _spotlightButtonRect = magnetButtonRect;
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
            Vector2 btnLocal = WorldRectCenterToLocal(magnetButtonRect, tutRect);
            if (_tutorialSpotlightGlowRect != null) _tutorialSpotlightGlowRect.anchoredPosition = btnLocal;
            if (_tutorialHandAnchorRect != null) _tutorialHandAnchorRect.anchoredPosition = btnLocal;
            if (_tutorialCardIconImg != null && _magnetIconSprite != null) _tutorialCardIconImg.sprite = _magnetIconSprite;

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

            _tutorialRoot = new GameObject("MagnetTutorialOverlay", typeof(RectTransform));
            _tutorialRoot.transform.SetParent(transform, false);
            StretchFull(_tutorialRoot.GetComponent<RectTransform>());

            _tutorialCanvas = _tutorialRoot.AddComponent<Canvas>();
            _tutorialCanvas.overrideSorting = true;
            _tutorialRoot.AddComponent<GraphicRaycaster>();
            _tutorialCanvasGroup = _tutorialRoot.AddComponent<CanvasGroup>();
            _tutorialCanvasGroup.alpha = 0f;

            // Dark scrim blocking board interactions while focusing on Magnet booster
            GameObject scrimObj = new GameObject("Scrim", typeof(RectTransform));
            scrimObj.transform.SetParent(_tutorialRoot.transform, false);
            StretchFull(scrimObj.GetComponent<RectTransform>());
            Image scrimImg = scrimObj.AddComponent<Image>();
            scrimImg.color = new Color(0.02f, 0.03f, 0.09f, 0.74f);
            scrimImg.raycastTarget = true;

            // Radial golden-cyan spotlight glow behind the Magnet button
            GameObject glowObj = new GameObject("SpotlightGlow", typeof(RectTransform));
            glowObj.transform.SetParent(_tutorialRoot.transform, false);
            _tutorialSpotlightGlowRect = glowObj.GetComponent<RectTransform>();
            _tutorialSpotlightGlowRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.sizeDelta = new Vector2(340f, 340f);
            _tutorialSpotlightGlowImg = glowObj.AddComponent<Image>();
            _tutorialSpotlightGlowImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _tutorialSpotlightGlowImg.color = new Color(1.0f, 0.84f, 0.34f, 0.85f);
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

            // Booster icon inside tooltip card (preserving user's booster2 sprite)
            GameObject cardIconObj = new GameObject("BoosterPreviewIcon", typeof(RectTransform));
            cardIconObj.transform.SetParent(cardObj.transform, false);
            _tutorialCardIconRect = cardIconObj.GetComponent<RectTransform>();
            _tutorialCardIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchoredPosition = new Vector2(-268f, 0f);
            _tutorialCardIconRect.sizeDelta = new Vector2(128f, 128f);

            _tutorialCardIconImg = cardIconObj.AddComponent<Image>();
            _tutorialCardIconImg.sprite = boosterIconSprite != null ? boosterIconSprite : GemSpriteManager.GetBoosterSprite("booster2");
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
            txt.text = "Use the Magnet to pull a block\nto merge, exit gate, or crack ice";
            txt.font = font;
            txt.fontSize = 35;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 22;
            txt.resizeTextMaxSize = 36;
            txt.color = new Color(0.24f, 0.16f, 0.50f);
            txt.raycastTarget = false;

            // Animated hand pointer layer above spotlighted Magnet button (non-blocking)
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
            _tutorialTapRippleImg.color = new Color(1.0f, 0.88f, 0.38f, 0f);
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

        public void BeginTargeting(RectTransform magnetButtonRect, Sprite magnetIconSprite, Action onConsumeCharge)
        {
            if (IsExecuting) return;

            DismissTutorial();
            StopTargetingOnly();

            _magnetButtonRect = magnetButtonRect;
            _magnetIconSprite = magnetIconSprite != null ? magnetIconSprite : GemSpriteManager.GetBoosterSprite("booster2");
            _onConsumeCharge = onConsumeCharge;
            _selectedSourcePiece = null;
            _phase = MagnetPhase.SelectFirst;
            _fxAnimTime = 0f;

            EnsurePromptOverlay(_magnetIconSprite);
            if (_promptIconImg != null && _magnetIconSprite != null) _promptIconImg.sprite = _magnetIconSprite;
            SetPromptText("Tap a block to pull\nor tap an ice block (-1)");

            if (_promptRoot != null)
            {
                _promptRoot.SetActive(true);
                PositionAboveModals(_promptRoot.transform);
                if (_promptCanvasGroup != null) _promptCanvasGroup.alpha = 1f;
            }

            if (_buttonHaloRect != null && _magnetButtonRect != null && _promptRoot != null)
            {
                _buttonHaloRect.gameObject.SetActive(true);
                _buttonHaloRect.anchoredPosition = WorldRectCenterToLocal(_magnetButtonRect, _promptRoot.GetComponent<RectTransform>());
            }

            ApplySelectFirstDimSpotlight();
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

            if (_selectedSourcePiece != null)
            {
                _selectedSourcePiece.SetLifted(false, false);
                _selectedSourcePiece = null;
            }

            ClearTargetHighlights();
            DestroyFloatingMagnet();
            ClearTransientWorldObjects();

            if (_buttonHaloRect != null) _buttonHaloRect.gameObject.SetActive(false);
            if (_promptRoot != null) _promptRoot.SetActive(false);

            _onConsumeCharge = null;
            _phase = MagnetPhase.Idle;
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

            _promptRoot = new GameObject("MagnetPromptOverlay", typeof(RectTransform));
            _promptRoot.transform.SetParent(transform, false);
            StretchFull(_promptRoot.GetComponent<RectTransform>());

            _promptCanvasGroup = _promptRoot.AddComponent<CanvasGroup>();
            _promptCanvasGroup.alpha = 1f;
            _promptCanvasGroup.blocksRaycasts = true;

            // Pulsing halo behind Booster_Magnet while targeting
            GameObject haloObj = new GameObject("MagnetActiveHalo", typeof(RectTransform));
            haloObj.transform.SetParent(_promptRoot.transform, false);
            _buttonHaloRect = haloObj.GetComponent<RectTransform>();
            _buttonHaloRect.anchorMin = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.anchorMax = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.pivot = new Vector2(0.5f, 0.5f);
            _buttonHaloRect.sizeDelta = new Vector2(280f, 280f);
            _buttonHaloImg = haloObj.AddComponent<Image>();
            _buttonHaloImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _buttonHaloImg.color = new Color(1.0f, 0.82f, 0.28f, 0.78f);
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
            _promptCardRect.sizeDelta = new Vector2(800f, 126f);

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.sprite = SpriteGenerator.GetFreezeTooltipCardSprite();
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;
            cardImg.raycastTarget = true;

            Shadow cardShadow = cardObj.AddComponent<Shadow>();
            cardShadow.effectColor = new Color(0.04f, 0.05f, 0.16f, 0.36f);
            cardShadow.effectDistance = new Vector2(0f, -8f);

            // Left Magnet icon
            GameObject iconObj = new GameObject("MagnetIcon", typeof(RectTransform));
            iconObj.transform.SetParent(cardObj.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(68f, 0f);
            iconRect.sizeDelta = new Vector2(88f, 88f);

            _promptIconImg = iconObj.AddComponent<Image>();
            _promptIconImg.sprite = iconSprite != null ? iconSprite : GemSpriteManager.GetBoosterSprite("booster2");
            _promptIconImg.preserveAspect = true;
            _promptIconImg.raycastTarget = false;

            // Center prompt text
            GameObject textObj = new GameObject("PromptText", typeof(RectTransform));
            textObj.transform.SetParent(cardObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(8f, 0f);
            textRect.sizeDelta = new Vector2(540f, 96f);

            _promptText = textObj.AddComponent<Text>();
            _promptText.text = "";
            _promptText.font = font;
            _promptText.fontSize = 30;
            _promptText.fontStyle = FontStyle.Bold;
            _promptText.alignment = TextAnchor.MiddleLeft;
            _promptText.resizeTextForBestFit = true;
            _promptText.resizeTextMinSize = 20;
            _promptText.resizeTextMaxSize = 32;
            _promptText.color = new Color(0.24f, 0.16f, 0.50f);
            _promptText.raycastTarget = false;

            // Right Cancel X button
            GameObject closeObj = new GameObject("Btn_CancelMagnet", typeof(RectTransform));
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

        private void SetPromptText(string msg)
        {
            if (_promptText != null) _promptText.text = msg;
        }

        public void HandleWorldTap(Vector3 worldPos)
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || !IsTargeting) return;

            if (_phase == MagnetPhase.SelectFirst)
            {
                PuzzlePiece tappedPiece = FindPieceAtWorldPos(worldPos);
                if (tappedPiece == null) return;

                if (tappedPiece.IsFrozen)
                {
                    Action consume = _onConsumeCharge;
                    _onConsumeCharge = null;
                    consume?.Invoke();
                    GameUI.Instance?.BeginTimerOnFirstInteraction();

                    HideTargetingUIBeforeExecution();
                    _phase = MagnetPhase.Executing;
                    _executionCoroutine = StartCoroutine(CoMagnetCrackIce(tappedPiece));
                    return;
                }

                List<PuzzlePiece> matchingPieces = new List<PuzzlePiece>();
                List<ExitGateView> matchingGates = new List<ExitGateView>();
                GatherValidTargets(lm, tappedPiece, matchingPieces, matchingGates);

                if (matchingPieces.Count == 0 && matchingGates.Count == 0)
                {
                    return;
                }

                _selectedSourcePiece = tappedPiece;
                _phase = MagnetPhase.SelectTarget;
                _selectedSourcePiece.SetLifted(true, false);
                SoundManager.Instance?.PlayMagnetPull();

                SpawnFloatingMagnetAbove(_selectedSourcePiece);
                ApplyTargetDimSpotlight(matchingPieces, matchingGates);
                SetPromptText("Tap a matching block to merge\nor a matching gate to exit");
            }
            else if (_phase == MagnetPhase.SelectTarget)
            {
                if (_selectedSourcePiece == null)
                {
                    _phase = MagnetPhase.SelectFirst;
                    ApplySelectFirstDimSpotlight();
                    SetPromptText("Tap a block to pull\nor tap an ice block (-1)");
                    return;
                }

                ExitGateView hitGate = FindMatchingGateAtWorldPos(lm, _selectedSourcePiece, worldPos);
                if (hitGate != null)
                {
                    PuzzlePiece source = _selectedSourcePiece;
                    _selectedSourcePiece = null;
                    Action consume = _onConsumeCharge;
                    _onConsumeCharge = null;
                    consume?.Invoke();
                    GameUI.Instance?.BeginTimerOnFirstInteraction();

                    HideTargetingUIBeforeExecution();
                    _phase = MagnetPhase.Executing;
                    _executionCoroutine = StartCoroutine(CoMagnetExecuteEscape(source, hitGate));
                    return;
                }

                PuzzlePiece hitPiece = FindPieceAtWorldPos(worldPos);
                if (hitPiece == null) return;

                if (hitPiece == _selectedSourcePiece)
                {
                    _selectedSourcePiece.SetLifted(false, false);
                    _selectedSourcePiece = null;
                    DestroyFloatingMagnet();
                    _phase = MagnetPhase.SelectFirst;
                    ApplySelectFirstDimSpotlight();
                    SetPromptText("Tap a block to pull\nor tap an ice block (-1)");
                    return;
                }

                if (LevelManager.CanPiecesMagnetMerge(_selectedSourcePiece, hitPiece))
                {
                    PuzzlePiece source = _selectedSourcePiece;
                    _selectedSourcePiece = null;
                    Action consume = _onConsumeCharge;
                    _onConsumeCharge = null;
                    consume?.Invoke();
                    GameUI.Instance?.BeginTimerOnFirstInteraction();

                    HideTargetingUIBeforeExecution();
                    _phase = MagnetPhase.Executing;
                    _executionCoroutine = StartCoroutine(CoMagnetExecuteMerge(source, hitPiece));
                }
            }
        }

        private void HideTargetingUIBeforeExecution()
        {
            ClearTargetHighlights();
            if (_buttonHaloRect != null) _buttonHaloRect.gameObject.SetActive(false);
            if (_promptRoot != null) _promptRoot.SetActive(false);
        }

        private static PuzzlePiece FindPieceAtWorldPos(Vector3 worldPos)
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

        private static ExitGateView FindMatchingGateAtWorldPos(LevelManager lm, PuzzlePiece sourcePiece, Vector3 worldPos)
        {
            if (lm == null || sourcePiece == null) return null;

            Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(worldPos.x, worldPos.y));
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                ExitGateView gate = hits[i].GetComponentInParent<ExitGateView>();
                if (LevelManager.CanPieceMagnetEscapeGate(sourcePiece, gate))
                {
                    return gate;
                }
            }

            if (lm.Gates != null)
            {
                for (int i = 0; i < lm.Gates.Count; i++)
                {
                    ExitGateView gate = lm.Gates[i];
                    if (!LevelManager.CanPieceMagnetEscapeGate(sourcePiece, gate)) continue;

                    Bounds b = gate.DetectionBounds;
                    if (b.size.sqrMagnitude <= 0.01f)
                    {
                        Collider2D col = gate.GetComponent<Collider2D>();
                        if (col != null) b = col.bounds;
                        else b = new Bounds(gate.transform.position, new Vector3(1.2f, 1.2f, 5f));
                    }
                    b.Expand(new Vector3(0.80f, 0.80f, 10f));
                    Vector3 checkPt = new Vector3(worldPos.x, worldPos.y, b.center.z);
                    if (b.Contains(checkPt))
                    {
                        return gate;
                    }
                }
            }

            return null;
        }

        private static void GatherValidTargets(LevelManager lm, PuzzlePiece source, List<PuzzlePiece> outPieces, List<ExitGateView> outGates)
        {
            outPieces.Clear();
            outGates.Clear();
            if (lm == null || source == null) return;

            if (lm.ActivePieces != null)
            {
                for (int i = 0; i < lm.ActivePieces.Count; i++)
                {
                    PuzzlePiece p = lm.ActivePieces[i];
                    if (LevelManager.CanPiecesMagnetMerge(source, p))
                    {
                        outPieces.Add(p);
                    }
                }
            }

            if (lm.Gates != null)
            {
                for (int i = 0; i < lm.Gates.Count; i++)
                {
                    ExitGateView g = lm.Gates[i];
                    if (LevelManager.CanPieceMagnetEscapeGate(source, g))
                    {
                        outGates.Add(g);
                    }
                }
            }
        }

        private void SpawnFloatingMagnetAbove(PuzzlePiece piece)
        {
            DestroyFloatingMagnet();
            if (piece == null) return;

            _floatingMagnetObj = new GameObject("MagnetSelectionBadge");
            _floatingMagnetObj.transform.position = piece.transform.position + new Vector3(0f, 0.72f, -0.8f);

            GameObject auraObj = new GameObject("Aura");
            auraObj.transform.SetParent(_floatingMagnetObj.transform, false);
            SpriteRenderer auraSr = auraObj.AddComponent<SpriteRenderer>();
            auraSr.sprite = SpriteGenerator.GetRadialGlowSprite();
            auraSr.sortingOrder = 93;
            auraSr.color = new Color(1.0f, 0.86f, 0.30f, 0.78f);
            auraObj.transform.localScale = Vector3.one * 1.15f;

            GameObject iconObj = new GameObject("MagnetSprite");
            iconObj.transform.SetParent(_floatingMagnetObj.transform, false);
            SpriteRenderer iconSr = iconObj.AddComponent<SpriteRenderer>();
            iconSr.sprite = _magnetIconSprite != null ? _magnetIconSprite : GemSpriteManager.GetBoosterSprite("booster2");
            iconSr.sortingOrder = 95;
            if (iconSr.sprite != null && iconSr.sprite.rect.width > 0f)
            {
                float nativeW = iconSr.sprite.rect.width / iconSr.sprite.pixelsPerUnit;
                float s = 0.78f / nativeW;
                iconObj.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        private void DestroyFloatingMagnet()
        {
            if (_floatingMagnetObj != null)
            {
                Destroy(_floatingMagnetObj);
                _floatingMagnetObj = null;
            }
        }

        private void ApplySelectFirstDimSpotlight()
        {
            LevelManager lm = LevelManager.Instance;
            List<PuzzlePiece> selectablePieces = new List<PuzzlePiece>();
            if (lm != null && lm.ActivePieces != null)
            {
                List<PuzzlePiece> tempPieces = new List<PuzzlePiece>();
                List<ExitGateView> tempGates = new List<ExitGateView>();
                for (int i = 0; i < lm.ActivePieces.Count; i++)
                {
                    PuzzlePiece p = lm.ActivePieces[i];
                    if (p == null || p.IsStone || p.IsSliding || p.IsEscaping) continue;
                    if (p.IsFrozen)
                    {
                        selectablePieces.Add(p);
                        continue;
                    }
                    GatherValidTargets(lm, p, tempPieces, tempGates);
                    if (tempPieces.Count > 0 || tempGates.Count > 0)
                    {
                        selectablePieces.Add(p);
                    }
                }
            }
            ApplyTargetDimSpotlight(selectablePieces, null);
        }

        private void ApplyTargetDimSpotlight(List<PuzzlePiece> validPieces, List<ExitGateView> validGates)
        {
            ClearTargetHighlights();

            Camera cam = Camera.main;
            Vector3 camCenter = cam != null ? cam.transform.position : Vector3.zero;
            float camH = cam != null ? cam.orthographicSize * 2f : 18f;
            float aspect = (cam != null && cam.aspect > 0.05f) ? cam.aspect : (1440f / 3200f);
            float camW = camH * aspect;

            _boardDimOverlay = new GameObject("MagnetBoardDimOverlay");
            _boardDimOverlay.transform.position = new Vector3(camCenter.x, camCenter.y, 0f);
            _boardDimOverlay.transform.localScale = new Vector3(camW * 1.6f, camH * 1.6f, 1f);

            SpriteRenderer dimSr = _boardDimOverlay.AddComponent<SpriteRenderer>();
            dimSr.sprite = SpriteGenerator.GetShardSprite();
            dimSr.sortingOrder = 40;
            dimSr.color = new Color(0.02f, 0.02f, 0.08f, 0.72f);

            if (_selectedSourcePiece != null && (validPieces == null || !validPieces.Contains(_selectedSourcePiece)))
            {
                ElevateObjectRenderers(_selectedSourcePiece.gameObject, 45);
            }

            if (validPieces != null)
            {
                for (int i = 0; i < validPieces.Count; i++)
                {
                    ElevateObjectRenderers(validPieces[i] != null ? validPieces[i].gameObject : null, 42);
                }
            }

            if (validGates != null)
            {
                for (int i = 0; i < validGates.Count; i++)
                {
                    ElevateObjectRenderers(validGates[i] != null ? validGates[i].gameObject : null, 42);
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

        private IEnumerator CoMagnetCrackIce(PuzzlePiece frozenPiece)
        {
            if (frozenPiece == null)
            {
                _phase = MagnetPhase.Idle;
                _executionCoroutine = null;
                yield break;
            }

            Vector3 targetPos = frozenPiece.transform.position;
            GameObject fxObj = new GameObject("MagnetIceCrackFX");
            fxObj.transform.position = targetPos + new Vector3(0.55f, 0.85f, -0.9f);
            _transientWorldObjects.Add(fxObj);

            SpriteRenderer iconSr = fxObj.AddComponent<SpriteRenderer>();
            iconSr.sprite = _magnetIconSprite != null ? _magnetIconSprite : GemSpriteManager.GetBoosterSprite("booster2");
            iconSr.sortingOrder = 98;
            float baseScale = 1f;
            if (iconSr.sprite != null && iconSr.sprite.rect.width > 0f)
            {
                baseScale = 0.88f / (iconSr.sprite.rect.width / iconSr.sprite.pixelsPerUnit);
            }
            fxObj.transform.localScale = Vector3.one * baseScale * 0.5f;

            SoundManager.Instance?.PlayMagnetPull();

            float swoopDur = 0.18f;
            float elapsed = 0f;
            Vector3 startPos = fxObj.transform.position;
            Vector3 strikePos = targetPos + new Vector3(0f, 0.25f, -0.9f);
            while (elapsed < swoopDur && fxObj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / swoopDur);
                fxObj.transform.position = Vector3.Lerp(startPos, strikePos, EaseOutBack(t));
                fxObj.transform.localScale = Vector3.one * baseScale * Mathf.Lerp(0.5f, 1.15f, t);
                fxObj.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-24f, 8f, t));
                yield return null;
            }

            if (frozenPiece != null)
            {
                SoundManager.Instance?.PlayIceCrack(0.85f, 1.18f);
                frozenPiece.DecrementIceCount();
                SpawnWorldSparkBurst(targetPos, new Color(0.45f, 0.94f, 1.0f, 0.95f));
            }

            yield return new WaitForSeconds(0.28f);

            if (fxObj != null)
            {
                _transientWorldObjects.Remove(fxObj);
                Destroy(fxObj);
            }

            _phase = MagnetPhase.Idle;
            _executionCoroutine = null;
        }

        private IEnumerator CoMagnetExecuteMerge(PuzzlePiece source, PuzzlePiece target)
        {
            if (source == null || target == null || LevelManager.Instance == null)
            {
                _phase = MagnetPhase.Idle;
                _executionCoroutine = null;
                yield break;
            }

            // Brief pull-surge pulse on floating magnet above source
            if (_floatingMagnetObj != null)
            {
                Vector3 basePos = _floatingMagnetObj.transform.position;
                float surgeDur = 0.14f;
                float elapsed = 0f;
                while (elapsed < surgeDur && _floatingMagnetObj != null)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / surgeDur);
                    _floatingMagnetObj.transform.position = basePos + new Vector3(0f, 0.28f * t, 0f);
                    _floatingMagnetObj.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.25f, t);
                    yield return null;
                }
            }

            DestroyFloatingMagnet();
            SpawnWorldSparkBurst(source.transform.position, new Color(1.0f, 0.88f, 0.30f, 0.95f));

            bool finished = false;
            LevelManager.Instance.ExecuteMagnetJumpMerge(source, target, () => finished = true);
            while (!finished)
            {
                yield return null;
            }

            _phase = MagnetPhase.Idle;
            _executionCoroutine = null;
        }

        private IEnumerator CoMagnetExecuteEscape(PuzzlePiece source, ExitGateView gate)
        {
            if (source == null || gate == null || LevelManager.Instance == null)
            {
                _phase = MagnetPhase.Idle;
                _executionCoroutine = null;
                yield break;
            }

            if (_floatingMagnetObj != null)
            {
                Vector3 basePos = _floatingMagnetObj.transform.position;
                float surgeDur = 0.12f;
                float elapsed = 0f;
                while (elapsed < surgeDur && _floatingMagnetObj != null)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / surgeDur);
                    _floatingMagnetObj.transform.position = basePos + new Vector3(0f, 0.24f * t, 0f);
                    _floatingMagnetObj.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.22f, t);
                    yield return null;
                }
            }

            DestroyFloatingMagnet();
            SpawnWorldSparkBurst(gate.transform.position, new Color(0.40f, 0.95f, 1.0f, 0.95f));

            bool finished = false;
            LevelManager.Instance.ExecuteMagnetJumpEscape(source, gate, () => finished = true);
            while (!finished)
            {
                yield return null;
            }

            _phase = MagnetPhase.Idle;
            _executionCoroutine = null;
        }

        private void SpawnWorldSparkBurst(Vector3 worldPos, Color burstColor)
        {
            StartCoroutine(CoWorldSparkBurst(worldPos, burstColor));
        }

        private IEnumerator CoWorldSparkBurst(Vector3 worldPos, Color burstColor)
        {
            GameObject obj = new GameObject("MagnetWorldBurst");
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
                if (_magnetButtonRect != null && _promptRoot != null)
                {
                    _buttonHaloRect.anchoredPosition = WorldRectCenterToLocal(_magnetButtonRect, _promptRoot.GetComponent<RectTransform>());
                }
                float s = 1.0f + 0.12f * Mathf.Sin(_fxAnimTime * 4.2f);
                _buttonHaloRect.localScale = new Vector3(s, s, 1f);
                Color c = _buttonHaloImg.color;
                c.a = 0.58f + 0.24f * Mathf.Sin(_fxAnimTime * 4.2f);
                _buttonHaloImg.color = c;
            }

            if (_floatingMagnetObj != null && _selectedSourcePiece != null)
            {
                Vector3 basePos = _selectedSourcePiece.transform.position + new Vector3(0f, 0.72f, -0.8f);
                _floatingMagnetObj.transform.position = basePos + new Vector3(0f, Mathf.Sin(_fxAnimTime * 4.8f) * 0.08f, 0f);
                _floatingMagnetObj.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_fxAnimTime * 3.5f) * 8f);
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
