using System;
using UnityEngine;
using BlockMergePuzzle.Core;

namespace BlockMergePuzzle.Gameplay
{
    public class ExitGateView : MonoBehaviour
    {
        [field: SerializeField] public PieceColor GateColor { get; private set; }
        [field: SerializeField] public int RequiredValue { get; private set; }
        [field: SerializeField] public GateSide Side { get; private set; }
        [field: SerializeField] public Vector2 ExitDirection { get; private set; }
        public Bounds DetectionBounds { get; private set; }

        private TextMesh _numberLabel;
        private TextMesh _shadowLabel;
        private MeshRenderer _numberMesh;
        private MeshRenderer _shadowMesh;
        private Transform _labelRoot;
        private float _pulseTimer;
        private bool _isHovered;

        public string ColorDisplayName => GameConfig.GetColorNameVietnamese(GateColor);

        public static ExitGateView Create(
            PieceColor gateColor,
            int requiredValue,
            GateSide side,
            Vector2 exitDir,
            Vector3 worldPos,
            Vector2 gateSize,
            Transform parent)
        {
            GameObject go = new GameObject($"Gate_{gateColor}_{side}_{requiredValue}");
            go.transform.SetParent(parent);
            go.transform.position = worldPos;

            ExitGateView gate = go.AddComponent<ExitGateView>();
            gate.Initialize(gateColor, requiredValue, side, exitDir, worldPos, gateSize);
            return gate;
        }

        private void Initialize(
            PieceColor gateColor,
            int requiredValue,
            GateSide side,
            Vector2 exitDir,
            Vector3 worldPos,
            Vector2 gateSize)
        {
            GateColor = gateColor;
            RequiredValue = requiredValue;
            Side = side;
            ExitDirection = exitDir;
            DetectionBounds = new Bounds(worldPos, new Vector3(gateSize.x, gateSize.y, 5f));

            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = gateSize;
            col.isTrigger = true;

            BuildNumberLabel();
        }

        private void BuildNumberLabel()
        {
            _labelRoot = new GameObject("LabelRoot").transform;
            _labelRoot.SetParent(transform, false);

            // Drop shadow text
            GameObject shadowObj = new GameObject("GateShadow");
            shadowObj.transform.SetParent(_labelRoot, false);
            shadowObj.transform.localPosition = new Vector3(0.025f, -0.025f, -0.1f);

            _shadowLabel = shadowObj.AddComponent<TextMesh>();
            _shadowLabel.text = RequiredValue.ToString();
            _shadowLabel.fontSize = 62;
            _shadowLabel.characterSize = 0.088f;
            _shadowLabel.anchor = TextAnchor.MiddleCenter;
            _shadowLabel.alignment = TextAlignment.Center;
            _shadowLabel.fontStyle = FontStyle.Bold;
            _shadowLabel.color = new UnityEngine.Color(0.05f, 0.05f, 0.12f, 0.95f);

            _shadowMesh = shadowObj.GetComponent<MeshRenderer>();
            _shadowMesh.sortingOrder = 5;

            // Main text
            GameObject textObj = new GameObject("GateNumber");
            textObj.transform.SetParent(_labelRoot, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.15f);

            _numberLabel = textObj.AddComponent<TextMesh>();
            _numberLabel.text = RequiredValue.ToString();
            _numberLabel.fontSize = 62;
            _numberLabel.characterSize = 0.088f;
            _numberLabel.anchor = TextAnchor.MiddleCenter;
            _numberLabel.alignment = TextAlignment.Center;
            _numberLabel.fontStyle = FontStyle.Bold;
            _numberLabel.color = UnityEngine.Color.white;

            _numberMesh = textObj.GetComponent<MeshRenderer>();
            _numberMesh.sortingOrder = 6;
        }

        private void Awake()
        {
            if (_labelRoot == null)
            {
                _labelRoot = transform.Find("LabelRoot");
                if (_labelRoot != null)
                {
                    Transform tNum = _labelRoot.Find("GateNumber");
                    if (tNum != null) _numberLabel = tNum.GetComponent<TextMesh>();
                    Transform tShadow = _labelRoot.Find("GateShadow");
                    if (tShadow != null) _shadowLabel = tShadow.GetComponent<TextMesh>();
                }
            }
        }

        private void Update()
        {
            // Subtle idle breathing pulse on the gate number
            _pulseTimer += Time.deltaTime * (_isHovered ? 6f : 2.5f);
            float scaleMultiplier = 1.0f + Mathf.Sin(_pulseTimer) * (_isHovered ? 0.12f : 0.04f);
            if (_labelRoot != null)
            {
                _labelRoot.localScale = Vector3.one * scaleMultiplier;
            }
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            if (_numberLabel != null)
            {
                _numberLabel.color = hovered ? new UnityEngine.Color(1f, 1f, 0.6f) : UnityEngine.Color.white;
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            return DetectionBounds.Contains(point);
        }
    }
}
