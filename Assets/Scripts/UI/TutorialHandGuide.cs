using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.UI
{
    public class TutorialHandGuide : MonoBehaviour
    {
        public enum TutorialPhase
        {
            Disabled,
            Merge,
            ExitGate,
            Completed
        }

        private TutorialPhase _currentPhase = TutorialPhase.Disabled;
        public TutorialPhase CurrentPhase => _currentPhase;

        private RectTransform _canvasRect;

        // Banner UI (Canvas)
        private GameObject _bannerObj;
        private RectTransform _bannerRect;
        private Text _bannerText;

        // World-Space Hand Sprite (exact world coordinates matching blocks)
        private GameObject _handRootObj;
        private SpriteRenderer _handSr;

        private const float BaseHandScale = 0.46f;
        private static readonly Vector3 FingerTipToCenterOffset = new Vector3(0.217f, -0.495f, 0f);

        // Animation state
        private float _cycleTimer = 0f;
        private float _idleResumeTimer = 0f;
        private const float CycleDuration = 2.0f;
        private bool _isPlayerInteracting = false;
        private bool _preserveBannerPos = false;

        private void Awake()
        {
            _canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelInitialized += HandleLevelReset;
                LevelManager.Instance.OnRemainingBlocksChanged += HandleRemainingBlocksChanged;
            }

            EvaluatePhase();
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelInitialized -= HandleLevelReset;
                LevelManager.Instance.OnRemainingBlocksChanged -= HandleRemainingBlocksChanged;
            }

            if (_handRootObj != null && !Application.isPlaying)
            {
                DestroyImmediate(_handRootObj);
            }
        }

        public void SetupComponents(GameObject bannerObj, Text bannerText)
        {
            _preserveBannerPos = false;
            if (_canvasRect == null)
            {
                _canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
            }
            _bannerObj = bannerObj;
            if (_bannerObj != null)
            {
                _bannerRect = _bannerObj.GetComponent<RectTransform>();
            }
            _bannerText = bannerText;

            EnsureWorldHand();
            EvaluatePhase();
            PositionBanner();
        }

        public void SetupExistingComponents(GameObject bannerObj, Text bannerText)
        {
            _preserveBannerPos = true;
            if (_canvasRect == null)
            {
                _canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
            }
            _bannerObj = bannerObj;
            if (_bannerObj != null)
            {
                _bannerRect = _bannerObj.GetComponent<RectTransform>();
            }
            _bannerText = bannerText;

            EnsureWorldHand();
            EvaluatePhase();
        }

        private void EnsureWorldHand()
        {
            if (_handRootObj == null)
            {
                GameObject existing = GameObject.Find("TutorialHand_World");
                if (existing != null)
                {
                    _handRootObj = existing;
                    _handSr = _handRootObj.GetComponentInChildren<SpriteRenderer>(true);
                }
            }

            if (_handRootObj == null)
            {
                _handRootObj = new GameObject("TutorialHand_World");
                GameObject spriteChild = new GameObject("HandSprite");
                spriteChild.transform.SetParent(_handRootObj.transform, false);
                spriteChild.transform.localPosition = FingerTipToCenterOffset;
                spriteChild.transform.localScale = new Vector3(BaseHandScale, BaseHandScale, 1f);

                _handSr = spriteChild.AddComponent<SpriteRenderer>();
                _handSr.sprite = GemSpriteManager.GetMainUISprite("UI_hand");
                _handSr.sortingOrder = 95;
            }
            else if (_handSr != null)
            {
                _handSr.sprite = GemSpriteManager.GetMainUISprite("UI_hand");
                _handSr.sortingOrder = 95;
                _handSr.transform.localPosition = FingerTipToCenterOffset;
                _handSr.transform.localScale = new Vector3(BaseHandScale, BaseHandScale, 1f);
            }
        }

        public void HandleLevelReset()
        {
            _cycleTimer = 0f;
            _idleResumeTimer = 0.5f;
            _isPlayerInteracting = false;
            EvaluatePhase();
            if (!_preserveBannerPos) PositionBanner();
        }

        private void HandleRemainingBlocksChanged(int count)
        {
            EvaluatePhase();
        }

        public void EvaluatePhase()
        {
            if (_bannerObj == null)
            {
                var banner = GameObject.Find("InstructionBanner");
                if (banner != null)
                {
                    _bannerObj = banner;
                    _bannerRect = banner.GetComponent<RectTransform>();
                    _bannerText = banner.GetComponentInChildren<Text>(true);
                }
            }

            EnsureWorldHand();

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

            if (lm == null || lm.CurrentLevel != LevelId.Level1)
            {
                SetVisible(false);
                _currentPhase = TutorialPhase.Disabled;
                return;
            }

            if (!Application.isPlaying)
            {
                _currentPhase = TutorialPhase.Merge;
                SetVisible(true);
                if (_bannerText != null) _bannerText.text = "Swipe block to merge";
                PositionInitialHand(lm);
                if (!_preserveBannerPos) PositionBanner();
                return;
            }

            int pieceCount = lm.RemainingBlocksCount;
            if (pieceCount >= 2)
            {
                _currentPhase = TutorialPhase.Merge;
                SetVisible(true);
                if (_bannerText != null) _bannerText.text = "Swipe block to merge";
            }
            else if (pieceCount == 1)
            {
                _currentPhase = TutorialPhase.ExitGate;
                SetVisible(true);
                if (_bannerText != null) _bannerText.text = "Swipe block to exit gate";
                _cycleTimer = 0f;
            }
            else
            {
                _currentPhase = TutorialPhase.Completed;
                SetVisible(false);
            }
        }

        private void PositionInitialHand(LevelManager lm)
        {
            if (_handRootObj == null || lm == null) return;
            Vector3 worldPos = lm.GridToWorld(0, 1);
            _handRootObj.transform.position = new Vector3(worldPos.x, worldPos.y, -2.0f);
            _handRootObj.transform.localScale = Vector3.one;
            _handRootObj.transform.localRotation = Quaternion.identity;
            if (_handSr != null) _handSr.color = Color.white;
        }

        private void SetVisible(bool visible)
        {
            if (_bannerObj != null) _bannerObj.SetActive(visible);
            else
            {
                var b = GameObject.Find("InstructionBanner");
                if (b != null) b.SetActive(visible);
            }
            if (_handRootObj != null) _handRootObj.SetActive(visible);
            else
            {
                var h = GameObject.Find("TutorialHand_World");
                if (h != null) h.SetActive(visible);
            }
        }

        private void Update()
        {
            if (_currentPhase == TutorialPhase.Disabled || _currentPhase == TutorialPhase.Completed)
            {
                return;
            }

            if (_bannerRect != null)
            {
                float breath = 1.0f + Mathf.Sin(Time.time * 2.5f) * 0.022f;
                _bannerRect.localScale = new Vector3(breath, breath, 1f);
            }

            bool isDragging = PlayerPieceController.Instance != null && PlayerPieceController.Instance.IsDragging;
            if (isDragging)
            {
                _isPlayerInteracting = true;
                _idleResumeTimer = 0.7f;
                SetHandAlpha(0f);
                return;
            }

            if (_isPlayerInteracting)
            {
                _idleResumeTimer -= Time.deltaTime;
                if (_idleResumeTimer <= 0f)
                {
                    _isPlayerInteracting = false;
                    _cycleTimer = 0f;
                }
                else
                {
                    SetHandAlpha(0f);
                    return;
                }
            }

            AnimateHandSwipe();
        }

        private void LateUpdate()
        {
            if (!_preserveBannerPos && _currentPhase != TutorialPhase.Disabled && _currentPhase != TutorialPhase.Completed)
            {
                PositionBanner();
            }
        }

        private void PositionBanner()
        {
            if (_bannerRect == null) return;
            Camera cam = Camera.main;
            if (cam == null || _canvasRect == null) return;

            // Convert world Y directly to Canvas local Y based on orthographic camera height
            float camHalfH = cam.orthographicSize;
            float canvasHalfH = _canvasRect.rect.height > 100f ? _canvasRect.rect.height * 0.5f : 960f;
            float worldY = 3.25f - cam.transform.position.y;
            float localY = (worldY / camHalfH) * canvasHalfH;
            _bannerRect.anchoredPosition = new Vector2(0f, localY);
        }

        private void SetHandAlpha(float alpha)
        {
            if (_handSr == null) return;
            Color c = _handSr.color;
            c.a = Mathf.Clamp01(alpha);
            _handSr.color = c;
        }

        private void AnimateHandSwipe()
        {
            if (_handRootObj == null || _handSr == null) return;
            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            List<Vector3> waypoints = GetCurrentWaypoints(lm);
            if (waypoints == null || waypoints.Count < 2)
            {
                SetHandAlpha(0f);
                return;
            }

            _cycleTimer += Time.deltaTime;
            if (_cycleTimer >= CycleDuration)
            {
                _cycleTimer = 0f;
            }

            float tCycle = _cycleTimer;
            float tMotion = Mathf.Clamp01((tCycle - 0.35f) / 1.15f);

            Vector3 tipWorldPos = SampleWaypoints(waypoints, EaseInOutCubic(tMotion));
            _handRootObj.transform.position = new Vector3(tipWorldPos.x, tipWorldPos.y, -2.0f);

            float targetAlpha;
            if (tCycle < 0.20f) targetAlpha = tCycle / 0.20f;
            else if (tCycle < 1.45f) targetAlpha = 1.0f;
            else if (tCycle < 1.70f) targetAlpha = 1.0f - (tCycle - 1.45f) / 0.25f;
            else targetAlpha = 0f;
            SetHandAlpha(targetAlpha);

            float scale;
            float rotZ = 0f;
            if (tCycle < 0.20f)
            {
                scale = Mathf.Lerp(1.10f, 1.0f, tCycle / 0.20f);
            }
            else if (tCycle < 0.35f)
            {
                float p = (tCycle - 0.20f) / 0.15f;
                scale = Mathf.Lerp(1.0f, 0.86f, p);
                rotZ = Mathf.Lerp(0f, -5f, p);
            }
            else if (tCycle < 1.45f)
            {
                scale = 0.86f;
                rotZ = -5f;
            }
            else
            {
                float r = Mathf.Clamp01((tCycle - 1.45f) / 0.15f);
                scale = Mathf.Lerp(0.86f, 1.0f, r);
                rotZ = Mathf.Lerp(-5f, 0f, r);
            }

            _handRootObj.transform.localScale = new Vector3(scale, scale, 1f);
            _handRootObj.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        }

        private List<Vector3> GetCurrentWaypoints(LevelManager lm)
        {
            var pieces = lm.ActivePieces;
            if (pieces == null) return null;

            List<Vector3> waypoints = new List<Vector3>();

            if (_currentPhase == TutorialPhase.Merge)
            {
                PuzzlePiece pieceA = null;
                PuzzlePiece pieceB = null;

                foreach (var p in pieces)
                {
                    if (p == null) continue;
                    if (pieceA == null) pieceA = p;
                    else if (pieceB == null) pieceB = p;
                }

                if (pieceA == null || pieceB == null) return null;

                if (pieceA.transform.position.x > pieceB.transform.position.x)
                {
                    var temp = pieceA;
                    pieceA = pieceB;
                    pieceB = temp;
                }

                waypoints.Add(pieceA.transform.position);
                waypoints.Add(pieceB.transform.position);
            }
            else if (_currentPhase == TutorialPhase.ExitGate)
            {
                PuzzlePiece targetPiece = null;
                foreach (var p in pieces)
                {
                    if (p != null) { targetPiece = p; break; }
                }

                if (targetPiece == null) return null;

                var gates = lm.Gates;
                if (gates == null || gates.Count == 0 || gates[0] == null) return null;

                Vector3 gatePos = gates[0].transform.position;
                Vector3 piecePos = targetPiece.transform.position;
                Vector2Int origin = targetPiece.GridOrigin;

                waypoints.Add(piecePos);

                if (origin.x != 2)
                {
                    Vector3 corridorCenter = lm.GridToWorld(2, 1);
                    waypoints.Add(corridorCenter);
                }

                waypoints.Add(gatePos);
            }

            return waypoints;
        }

        private Vector3 SampleWaypoints(List<Vector3> pts, float t)
        {
            if (pts.Count == 0) return Vector3.zero;
            if (pts.Count == 1) return pts[0];
            if (pts.Count == 2) return Vector3.Lerp(pts[0], pts[1], t);

            float totalDist = 0f;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                totalDist += Vector3.Distance(pts[i], pts[i + 1]);
            }

            if (totalDist < 0.001f) return pts[0];

            float targetDist = t * totalDist;
            float accum = 0f;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                float segmentDist = Vector3.Distance(pts[i], pts[i + 1]);
                if (accum + segmentDist >= targetDist)
                {
                    float segT = (targetDist - accum) / segmentDist;
                    return Vector3.Lerp(pts[i], pts[i + 1], segT);
                }
                accum += segmentDist;
            }

            return pts[pts.Count - 1];
        }

        private float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
