using System;
using UnityEngine;
using BlockMergePuzzle.Core;
using BlockMergePuzzle.UI;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public int CurrentValue { get; private set; }
        public int NextValue { get; private set; }
        public bool InputEnabled { get; set; } = true;

        private Camera _mainCamera;
        private int _hoveredCol = 2; // Default to center column
        private GameObject _ghostIndicator;
        private SpriteRenderer _ghostRenderer;
        private TextMesh _ghostText;
        private MeshRenderer _ghostTextRenderer;

        public event Action<int, int> OnHandValuesChanged; // (currentVal, nextVal)

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#if UNITY_2023_1_OR_NEWER
            _mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
#else
            _mainCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
#endif
        }

        private void Start()
        {
            CreateGhostIndicator();
        }

        public void InitializeGame()
        {
            int maxVal = GridManager.Instance != null ? GridManager.Instance.GetMaxBlockValue() : 4;
            CurrentValue = GameConfig.GetRandomSpawnValue(maxVal);
            NextValue = GameConfig.GetRandomSpawnValue(maxVal);
            InputEnabled = true;

            UpdateGhostVisuals();
            OnHandValuesChanged?.Invoke(CurrentValue, NextValue);
        }

        private void CreateGhostIndicator()
        {
            if (_ghostIndicator != null) return;

            _ghostIndicator = new GameObject("GhostBlock_Indicator");
            _ghostIndicator.transform.SetParent(transform);

            _ghostRenderer = _ghostIndicator.AddComponent<SpriteRenderer>();
            _ghostRenderer.sprite = SpriteGenerator.GetBlockSprite();
            _ghostRenderer.sortingOrder = 20;

            float spriteWidth = _ghostRenderer.sprite.rect.width / _ghostRenderer.sprite.pixelsPerUnit;
            float targetScale = GameConfig.CellSize / spriteWidth;
            _ghostIndicator.transform.localScale = Vector3.one * targetScale;

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(_ghostIndicator.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);

            _ghostText = textObj.AddComponent<TextMesh>();
            _ghostText.fontSize = 54;
            _ghostText.characterSize = 0.08f;
            _ghostText.anchor = TextAnchor.MiddleCenter;
            _ghostText.alignment = TextAlignment.Center;
            _ghostText.fontStyle = FontStyle.Bold;

            _ghostTextRenderer = textObj.GetComponent<MeshRenderer>();
            _ghostTextRenderer.sortingLayerID = _ghostRenderer.sortingLayerID;
            _ghostTextRenderer.sortingOrder = _ghostRenderer.sortingOrder + 1;

            UpdateGhostVisuals();
        }

        private void Update()
        {
            if (!InputEnabled || GameManager.Instance == null || GameManager.Instance.IsGameOver)
            {
                if (_ghostIndicator != null && _ghostIndicator.activeSelf)
                    _ghostIndicator.SetActive(false);
                return;
            }

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            // Check mouse/touch position
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            // Calculate corresponding column
            Vector3 origin = GridManager.Instance.BoardOrigin;
            float relativeX = worldPos.x - origin.x;
            float colWidth = GameConfig.CellSize + GameConfig.CellSpacing;
            int col = Mathf.FloorToInt(relativeX / colWidth);
            col = Mathf.Clamp(col, 0, GameConfig.Columns - 1);

            _hoveredCol = col;

            // Position ghost block at top of column
            if (_ghostIndicator != null)
            {
                if (!_ghostIndicator.activeSelf) _ghostIndicator.SetActive(true);
                Vector3 ghostPos = GridManager.Instance.GetDropStartPosition(_hoveredCol);
                _ghostIndicator.transform.position = ghostPos;

                // Subtle bobbing animation
                float bob = Mathf.Sin(Time.time * 6f) * 0.05f;
                _ghostIndicator.transform.position += new Vector3(0f, bob, 0f);
            }

            // Click / Tap to drop
            if (Input.GetMouseButtonUp(0))
            {
                // Ensure click is inside game board X bounds and not on UI buttons
                if (relativeX >= -0.5f && relativeX <= GameConfig.BoardWidth + 0.5f)
                {
                    TryDropCurrentBlock(_hoveredCol);
                }
            }
        }

        public void TryDropCurrentBlock(int col)
        {
            if (!InputEnabled || GridManager.Instance.IsColumnFull(col)) return;

            InputEnabled = false;
            int dropVal = CurrentValue;

            // Roll next values
            int maxVal = GridManager.Instance.GetMaxBlockValue();
            CurrentValue = NextValue;
            NextValue = GameConfig.GetRandomSpawnValue(maxVal);

            UpdateGhostVisuals();
            OnHandValuesChanged?.Invoke(CurrentValue, NextValue);

            GridManager.Instance.DropBlock(col, dropVal, (landedBlock) =>
            {
                GridManager.Instance.ResolveMerges(landedBlock, () =>
                {
                    // Check Game Over
                    if (GridManager.Instance.IsBoardFull())
                    {
                        GameManager.Instance.TriggerGameOver();
                    }
                    else
                    {
                        InputEnabled = true;
                    }
                });
            });
        }

        private void UpdateGhostVisuals()
        {
            if (_ghostRenderer != null)
            {
                _ghostRenderer.sprite = GameConfig.GetBlockSprite(CurrentValue);
                Color c = GameConfig.GetBlockColor(CurrentValue);
                c.a = 0.85f;
                _ghostRenderer.color = c;

                if (_ghostRenderer.sprite != null)
                {
                    float spriteWidth = _ghostRenderer.sprite.rect.width / _ghostRenderer.sprite.pixelsPerUnit;
                    float spriteHeight = _ghostRenderer.sprite.rect.height / _ghostRenderer.sprite.pixelsPerUnit;
                    float maxDim = Mathf.Max(spriteWidth, spriteHeight);
                    float targetScale = (GameConfig.CellSize * 0.96f) / maxDim;
                    _ghostIndicator.transform.localScale = Vector3.one * targetScale;
                }
            }

            if (_ghostText != null)
            {
                bool showText = GameConfig.ShowNumbersOnBlocks;
                _ghostText.gameObject.SetActive(showText);
                if (showText)
                {
                    _ghostText.text = GameConfig.FormatNumber(CurrentValue);
                    Color tc = GameConfig.GetTextColor(CurrentValue);
                    _ghostText.color = tc;
                }
            }
        }
    }
}
