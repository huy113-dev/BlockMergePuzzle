using System;
using System.Collections;
using UnityEngine;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Gameplay
{
    public class BlockView : MonoBehaviour
    {
        public int Value { get; private set; }
        public int Col { get; set; }
        public int Row { get; set; }

        private SpriteRenderer _spriteRenderer;
        private TextMesh _textMesh;
        private TextMesh _shadowTextMesh;
        private MeshRenderer _textMeshRenderer;
        private MeshRenderer _shadowRenderer;
        private Coroutine _moveCoroutine;
        private Coroutine _scaleCoroutine;

        public static BlockView Create(int value, int col, int row, Vector3 worldPos, Transform parent = null)
        {
            GameObject go = new GameObject($"GemBlock_{value}_{col}_{row}");
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = worldPos;

            BlockView view = go.AddComponent<BlockView>();
            view.Initialize(value, col, row);
            return view;
        }

        private void Initialize(int value, int col, int row)
        {
            Value = value;
            Col = col;
            Row = row;

            // Setup SpriteRenderer with custom Gem Sprite
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = GameConfig.GetBlockSprite(value);
            _spriteRenderer.sortingOrder = 10;

            transform.localScale = Vector3.one * GetBaseScale();

            // Setup Text Shadow for readability over gem facets
            GameObject shadowObj = new GameObject("LabelShadow");
            shadowObj.transform.SetParent(transform, false);
            shadowObj.transform.localPosition = new Vector3(0.02f, -0.02f, -0.03f);

            _shadowTextMesh = shadowObj.AddComponent<TextMesh>();
            _shadowTextMesh.fontSize = 54;
            _shadowTextMesh.characterSize = 0.08f;
            _shadowTextMesh.anchor = TextAnchor.MiddleCenter;
            _shadowTextMesh.alignment = TextAlignment.Center;
            _shadowTextMesh.fontStyle = FontStyle.Bold;
            _shadowTextMesh.color = new Color(0f, 0f, 0f, 0.75f);

            _shadowRenderer = shadowObj.GetComponent<MeshRenderer>();
            _shadowRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _shadowRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1;

            // Setup Front TextMesh for number display
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.06f);

            _textMesh = textObj.AddComponent<TextMesh>();
            _textMesh.fontSize = 54;
            _textMesh.characterSize = 0.08f;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.fontStyle = FontStyle.Bold;

            _textMeshRenderer = textObj.GetComponent<MeshRenderer>();
            _textMeshRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _textMeshRenderer.sortingOrder = _spriteRenderer.sortingOrder + 2;

            UpdateVisuals();
        }

        public float GetBaseScale()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return 1f;
            float spriteWidth = _spriteRenderer.sprite.rect.width / _spriteRenderer.sprite.pixelsPerUnit;
            float spriteHeight = _spriteRenderer.sprite.rect.height / _spriteRenderer.sprite.pixelsPerUnit;
            float maxDim = Mathf.Max(spriteWidth, spriteHeight);
            return (GameConfig.CellSize * 0.96f) / maxDim;
        }

        public void SetValue(int newValue)
        {
            Value = newValue;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = GameConfig.GetBlockSprite(Value);
                _spriteRenderer.color = GameConfig.GetBlockColor(Value);
            }

            bool showText = GameConfig.ShowNumbersOnBlocks;
            if (_textMesh != null)
            {
                _textMesh.gameObject.SetActive(showText);
                if (showText)
                {
                    string text = GameConfig.FormatNumber(Value);
                    _textMesh.text = text;
                    _textMesh.color = GameConfig.GetTextColor(Value);

                    float charSize = 0.082f;
                    if (Value >= 100000) charSize = 0.052f;
                    else if (Value >= 10000) charSize = 0.062f;
                    else if (Value >= 1000) charSize = 0.072f;

                    _textMesh.characterSize = charSize;

                    if (_shadowTextMesh != null)
                    {
                        _shadowTextMesh.gameObject.SetActive(true);
                        _shadowTextMesh.text = text;
                        _shadowTextMesh.characterSize = charSize;
                    }
                }
                else
                {
                    if (_shadowTextMesh != null) _shadowTextMesh.gameObject.SetActive(false);
                }
            }
        }

        public void MoveTo(Vector3 targetPos, float duration, Action onComplete = null)
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveRoutine(targetPos, duration, onComplete));
        }

        private IEnumerator MoveRoutine(Vector3 targetPos, float duration, Action onComplete)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease out quad
                t = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            _moveCoroutine = null;
            onComplete?.Invoke();
        }

        public void PlayMergePop(int newValue, Action onComplete = null)
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(MergePopRoutine(newValue, onComplete));
        }

        private IEnumerator MergePopRoutine(int newValue, Action onComplete)
        {
            float duration = 0.22f;
            float elapsed = 0f;
            bool updatedValue = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (!updatedValue && t >= 0.4f)
                {
                    SetValue(newValue);
                    updatedValue = true;
                }

                float baseScale = GetBaseScale();
                float scaleMult = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
                transform.localScale = Vector3.one * (baseScale * scaleMult);
                yield return null;
            }

            SetValue(newValue);
            transform.localScale = Vector3.one * GetBaseScale();
            _scaleCoroutine = null;
            onComplete?.Invoke();
        }

        public void PlayMergeInto(Vector3 targetPos, float duration, Action onComplete = null)
        {
            StartCoroutine(MergeIntoRoutine(targetPos, duration, onComplete));
        }

        private IEnumerator MergeIntoRoutine(Vector3 targetPos, float duration, Action onComplete)
        {
            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            Color startColor = _spriteRenderer.color;
            Color startTextColor = _textMesh.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = t * t; // Ease in

                transform.position = Vector3.Lerp(startPos, targetPos, easeT);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, easeT);

                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, easeT);
                _spriteRenderer.color = c;

                Color tc = startTextColor;
                tc.a = Mathf.Lerp(1f, 0f, easeT);
                _textMesh.color = tc;
                if (_shadowTextMesh != null) _shadowTextMesh.color = new Color(0, 0, 0, tc.a * 0.75f);

                yield return null;
            }

            onComplete?.Invoke();
            Destroy(gameObject);
        }

        public void PlaySpawnAnimation()
        {
            StartCoroutine(SpawnScaleRoutine());
        }

        private IEnumerator SpawnScaleRoutine()
        {
            float baseScale = GetBaseScale();
            float duration = 0.18f;
            float elapsed = 0f;

            transform.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.1f;
                if (t > 0.8f) s = Mathf.Lerp(1.1f, 1.0f, (t - 0.8f) / 0.2f);
                transform.localScale = Vector3.one * (baseScale * s);
                yield return null;
            }

            transform.localScale = Vector3.one * baseScale;
        }

        public void SetSortingOrder(int order)
        {
            if (_spriteRenderer != null) _spriteRenderer.sortingOrder = order;
            if (_shadowRenderer != null) _shadowRenderer.sortingOrder = order + 1;
            if (_textMeshRenderer != null) _textMeshRenderer.sortingOrder = order + 2;
        }
    }
}
