using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockMergePuzzle.Audio;
using BlockMergePuzzle.Gameplay;
using BlockMergePuzzle.Utils;

namespace BlockMergePuzzle.Core
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        private BlockView[,] _grid;
        private Transform _boardTransform;
        private Transform _blocksContainer;
        private Vector3 _boardOrigin;

        public Vector3 BoardOrigin => _boardOrigin;
        public int Columns => GameConfig.Columns;
        public int Rows => GameConfig.Rows;

        public event Action<int, int> OnBlocksMerged; // (mergedValue, combo)
        public event Action<int> OnScoreGained;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _grid = new BlockView[Columns, Rows];
            CalculateBoardOrigin();
        }

        private void CalculateBoardOrigin()
        {
            // Position board slightly below center to leave room for Top UI
            float originX = -GameConfig.BoardWidth * 0.5f;
            float originY = -GameConfig.BoardHeight * 0.5f - 0.45f;
            _boardOrigin = new Vector3(originX, originY, 0f);
        }

        public void InitializeBoard()
        {
            CalculateBoardOrigin();

            // Clear old slots and blocks if any
            if (_boardTransform != null) Destroy(_boardTransform.gameObject);
            if (_blocksContainer != null) Destroy(_blocksContainer.gameObject);

            _boardTransform = new GameObject("Board_Slots").transform;
            _boardTransform.SetParent(transform);

            _blocksContainer = new GameObject("Blocks_Container").transform;
            _blocksContainer.SetParent(transform);

            _grid = new BlockView[Columns, Rows];

            // Create slot background visual for each cell
            Sprite slotSprite = SpriteGenerator.GetSlotSprite();
            float spriteWidth = slotSprite.rect.width / slotSprite.pixelsPerUnit;
            float targetScale = GameConfig.CellSize / spriteWidth;

            for (int c = 0; c < Columns; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    Vector3 pos = GameConfig.GridToWorldPosition(c, r, _boardOrigin);
                    GameObject slotObj = new GameObject($"Slot_{c}_{r}");
                    slotObj.transform.SetParent(_boardTransform);
                    slotObj.transform.position = pos;
                    slotObj.transform.localScale = Vector3.one * targetScale;

                    SpriteRenderer sr = slotObj.AddComponent<SpriteRenderer>();
                    sr.sprite = slotSprite;
                    sr.sortingOrder = 1;
                }
            }

            // Spawn initial 2 rows of random blocks
            SpawnInitialRows(2);
        }

        private void SpawnInitialRows(int numRows)
        {
            int[] starterValues = { 2, 4, 8, 16 };
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    // Random pick avoiding 3 identical adjacent in a row initially
                    int val = starterValues[UnityEngine.Random.Range(0, starterValues.Length)];
                    Vector3 pos = GameConfig.GridToWorldPosition(c, r, _boardOrigin);
                    BlockView block = BlockView.Create(val, c, r, pos, _blocksContainer);
                    _grid[c, r] = block;
                    block.PlaySpawnAnimation();
                }
            }
        }

        public int GetLowestEmptyRow(int col)
        {
            if (col < 0 || col >= Columns) return -1;
            for (int r = 0; r < Rows; r++)
            {
                if (_grid[col, r] == null) return r;
            }
            return -1;
        }

        public bool IsColumnFull(int col)
        {
            return GetLowestEmptyRow(col) == -1;
        }

        public Vector3 GetDropStartPosition(int col)
        {
            return GameConfig.GridToWorldPosition(col, Rows, _boardOrigin);
        }

        public void DropBlock(int col, int value, Action<BlockView> onLanded)
        {
            int targetRow = GetLowestEmptyRow(col);
            if (targetRow == -1) return;

            Vector3 startPos = GetDropStartPosition(col);
            Vector3 targetPos = GameConfig.GridToWorldPosition(col, targetRow, _boardOrigin);

            BlockView block = BlockView.Create(value, col, targetRow, startPos, _blocksContainer);
            _grid[col, targetRow] = block;

            float distance = Mathf.Abs(startPos.y - targetPos.y);
            float duration = Mathf.Clamp(distance * 0.055f, 0.12f, 0.35f);

            block.MoveTo(targetPos, duration, () =>
            {
                SoundManager.Instance.PlayDrop();
                onLanded?.Invoke(block);
            });
        }

        public void ResolveMerges(BlockView lastPlacedBlock, Action onComplete)
        {
            StartCoroutine(ResolveMergesRoutine(lastPlacedBlock, onComplete));
        }

        private IEnumerator ResolveMergesRoutine(BlockView triggerBlock, Action onComplete)
        {
            int combo = 0;
            bool foundAnyMerge = true;
            BlockView focusBlock = triggerBlock;

            while (foundAnyMerge)
            {
                foundAnyMerge = false;
                List<BlockGroup> matches = FindAllMatches(focusBlock);

                if (matches.Count > 0)
                {
                    foundAnyMerge = true;
                    combo++;

                    // Process matches
                    foreach (var group in matches)
                    {
                        yield return StartCoroutine(MergeGroupRoutine(group, combo));
                    }

                    // Apply gravity
                    yield return StartCoroutine(ApplyGravityRoutine());

                    // Reset focus block to check full board for chain reaction
                    focusBlock = null;
                    yield return new WaitForSeconds(0.08f);
                }
            }

            onComplete?.Invoke();
        }

        private class BlockGroup
        {
            public BlockView Primary;
            public List<BlockView> Neighbors = new List<BlockView>();
            public int ResultValue;
        }

        private List<BlockGroup> FindAllMatches(BlockView preferredFocus)
        {
            List<BlockGroup> groups = new List<BlockGroup>();
            HashSet<BlockView> visited = new HashSet<BlockView>();

            // If we have a preferred focus block, check it first
            if (preferredFocus != null && _grid[preferredFocus.Col, preferredFocus.Row] == preferredFocus)
            {
                List<BlockView> connected = GetConnectedMatches(preferredFocus.Col, preferredFocus.Row, visited);
                if (connected.Count > 1)
                {
                    BlockGroup group = BuildGroup(preferredFocus, connected);
                    groups.Add(group);
                }
            }

            // Check remainder of the grid
            for (int c = 0; c < Columns; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    BlockView b = _grid[c, r];
                    if (b != null && !visited.Contains(b))
                    {
                        List<BlockView> connected = GetConnectedMatches(c, r, visited);
                        if (connected.Count > 1)
                        {
                            BlockGroup group = BuildGroup(connected[0], connected);
                            groups.Add(group);
                        }
                    }
                }
            }

            return groups;
        }

        private BlockGroup BuildGroup(BlockView primary, List<BlockView> all)
        {
            BlockGroup group = new BlockGroup();
            group.Primary = primary;

            int n = all.Count;
            // Value multiplies by 2 for each additional matching block
            int multiplier = 1;
            for (int i = 1; i < n; i++) multiplier *= 2;
            group.ResultValue = primary.Value * multiplier;

            foreach (var b in all)
            {
                if (b != primary)
                {
                    group.Neighbors.Add(b);
                }
            }
            return group;
        }

        private List<BlockView> GetConnectedMatches(int startCol, int startRow, HashSet<BlockView> visited)
        {
            List<BlockView> result = new List<BlockView>();
            BlockView start = _grid[startCol, startRow];
            if (start == null) return result;

            int targetValue = start.Value;
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startCol, startRow));
            visited.Add(start);
            result.Add(start);

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            while (queue.Count > 0)
            {
                Vector2Int curr = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = curr.x + dx[i];
                    int ny = curr.y + dy[i];

                    if (nx >= 0 && nx < Columns && ny >= 0 && ny < Rows)
                    {
                        BlockView neighbor = _grid[nx, ny];
                        if (neighbor != null && !visited.Contains(neighbor) && neighbor.Value == targetValue)
                        {
                            visited.Add(neighbor);
                            result.Add(neighbor);
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }

            return result;
        }

        private IEnumerator MergeGroupRoutine(BlockGroup group, int combo)
        {
            Vector3 targetPos = group.Primary.transform.position;
            float animDuration = 0.16f;

            // Remove neighbors from grid reference immediately
            foreach (var n in group.Neighbors)
            {
                _grid[n.Col, n.Row] = null;
                n.SetSortingOrder(15);
                n.PlayMergeInto(targetPos, animDuration);
            }

            yield return new WaitForSeconds(animDuration);

            // Pop animation on primary block
            group.Primary.PlayMergePop(group.ResultValue);
            SoundManager.Instance.PlayMerge(combo);

            int earnedScore = group.ResultValue * combo;
            OnScoreGained?.Invoke(earnedScore);
            OnBlocksMerged?.Invoke(group.ResultValue, combo);

            yield return new WaitForSeconds(0.18f);
        }

        private IEnumerator ApplyGravityRoutine()
        {
            bool anyFell = false;
            float maxFallTime = 0f;

            for (int c = 0; c < Columns; c++)
            {
                int targetRow = 0;
                for (int r = 0; r < Rows; r++)
                {
                    if (_grid[c, r] != null)
                    {
                        if (targetRow != r)
                        {
                            // Move block down to targetRow
                            BlockView block = _grid[c, r];
                            _grid[c, targetRow] = block;
                            _grid[c, r] = null;

                            block.Col = c;
                            block.Row = targetRow;

                            Vector3 newWorldPos = GameConfig.GridToWorldPosition(c, targetRow, _boardOrigin);
                            float fallDist = Mathf.Abs(block.transform.position.y - newWorldPos.y);
                            float duration = Mathf.Clamp(fallDist * 0.045f, 0.08f, 0.22f);
                            if (duration > maxFallTime) maxFallTime = duration;

                            block.MoveTo(newWorldPos, duration);
                            anyFell = true;
                        }
                        targetRow++;
                    }
                }
            }

            if (anyFell)
            {
                yield return new WaitForSeconds(maxFallTime + 0.04f);
            }
        }

        public int GetMaxBlockValue()
        {
            int max = 2;
            for (int c = 0; c < Columns; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    if (_grid[c, r] != null && _grid[c, r].Value > max)
                    {
                        max = _grid[c, r].Value;
                    }
                }
            }
            return max;
        }

        public bool IsBoardFull()
        {
            for (int c = 0; c < Columns; c++)
            {
                if (!IsColumnFull(c)) return false;
            }
            return true;
        }

        public void ClearGrid()
        {
            for (int c = 0; c < Columns; c++)
            {
                for (int r = 0; r < Rows; r++)
                {
                    if (_grid[c, r] != null)
                    {
                        Destroy(_grid[c, r].gameObject);
                        _grid[c, r] = null;
                    }
                }
            }
        }
    }
}
