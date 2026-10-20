using System;
using UnityEngine;
using UnityEngine.EventSystems;
using BlockMergePuzzle.Core;

namespace BlockMergePuzzle.Gameplay
{
    public class PlayerPieceController : MonoBehaviour
    {
        public static PlayerPieceController Instance { get; private set; }

        private Camera _mainCamera;
        private PuzzlePiece _selectedPiece;
        private Vector3 _startPieceWorldPos;
        private Vector3 _pointerDownPos;
        private bool _isDragging;
        private Vector2Int _dragDirection;
        private int _maxFreeSteps;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            HandleMouseOrTouchInput(mouseWorld);
            HandleKeyboardInput();
        }

        private void HandleMouseOrTouchInput(Vector3 mouseWorld)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
                if (hit != null)
                {
                    PuzzlePiece piece = hit.GetComponentInParent<PuzzlePiece>();
                    if (piece != null && !piece.IsSliding && !piece.IsEscaping)
                    {
                        _selectedPiece = piece;
                        _startPieceWorldPos = piece.transform.position;
                        _pointerDownPos = mouseWorld;
                        _isDragging = true;
                        _dragDirection = Vector2Int.zero;
                        _maxFreeSteps = 0;

                        // Game feel: Lift block on touch
                        _selectedPiece.SetLifted(true);
                        Audio.SoundManager.Instance.PlayPickup();
                    }
                }
                else
                {
                    _selectedPiece = null;
                }
            }
            else if (_isDragging && Input.GetMouseButton(0))
            {
                if (_selectedPiece == null || _selectedPiece.IsSliding || _selectedPiece.IsEscaping)
                {
                    CancelDrag();
                    return;
                }

                Vector3 delta = mouseWorld - _pointerDownPos;

                if (delta.magnitude < 0.015f)
                {
                    _selectedPiece.transform.position = _startPieceWorldPos;
                    _selectedPiece.SetTilt(0f);
                    _dragDirection = Vector2Int.zero;
                    _maxFreeSteps = 0;
                    return;
                }

                // Determine active axis dynamically from finger movement
                Vector2Int currentDir;
                float rawDist;
                float cellDim;
                Vector3 worldDir;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    // Horizontal drag
                    currentDir = (delta.x >= 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                    rawDist = Mathf.Abs(delta.x);
                    cellDim = GameConfig.CellWidth;
                    worldDir = new Vector3(Mathf.Sign(delta.x), 0f, 0f);
                }
                else
                {
                    // Vertical drag (in grid coordinates: delta.y > 0 is UP (0, -1), delta.y < 0 is DOWN (0, 1))
                    currentDir = (delta.y >= 0) ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                    rawDist = Mathf.Abs(delta.y);
                    cellDim = GameConfig.CellHeight;
                    worldDir = new Vector3(0f, Mathf.Sign(delta.y), 0f);
                }

                // Check slide clearance
                var sim = LevelManager.Instance.RaycastSlide(_selectedPiece, currentDir, 8);
                int freeSteps = sim.Steps;
                float maxWorldDist = freeSteps * cellDim;

                float clampedDist;
                if (freeSteps > 0)
                {
                    if (rawDist <= maxWorldDist)
                    {
                        clampedDist = rawDist;
                    }
                    else
                    {
                        // Elastic rubber-band when dragging past available steps
                        clampedDist = maxWorldDist + Mathf.Clamp((rawDist - maxWorldDist) * 0.18f, 0f, 0.14f);
                    }
                }
                else
                {
                    // Elastic rubber-band resistance when pulling into an obstacle
                    clampedDist = Mathf.Clamp(rawDist * 0.18f, 0f, 0.08f);
                }

                _selectedPiece.transform.position = _startPieceWorldPos + worldDir * clampedDist;

                // Subtle dynamic tilt to enhance physical feel
                if (currentDir.x != 0)
                {
                    float tilt = -worldDir.x * Mathf.Clamp(clampedDist * 2.2f, 0f, 3.2f);
                    _selectedPiece.SetTilt(tilt);
                }
                else
                {
                    _selectedPiece.SetTilt(0f);
                }

                _dragDirection = currentDir;
                _maxFreeSteps = freeSteps;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_isDragging && _selectedPiece != null && !_selectedPiece.IsSliding && !_selectedPiece.IsEscaping)
                {
                    Vector3 delta = mouseWorld - _pointerDownPos;
                    bool slideExecuted = false;

                    if (_dragDirection != Vector2Int.zero && _maxFreeSteps > 0)
                    {
                        float cellDim = (_dragDirection.x != 0) ? GameConfig.CellWidth : GameConfig.CellHeight;
                        float rawDist = 0f;
                        if (_dragDirection == new Vector2Int(1, 0)) rawDist = delta.x;
                        else if (_dragDirection == new Vector2Int(-1, 0)) rawDist = -delta.x;
                        else if (_dragDirection == new Vector2Int(0, -1)) rawDist = delta.y;
                        else if (_dragDirection == new Vector2Int(0, 1)) rawDist = -delta.y;

                        // Threshold: dragging at least 35% of a cell snaps to the next cell
                        int targetSteps = Mathf.Clamp(Mathf.FloorToInt(rawDist / cellDim + 0.65f), 0, _maxFreeSteps);

                        if (targetSteps > 0)
                        {
                            // Piece smoothly continues from current dragged position to target cell
                            slideExecuted = LevelManager.Instance.TrySlidePieceSteps(_selectedPiece, _dragDirection, targetSteps);
                        }
                    }

                    if (!slideExecuted)
                    {
                        // Return piece smoothly to start position and lower lift
                        _selectedPiece.SetLifted(false);
                        _selectedPiece.SetTilt(0f);

                        float dist = Vector3.Distance(_selectedPiece.transform.position, _startPieceWorldPos);
                        if (dist > 0.01f)
                        {
                            float returnDur = Mathf.Clamp(dist / 9.5f, 0.06f, 0.14f);
                            _selectedPiece.SlideTo(_startPieceWorldPos, _selectedPiece.GridOrigin, returnDur, null);
                        }
                        else
                        {
                            _selectedPiece.transform.position = _startPieceWorldPos;
                        }

                        Audio.SoundManager.Instance.PlayDrop();
                    }
                }

                _isDragging = false;
                _dragDirection = Vector2Int.zero;
                _maxFreeSteps = 0;
            }
        }

        private void CancelDrag()
        {
            if (_selectedPiece != null)
            {
                _selectedPiece.SetLifted(false);
                _selectedPiece.SetTilt(0f);

                float dist = Vector3.Distance(_selectedPiece.transform.position, _startPieceWorldPos);
                if (dist > 0.01f)
                {
                    _selectedPiece.SlideTo(_startPieceWorldPos, _selectedPiece.GridOrigin, 0.08f, null);
                }
                else
                {
                    _selectedPiece.transform.position = _startPieceWorldPos;
                }
            }
            _isDragging = false;
            _dragDirection = Vector2Int.zero;
            _maxFreeSteps = 0;
            _selectedPiece = null;
        }

        private void HandleKeyboardInput()
        {
            if (_selectedPiece == null || _selectedPiece.IsSliding || _selectedPiece.IsEscaping) return;

            Vector2Int keyDir = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) keyDir = new Vector2Int(0, -1);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) keyDir = new Vector2Int(0, 1);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) keyDir = new Vector2Int(-1, 0);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) keyDir = new Vector2Int(1, 0);

            if (keyDir != Vector2Int.zero)
            {
                // Keyboard arrow moves 1 step at a time for fine control
                LevelManager.Instance.TrySlidePieceSteps(_selectedPiece, keyDir, 1);
            }
        }

        private Vector2Int DetectCardinalDirection(Vector3 delta)
        {
            // Note: in grid coordinates, (0, -1) is UP (towards row 0), (0, 1) is DOWN (towards row 7)
            // In Unity world coordinates: delta.y > 0 is UP, delta.y < 0 is DOWN
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
            }
            else
            {
                return delta.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
            }
        }
    }
}
