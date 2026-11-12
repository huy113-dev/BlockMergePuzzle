using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Gameplay
{
    public class PuzzlePiece : MonoBehaviour
    {
        [field: SerializeField] public PieceColor PieceColor { get; private set; }
        [field: SerializeField] public PieceShape Shape { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        [field: SerializeField] public Vector2Int GridOrigin { get; private set; }
        [field: SerializeField] public List<Vector2Int> ShapeOffsets { get; private set; } = new List<Vector2Int>();
        public List<Vector2Int> OccupiedCells => GetOccupiedCellsForOrigin(GridOrigin);

        public bool IsSliding { get; private set; }
        public bool IsEscaping { get; private set; }

        private Transform _visualRoot;
        private List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private List<TextMesh> _textLayers = new List<TextMesh>();
        private List<MeshRenderer> _textMeshLayers = new List<MeshRenderer>();
        private TextMesh _numberLabel;
        private MeshRenderer _numberMesh;
        private List<Collider2D> _colliders = new List<Collider2D>();
        private int _baseSortingOrder = 10;
        private Coroutine _activeTween;
        private Coroutine _liftCoroutine;
        private float _currentLiftProgress = 0f;

        public string ColorDisplayName => GameConfig.GetColorNameVietnamese(PieceColor);

        private void Awake()
        {
            if (ShapeOffsets == null || ShapeOffsets.Count == 0)
            {
                SetupShapeOffsets(Shape);
            }

            EnsureVisualReferences();
        }

        private void EnsureVisualReferences()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform.Find("VisualRoot");
            }

            if (_visualRoot != null)
            {
                if (_renderers == null || _renderers.Count == 0)
                {
                    _renderers = new List<SpriteRenderer>(_visualRoot.GetComponentsInChildren<SpriteRenderer>(true));
                }

                if (_textLayers == null || _textLayers.Count == 0)
                {
                    _textLayers = new List<TextMesh>();
                    _textMeshLayers = new List<MeshRenderer>();
                    string[] layerNames = new[] { "NumberCarveHighlight", "NumberCarveShadow", "NumberCarveDepth", "NumberLabel", "NumberShadow" };
                    foreach (var n in layerNames)
                    {
                        Transform t = _visualRoot.Find(n);
                        if (t != null)
                        {
                            var tm = t.GetComponent<TextMesh>();
                            var mr = t.GetComponent<MeshRenderer>();
                            if (tm != null && !_textLayers.Contains(tm)) _textLayers.Add(tm);
                            if (mr != null && !_textMeshLayers.Contains(mr)) _textMeshLayers.Add(mr);
                            if (n == "NumberLabel") { _numberLabel = tm; _numberMesh = mr; }
                        }
                    }
                }
            }
        }

        public static PuzzlePiece Create(
            PieceColor pieceColor,
            PieceShape shape,
            int value,
            Vector2Int origin,
            Vector3 worldPos,
            Transform parent,
            Vector2? customCellSize = null)
        {
            GameObject go = new GameObject($"Piece_{pieceColor}_{shape}_{value}");
            go.transform.SetParent(parent);
            go.transform.position = worldPos;

            PuzzlePiece piece = go.AddComponent<PuzzlePiece>();
            piece.Initialize(pieceColor, shape, value, origin, worldPos, customCellSize);
            return piece;
        }

        private void Initialize(PieceColor pieceColor, PieceShape shape, int value, Vector2Int origin, Vector3 worldPos, Vector2? customCellSize = null)
        {
            PieceColor = pieceColor;
            Shape = shape;
            Value = value;
            GridOrigin = origin;
            SetupShapeOffsets(shape);

            _visualRoot = new GameObject("VisualRoot").transform;
            _visualRoot.SetParent(transform, false);

            Vector2 cellSize = customCellSize ?? new Vector2(GameConfig.CellWidth, GameConfig.CellHeight);
            BuildVisuals(cellSize);
            BuildColliders(cellSize);
            BuildNumberLabel();
        }

        private void SetupShapeOffsets(PieceShape shape)
        {
            ShapeOffsets.Clear();
            switch (shape)
            {
                case PieceShape.Single_1x1:
                    ShapeOffsets.Add(new Vector2Int(0, 0));
                    break;

                case PieceShape.Vertical_1x2:
                    ShapeOffsets.Add(new Vector2Int(0, 0));
                    ShapeOffsets.Add(new Vector2Int(0, 1));
                    break;

                case PieceShape.Horizontal_2x1:
                    ShapeOffsets.Add(new Vector2Int(0, 0));
                    ShapeOffsets.Add(new Vector2Int(1, 0));
                    break;

                case PieceShape.ShapeL_TopRight:
                    // Top-right L (like green L in designer screenshot)
                    // [(0, 0), (1, 0), (1, 1)]
                    ShapeOffsets.Add(new Vector2Int(0, 0));
                    ShapeOffsets.Add(new Vector2Int(1, 0));
                    ShapeOffsets.Add(new Vector2Int(1, 1));
                    break;

                case PieceShape.ShapeL_BottomLeft:
                    // Bottom-left L (like orange L in designer screenshot)
                    // [(0, 0), (0, 1), (1, 1)]
                    ShapeOffsets.Add(new Vector2Int(0, 0));
                    ShapeOffsets.Add(new Vector2Int(0, 1));
                    ShapeOffsets.Add(new Vector2Int(1, 1));
                    break;
            }
        }

        public List<Vector2Int> GetOccupiedCellsForOrigin(Vector2Int origin)
        {
            List<Vector2Int> cells = new List<Vector2Int>(ShapeOffsets.Count);
            foreach (var off in ShapeOffsets)
            {
                cells.Add(origin + off);
            }
            return cells;
        }

        private void BuildVisuals(Vector2 cellSize)
        {
            float cw = cellSize.x;
            float ch = cellSize.y;
            Sprite sprite = GemSpriteManager.GetPieceSprite(PieceColor, Shape);

            switch (Shape)
            {
                case PieceShape.Single_1x1:
                    CreateSubRenderer("Cell_1x1", sprite, Vector3.zero, new Vector2(cw * 0.96f, ch * 0.96f));
                    break;

                case PieceShape.Vertical_1x2:
                    CreateSubRenderer("Cell_1x2", sprite, Vector3.zero, new Vector2(cw * 0.96f, ch * 1.96f));
                    break;

                case PieceShape.Horizontal_2x1:
                    CreateSubRenderer("Cell_2x1", sprite, Vector3.zero, new Vector2(cw * 1.96f, ch * 0.96f));
                    break;

                case PieceShape.ShapeL_TopRight:
                    // Uses exact Vector 114 from designer (288x270 L-shape)
                    CreateSubRenderer("ShapeL_TopRight", sprite, Vector3.zero, new Vector2(cw * 1.96f, ch * 1.96f));
                    break;

                case PieceShape.ShapeL_BottomLeft:
                    // Uses exact Vector 115 from designer (288x265 L-shape)
                    CreateSubRenderer("ShapeL_BottomLeft", sprite, Vector3.zero, new Vector2(cw * 1.96f, ch * 1.96f));
                    break;
            }
        }

        private SpriteRenderer CreateSubRenderer(string subName, Sprite sprite, Vector3 localPos, Vector2 targetSize)
        {
            GameObject subObj = new GameObject(subName);
            subObj.transform.SetParent(_visualRoot, false);
            subObj.transform.localPosition = localPos;

            SpriteRenderer sr = subObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = _baseSortingOrder;

            // Only tint uncolored procedural sprites; pre-colored assets (Vector*, Group*, gem_*) stay true to artwork
            bool isPreColored = sprite != null &&
                (sprite.name.StartsWith("Vector") || sprite.name.StartsWith("Group") || sprite.name.StartsWith("gem_"));
            sr.color = isPreColored ? UnityEngine.Color.white : GameConfig.GetColorRGB(PieceColor);

            if (sprite != null && sprite.rect.width > 0 && sprite.rect.height > 0)
            {
                float nativeW = sprite.rect.width / sprite.pixelsPerUnit;
                float nativeH = sprite.rect.height / sprite.pixelsPerUnit;
                subObj.transform.localScale = new Vector3(targetSize.x / nativeW, targetSize.y / nativeH, 1f);
            }

            _renderers.Add(sr);
            return sr;
        }

        private void BuildColliders(Vector2 cellSize)
        {
            float cw = cellSize.x;
            float ch = cellSize.y;

            switch (Shape)
            {
                case PieceShape.Single_1x1:
                    BoxCollider2D box1 = gameObject.AddComponent<BoxCollider2D>();
                    box1.size = new Vector2(cw * 0.96f, ch * 0.96f);
                    _colliders.Add(box1);
                    break;

                case PieceShape.Vertical_1x2:
                    BoxCollider2D boxV = gameObject.AddComponent<BoxCollider2D>();
                    boxV.size = new Vector2(cw * 0.96f, ch * 1.96f);
                    _colliders.Add(boxV);
                    break;

                case PieceShape.Horizontal_2x1:
                    BoxCollider2D boxH = gameObject.AddComponent<BoxCollider2D>();
                    boxH.size = new Vector2(cw * 1.96f, ch * 0.96f);
                    _colliders.Add(boxH);
                    break;

                case PieceShape.ShapeL_TopRight:
                    BoxCollider2D boxTop = gameObject.AddComponent<BoxCollider2D>();
                    boxTop.offset = new Vector2(0f, ch * 0.5f);
                    boxTop.size = new Vector2(cw * 1.96f, ch * 0.96f);
                    _colliders.Add(boxTop);

                    BoxCollider2D boxBR = gameObject.AddComponent<BoxCollider2D>();
                    boxBR.offset = new Vector2(cw * 0.5f, -ch * 0.5f);
                    boxBR.size = new Vector2(cw * 0.96f, ch * 0.96f);
                    _colliders.Add(boxBR);
                    break;

                case PieceShape.ShapeL_BottomLeft:
                    BoxCollider2D boxTL = gameObject.AddComponent<BoxCollider2D>();
                    boxTL.offset = new Vector2(-cw * 0.5f, ch * 0.5f);
                    boxTL.size = new Vector2(cw * 0.96f, ch * 0.96f);
                    _colliders.Add(boxTL);

                    BoxCollider2D boxBot = gameObject.AddComponent<BoxCollider2D>();
                    boxBot.offset = new Vector2(0f, -ch * 0.5f);
                    boxBot.size = new Vector2(cw * 1.96f, ch * 0.96f);
                    _colliders.Add(boxBot);
                    break;
            }
        }

        private static UnityEngine.Color GetStoneSurfaceColor(PieceColor color)
        {
            switch (color)
            {
                case PieceColor.Red:    return new UnityEngine.Color(0.855f, 0.180f, 0.204f);
                case PieceColor.Pink:   return new UnityEngine.Color(0.859f, 0.192f, 0.573f);
                case PieceColor.Green:  return new UnityEngine.Color(0.306f, 0.694f, 0.176f);
                case PieceColor.Yellow: return new UnityEngine.Color(0.980f, 0.733f, 0.224f);
                case PieceColor.Orange: return new UnityEngine.Color(0.980f, 0.443f, 0.031f);
                case PieceColor.Blue:   return new UnityEngine.Color(0.020f, 0.561f, 0.937f);
                case PieceColor.Purple: return new UnityEngine.Color(0.537f, 0.216f, 0.980f);
                default:                return GameConfig.GetColorRGB(color);
            }
        }

        private TextMesh CreateDigitLayer(string layerName, Vector3 localPos, int fontSize, float charSize, UnityEngine.Color color, int sortOrder)
        {
            GameObject obj = new GameObject(layerName);
            obj.transform.SetParent(_visualRoot, false);
            obj.transform.localPosition = localPos;

            TextMesh tm = obj.AddComponent<TextMesh>();
            tm.text = Value.ToString();
            tm.fontSize = fontSize;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.sortingOrder = sortOrder;

            _textLayers.Add(tm);
            _textMeshLayers.Add(mr);
            return tm;
        }

        private void BuildNumberLabel()
        {
            Vector3 textOffset = Vector3.zero;
            float cw = GameConfig.CellWidth;
            float ch = GameConfig.CellHeight;

            if (Shape == PieceShape.ShapeL_TopRight)
            {
                textOffset = new Vector3(cw * 0.40f, ch * 0.45f, 0f);
            }
            else if (Shape == PieceShape.ShapeL_BottomLeft)
            {
                textOffset = new Vector3(-cw * 0.40f, -ch * 0.45f, 0f);
            }

            _textLayers.Clear();
            _textMeshLayers.Clear();

            const int fs = 140;
            const float cs = 0.036f;
            UnityEngine.Color stone = GetStoneSurfaceColor(PieceColor);

            // Reversed lighting (top-left light source) creates recessed chiseled-in-stone deboss:
            // 1. Bottom-right specular catchlight lip (light catching lower carved edge)
            UnityEngine.Color rimHighlight = UnityEngine.Color.Lerp(stone, UnityEngine.Color.white, 0.72f);
            rimHighlight.a = 0.90f;
            CreateDigitLayer("NumberCarveHighlight", textOffset + new Vector3(0.016f, -0.019f, -0.02f), fs, cs, rimHighlight, _baseSortingOrder + 1);

            // 2. Top-left deep inner shadow wall (stone lip casts shadow down-right into groove)
            UnityEngine.Color deepShadow = new UnityEngine.Color(stone.r * 0.08f, stone.g * 0.05f, stone.b * 0.09f, 0.96f);
            CreateDigitLayer("NumberCarveShadow", textOffset + new Vector3(-0.015f, 0.018f, -0.04f), fs, cs, deepShadow, _baseSortingOrder + 2);

            // 3. Recessed bevel slope transition
            UnityEngine.Color bevelStep = new UnityEngine.Color(stone.r * 0.28f, stone.g * 0.19f, stone.b * 0.33f, 0.96f);
            CreateDigitLayer("NumberCarveDepth", textOffset + new Vector3(-0.005f, 0.006f, -0.06f), fs, cs, bevelStep, _baseSortingOrder + 3);

            // 4. Deep sunken stone cavity floor
            UnityEngine.Color cavityFloor = new UnityEngine.Color(stone.r * 0.48f, stone.g * 0.34f, stone.b * 0.52f, 0.98f);
            _numberLabel = CreateDigitLayer("NumberLabel", textOffset + new Vector3(0f, 0f, -0.08f), fs, cs, cavityFloor, _baseSortingOrder + 4);
            _numberMesh = _numberLabel.GetComponent<MeshRenderer>();
        }

        public void SetValue(int newValue)
        {
            Value = newValue;
            EnsureVisualReferences();
            if (_textLayers != null)
            {
                for (int i = 0; i < _textLayers.Count; i++)
                {
                    if (_textLayers[i] != null) _textLayers[i].text = Value.ToString();
                }
            }
        }

        public void SetGridOrigin(Vector2Int newOrigin)
        {
            GridOrigin = newOrigin;
        }

        public void SetSortingOrder(int order)
        {
            EnsureVisualReferences();
            foreach (var r in _renderers)
            {
                if (r != null) r.sortingOrder = order;
            }
            if (_textMeshLayers != null)
            {
                for (int i = 0; i < _textMeshLayers.Count; i++)
                {
                    if (_textMeshLayers[i] != null) _textMeshLayers[i].sortingOrder = order + 1 + i;
                }
            }
        }

        public void SetLifted(bool lifted)
        {
            if (IsSliding || IsEscaping) return;
            EnsureVisualReferences();
            if (_liftCoroutine != null) StopCoroutine(_liftCoroutine);
            _liftCoroutine = StartCoroutine(CoAnimateLift(lifted));
        }

        public void LockForMerge()
        {
            IsSliding = true;
            if (_liftCoroutine != null)
            {
                StopCoroutine(_liftCoroutine);
                _liftCoroutine = null;
            }
            if (_activeTween != null)
            {
                StopCoroutine(_activeTween);
                _activeTween = null;
            }
        }

        public void SetTilt(float angleZ)
        {
            if (_visualRoot != null)
            {
                _visualRoot.localRotation = Quaternion.Euler(0f, 0f, angleZ);
            }
        }

        private IEnumerator CoAnimateLift(bool lifted)
        {
            float startProg = _currentLiftProgress;
            float targetProg = lifted ? 1f : 0f;
            float duration = lifted ? 0.08f : 0.09f;
            float elapsed = 0f;

            if (lifted)
            {
                SetSortingOrder(50);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - (1f - t) * (1f - t);
                _currentLiftProgress = Mathf.Lerp(startProg, targetProg, ease);
                ApplyLiftVisuals(_currentLiftProgress);
                yield return null;
            }

            _currentLiftProgress = targetProg;
            ApplyLiftVisuals(_currentLiftProgress);

            if (!lifted && !IsSliding && !IsEscaping)
            {
                SetSortingOrder(_baseSortingOrder);
                SetTilt(0f);
            }
            _liftCoroutine = null;
        }

        private void ApplyLiftVisuals(float progress)
        {
            if (_visualRoot == null) return;
            float scale = Mathf.Lerp(1.0f, 1.075f, progress);
            _visualRoot.localScale = Vector3.one * scale;
            float yOffset = Mathf.Lerp(0f, 0.08f, progress);
            _visualRoot.localPosition = new Vector3(0f, yOffset, -0.1f * progress);
        }

        public void SlideTo(Vector3 targetWorldPos, Vector2Int newOrigin, float duration, Action onArrival)
        {
            if (_liftCoroutine != null)
            {
                StopCoroutine(_liftCoroutine);
                _liftCoroutine = null;
            }
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoSlideTo(targetWorldPos, newOrigin, duration, onArrival));
        }

        public void SlideAndSlamMerge(Vector3 targetWorldPos, Vector2Int targetOrigin, PuzzlePiece targetPiece, Action onImpact, Action onArrival, float approachDuration = 0.12f)
        {
            if (_liftCoroutine != null)
            {
                StopCoroutine(_liftCoroutine);
                _liftCoroutine = null;
            }
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoSlideAndSlamMerge(targetWorldPos, targetOrigin, targetPiece, onImpact, onArrival, approachDuration));
        }

        private IEnumerator CoSlideAndSlamMerge(Vector3 targetWorldPos, Vector2Int targetOrigin, PuzzlePiece targetPiece, Action onImpact, Action onArrival, float approachDuration)
        {
            IsSliding = true;
            EnsureVisualReferences();

            SetSortingOrder(65);
            Vector3 startPos = transform.position;
            Vector3 startRootPos = _visualRoot != null ? _visualRoot.localPosition : Vector3.zero;
            Vector3 startRootScale = _visualRoot != null ? _visualRoot.localScale : Vector3.one;
            Quaternion startRot = _visualRoot != null ? _visualRoot.localRotation : Quaternion.identity;

            float dist = Vector3.Distance(startPos, targetWorldPos);
            float approachDur = Mathf.Clamp(approachDuration > 0f ? approachDuration : (dist / 9.5f), 0.05f, 0.14f);
            float elapsed = 0f;

            // Phase 1: Approach target while hovering above
            while (elapsed < approachDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / approachDur);
                float ease = 1f - (1f - t) * (1f - t);

                transform.position = Vector3.Lerp(startPos, targetWorldPos, ease);

                if (_visualRoot != null)
                {
                    float hoverY = Mathf.Lerp(startRootPos.y, 0.24f, ease);
                    float hoverZ = Mathf.Lerp(startRootPos.z, -0.40f, ease);
                    _visualRoot.localPosition = new Vector3(0f, hoverY, hoverZ);
                    _visualRoot.localScale = Vector3.Lerp(startRootScale, Vector3.one * 1.16f, ease);
                    _visualRoot.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, ease);
                }

                yield return null;
            }

            transform.position = targetWorldPos;

            // Phase 2: Slam down onto target block
            float slamDur = 0.065f;
            elapsed = 0f;
            Vector3 targetInitialScale = (targetPiece != null) ? targetPiece.transform.localScale : Vector3.one;

            while (elapsed < slamDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slamDur);
                float slamEase = t * t;

                if (_visualRoot != null)
                {
                    float y = Mathf.Lerp(0.24f, 0f, slamEase);
                    _visualRoot.localPosition = new Vector3(0f, y, -0.40f * (1f - slamEase));

                    float sx = Mathf.Lerp(1.16f, 1.28f, slamEase);
                    float sy = Mathf.Lerp(1.16f, 0.72f, slamEase);
                    _visualRoot.localScale = new Vector3(sx, sy, 1f);
                }

                if (targetPiece != null)
                {
                    float tx = Mathf.Lerp(targetInitialScale.x, 1.15f, slamEase);
                    float ty = Mathf.Lerp(targetInitialScale.y, 0.35f, slamEase);
                    targetPiece.transform.localScale = new Vector3(tx, ty, 1f);
                }

                yield return null;
            }

            if (_visualRoot != null)
            {
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localScale = new Vector3(1.28f, 0.72f, 1f);
            }

            // Phase 3: Impact callbacks + white silhouette flash
            onImpact?.Invoke();
            StartCoroutine(CoFlashOverlay(0.16f));

            // Phase 4: Elastic rebound back to 1.0
            float reboundDur = 0.22f;
            elapsed = 0f;

            while (elapsed < reboundDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / reboundDur);

                if (_visualRoot != null)
                {
                    float d = Mathf.Cos(t * Mathf.PI * 2.5f) * Mathf.Exp(-t * 3.2f) * (1f - t);
                    float sx = 1.0f + 0.28f * d;
                    float sy = 1.0f - 0.28f * d;
                    _visualRoot.localScale = new Vector3(sx, sy, 1f);
                }

                yield return null;
            }

            if (_visualRoot != null)
            {
                _visualRoot.localScale = Vector3.one;
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localRotation = Quaternion.identity;
            }

            GridOrigin = targetOrigin;
            IsSliding = false;
            _currentLiftProgress = 0f;
            SetSortingOrder(_baseSortingOrder);
            _activeTween = null;

            onArrival?.Invoke();
        }

        private static Material _cachedWhiteFlashMat;

        private static Material GetWhiteFlashMaterial()
        {
            if (_cachedWhiteFlashMat != null) return _cachedWhiteFlashMat;
            // ponytail: built-in GUI/Text Shader gives solid white silhouette without custom shader asset
            Shader s = Shader.Find("GUI/Text Shader");
            if (s != null)
            {
                _cachedWhiteFlashMat = new Material(s);
                _cachedWhiteFlashMat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
            return _cachedWhiteFlashMat;
        }

        private IEnumerator CoFlashOverlay(float duration)
        {
            if (_visualRoot == null) yield break;

            GameObject flashObj = new GameObject("MergeFlashOverlay");
            flashObj.transform.SetParent(_visualRoot, false);
            flashObj.transform.localPosition = Vector3.zero;

            Material flashMat = GetWhiteFlashMaterial();
            List<SpriteRenderer> flashRenderers = new List<SpriteRenderer>();

            foreach (var r in _renderers)
            {
                if (r == null || r.gameObject == null) continue;
                GameObject subFlash = new GameObject("Flash_" + r.name);
                subFlash.transform.SetParent(flashObj.transform, false);
                subFlash.transform.localPosition = r.transform.localPosition;
                subFlash.transform.localScale = r.transform.localScale;
                subFlash.transform.localRotation = r.transform.localRotation;

                SpriteRenderer fsr = subFlash.AddComponent<SpriteRenderer>();
                fsr.sprite = (r.sprite != null && flashMat != null) ? r.sprite : SpriteGenerator.GetBlockSprite();
                if (flashMat != null) fsr.material = flashMat;
                fsr.sortingOrder = 75;
                fsr.color = new UnityEngine.Color(1f, 1f, 1f, 0.88f);
                flashRenderers.Add(fsr);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = (1f - t) * 0.88f;

                for (int i = 0; i < flashRenderers.Count; i++)
                {
                    if (flashRenderers[i] != null)
                    {
                        flashRenderers[i].color = new UnityEngine.Color(1f, 1f, 1f, alpha);
                    }
                }

                yield return null;
            }

            Destroy(flashObj);
        }

        private IEnumerator CoSlideTo(Vector3 targetWorldPos, Vector2Int newOrigin, float duration, Action onArrival)
        {
            IsSliding = true;
            SetSortingOrder(40);
            Vector3 startPos = transform.position;
            float startLift = _currentLiftProgress;
            Quaternion startRot = (_visualRoot != null) ? _visualRoot.localRotation : Quaternion.identity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad: smooth continuation and gentle deceleration into target slot
                float ease = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetWorldPos, ease);

                // Smoothly lower lifted block back to board as it slides into place
                _currentLiftProgress = Mathf.Lerp(startLift, 0f, ease);
                ApplyLiftVisuals(_currentLiftProgress);
                if (_visualRoot != null)
                {
                    _visualRoot.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, ease);
                }

                yield return null;
            }

            transform.position = targetWorldPos;
            GridOrigin = newOrigin;
            IsSliding = false;
            _currentLiftProgress = 0f;
            ApplyLiftVisuals(0f);
            SetTilt(0f);
            SetSortingOrder(_baseSortingOrder);
            _activeTween = null;

            onArrival?.Invoke();
        }

        public void Bump(Vector2 direction)
        {
            if (_activeTween != null || IsSliding || IsEscaping) return;
            _activeTween = StartCoroutine(CoBump(direction));
        }

        private IEnumerator CoBump(Vector2 direction)
        {
            Vector3 startPos = transform.position;
            Vector3 bumpTarget = startPos + (Vector3)(direction.normalized * 0.12f);
            float duration = 0.12f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI);
                transform.position = Vector3.Lerp(startPos, bumpTarget, curve);
                yield return null;
            }

            transform.position = startPos;
            _activeTween = null;
        }

        public void PlayMergeEffect()
        {
            StartCoroutine(CoFlashOverlay(0.16f));
        }

        public void PlayEscapeAnimation(Vector2 exitDir, Action onDone)
        {
            IsEscaping = true;
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoEscape(exitDir, onDone));
        }

        private IEnumerator CoEscape(Vector2 exitDir, Action onDone)
        {
            EnsureVisualReferences();
            SetSortingOrder(120);

            Vector2 dir = exitDir.sqrMagnitude > 0.001f ? exitDir.normalized : Vector2.up;
            Vector2 perp = new Vector2(-dir.y, dir.x);

            Bounds worldBounds = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;
            foreach (var r in _renderers)
            {
                if (r != null)
                {
                    if (!hasBounds) { worldBounds = r.bounds; hasBounds = true; }
                    else worldBounds.Encapsulate(r.bounds);
                }
            }

            float halfAlong = hasBounds ? (Mathf.Abs(dir.x) * worldBounds.extents.x + Mathf.Abs(dir.y) * worldBounds.extents.y) : 0.65f;
            float halfPerp = hasBounds ? (Mathf.Abs(perp.x) * worldBounds.extents.x + Mathf.Abs(perp.y) * worldBounds.extents.y) : 0.65f;
            halfAlong = Mathf.Max(halfAlong, 0.45f);
            halfPerp = Mathf.Max(halfPerp, 0.45f);

            Vector3 startPos = transform.position;
            Vector3 gatePlanePos = startPos + (Vector3)(dir * (halfAlong * 0.78f));
            float moveDist = halfAlong * 2.05f + 0.40f;
            Vector3 targetPos = startPos + (Vector3)(dir * moveDist);
            float duration = 0.50f;

            // Load directional voxel dissolve shader
            Shader dissolveShader = Resources.Load<Shader>("Shaders/SpritePortalDissolve") ?? Shader.Find("Custom/SpritePortalDissolve");
            List<Material> tempMats = null;
            if (dissolveShader != null)
            {
                tempMats = new List<Material>(_renderers.Count);
                foreach (var r in _renderers)
                {
                    if (r != null)
                    {
                        Material m = new Material(dissolveShader);
                        m.SetVector("_DissolveDir", new Vector4(dir.x, dir.y, 0f, 0f));
                        m.SetVector("_PlaneOrigin", new Vector4(gatePlanePos.x, gatePlanePos.y, 0f, 0f));
                        m.SetFloat("_EdgeWidth", 0.44f);
                        m.SetColor("_GlowColor", new UnityEngine.Color(0.32f, 0.96f, 1.0f, 1.0f));
                        r.material = m;
                        tempMats.Add(m);
                    }
                }
            }

            // Spawn independent portal curtain & disintegrating shard particles
            GameObject vfxObj = new GameObject("EscapeDisintegrationVFX");
            EscapeVfxRunner vfxRunner = vfxObj.AddComponent<EscapeVfxRunner>();
            vfxRunner.Play(gatePlanePos, dir, perp, halfPerp, GameConfig.GetColorRGB(PieceColor), duration);

            // Cache initial text colors
            UnityEngine.Color[] textInitColors = null;
            if (_textLayers != null && _textLayers.Count > 0)
            {
                textInitColors = new UnityEngine.Color[_textLayers.Count];
                for (int i = 0; i < _textLayers.Count; i++)
                {
                    if (_textLayers[i] != null) textInitColors[i] = _textLayers[i].color;
                }
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = t * (0.45f + 0.55f * t);

                transform.position = Vector3.Lerp(startPos, targetPos, ease);

                if (tempMats == null)
                {
                    // Fallback if custom shader unavailable
                    transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 0.25f, t * t);
                    float alpha = 1f - t;
                    foreach (var r in _renderers)
                    {
                        if (r != null)
                        {
                            UnityEngine.Color c = r.color;
                            c.a = alpha;
                            r.color = c;
                        }
                    }
                }

                // Fade carved digit smoothly as block crosses the portal curtain
                if (_textLayers != null && textInitColors != null)
                {
                    float textAlpha = Mathf.Clamp01(1f - Mathf.Max(0f, t - 0.12f) / 0.42f);
                    for (int i = 0; i < _textLayers.Count; i++)
                    {
                        if (_textLayers[i] != null)
                        {
                            UnityEngine.Color c = textInitColors[i];
                            c.a *= textAlpha;
                            _textLayers[i].color = c;
                        }
                    }
                }

                yield return null;
            }

            if (tempMats != null)
            {
                for (int i = 0; i < tempMats.Count; i++)
                {
                    if (tempMats[i] != null) Destroy(tempMats[i]);
                }
            }

            onDone?.Invoke();
            Destroy(gameObject);
        }

        private class EscapeVfxRunner : MonoBehaviour
        {
            private struct Shard
            {
                public Transform Transform;
                public SpriteRenderer Renderer;
                public UnityEngine.Color BaseColor;
                public Vector3 Velocity;
                public float SpinSpeed;
                public float StartScale;
                public float Age;
                public float Lifetime;
            }

            public void Play(Vector3 planePos, Vector2 dir, Vector2 perp, float halfPerp, UnityEngine.Color gemColor, float dissolveDuration)
            {
                StartCoroutine(CoRun(planePos, dir, perp, halfPerp, gemColor, dissolveDuration));
            }

            private IEnumerator CoRun(Vector3 planePos, Vector2 dir, Vector2 perp, float halfPerp, UnityEngine.Color gemColor, float dissolveDuration)
            {
                Sprite shardSprite = SpriteGenerator.GetShardSprite();
                Sprite sparkSprite = SpriteGenerator.GetCircleSprite();
                Sprite glowSprite = SpriteGenerator.GetRadialGlowSprite();

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                UnityEngine.Color cyanPortal = new UnityEngine.Color(0.28f, 0.96f, 1.0f, 1.0f);
                UnityEngine.Color darkFacet = UnityEngine.Color.Lerp(gemColor, new UnityEngine.Color(0.06f, 0.22f, 0.30f, 1f), 0.65f);

                // Portal curtain beam + soft glow along gate contact line
                GameObject curtainObj = new GameObject("PortalCurtain");
                curtainObj.transform.SetParent(transform, false);
                curtainObj.transform.position = new Vector3(planePos.x, planePos.y, -0.35f);
                curtainObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                curtainObj.transform.localScale = new Vector3(halfPerp * 2.35f + 0.35f, 0.085f, 1f);
                SpriteRenderer curtainSr = curtainObj.AddComponent<SpriteRenderer>();
                curtainSr.sprite = shardSprite;
                curtainSr.sortingOrder = 126;
                curtainSr.color = new UnityEngine.Color(0.45f, 0.98f, 1.0f, 0.92f);

                GameObject haloObj = new GameObject("PortalHalo");
                haloObj.transform.SetParent(transform, false);
                haloObj.transform.position = new Vector3(planePos.x, planePos.y, -0.25f);
                haloObj.transform.localScale = Vector3.one * (halfPerp * 2.2f + 0.6f);
                SpriteRenderer haloSr = haloObj.AddComponent<SpriteRenderer>();
                haloSr.sprite = glowSprite;
                haloSr.sortingOrder = 115;
                haloSr.color = new UnityEngine.Color(0.28f, 0.95f, 1.0f, 0.55f);

                List<Shard> shards = new List<Shard>(56);
                float spawnInterval = dissolveDuration / 14f;
                float nextSpawn = 0f;
                float elapsed = 0f;
                float maxTotalDuration = dissolveDuration + 0.50f;

                while (elapsed < maxTotalDuration)
                {
                    float dt = Time.deltaTime;
                    elapsed += dt;

                    // Emit batches of square shards & glowing sparks while block crosses portal line
                    if (elapsed <= dissolveDuration * 0.90f && elapsed >= nextSpawn)
                    {
                        nextSpawn += spawnInterval;
                        int count = 4;
                        for (int i = 0; i < count; i++)
                        {
                            float lat = UnityEngine.Random.Range(-halfPerp * 0.95f, halfPerp * 0.95f);
                            float jitter = UnityEngine.Random.Range(-0.10f, 0.14f);
                            Vector3 spawnPos = planePos + (Vector3)(perp * lat + dir * jitter);
                            spawnPos.z = -0.45f;

                            float roll = UnityEngine.Random.value;
                            UnityEngine.Color col;
                            Sprite sp;
                            float size;
                            if (roll < 0.45f)
                            {
                                col = gemColor;
                                sp = shardSprite;
                                size = UnityEngine.Random.Range(0.08f, 0.18f);
                            }
                            else if (roll < 0.68f)
                            {
                                col = darkFacet;
                                sp = shardSprite;
                                size = UnityEngine.Random.Range(0.06f, 0.14f);
                            }
                            else if (roll < 0.88f)
                            {
                                col = cyanPortal;
                                sp = UnityEngine.Random.value < 0.5f ? shardSprite : sparkSprite;
                                size = UnityEngine.Random.Range(0.045f, 0.10f);
                            }
                            else
                            {
                                col = UnityEngine.Color.white;
                                sp = sparkSprite;
                                size = UnityEngine.Random.Range(0.035f, 0.075f);
                            }

                            GameObject pObj = new GameObject("Shard");
                            pObj.transform.SetParent(transform, false);
                            pObj.transform.position = spawnPos;
                            pObj.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
                            pObj.transform.localScale = Vector3.one * size;

                            SpriteRenderer psr = pObj.AddComponent<SpriteRenderer>();
                            psr.sprite = sp;
                            psr.sortingOrder = roll >= 0.68f ? 128 : 124;
                            psr.color = col;

                            Vector3 vel = (Vector3)(dir * UnityEngine.Random.Range(1.6f, 3.8f) + perp * UnityEngine.Random.Range(-1.25f, 1.25f));
                            shards.Add(new Shard
                            {
                                Transform = pObj.transform,
                                Renderer = psr,
                                BaseColor = col,
                                Velocity = vel,
                                SpinSpeed = UnityEngine.Random.Range(-380f, 380f),
                                StartScale = size,
                                Age = 0f,
                                Lifetime = UnityEngine.Random.Range(0.28f, 0.48f)
                            });
                        }
                    }

                    // Update portal beam & halo fade
                    if (curtainSr != null)
                    {
                        float beamAlpha = elapsed < dissolveDuration
                            ? 0.85f + Mathf.Sin(elapsed * 40f) * 0.12f
                            : Mathf.Clamp01(1f - (elapsed - dissolveDuration) / 0.18f) * 0.85f;
                        curtainSr.color = new UnityEngine.Color(0.45f, 0.98f, 1.0f, beamAlpha);
                    }
                    if (haloSr != null)
                    {
                        float haloAlpha = elapsed < dissolveDuration
                            ? 0.50f
                            : Mathf.Clamp01(1f - (elapsed - dissolveDuration) / 0.22f) * 0.50f;
                        haloSr.color = new UnityEngine.Color(0.28f, 0.95f, 1.0f, haloAlpha);
                    }

                    // Update active shards
                    for (int i = shards.Count - 1; i >= 0; i--)
                    {
                        Shard s = shards[i];
                        s.Age += dt;
                        if (s.Age >= s.Lifetime)
                        {
                            if (s.Transform != null) Destroy(s.Transform.gameObject);
                            shards.RemoveAt(i);
                            continue;
                        }

                        float pt = s.Age / s.Lifetime;
                        float drag = Mathf.Lerp(1f, 0.35f, pt);
                        if (s.Transform != null)
                        {
                            s.Transform.position += s.Velocity * (drag * dt);
                            s.Transform.Rotate(0f, 0f, s.SpinSpeed * dt);
                            s.Transform.localScale = Vector3.one * (s.StartScale * Mathf.Lerp(1f, 0.15f, pt * pt));
                        }
                        if (s.Renderer != null)
                        {
                            UnityEngine.Color c = s.BaseColor;
                            c.a = 1f - pt * pt;
                            s.Renderer.color = c;
                        }
                        shards[i] = s;
                    }

                    if (elapsed > dissolveDuration && shards.Count == 0)
                    {
                        break;
                    }

                    yield return null;
                }

                Destroy(gameObject);
            }
        }
    }
}
