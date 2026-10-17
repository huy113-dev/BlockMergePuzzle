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

                // Re-evaluate direction if uninitialized or if current direction is blocked
                if (_dragDirection == Vector2Int.zero || _maxFreeSteps == 0)
                {
                    if (delta.magnitude > 0.05f)
                    {
                        Vector2Int candidateDir = DetectCardinalDirection(delta);
                        var sim = LevelManager.Instance.RaycastSlide(_selectedPiece, candidateDir, 8);
                        if (sim.Steps > 0 || _dragDirection == Vector2Int.zero)
                        {
                            _dragDirection = candidateDir;
                            _maxFreeSteps = sim.Steps;
                        }
                    }
                }

                if (_dragDirection != Vector2Int.zero)
                {
                    float cellDim = (_dragDirection.x != 0) ? GameConfig.CellWidth : GameConfig.CellHeight;
                    float rawDist = 0f;
                    if (_dragDirection == new Vector2Int(1, 0)) rawDist = delta.x;
                    else if (_dragDirection == new Vector2Int(-1, 0)) rawDist = -delta.x;
                    else if (_dragDirection == new Vector2Int(0, -1)) rawDist = delta.y; // screen UP is grid -Y
                    else if (_dragDirection == new Vector2Int(0, 1)) rawDist = -delta.y; // screen DOWN is grid +Y

                    float maxWorldDist = _maxFreeSteps * cellDim;
                    float clampedDist;
                    if (_maxFreeSteps > 0)
                    {
                        clampedDist = Mathf.Clamp(rawDist, 0f, maxWorldDist);
                    }
                    else
                    {
                        // Slight rubber-band resistance when pulling against an obstacle
                        clampedDist = Mathf.Clamp(rawDist * 0.25f, 0f, 0.06f);
                    }

                    Vector3 visualOffset = Vector3.zero;
                    if (_dragDirection == new Vector2Int(1, 0)) visualOffset = new Vector3(clampedDist, 0f, 0f);
                    else if (_dragDirection == new Vector2Int(-1, 0)) visualOffset = new Vector3(-clampedDist, 0f, 0f);
                    else if (_dragDirection == new Vector2Int(0, -1)) visualOffset = new Vector3(0f, clampedDist, 0f);
                    else if (_dragDirection == new Vector2Int(0, 1)) visualOffset = new Vector3(0f, -clampedDist, 0f);

                    _selectedPiece.transform.position = _startPieceWorldPos + visualOffset;
                }
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
                            // Piece smoothly continues from current dragged position to target cell (no jerk/teleport)
                            slideExecuted = LevelManager.Instance.TrySlidePieceSteps(_selectedPiece, _dragDirection, targetSteps);
                        }
                    }

                    if (!slideExecuted)
                    {
                        // Return piece smoothly to start position without popping
                        float dist = Vector3.Distance(_selectedPiece.transform.position, _startPieceWorldPos);
                        if (dist > 0.01f)
                        {
                            float returnDur = Mathf.Clamp(dist / 9.5f, 0.05f, 0.12f);
                            _selectedPiece.SlideTo(_startPieceWorldPos, _selectedPiece.GridOrigin, returnDur, null);
                        }
                        else
                        {
                            _selectedPiece.transform.position = _startPieceWorldPos;
                        }
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
