using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.UI
{
    public class FreezeBoosterEffectController : MonoBehaviour
    {
        private Canvas _rootCanvas;
        private RectTransform _rootRect;

        // First-time Level 8 tutorial overlay
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

        // Freeze gameplay effect layer
        private RectTransform _effectLayer;
        private CanvasGroup _effectLayerGroup;
        private CanvasGroup _screenFrostGroup;
        private Image _screenFrostImg;
        private CanvasGroup _ambientSnowflakesGroup;
        private readonly List<AmbientSnowflake> _ambientSnowflakes = new List<AmbientSnowflake>();

        // Timer banner frost overlay
        private RectTransform _boundTimerBannerRect;
        private RectTransform _timerFrostRoot;
        private CanvasGroup _timerFrostGroup;
        private Image _timerFrostGlowImg;
        private RectTransform _timerFrostCornerLeft;
        private RectTransform _timerFrostCornerRight;

        // Transient activation/melt state
        private readonly List<GameObject> _transientObjects = new List<GameObject>();
        private Coroutine _activationCoroutine;
        private Coroutine _meltCoroutine;
        private Coroutine _timerPunchCoroutine;
        private bool _isActivating = false;
        private bool _isFreezeActive = false;
        private float _freezePulseTime = 0f;

        private struct AmbientSnowflake
        {
            public RectTransform Rect;
            public Image Img;
            public Vector2 BasePos;
            public float SpeedY;
            public float SwayAmp;
            public float SwayFreq;
            public float RotSpeed;
            public float Phase;
            public float BaseScale;
            public float BaseAlpha;
            public int Zone; // 0: top, 1: left, 2: right, 3: bottom
        }

        public bool IsTutorialShowing => _tutorialRoot != null && _tutorialRoot.activeSelf;
        public bool IsActivating => _isActivating;
        public bool IsFreezeVisualActive => _isFreezeActive || _isActivating;

        private void Awake()
        {
            EnsureRootReferences();
        }

        private bool _isDestroying = false;

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

        public void ShowFirstTimeTutorial(RectTransform freezeButtonRect, Sprite boosterIconSprite)
        {
            if (freezeButtonRect == null || IsTutorialShowing) return;

            EnsureRootReferences();
            EnsureTutorialOverlay(boosterIconSprite);

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

            // Elevate the existing Freeze booster button without changing its sprite/shape/parent
            RemoveSpotlightComponents();
            _spotlightButtonRect = freezeButtonRect;
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
            Vector2 btnLocal = WorldRectCenterToLocal(freezeButtonRect, tutRect);
            if (_tutorialSpotlightGlowRect != null)
            {
                _tutorialSpotlightGlowRect.anchoredPosition = btnLocal;
            }
            if (_tutorialHandAnchorRect != null)
            {
                _tutorialHandAnchorRect.anchoredPosition = btnLocal;
            }
            if (_tutorialCardIconImg != null && boosterIconSprite != null)
            {
                _tutorialCardIconImg.sprite = boosterIconSprite;
            }

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

            _tutorialRoot = new GameObject("FreezeTutorialOverlay", typeof(RectTransform));
            _tutorialRoot.transform.SetParent(transform, false);
            RectTransform rootRect = _tutorialRoot.GetComponent<RectTransform>();
            StretchFull(rootRect);

            _tutorialCanvas = _tutorialRoot.AddComponent<Canvas>();
            _tutorialCanvas.overrideSorting = true;
            _tutorialRoot.AddComponent<GraphicRaycaster>();
            _tutorialCanvasGroup = _tutorialRoot.AddComponent<CanvasGroup>();
            _tutorialCanvasGroup.alpha = 0f;

            // Dark scrim blocking board interactions while focusing on Freeze booster
            GameObject scrimObj = new GameObject("Scrim", typeof(RectTransform));
            scrimObj.transform.SetParent(_tutorialRoot.transform, false);
            StretchFull(scrimObj.GetComponent<RectTransform>());
            Image scrimImg = scrimObj.AddComponent<Image>();
            scrimImg.color = new Color(0.02f, 0.03f, 0.09f, 0.74f);
            scrimImg.raycastTarget = true;

            // Radial cyan spotlight glow behind the Freeze button
            GameObject glowObj = new GameObject("SpotlightGlow", typeof(RectTransform));
            glowObj.transform.SetParent(_tutorialRoot.transform, false);
            _tutorialSpotlightGlowRect = glowObj.GetComponent<RectTransform>();
            _tutorialSpotlightGlowRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialSpotlightGlowRect.sizeDelta = new Vector2(340f, 340f);
            _tutorialSpotlightGlowImg = glowObj.AddComponent<Image>();
            _tutorialSpotlightGlowImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _tutorialSpotlightGlowImg.color = new Color(0.32f, 0.88f, 1.0f, 0.85f);
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

            // Booster icon inside tooltip card (preserving user's booster1 sprite)
            GameObject cardIconObj = new GameObject("BoosterPreviewIcon", typeof(RectTransform));
            cardIconObj.transform.SetParent(cardObj.transform, false);
            _tutorialCardIconRect = cardIconObj.GetComponent<RectTransform>();
            _tutorialCardIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.pivot = new Vector2(0.5f, 0.5f);
            _tutorialCardIconRect.anchoredPosition = new Vector2(-268f, 0f);
            _tutorialCardIconRect.sizeDelta = new Vector2(128f, 128f);

            _tutorialCardIconImg = cardIconObj.AddComponent<Image>();
            _tutorialCardIconImg.sprite = boosterIconSprite != null ? boosterIconSprite : GemSpriteManager.GetBoosterSprite("booster1");
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
            txt.text = "Use the booster to freeze\nthe timer for 15 seconds";
            txt.font = font;
            txt.fontSize = 36;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 24;
            txt.resizeTextMaxSize = 38;
            txt.color = new Color(0.24f, 0.16f, 0.50f);
            txt.raycastTarget = false;

            // Animated hand pointer layer above spotlighted Freeze button (non-blocking)
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
            _tutorialTapRippleImg.color = new Color(0.45f, 0.94f, 1.0f, 0f);
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

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (IsTutorialShowing)
            {
                UpdateTutorialIdleAnimation(Time.unscaledDeltaTime);
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

            // Looping tap animation on hand + cyan ripple
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

        public void PlayFreezeActivation(
            RectTransform freezeButtonRect,
            RectTransform timerBannerRect,
            Sprite boosterIconSprite,
            Action onImpactTimerBanner)
        {
            if (_activationCoroutine != null)
            {
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
            }
            if (_meltCoroutine != null)
            {
                StopCoroutine(_meltCoroutine);
                _meltCoroutine = null;
            }
            if (_timerPunchCoroutine != null)
            {
                StopCoroutine(_timerPunchCoroutine);
                _timerPunchCoroutine = null;
            }

            ClearTransientObjects();
            EnsureEffectLayer();
            EnsureTimerFrostOverlay(timerBannerRect);

            if (timerBannerRect != null)
            {
                timerBannerRect.localScale = Vector3.one;
            }
            _boundTimerBannerRect = timerBannerRect;
            _isActivating = true;

            _activationCoroutine = StartCoroutine(CoFreezeActivationSequence(
                freezeButtonRect,
                timerBannerRect,
                boosterIconSprite,
                onImpactTimerBanner));
        }

        public void UpdateFreezeRemaining(float remainingSeconds, float totalDuration)
        {
            if (!IsFreezeVisualActive) return;

            float dt = Time.deltaTime;
            _freezePulseTime += dt;

            // Gentle breathing on frost screen vignette, with subtle shimmer in final 2.5s
            if (_screenFrostGroup != null && _screenFrostImg != null && _screenFrostImg.gameObject.activeSelf)
            {
                float basePulse = 0.88f + 0.08f * Mathf.Sin(_freezePulseTime * 2.4f);
                if (remainingSeconds <= 2.5f && remainingSeconds > 0f)
                {
                    float urgent = 0.72f + 0.28f * Mathf.Sin(_freezePulseTime * 10f);
                    _screenFrostGroup.alpha = Mathf.Clamp01(basePulse * urgent);
                }
                else
                {
                    _screenFrostGroup.alpha = Mathf.Clamp01(basePulse);
                }
            }

            // Ambient perimeter snowflakes drifting and twinkling
            if (_ambientSnowflakesGroup != null && _ambientSnowflakesGroup.gameObject.activeSelf && _effectLayer != null)
            {
                float halfW = _effectLayer.rect.width > 100f ? _effectLayer.rect.width * 0.5f : 540f;
                float halfH = _effectLayer.rect.height > 100f ? _effectLayer.rect.height * 0.5f : 960f;

                for (int i = 0; i < _ambientSnowflakes.Count; i++)
                {
                    AmbientSnowflake sf = _ambientSnowflakes[i];
                    if (sf.Rect == null || sf.Img == null) continue;

                    Vector2 pos = sf.Rect.anchoredPosition;
                    pos.y -= sf.SpeedY * dt;
                    float sway = Mathf.Sin(_freezePulseTime * sf.SwayFreq + sf.Phase) * sf.SwayAmp * dt;
                    pos.x += sway;

                    // Wrap within assigned perimeter zone
                    if (sf.Zone == 0 && pos.y < halfH - 420f) pos.y = halfH - 20f;
                    else if (sf.Zone == 3 && pos.y < -halfH - 40f) pos.y = -halfH + 340f;
                    else if ((sf.Zone == 1 || sf.Zone == 2) && pos.y < -halfH + 140f) pos.y = halfH - 180f;

                    if (sf.Zone == 1 && pos.x > -halfW + 170f) pos.x = -halfW + 30f;
                    if (sf.Zone == 2 && pos.x < halfW - 170f) pos.x = halfW - 30f;

                    sf.Rect.anchoredPosition = pos;
                    sf.Rect.Rotate(0f, 0f, sf.RotSpeed * dt);

                    float twinkle = 0.65f + 0.35f * Mathf.Sin(_freezePulseTime * 3.2f + sf.Phase);
                    Color c = sf.Img.color;
                    c.a = sf.BaseAlpha * twinkle;
                    sf.Img.color = c;
                }
            }

            // Timer banner frost breathing & rotating corner snowflakes
            if (_timerFrostRoot != null && _timerFrostRoot.gameObject.activeSelf)
            {
                if (_timerFrostGlowImg != null)
                {
                    Color gc = _timerFrostGlowImg.color;
                    gc.a = 0.52f + 0.22f * Mathf.Sin(_freezePulseTime * 3.1f);
                    _timerFrostGlowImg.color = gc;
                }
                if (_timerFrostCornerLeft != null)
                {
                    _timerFrostCornerLeft.Rotate(0f, 0f, 22f * dt);
                }
                if (_timerFrostCornerRight != null)
                {
                    _timerFrostCornerRight.Rotate(0f, 0f, -24f * dt);
                }
            }
        }

        public void PlayFreezeMeltOut()
        {
            if (!_isFreezeActive && !_isActivating) return;
            if (_meltCoroutine != null) StopCoroutine(_meltCoroutine);
            _meltCoroutine = StartCoroutine(CoMeltOutSequence());
        }

        public void StopImmediate()
        {
            DismissTutorial();
            StopFreezeEffectsOnly();
        }

        public void StopFreezeEffectsOnly()
        {
            if (_activationCoroutine != null)
            {
                StopCoroutine(_activationCoroutine);
                _activationCoroutine = null;
            }
            if (_meltCoroutine != null)
            {
                StopCoroutine(_meltCoroutine);
                _meltCoroutine = null;
            }
            if (_timerPunchCoroutine != null)
            {
                StopCoroutine(_timerPunchCoroutine);
                _timerPunchCoroutine = null;
            }

            ClearTransientObjects();

            if (_boundTimerBannerRect != null)
            {
                _boundTimerBannerRect.localScale = Vector3.one;
            }

            if (_screenFrostGroup != null)
            {
                _screenFrostGroup.alpha = 0f;
                _screenFrostGroup.gameObject.SetActive(false);
            }
            if (_ambientSnowflakesGroup != null)
            {
                _ambientSnowflakesGroup.alpha = 0f;
                _ambientSnowflakesGroup.gameObject.SetActive(false);
            }
            if (_timerFrostGroup != null)
            {
                _timerFrostGroup.alpha = 0f;
                _timerFrostGroup.gameObject.SetActive(false);
            }

            _isActivating = false;
            _isFreezeActive = false;
        }

        private void EnsureEffectLayer()
        {
            if (_effectLayer == null)
            {
                GameObject layerObj = new GameObject("FreezeEffectLayer", typeof(RectTransform));
                layerObj.transform.SetParent(transform, false);
                _effectLayer = layerObj.GetComponent<RectTransform>();
                StretchFull(_effectLayer);

                _effectLayerGroup = layerObj.AddComponent<CanvasGroup>();
                _effectLayerGroup.blocksRaycasts = false;
                _effectLayerGroup.interactable = false;

                // Keep effect layer above BoosterBar/board, below Pause/Victory/TimeOut modals
                Transform boosterBar = transform.Find("BoosterBar");
                if (boosterBar != null)
                {
                    _effectLayer.SetSiblingIndex(boosterBar.GetSiblingIndex() + 1);
                }
                else
                {
                    Transform pausePanel = transform.Find("PausePanel");
                    if (pausePanel != null)
                    {
                        _effectLayer.SetSiblingIndex(Mathf.Max(0, pausePanel.GetSiblingIndex()));
                    }
                }
            }

            if (_screenFrostImg == null)
            {
                GameObject frostObj = new GameObject("ScreenFrostBorder", typeof(RectTransform));
                frostObj.transform.SetParent(_effectLayer, false);
                StretchFull(frostObj.GetComponent<RectTransform>());
                _screenFrostImg = frostObj.AddComponent<Image>();
                _screenFrostImg.sprite = SpriteGenerator.GetFreezeScreenBorderSprite();
                _screenFrostImg.color = Color.white;
                _screenFrostImg.raycastTarget = false;

                _screenFrostGroup = frostObj.AddComponent<CanvasGroup>();
                _screenFrostGroup.alpha = 0f;
                _screenFrostGroup.blocksRaycasts = false;
                frostObj.SetActive(false);
            }

            if (_ambientSnowflakesGroup == null)
            {
                GameObject snowRoot = new GameObject("AmbientSnowflakes", typeof(RectTransform));
                snowRoot.transform.SetParent(_effectLayer, false);
                StretchFull(snowRoot.GetComponent<RectTransform>());
                _ambientSnowflakesGroup = snowRoot.AddComponent<CanvasGroup>();
                _ambientSnowflakesGroup.alpha = 0f;
                _ambientSnowflakesGroup.blocksRaycasts = false;

                BuildAmbientSnowflakes(snowRoot.transform);
                snowRoot.SetActive(false);
            }
        }

        private void BuildAmbientSnowflakes(Transform parent)
        {
            _ambientSnowflakes.Clear();
            float halfW = _effectLayer.rect.width > 100f ? _effectLayer.rect.width * 0.5f : 540f;
            float halfH = _effectLayer.rect.height > 100f ? _effectLayer.rect.height * 0.5f : 960f;

            Sprite flakeSprite = SpriteGenerator.GetSnowflakeSprite();
            Sprite flareSprite = SpriteGenerator.GetStarFlareSprite();

            // 22 perimeter snowflakes across top (0..6), left (7..12), right (13..18), bottom (19..21)
            for (int i = 0; i < 22; i++)
            {
                int zone;
                Vector2 pos;
                if (i < 7)
                {
                    zone = 0;
                    pos = new Vector2(UnityEngine.Random.Range(-halfW + 40f, halfW - 40f), UnityEngine.Random.Range(halfH - 360f, halfH - 25f));
                }
                else if (i < 13)
                {
                    zone = 1;
                    pos = new Vector2(UnityEngine.Random.Range(-halfW + 20f, -halfW + 150f), UnityEngine.Random.Range(-halfH + 200f, halfH - 200f));
                }
                else if (i < 19)
                {
                    zone = 2;
                    pos = new Vector2(UnityEngine.Random.Range(halfW - 150f, halfW - 20f), UnityEngine.Random.Range(-halfH + 200f, halfH - 200f));
                }
                else
                {
                    zone = 3;
                    pos = new Vector2(UnityEngine.Random.Range(-halfW + 50f, halfW - 50f), UnityEngine.Random.Range(-halfH + 35f, -halfH + 310f));
                }

                GameObject sfObj = new GameObject($"Snowflake_{i}", typeof(RectTransform));
                sfObj.transform.SetParent(parent, false);
                RectTransform r = sfObj.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = pos;
                bool useFlake = (i % 3 != 2);
                float size = useFlake ? UnityEngine.Random.Range(26f, 54f) : UnityEngine.Random.Range(22f, 40f);
                r.sizeDelta = new Vector2(size, size);

                Image img = sfObj.AddComponent<Image>();
                img.sprite = useFlake ? flakeSprite : flareSprite;
                img.preserveAspect = true;
                float baseAlpha = useFlake ? UnityEngine.Random.Range(0.55f, 0.90f) : UnityEngine.Random.Range(0.45f, 0.80f);
                img.color = new Color(0.84f, 0.97f, 1.0f, baseAlpha);
                img.raycastTarget = false;

                _ambientSnowflakes.Add(new AmbientSnowflake
                {
                    Rect = r,
                    Img = img,
                    BasePos = pos,
                    SpeedY = UnityEngine.Random.Range(14f, 34f),
                    SwayAmp = UnityEngine.Random.Range(8f, 20f),
                    SwayFreq = UnityEngine.Random.Range(1.0f, 2.2f),
                    RotSpeed = UnityEngine.Random.Range(-40f, 40f),
                    Phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                    BaseScale = 1f,
                    BaseAlpha = baseAlpha,
                    Zone = zone
                });
            }
        }

        private void EnsureTimerFrostOverlay(RectTransform timerBannerRect)
        {
            if (timerBannerRect == null) return;

            Transform existing = timerBannerRect.Find("TimerFrostOverlay");
            if (existing != null)
            {
                _timerFrostRoot = existing.GetComponent<RectTransform>();
                _timerFrostGroup = existing.GetComponent<CanvasGroup>();
                _timerFrostGlowImg = existing.Find("FrostGlow")?.GetComponent<Image>();
                _timerFrostCornerLeft = existing.Find("CornerFlakeLeft")?.GetComponent<RectTransform>();
                _timerFrostCornerRight = existing.Find("CornerFlakeRight")?.GetComponent<RectTransform>();
                return;
            }

            GameObject root = new GameObject("TimerFrostOverlay", typeof(RectTransform));
            root.transform.SetParent(timerBannerRect, false);
            _timerFrostRoot = root.GetComponent<RectTransform>();
            _timerFrostRoot.anchorMin = Vector2.zero;
            _timerFrostRoot.anchorMax = Vector2.one;
            _timerFrostRoot.offsetMin = new Vector2(-18f, -34f);
            _timerFrostRoot.offsetMax = new Vector2(18f, 16f);

            _timerFrostGroup = root.AddComponent<CanvasGroup>();
            _timerFrostGroup.alpha = 0f;
            _timerFrostGroup.blocksRaycasts = false;

            // Outer icy cyan glow halo
            GameObject glowObj = new GameObject("FrostGlow", typeof(RectTransform));
            glowObj.transform.SetParent(_timerFrostRoot, false);
            RectTransform glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-28f, -18f);
            glowRect.offsetMax = new Vector2(28f, 22f);
            _timerFrostGlowImg = glowObj.AddComponent<Image>();
            _timerFrostGlowImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            _timerFrostGlowImg.color = new Color(0.32f, 0.90f, 1.0f, 0.65f);
            _timerFrostGlowImg.raycastTarget = false;

            // Frost cap & hanging icicles frame around Banner_Time (transparent middle keeps TimeVal crisp)
            GameObject frameObj = new GameObject("FrostFrame", typeof(RectTransform));
            frameObj.transform.SetParent(_timerFrostRoot, false);
            StretchFull(frameObj.GetComponent<RectTransform>());
            Image frameImg = frameObj.AddComponent<Image>();
            frameImg.sprite = SpriteGenerator.GetTimerFrostFrameSprite();
            frameImg.color = Color.white;
            frameImg.raycastTarget = false;

            // Two rotating corner snowflakes
            Sprite flake = SpriteGenerator.GetSnowflakeSprite();
            _timerFrostCornerLeft = CreateCornerSnowflake(_timerFrostRoot, "CornerFlakeLeft", new Vector2(0f, 1f), new Vector2(22f, -16f), 42f, flake);
            _timerFrostCornerRight = CreateCornerSnowflake(_timerFrostRoot, "CornerFlakeRight", new Vector2(1f, 0.5f), new Vector2(-18f, 18f), 36f, flake);

            root.transform.SetAsLastSibling();
            root.SetActive(false);
        }

        private RectTransform CreateCornerSnowflake(Transform parent, string objName, Vector2 anchor, Vector2 offset, float size, Sprite flakeSprite)
        {
            GameObject obj = new GameObject(objName, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(size, size);

            Image img = obj.AddComponent<Image>();
            img.sprite = flakeSprite;
            img.preserveAspect = true;
            img.color = new Color(0.92f, 0.99f, 1.0f, 0.95f);
            img.raycastTarget = false;
            return rt;
        }

        private IEnumerator CoFreezeActivationSequence(
            RectTransform freezeButtonRect,
            RectTransform timerBannerRect,
            Sprite boosterIconSprite,
            Action onImpactTimerBanner)
        {
            Vector2 startPos = freezeButtonRect != null
                ? WorldRectCenterToLocal(freezeButtonRect, _effectLayer)
                : new Vector2(-280f, -680f);
            Vector2 centerPos = new Vector2(0f, 70f);
            Vector2 timerPos = timerBannerRect != null
                ? WorldRectCenterToLocal(timerBannerRect, _effectLayer)
                : new Vector2(0f, 760f);

            // Spawn flying stopwatch + child ice shell
            GameObject flyObj = new GameObject("FlyingFreezeBooster", typeof(RectTransform));
            flyObj.transform.SetParent(_effectLayer, false);
            _transientObjects.Add(flyObj);

            RectTransform flyRect = flyObj.GetComponent<RectTransform>();
            flyRect.anchorMin = new Vector2(0.5f, 0.5f);
            flyRect.anchorMax = new Vector2(0.5f, 0.5f);
            flyRect.pivot = new Vector2(0.5f, 0.5f);
            flyRect.anchoredPosition = startPos;
            flyRect.sizeDelta = new Vector2(260f, 260f);
            flyRect.localScale = Vector3.one * 0.48f;

            GameObject auraObj = new GameObject("FlyAura", typeof(RectTransform));
            auraObj.transform.SetParent(flyRect, false);
            RectTransform auraRect = auraObj.GetComponent<RectTransform>();
            auraRect.anchorMin = new Vector2(0.5f, 0.5f);
            auraRect.anchorMax = new Vector2(0.5f, 0.5f);
            auraRect.pivot = new Vector2(0.5f, 0.5f);
            auraRect.sizeDelta = new Vector2(320f, 320f);
            Image auraImg = auraObj.AddComponent<Image>();
            auraImg.sprite = SpriteGenerator.GetRadialGlowSprite();
            auraImg.color = new Color(0.35f, 0.92f, 1.0f, 0.72f);
            auraImg.raycastTarget = false;

            GameObject iconObj = new GameObject("StopwatchIcon", typeof(RectTransform));
            iconObj.transform.SetParent(flyRect, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(240f, 240f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = boosterIconSprite != null ? boosterIconSprite : GemSpriteManager.GetBoosterSprite("booster1");
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            GameObject shellObj = new GameObject("IceShell", typeof(RectTransform));
            shellObj.transform.SetParent(flyRect, false);
            RectTransform shellRect = shellObj.GetComponent<RectTransform>();
            shellRect.anchorMin = new Vector2(0.5f, 0.5f);
            shellRect.anchorMax = new Vector2(0.5f, 0.5f);
            shellRect.pivot = new Vector2(0.5f, 0.5f);
            shellRect.anchoredPosition = new Vector2(0f, -18f);
            shellRect.sizeDelta = new Vector2(276f, 276f);
            shellRect.localScale = Vector3.zero;
            Image shellImg = shellObj.AddComponent<Image>();
            shellImg.sprite = SpriteGenerator.GetFreezeIceShellSprite();
            shellImg.preserveAspect = true;
            shellImg.color = new Color(1f, 1f, 1f, 0f);
            shellImg.raycastTarget = false;

            SoundManager.Instance?.PlayFreezeTime();

            // Phase 1 (0.00s - 0.34s): Fly from Freeze slot to center board with slight arc + sparkle trail
            float phase1Dur = 0.34f;
            float elapsed = 0f;
            int trailSpawned1 = 0;
            Vector2 arcCtrl = new Vector2((startPos.x + centerPos.x) * 0.5f - 65f, Mathf.Lerp(startPos.y, centerPos.y, 0.55f) + 120f);
            while (elapsed < phase1Dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase1Dur);
                float eased = EaseOutCubic(t);
                flyRect.anchoredPosition = QuadBezier(startPos, arcCtrl, centerPos, eased);

                float s = t < 0.75f
                    ? Mathf.Lerp(0.48f, 1.06f, EaseOutBack(t / 0.75f))
                    : Mathf.Lerp(1.06f, 1.00f, (t - 0.75f) / 0.25f);
                flyRect.localScale = Vector3.one * s;
                flyRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-16f, 6f, eased));

                int targetTrail = Mathf.FloorToInt(t * 6f);
                while (trailSpawned1 < targetTrail)
                {
                    trailSpawned1++;
                    SpawnFrostSparkle(flyRect.anchoredPosition + UnityEngine.Random.insideUnitCircle * 28f, UnityEngine.Random.Range(24f, 44f), 0.32f);
                }

                yield return null;
            }
            flyRect.anchoredPosition = centerPos;
            flyRect.localScale = Vector3.one;

            // Phase 2 (0.34s - 0.80s): Vibrate, crack & encase in ice shell + snowflake burst
            float phase2Dur = 0.46f;
            elapsed = 0f;
            bool icePopped = false;
            while (elapsed < phase2Dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase2Dur);

                float shakeDamp = 1f - t;
                float rotZ = Mathf.Sin(elapsed * 54f) * 11f * shakeDamp;
                flyRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                if (!icePopped && elapsed >= 0.12f)
                {
                    icePopped = true;
                    SoundManager.Instance?.PlayIceCrack(0.55f, 1.38f);
                    SpawnIcyShockwave(centerPos);
                    SpawnSnowflakeBurst(centerPos, 16, 130f, 290f);
                }

                if (icePopped)
                {
                    float st = Mathf.Clamp01((elapsed - 0.12f) / 0.22f);
                    float ss = st < 0.72f
                        ? Mathf.Lerp(0.25f, 1.12f, EaseOutBack(st / 0.72f))
                        : Mathf.Lerp(1.12f, 1.0f, (st - 0.72f) / 0.28f);
                    shellRect.localScale = Vector3.one * ss;
                    shellImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.96f, st));
                }

                yield return null;
            }

            flyRect.localRotation = Quaternion.identity;
            shellRect.localScale = Vector3.one;
            shellImg.color = new Color(1f, 1f, 1f, 0.96f);

            // Phase 3 (0.80s - 1.10s): Zip frosted stopwatch up into Banner_Time
            float phase3Dur = 0.30f;
            elapsed = 0f;
            int trailSpawned3 = 0;
            Vector2 zipCtrl = new Vector2((centerPos.x + timerPos.x) * 0.5f + 40f, Mathf.Lerp(centerPos.y, timerPos.y, 0.55f));
            while (elapsed < phase3Dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / phase3Dur);
                float eased = EaseInCubic(t);
                flyRect.anchoredPosition = QuadBezier(centerPos, zipCtrl, timerPos, eased);

                float s = Mathf.Lerp(1.0f, 0.34f, eased);
                flyRect.localScale = Vector3.one * s;
                flyRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -14f, eased));

                int targetTrail = Mathf.FloorToInt(t * 5f);
                while (trailSpawned3 < targetTrail)
                {
                    trailSpawned3++;
                    SpawnFrostSparkle(flyRect.anchoredPosition + UnityEngine.Random.insideUnitCircle * 20f, UnityEngine.Random.Range(22f, 38f), 0.28f);
                }

                yield return null;
            }

            _transientObjects.Remove(flyObj);
            Destroy(flyObj);

            // Impact at Banner_Time
            _isActivating = false;
            _isFreezeActive = true;
            _freezePulseTime = 0f;
            onImpactTimerBanner?.Invoke();

            if (timerBannerRect != null)
            {
                _timerPunchCoroutine = StartCoroutine(CoPunchRect(timerBannerRect, 1.15f, 0.24f));
            }
            SpawnIcyImpactFlash(timerPos);
            SpawnSnowflakeBurst(timerPos, 12, 85f, 210f);

            // Fade in screen frost border, ambient perimeter snowflakes & timer frost overlay
            if (_screenFrostImg != null) _screenFrostImg.gameObject.SetActive(true);
            if (_ambientSnowflakesGroup != null) _ambientSnowflakesGroup.gameObject.SetActive(true);
            if (_timerFrostRoot != null)
            {
                _timerFrostRoot.gameObject.SetActive(true);
                _timerFrostRoot.SetAsLastSibling();
            }

            float fadeInDur = 0.25f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeInDur)
            {
                fadeElapsed += Time.deltaTime;
                float ft = Mathf.Clamp01(fadeElapsed / fadeInDur);
                if (_screenFrostGroup != null) _screenFrostGroup.alpha = ft;
                if (_ambientSnowflakesGroup != null) _ambientSnowflakesGroup.alpha = ft;
                if (_timerFrostGroup != null) _timerFrostGroup.alpha = ft;
                yield return null;
            }

            if (_screenFrostGroup != null) _screenFrostGroup.alpha = 1f;
            if (_ambientSnowflakesGroup != null) _ambientSnowflakesGroup.alpha = 1f;
            if (_timerFrostGroup != null) _timerFrostGroup.alpha = 1f;
            _activationCoroutine = null;
        }

        private IEnumerator CoMeltOutSequence()
        {
            float startScreenA = _screenFrostGroup != null ? _screenFrostGroup.alpha : 0f;
            float startSnowA = _ambientSnowflakesGroup != null ? _ambientSnowflakesGroup.alpha : 0f;
            float startTimerA = _timerFrostGroup != null ? _timerFrostGroup.alpha : 0f;

            float dur = 0.45f;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / dur);
                if (_screenFrostGroup != null) _screenFrostGroup.alpha = startScreenA * t;
                if (_ambientSnowflakesGroup != null) _ambientSnowflakesGroup.alpha = startSnowA * t;
                if (_timerFrostGroup != null) _timerFrostGroup.alpha = startTimerA * t;
                yield return null;
            }

            if (_screenFrostGroup != null)
            {
                _screenFrostGroup.alpha = 0f;
                _screenFrostGroup.gameObject.SetActive(false);
            }
            if (_ambientSnowflakesGroup != null)
            {
                _ambientSnowflakesGroup.alpha = 0f;
                _ambientSnowflakesGroup.gameObject.SetActive(false);
            }
            if (_timerFrostGroup != null)
            {
                _timerFrostGroup.alpha = 0f;
                _timerFrostGroup.gameObject.SetActive(false);
            }

            _isFreezeActive = false;
            _meltCoroutine = null;
        }

        private IEnumerator CoPunchRect(RectTransform target, float maxScale, float duration)
        {
            if (target == null) yield break;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = 1.0f + Mathf.Sin(t * Mathf.PI) * (maxScale - 1.0f);
                target.localScale = Vector3.one * s;
                yield return null;
            }
            target.localScale = Vector3.one;
            _timerPunchCoroutine = null;
        }

        private void SpawnFrostSparkle(Vector2 localPos, float size, float duration)
        {
            StartCoroutine(CoTransientParticle(localPos, Vector2.zero, size, duration,
                SpriteGenerator.GetStarFlareSprite(), new Color(0.75f, 0.96f, 1.0f, 0.92f), UnityEngine.Random.Range(-180f, 180f)));
        }

        private void SpawnIcyShockwave(Vector2 localPos)
        {
            StartCoroutine(CoExpandingRing(localPos, 70f, 390f, 0.34f, new Color(0.55f, 0.95f, 1.0f, 0.90f)));
        }

        private void SpawnIcyImpactFlash(Vector2 localPos)
        {
            StartCoroutine(CoExpandingRing(localPos, 90f, 320f, 0.28f, new Color(0.72f, 0.98f, 1.0f, 0.95f)));
        }

        private void SpawnSnowflakeBurst(Vector2 origin, int count, float minSpeed, float maxSpeed)
        {
            Sprite flake = SpriteGenerator.GetSnowflakeSprite();
            Sprite flare = SpriteGenerator.GetStarFlareSprite();
            Sprite shard = SpriteGenerator.GetShardSprite();

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.22f, 0.22f);
                float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                Sprite sp = (i % 3 == 0) ? flake : ((i % 3 == 1) ? flare : shard);
                float size = (i % 3 == 0) ? UnityEngine.Random.Range(34f, 56f) : UnityEngine.Random.Range(20f, 34f);
                Color col = (i % 2 == 0) ? new Color(0.88f, 0.99f, 1.0f, 0.98f) : new Color(0.45f, 0.92f, 1.0f, 0.95f);
                float dur = UnityEngine.Random.Range(0.36f, 0.56f);
                float rotSpeed = UnityEngine.Random.Range(-260f, 260f);

                StartCoroutine(CoTransientParticle(origin, vel, size, dur, sp, col, rotSpeed));
            }
        }

        private IEnumerator CoTransientParticle(Vector2 startPos, Vector2 velocity, float size, float duration, Sprite sprite, Color baseColor, float rotSpeed)
        {
            if (_effectLayer == null) yield break;
            GameObject obj = new GameObject("FrostParticle", typeof(RectTransform));
            obj.transform.SetParent(_effectLayer, false);
            _transientObjects.Add(obj);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = startPos;
            rt.sizeDelta = new Vector2(size, size);

            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.color = baseColor;
            img.raycastTarget = false;

            float elapsed = 0f;
            while (elapsed < duration && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float drag = 1f - 0.55f * t;
                rt.anchoredPosition += velocity * drag * Time.deltaTime;
                rt.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
                float s = Mathf.Lerp(1.0f, 0.35f, t);
                rt.localScale = new Vector3(s, s, 1f);

                Color c = baseColor;
                c.a = baseColor.a * (1f - t * t);
                img.color = c;
                yield return null;
            }

            if (obj != null)
            {
                _transientObjects.Remove(obj);
                Destroy(obj);
            }
        }

        private IEnumerator CoExpandingRing(Vector2 center, float startSize, float endSize, float duration, Color baseColor)
        {
            if (_effectLayer == null) yield break;
            GameObject obj = new GameObject("FrostShockwave", typeof(RectTransform));
            obj.transform.SetParent(_effectLayer, false);
            _transientObjects.Add(obj);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = center;
            rt.sizeDelta = new Vector2(startSize, startSize);

            Image img = obj.AddComponent<Image>();
            img.sprite = SpriteGenerator.GetRadialGlowSprite();
            img.color = baseColor;
            img.raycastTarget = false;

            float elapsed = 0f;
            while (elapsed < duration && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseOutCubic(t);
                float sz = Mathf.Lerp(startSize, endSize, eased);
                rt.sizeDelta = new Vector2(sz, sz);

                Color c = baseColor;
                c.a = baseColor.a * (1f - t);
                img.color = c;
                yield return null;
            }

            if (obj != null)
            {
                _transientObjects.Remove(obj);
                Destroy(obj);
            }
        }

        private void ClearTransientObjects()
        {
            for (int i = _transientObjects.Count - 1; i >= 0; i--)
            {
                if (_transientObjects[i] != null)
                {
                    Destroy(_transientObjects[i]);
                }
            }
            _transientObjects.Clear();
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

        private static Vector2 QuadBezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static float EaseOutCubic(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }

        private static float EaseInCubic(float t)
        {
            return t * t * t;
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
