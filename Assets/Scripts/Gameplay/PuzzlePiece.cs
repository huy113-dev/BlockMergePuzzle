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
        private TextMesh _numberLabel;
        private TextMesh _shadowLabel;
        private MeshRenderer _numberMesh;
        private MeshRenderer _shadowMesh;
        private List<Collider2D> _colliders = new List<Collider2D>();
        private int _baseSortingOrder = 10;
        private Coroutine _activeTween;

        public string ColorDisplayName => GameConfig.GetColorNameVietnamese(PieceColor);

        private void Awake()
        {
            if (ShapeOffsets == null || ShapeOffsets.Count == 0)
            {
                SetupShapeOffsets(Shape);
            }

            if (_visualRoot == null)
            {
                _visualRoot = transform.Find("VisualRoot");
                if (_visualRoot != null)
                {
                    Transform tNum = _visualRoot.Find("NumberLabel");
                    if (tNum != null) _numberLabel = tNum.GetComponent<TextMesh>();
                    Transform tShadow = _visualRoot.Find("NumberShadow");
                    if (tShadow != null) _shadowLabel = tShadow.GetComponent<TextMesh>();
                }
            }
        }

        public static PuzzlePiece Create(
            PieceColor pieceColor,
            PieceShape shape,
            int value,
            Vector2Int origin,
            Vector3 worldPos,
            Transform parent)
        {
            GameObject go = new GameObject($"Piece_{pieceColor}_{shape}_{value}");
            go.transform.SetParent(parent);
            go.transform.position = worldPos;

            PuzzlePiece piece = go.AddComponent<PuzzlePiece>();
            piece.Initialize(pieceColor, shape, value, origin, worldPos);
            return piece;
        }

        private void Initialize(PieceColor pieceColor, PieceShape shape, int value, Vector2Int origin, Vector3 worldPos)
        {
            PieceColor = pieceColor;
            Shape = shape;
            Value = value;
            GridOrigin = origin;
            SetupShapeOffsets(shape);

            _visualRoot = new GameObject("VisualRoot").transform;
            _visualRoot.SetParent(transform, false);

            BuildVisuals();
            BuildColliders();
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

        private void BuildVisuals()
        {
            float cw = GameConfig.CellWidth;
            float ch = GameConfig.CellHeight;
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

            // Apply vibrant color tint for procedural or non-vector sprites
            if (sprite == null || !sprite.name.StartsWith("Vector"))
            {
                sr.color = GameConfig.GetColorRGB(PieceColor);
            }

            if (sprite != null && sprite.rect.width > 0 && sprite.rect.height > 0)
            {
                float nativeW = sprite.rect.width / sprite.pixelsPerUnit;
                float nativeH = sprite.rect.height / sprite.pixelsPerUnit;
                subObj.transform.localScale = new Vector3(targetSize.x / nativeW, targetSize.y / nativeH, 1f);
            }

            _renderers.Add(sr);
            return sr;
        }

        private void BuildColliders()
        {
            float cw = GameConfig.CellWidth;
            float ch = GameConfig.CellHeight;

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

            // Drop shadow label
            GameObject shadowObj = new GameObject("NumberShadow");
            shadowObj.transform.SetParent(_visualRoot, false);
            shadowObj.transform.localPosition = textOffset + new Vector3(0.025f, -0.025f, -0.05f);

            _shadowLabel = shadowObj.AddComponent<TextMesh>();
            _shadowLabel.text = Value.ToString();
            _shadowLabel.fontSize = 58;
            _shadowLabel.characterSize = 0.082f;
            _shadowLabel.anchor = TextAnchor.MiddleCenter;
            _shadowLabel.alignment = TextAlignment.Center;
            _shadowLabel.fontStyle = FontStyle.Bold;
            _shadowLabel.color = new UnityEngine.Color(0.08f, 0.08f, 0.16f, 0.9f);

            _shadowMesh = shadowObj.GetComponent<MeshRenderer>();
            _shadowMesh.sortingOrder = _baseSortingOrder + 2;

            // Main white label
            GameObject textObj = new GameObject("NumberLabel");
            textObj.transform.SetParent(_visualRoot, false);
            textObj.transform.localPosition = textOffset + new Vector3(0f, 0f, -0.1f);

            _numberLabel = textObj.AddComponent<TextMesh>();
            _numberLabel.text = Value.ToString();
            _numberLabel.fontSize = 58;
            _numberLabel.characterSize = 0.082f;
            _numberLabel.anchor = TextAnchor.MiddleCenter;
            _numberLabel.alignment = TextAlignment.Center;
            _numberLabel.fontStyle = FontStyle.Bold;
            _numberLabel.color = UnityEngine.Color.white;

            _numberMesh = textObj.GetComponent<MeshRenderer>();
            _numberMesh.sortingOrder = _baseSortingOrder + 3;
        }

        public void SetValue(int newValue)
        {
            Value = newValue;
            if (_numberLabel != null) _numberLabel.text = Value.ToString();
            if (_shadowLabel != null) _shadowLabel.text = Value.ToString();
        }

        public void SetGridOrigin(Vector2Int newOrigin)
        {
            GridOrigin = newOrigin;
        }

        public void SetSortingOrder(int order)
        {
            foreach (var r in _renderers)
            {
                if (r != null) r.sortingOrder = order;
            }
            if (_shadowMesh != null) _shadowMesh.sortingOrder = order + 2;
            if (_numberMesh != null) _numberMesh.sortingOrder = order + 3;
        }

        public void SlideTo(Vector3 targetWorldPos, Vector2Int newOrigin, float duration, Action onArrival)
        {
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoSlideTo(targetWorldPos, newOrigin, duration, onArrival));
        }

        private IEnumerator CoSlideTo(Vector3 targetWorldPos, Vector2Int newOrigin, float duration, Action onArrival)
        {
            IsSliding = true;
            SetSortingOrder(40);
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad: smooth continuation and gentle deceleration into target slot
                float ease = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetWorldPos, ease);
                yield return null;
            }

            transform.position = targetWorldPos;
            GridOrigin = newOrigin;
            IsSliding = false;
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
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoPunchScaleAndFlash());
        }

        private IEnumerator CoPunchScaleAndFlash()
        {
            float duration = 0.32f;
            float elapsed = 0f;

            // Flash white
            foreach (var r in _renderers)
            {
                if (r != null) r.color = new UnityEngine.Color(1.8f, 1.8f, 1.8f, 1f);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Scale punch up to 1.35x and bounce down
                float scale = 1.0f + Mathf.Sin(t * Mathf.PI) * 0.35f;
                transform.localScale = Vector3.one * scale;

                // Restore tint smoothly
                UnityEngine.Color curColor = UnityEngine.Color.Lerp(new UnityEngine.Color(1.8f, 1.8f, 1.8f, 1f), UnityEngine.Color.white, t);
                foreach (var r in _renderers)
                {
                    if (r != null) r.color = curColor;
                }

                yield return null;
            }

            transform.localScale = Vector3.one;
            foreach (var r in _renderers)
            {
                if (r != null) r.color = UnityEngine.Color.white;
            }
            _activeTween = null;
        }

        public void PlayEscapeAnimation(Vector2 exitDir, Action onDone)
        {
            IsEscaping = true;
            if (_activeTween != null) StopCoroutine(_activeTween);
            _activeTween = StartCoroutine(CoEscape(exitDir, onDone));
        }

        private IEnumerator CoEscape(Vector2 exitDir, Action onDone)
        {
            SetSortingOrder(120);
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (Vector3)(exitDir.normalized * 3.5f);
            float duration = 0.36f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = t * t; // Acceleration out

                transform.position = Vector3.Lerp(startPos, targetPos, ease);
                transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 0.2f, ease);

                float alpha = 1f - ease;
                foreach (var r in _renderers)
                {
                    if (r != null)
                    {
                        UnityEngine.Color c = r.color;
                        c.a = alpha;
                        r.color = c;
                    }
                }
                if (_numberLabel != null)
                {
                    UnityEngine.Color c = _numberLabel.color;
                    c.a = alpha;
                    _numberLabel.color = c;
                }
                if (_shadowLabel != null)
                {
                    UnityEngine.Color c = _shadowLabel.color;
                    c.a = alpha;
                    _shadowLabel.color = c;
                }

                yield return null;
            }

            onDone?.Invoke();
            Destroy(gameObject);
        }
    }
}
