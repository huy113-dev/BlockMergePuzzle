using System;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Gameplay
{
    public class ExitGateView : MonoBehaviour
    {
        [field: SerializeField] public PieceColor GateColor { get; private set; }
        [field: SerializeField] public int RequiredValue { get; private set; }
        [field: SerializeField] public GateSide Side { get; private set; }
        [field: SerializeField] public Vector2 ExitDirection { get; private set; }
        [field: SerializeField] public Vector2Int TriggerDirection { get; private set; }
        [field: SerializeField] public bool IsTechHologram { get; private set; }
        [field: SerializeField] public List<Vector2Int> TriggerCoords { get; private set; } = new List<Vector2Int>();
        [field: SerializeField] public Bounds DetectionBounds { get; private set; }

        private TextMesh _numberLabel;
        private TextMesh _shadowLabel;
        private MeshRenderer _numberMesh;
        private MeshRenderer _shadowMesh;
        private Transform _labelRoot;
        private Transform _techRingTransform;
        private Transform _innerRingTransform;
        private Transform _floatingDigitRoot;
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
            Transform parent,
            Vector2Int[] triggerCoords = null,
            Vector2Int triggerDir = default,
            bool isTechHologram = false)
        {
            GameObject go = new GameObject($"Gate_{gateColor}_{side}_{requiredValue}");
            go.transform.SetParent(parent);
            go.transform.position = worldPos;

            ExitGateView gate = go.AddComponent<ExitGateView>();
            gate.Initialize(gateColor, requiredValue, side, exitDir, worldPos, gateSize, triggerCoords, triggerDir, isTechHologram);
            return gate;
        }

        private void Initialize(
            PieceColor gateColor,
            int requiredValue,
            GateSide side,
            Vector2 exitDir,
            Vector3 worldPos,
            Vector2 gateSize,
            Vector2Int[] triggerCoords,
            Vector2Int triggerDir,
            bool isTechHologram)
        {
            GateColor = gateColor;
            RequiredValue = requiredValue;
            Side = side;
            ExitDirection = exitDir;
            IsTechHologram = isTechHologram;
            DetectionBounds = new Bounds(worldPos, new Vector3(gateSize.x, gateSize.y, 5f));

            if (triggerDir != default)
            {
                TriggerDirection = triggerDir;
            }
            else
            {
                switch (side)
                {
                    case GateSide.Top: TriggerDirection = new Vector2Int(0, -1); break;
                    case GateSide.Bottom: TriggerDirection = new Vector2Int(0, 1); break;
                    case GateSide.Left: TriggerDirection = new Vector2Int(-1, 0); break;
                    case GateSide.Right: TriggerDirection = new Vector2Int(1, 0); break;
                }
            }

            if (triggerCoords != null && triggerCoords.Length > 0)
            {
                TriggerCoords.AddRange(triggerCoords);
            }

            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = gateSize;
            col.isTrigger = true;

            if (IsTechHologram)
                BuildTechHologramLabel();
            else
                BuildNumberLabel();
        }

        private TextMesh CreateDigitLayer(string name, Transform parent, Vector3 localPos, int fontSize, float charSize, Color color, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;

            TextMesh tm = obj.AddComponent<TextMesh>();
            tm.text = RequiredValue.ToString();
            tm.fontSize = fontSize;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.sortingOrder = sortingOrder;
            return tm;
        }

        private static Color GetPureNeonColor(PieceColor gateColor)
        {
            Color baseCol = GameConfig.GetColorRGB(gateColor);
            float maxC = Mathf.Max(baseCol.r, Mathf.Max(baseCol.g, baseCol.b));
            if (maxC > 0.01f)
            {
                baseCol.r /= maxC;
                baseCol.g /= maxC;
                baseCol.b /= maxC;
            }
            baseCol.a = 1f;
            return baseCol;
        }

        private void BuildTechHologramLabel()
        {
            _labelRoot = new GameObject("LabelRoot").transform;
            _labelRoot.SetParent(transform, false);

            Color pureNeon = GetPureNeonColor(GateColor);
            Color padTint = Color.Lerp(pureNeon, Color.white, 0.12f);
            Color ringTint = Color.Lerp(pureNeon, Color.white, 0.16f);
            Color innerTint = Color.Lerp(pureNeon, Color.white, 0.10f);

            // 1. Holographic emitter pad & glowing corner brackets tinted to gate color
            Sprite padSprite = GemSpriteManager.GetTechGateHoloPadSprite();
            if (padSprite != null)
            {
                GameObject padObj = new GameObject("GateHoloPad");
                padObj.transform.SetParent(_labelRoot, false);
                padObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                padObj.transform.localScale = new Vector3(0.485f, 0.39f, 1f);
                SpriteRenderer padSr = padObj.AddComponent<SpriteRenderer>();
                padSr.sprite = padSprite;
                padSr.color = padTint;
                padSr.sortingOrder = 3;
            }

            // 2. Outer rotating segmented cyber ring tinted to gate color
            Sprite ringSprite = GemSpriteManager.GetTechGateRingSprite();
            if (ringSprite != null)
            {
                GameObject ringObj = new GameObject("GateTechRing");
                ringObj.transform.SetParent(_labelRoot, false);
                ringObj.transform.localPosition = new Vector3(0f, 0f, -0.08f);
                ringObj.transform.localScale = new Vector3(0.345f, 0.345f, 1f);
                SpriteRenderer ringSr = ringObj.AddComponent<SpriteRenderer>();
                ringSr.sprite = ringSprite;
                ringSr.color = ringTint;
                ringSr.sortingOrder = 4;
                _techRingTransform = ringObj.transform;
            }

            // 3. Inner counter-rotating precision ring & dark glass visor tinted to gate color
            Sprite innerSprite = GemSpriteManager.GetTechGateInnerRingSprite();
            if (innerSprite != null)
            {
                GameObject innerObj = new GameObject("GateInnerRing");
                innerObj.transform.SetParent(_labelRoot, false);
                innerObj.transform.localPosition = new Vector3(0f, 0f, -0.11f);
                innerObj.transform.localScale = new Vector3(0.340f, 0.340f, 1f);
                SpriteRenderer innerSr = innerObj.AddComponent<SpriteRenderer>();
                innerSr.sprite = innerSprite;
                innerSr.color = innerTint;
                innerSr.sortingOrder = 5;
                _innerRingTransform = innerObj.transform;
            }

            // 4. Projected floor cast shadow so the digit visibly levitates above the gate
            _shadowLabel = CreateDigitLayer(
                "GateCastShadow", _labelRoot,
                new Vector3(0.038f, -0.046f, -0.12f),
                140, 0.040f,
                new Color(pureNeon.r * 0.08f, pureNeon.g * 0.05f, pureNeon.b * 0.05f, 0.85f),
                6);
            _shadowMesh = _shadowLabel.GetComponent<MeshRenderer>();

            // 5. Floating 3D-extruded & neon-glowing digit root matched to gate color
            _floatingDigitRoot = new GameObject("FloatingDigit").transform;
            _floatingDigitRoot.SetParent(_labelRoot, false);
            _floatingDigitRoot.localPosition = new Vector3(0f, 0.014f, -0.15f);

            const int fs = 140;
            const float cs = 0.040f;

            // Outer neon glow aura matching gate color (8 directions)
            Vector2[] haloDirs = {
                new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1),
                new Vector2(-0.707f, -0.707f), new Vector2(0.707f, -0.707f),
                new Vector2(-0.707f, 0.707f), new Vector2(0.707f, 0.707f)
            };
            Color outerHaloCol = new Color(pureNeon.r, pureNeon.g * 0.7f, pureNeon.b * 0.7f, 0.56f);
            for (int i = 0; i < haloDirs.Length; i++)
            {
                Vector3 off = new Vector3(haloDirs[i].x * 0.022f, haloDirs[i].y * 0.022f, 0.01f);
                CreateDigitLayer($"HoloGlow_{i}", _floatingDigitRoot, off, fs, cs, outerHaloCol, 7);
            }

            // 3D Extruded embossed depth stack matching gate color (dark deep hue -> mid hue)
            Color ext3 = new Color(pureNeon.r * 0.18f, pureNeon.g * 0.18f, pureNeon.b * 0.18f, 0.98f);
            Color ext2 = new Color(pureNeon.r * 0.42f, pureNeon.g * 0.42f, pureNeon.b * 0.42f, 0.98f);
            Color ext1 = new Color(pureNeon.r * 0.74f, pureNeon.g * 0.74f, pureNeon.b * 0.74f, 0.98f);
            CreateDigitLayer("Extrude_3", _floatingDigitRoot, new Vector3(0.022f, -0.025f, -0.01f), fs, cs, ext3, 8);
            CreateDigitLayer("Extrude_2", _floatingDigitRoot, new Vector3(0.015f, -0.017f, -0.02f), fs, cs, ext2, 8);
            CreateDigitLayer("Extrude_1", _floatingDigitRoot, new Vector3(0.008f, -0.009f, -0.03f), fs, cs, ext1, 9);

            // Crisp neon inner rim (4 cardinal directions)
            Color rimCol = Color.Lerp(pureNeon, Color.white, 0.28f);
            rimCol.a = 0.95f;
            Vector2[] rimDirs = { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) };
            for (int i = 0; i < rimDirs.Length; i++)
            {
                Vector3 off = new Vector3(rimDirs[i].x * 0.011f, rimDirs[i].y * 0.011f, -0.035f);
                CreateDigitLayer($"NeonRim_{i}", _floatingDigitRoot, off, fs, cs, rimCol, 9);
            }

            // Top-left specular bevel highlight
            Color bevelCol = Color.Lerp(pureNeon, Color.white, 0.52f);
            bevelCol.a = 0.95f;
            CreateDigitLayer("BevelHighlight", _floatingDigitRoot, new Vector3(-0.006f, 0.008f, -0.04f), fs, cs, bevelCol, 10);

            // Main luminous gate-colored digit face
            Color faceCol = Color.Lerp(pureNeon, Color.white, 0.14f);
            _numberLabel = CreateDigitLayer(
                "GateNumber", _floatingDigitRoot,
                new Vector3(0f, 0f, -0.05f),
                fs, cs,
                faceCol,
                11);
            _numberMesh = _numberLabel.GetComponent<MeshRenderer>();
        }

        private void BuildNumberLabel()
        {
            _labelRoot = new GameObject("LabelRoot").transform;
            _labelRoot.SetParent(transform, false);

            // Contrast badge behind number for clear visibility over gate arrows or backgrounds
            GameObject badgeObj = new GameObject("GateBadge");
            badgeObj.transform.SetParent(_labelRoot, false);
            badgeObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            badgeObj.transform.localScale = new Vector3(0.52f, 0.52f, 1f);

            SpriteRenderer badgeSr = badgeObj.AddComponent<SpriteRenderer>();
            badgeSr.sprite = SpriteGenerator.GetCircleSprite();
            badgeSr.color = new UnityEngine.Color(0.08f, 0.09f, 0.14f, 0.88f);
            badgeSr.sortingOrder = 4;

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
                    _floatingDigitRoot = _labelRoot.Find("FloatingDigit");
                    _techRingTransform = _labelRoot.Find("GateTechRing");
                    _innerRingTransform = _labelRoot.Find("GateInnerRing");

                    Transform tNum = _floatingDigitRoot != null ? _floatingDigitRoot.Find("GateNumber") : _labelRoot.Find("GateNumber");
                    if (tNum != null) _numberLabel = tNum.GetComponent<TextMesh>();
                    Transform tShadow = _labelRoot.Find("GateCastShadow") ?? _labelRoot.Find("GateShadow");
                    if (tShadow != null) _shadowLabel = tShadow.GetComponent<TextMesh>();
                }
            }
        }

        private void Update()
        {
            _pulseTimer += Time.deltaTime * (_isHovered ? 6f : 2.5f);

            if (IsTechHologram)
            {
                if (_techRingTransform != null)
                {
                    _techRingTransform.Rotate(0f, 0f, -18f * Time.deltaTime);
                }
                if (_innerRingTransform != null)
                {
                    _innerRingTransform.Rotate(0f, 0f, 25f * Time.deltaTime);
                    float ringPulse = 0.340f + Mathf.Sin(_pulseTimer * 1.8f) * 0.014f;
                    _innerRingTransform.localScale = new Vector3(ringPulse, ringPulse, 1f);
                }
                if (_floatingDigitRoot != null)
                {
                    float bobY = 0.014f + Mathf.Sin(_pulseTimer * 1.4f) * (_isHovered ? 0.030f : 0.020f);
                    _floatingDigitRoot.localPosition = new Vector3(0f, bobY, -0.15f);
                    float scaleBoost = _isHovered ? 1.12f : (1.0f + Mathf.Sin(_pulseTimer) * 0.025f);
                    _floatingDigitRoot.localScale = Vector3.one * scaleBoost;
                }
            }
            else
            {
                // Subtle idle breathing pulse on the gate number
                float scaleMultiplier = 1.0f + Mathf.Sin(_pulseTimer) * (_isHovered ? 0.12f : 0.04f);
                if (_labelRoot != null)
                {
                    _labelRoot.localScale = Vector3.one * scaleMultiplier;
                }
            }
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            if (_numberLabel != null)
            {
                if (IsTechHologram)
                {
                    Color pureNeon = GetPureNeonColor(GateColor);
                    _numberLabel.color = hovered
                        ? Color.Lerp(pureNeon, Color.white, 0.38f)
                        : Color.Lerp(pureNeon, Color.white, 0.14f);
                }
                else
                {
                    _numberLabel.color = hovered ? new UnityEngine.Color(1f, 1f, 0.6f) : UnityEngine.Color.white;
                }
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            return DetectionBounds.Contains(point);
        }
    }
}
