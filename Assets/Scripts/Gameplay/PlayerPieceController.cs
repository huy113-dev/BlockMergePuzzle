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
        private Vector2Int _startOrigin;
        private Vector2Int _currentOrigin;
        private Vector3 _grabOffset;
        private bool _isDragging;

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
                        _startOrigin = piece.GridOrigin;
                        _currentOrigin = _startOrigin;
                        _grabOffset = piece.transform.position - mouseWorld;
                        _isDragging = true;

                        // Game feel: Lift block on touch
                        _selectedPiece.SetLifted(true);
                        Audio.SoundManager.Instance.PlayPickup();

                        // Temporarily clear from grid so piece can move through its own start cells
                        LevelManager.Instance.ClearPieceFromGrid(_selectedPiece);
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

                Vector3 targetWorld = mouseWorld + _grabOffset;
                float cw = GameConfig.CellWidth;
                float ch = GameConfig.CellHeight;

                // Advance current origin along open cells as finger moves
                bool advanced = true;
                int maxAdvances = 4;
                Vector3 currentCellCenter = LevelManager.Instance.CalculatePieceWorldCenter(_selectedPiece.Shape, _currentOrigin);
                Vector3 delta = targetWorld - currentCellCenter;

                while (advanced && maxAdvances-- > 0)
                {
                    advanced = false;
                    currentCellCenter = LevelManager.Instance.CalculatePieceWorldCenter(_selectedPiece.Shape, _currentOrigin);
                    delta = targetWorld - currentCellCenter;

                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        Vector2Int dX = (delta.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                        if (Mathf.Abs(delta.x) >= 0.5f * cw && LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dX))
                        {
                            _currentOrigin += dX;
                            advanced = true;
                            continue;
                        }
                        Vector2Int dY = (delta.y > 0) ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                        if (Mathf.Abs(delta.y) >= 0.5f * ch && LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dY))
                        {
                            _currentOrigin += dY;
                            advanced = true;
                            continue;
                        }
                    }
                    else
                    {
                        Vector2Int dY = (delta.y > 0) ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                        if (Mathf.Abs(delta.y) >= 0.5f * ch && LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dY))
                        {
                            _currentOrigin += dY;
                            advanced = true;
                            continue;
                        }
                        Vector2Int dX = (delta.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                        if (Mathf.Abs(delta.x) >= 0.5f * cw && LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dX))
                        {
                            _currentOrigin += dX;
                            advanced = true;
                            continue;
                        }
                    }
                }

                // Re-evaluate from current cell center
                currentCellCenter = LevelManager.Instance.CalculatePieceWorldCenter(_selectedPiece.Shape, _currentOrigin);
                delta = targetWorld - currentCellCenter;

                Vector2Int dirX = (delta.x >= 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                bool canMoveX = LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dirX);
                bool isSpecialX = LevelManager.Instance.CheckMergeOpportunity(_selectedPiece, _currentOrigin, dirX, out _) ||
                                  LevelManager.Instance.CheckEscapeOpportunity(_selectedPiece, _currentOrigin, dirX, out _);

                Vector2Int dirY = (delta.y >= 0) ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                bool canMoveY = LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + dirY);
                bool isSpecialY = LevelManager.Instance.CheckMergeOpportunity(_selectedPiece, _currentOrigin, dirY, out _) ||
                                  LevelManager.Instance.CheckEscapeOpportunity(_selectedPiece, _currentOrigin, dirY, out _);

                float moveX;
                if (delta.x >= 0)
                {
                    if (canMoveX) moveX = Mathf.Min(delta.x, cw);
                    else if (isSpecialX) moveX = Mathf.Min(delta.x, cw * 0.85f);
                    else moveX = Mathf.Clamp(delta.x * 0.15f, 0f, 0.08f);
                }
                else
                {
                    if (canMoveX) moveX = Mathf.Max(delta.x, -cw);
                    else if (isSpecialX) moveX = Mathf.Max(delta.x, -cw * 0.85f);
                    else moveX = Mathf.Clamp(delta.x * 0.15f, -0.08f, 0f);
                }

                float moveY;
                if (delta.y >= 0)
                {
                    if (canMoveY) moveY = Mathf.Min(delta.y, ch);
                    else if (isSpecialY) moveY = Mathf.Min(delta.y, ch * 0.85f);
                    else moveY = Mathf.Clamp(delta.y * 0.15f, 0f, 0.08f);
                }
                else
                {
                    if (canMoveY) moveY = Mathf.Max(delta.y, -ch);
                    else if (isSpecialY) moveY = Mathf.Max(delta.y, -ch * 0.85f);
                    else moveY = Mathf.Clamp(delta.y * 0.15f, -0.08f, 0f);
                }

                // Prevent cutting corners through diagonal obstacles
                if (Mathf.Abs(moveX) > 0.05f && Mathf.Abs(moveY) > 0.05f)
                {
                    Vector2Int diagDir = dirX + dirY;
                    if (!LevelManager.Instance.CanPieceFitAt(_selectedPiece, _currentOrigin + diagDir))
                    {
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) moveY = 0f;
                        else moveX = 0f;
                    }
                }

                _selectedPiece.transform.position = currentCellCenter + new Vector3(moveX, moveY, 0f);

                // Subtle dynamic tilt
                float tilt = -Mathf.Clamp(moveX * 3.0f, -3.5f, 3.5f);
                _selectedPiece.SetTilt(tilt);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_isDragging && _selectedPiece != null && !_selectedPiece.IsSliding && !_selectedPiece.IsEscaping)
                {
                    Vector3 currentCellCenter = LevelManager.Instance.CalculatePieceWorldCenter(_selectedPiece.Shape, _currentOrigin);
                    Vector3 offset = _selectedPiece.transform.position - currentCellCenter;
                    float cw = GameConfig.CellWidth;
                    float ch = GameConfig.CellHeight;

                    bool actionExecuted = false;

                    // Check Merge or Escape trigger along X
                    if (Mathf.Abs(offset.x) >= 0.35f * cw)
                    {
                        Vector2Int dirX = (offset.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
                        if (LevelManager.Instance.CheckMergeOpportunity(_selectedPiece, _currentOrigin, dirX, out PuzzlePiece mergeTarget))
                        {
                            LevelManager.Instance.ExecuteMerge(_selectedPiece, mergeTarget);
                            actionExecuted = true;
                        }
                        else if (LevelManager.Instance.CheckEscapeOpportunity(_selectedPiece, _currentOrigin, dirX, out ExitGateView escapeGate))
                        {
                            LevelManager.Instance.ExecuteEscape(_selectedPiece, escapeGate);
                            actionExecuted = true;
                        }
                    }

                    // Check Merge or Escape trigger along Y
                    if (!actionExecuted && Mathf.Abs(offset.y) >= 0.35f * ch)
                    {
                        Vector2Int dirY = (offset.y > 0) ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
                        if (LevelManager.Instance.CheckMergeOpportunity(_selectedPiece, _currentOrigin, dirY, out PuzzlePiece mergeTarget))
                        {
                            LevelManager.Instance.ExecuteMerge(_selectedPiece, mergeTarget);
                            actionExecuted = true;
                        }
                        else if (LevelManager.Instance.CheckEscapeOpportunity(_selectedPiece, _currentOrigin, dirY, out ExitGateView escapeGate))
                        {
                            LevelManager.Instance.ExecuteEscape(_selectedPiece, escapeGate);
                            actionExecuted = true;
                        }
                    }

                    // Otherwise, snap into current cell origin
                    if (!actionExecuted)
                    {
                        LevelManager.Instance.ExecuteSlide(_selectedPiece, _currentOrigin);
                    }

                    _selectedPiece.SetLifted(false);
                    _selectedPiece.SetTilt(0f);
                }

                _isDragging = false;
                _selectedPiece = null;
            }
        }

        private void CancelDrag()
        {
            if (_selectedPiece != null)
            {
                LevelManager.Instance.ExecuteSlide(_selectedPiece, _startOrigin);
                _selectedPiece.SetLifted(false);
                _selectedPiece.SetTilt(0f);
            }
            _isDragging = false;
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
