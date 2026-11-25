using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockMergePuzzle.Core
{
    [Serializable]
    public struct GridCoord
    {
        public int x;
        public int y;

        public GridCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int ToVector2Int() => new Vector2Int(x, y);
        public static GridCoord FromVector2Int(Vector2Int v) => new GridCoord(v.x, v.y);
    }

    [Serializable]
    public class CustomGateData
    {
        public string color = "Red";
        public int requiredValue = 2;
        public string side = "Top";
        public GridCoord startCell;
        public int span = 1;

        public PieceColor GetPieceColor()
        {
            if (Enum.TryParse(color, true, out PieceColor parsed)) return parsed;
            return PieceColor.Red;
        }

        public GateSide GetGateSide()
        {
            if (Enum.TryParse(side, true, out GateSide parsed)) return parsed;
            return GateSide.Top;
        }

        public Vector2 GetExitDirection()
        {
            switch (GetGateSide())
            {
                case GateSide.Top: return Vector2.up;
                case GateSide.Bottom: return Vector2.down;
                case GateSide.Left: return Vector2.left;
                case GateSide.Right: return Vector2.right;
                default: return Vector2.up;
            }
        }

        public Vector2Int GetTriggerDirection()
        {
            switch (GetGateSide())
            {
                case GateSide.Top: return new Vector2Int(0, -1);
                case GateSide.Bottom: return new Vector2Int(0, 1);
                case GateSide.Left: return new Vector2Int(-1, 0);
                case GateSide.Right: return new Vector2Int(1, 0);
                default: return new Vector2Int(0, -1);
            }
        }

        public List<Vector2Int> GetGateCoords()
        {
            int count = Mathf.Max(1, span);
            List<Vector2Int> coords = new List<Vector2Int>(count);
            GateSide s = GetGateSide();
            bool horizontal = (s == GateSide.Top || s == GateSide.Bottom);
            for (int i = 0; i < count; i++)
            {
                coords.Add(horizontal
                    ? new Vector2Int(startCell.x + i, startCell.y)
                    : new Vector2Int(startCell.x, startCell.y + i));
            }
            return coords;
        }

        public List<Vector2Int> GetTriggerCoords()
        {
            List<Vector2Int> gateCoords = GetGateCoords();
            Vector2Int dir = GetTriggerDirection();
            List<Vector2Int> triggers = new List<Vector2Int>(gateCoords.Count);
            for (int i = 0; i < gateCoords.Count; i++)
            {
                triggers.Add(gateCoords[i] + dir);
            }
            return triggers;
        }

        /// <summary>Returns the adjacent inner playable cell coordinates that enter this gate.</summary>
        public List<Vector2Int> GetApproachCoords()
        {
            List<Vector2Int> gateCoords = GetGateCoords();
            Vector2Int dir = GetTriggerDirection();
            List<Vector2Int> approach = new List<Vector2Int>(gateCoords.Count);
            for (int i = 0; i < gateCoords.Count; i++)
            {
                approach.Add(gateCoords[i] - dir);
            }
            return approach;
        }

        public CustomGateData Clone()
        {
            return new CustomGateData
            {
                color = color,
                requiredValue = requiredValue,
                side = side,
                startCell = startCell,
                span = span
            };
        }
    }

    [Serializable]
    public class CustomPieceData
    {
        public string color = "Red";
        public string shape = "Single_1x1";
        public int value = 1;
        public GridCoord origin;
        public int iceCount = 0;
        public string moveAxis = "None";
        public bool isStone = false;

        public PieceColor GetPieceColor()
        {
            if (Enum.TryParse(color, true, out PieceColor parsed)) return parsed;
            return PieceColor.Red;
        }

        public PieceShape GetPieceShape()
        {
            if (Enum.TryParse(shape, true, out PieceShape parsed)) return parsed;
            return PieceShape.Single_1x1;
        }

        public MoveAxisConstraint GetMoveAxis()
        {
            if (Enum.TryParse(moveAxis, true, out MoveAxisConstraint parsed)) return parsed;
            return MoveAxisConstraint.None;
        }

        public List<Vector2Int> GetOccupiedCells()
        {
            Vector2Int[] offsets = GameConfig.GetShapeOffsets(GetPieceShape());
            List<Vector2Int> result = new List<Vector2Int>(offsets.Length);
            for (int i = 0; i < offsets.Length; i++)
            {
                result.Add(new Vector2Int(origin.x + offsets[i].x, origin.y + offsets[i].y));
            }
            return result;
        }

        public CustomPieceData Clone()
        {
            return new CustomPieceData
            {
                color = color,
                shape = shape,
                value = value,
                origin = origin,
                iceCount = iceCount,
                moveAxis = moveAxis,
                isStone = isStone
            };
        }
    }

    [Serializable]
    public class CustomLevelData
    {
        public int schemaVersion = 1;
        public int levelNumber = 11;
        public string name = "Level 11";
        public string difficulty = "Easy";
        public int timeSeconds = 120;
        public int columns = 6;
        public int rows = 8;
        public float boardScale = 1.0f;
        public float boardOffsetX = 0f;
        public float boardOffsetY = 0f;
        public List<string> cells = new List<string>();
        public List<CustomGateData> gates = new List<CustomGateData>();
        public List<CustomPieceData> pieces = new List<CustomPieceData>();

        public const int MinGridSize = 3;
        public const int MaxGridSize = 14;
        public const float ReferencePhoneAspect = 1440f / 3200f;

        public static CustomLevelData CreateDefault(int levelNumber = 11, int cols = 6, int rows = 8)
        {
            var data = new CustomLevelData
            {
                schemaVersion = 1,
                levelNumber = Mathf.Max(1, levelNumber),
                name = $"Level {Mathf.Max(1, levelNumber)}",
                difficulty = "Easy",
                timeSeconds = 120,
                columns = Mathf.Clamp(cols, MinGridSize, MaxGridSize),
                rows = Mathf.Clamp(rows, MinGridSize, MaxGridSize),
                boardScale = 1.0f,
                boardOffsetX = 0f,
                boardOffsetY = 0f,
                cells = new List<string>(),
                gates = new List<CustomGateData>(),
                pieces = new List<CustomPieceData>()
            };

            for (int r = 0; r < data.rows; r++)
            {
                if (r == 0 || r == data.rows - 1)
                {
                    data.cells.Add(new string('0', data.columns));
                }
                else
                {
                    char[] rowChars = new char[data.columns];
                    for (int c = 0; c < data.columns; c++)
                    {
                        rowChars[c] = (c == 0 || c == data.columns - 1) ? '0' : '1';
                    }
                    data.cells.Add(new string(rowChars));
                }
            }
            return data;
        }

        public void Normalize()
        {
            if (schemaVersion <= 0) schemaVersion = 1;
            levelNumber = Mathf.Max(1, levelNumber);
            if (string.IsNullOrWhiteSpace(name)) name = $"Level {levelNumber}";
            if (string.IsNullOrWhiteSpace(difficulty)) difficulty = "Easy";
            timeSeconds = Mathf.Clamp(timeSeconds <= 0 ? 120 : timeSeconds, 5, 3600);
            columns = Mathf.Clamp(columns <= 0 ? 6 : columns, MinGridSize, MaxGridSize);
            rows = Mathf.Clamp(rows <= 0 ? 8 : rows, MinGridSize, MaxGridSize);
            if (boardScale <= 0.05f || float.IsNaN(boardScale) || float.IsInfinity(boardScale)) boardScale = 1.0f;
            boardScale = Mathf.Clamp(boardScale, 0.45f, 1.80f);
            if (float.IsNaN(boardOffsetX) || float.IsInfinity(boardOffsetX)) boardOffsetX = 0f;
            if (float.IsNaN(boardOffsetY) || float.IsInfinity(boardOffsetY)) boardOffsetY = 0f;
            boardOffsetX = Mathf.Clamp(boardOffsetX, -3.5f, 3.5f);
            boardOffsetY = Mathf.Clamp(boardOffsetY, -4.5f, 4.5f);

            if (cells == null) cells = new List<string>();
            while (cells.Count < rows) cells.Add(new string('0', columns));
            if (cells.Count > rows) cells.RemoveRange(rows, cells.Count - rows);

            for (int r = 0; r < rows; r++)
            {
                string raw = cells[r] ?? string.Empty;
                char[] fixedRow = new char[columns];
                for (int c = 0; c < columns; c++)
                {
                    fixedRow[c] = (c < raw.Length && raw[c] == '1') ? '1' : '0';
                }
                cells[r] = new string(fixedRow);
            }

            if (gates == null) gates = new List<CustomGateData>();
            foreach (var g in gates)
            {
                if (g == null) continue;
                g.requiredValue = Mathf.Clamp(g.requiredValue, 1, 999);
                g.span = Mathf.Clamp(g.span, 1, 6);
                g.color = g.GetPieceColor().ToString();
                g.side = g.GetGateSide().ToString();
            }

            if (pieces == null) pieces = new List<CustomPieceData>();
            foreach (var p in pieces)
            {
                if (p == null) continue;
                p.value = Mathf.Clamp(p.value, 1, 999);
                p.iceCount = Mathf.Max(0, p.iceCount);
                if (p.isStone) p.iceCount = 0;
                p.color = p.GetPieceColor().ToString();
                p.shape = p.GetPieceShape().ToString();
                p.moveAxis = p.GetMoveAxis().ToString();
            }
        }

        public void ResizeGrid(int newCols, int newRows)
        {
            newCols = Mathf.Clamp(newCols, MinGridSize, MaxGridSize);
            newRows = Mathf.Clamp(newRows, MinGridSize, MaxGridSize);
            Normalize();

            var newCells = new List<string>(newRows);
            for (int r = 0; r < newRows; r++)
            {
                char[] rowChars = new char[newCols];
                for (int c = 0; c < newCols; c++)
                {
                    rowChars[c] = (r < rows && c < columns) ? cells[r][c] : '0';
                }
                newCells.Add(new string(rowChars));
            }
            columns = newCols;
            rows = newRows;
            cells = newCells;
        }

        public void SetPlayableCell(int col, int row, bool playable)
        {
            Normalize();
            if (col < 0 || col >= columns || row < 0 || row >= rows) return;
            char[] rowChars = cells[row].ToCharArray();
            rowChars[col] = playable ? '1' : '0';
            cells[row] = new string(rowChars);
        }

        public bool IsPlayableCell(int col, int row)
        {
            if (col < 0 || col >= columns || row < 0 || row >= rows) return false;
            if (cells == null || row >= cells.Count || cells[row] == null || col >= cells[row].Length) return false;
            return cells[row][col] == '1';
        }

        public bool IsGateCell(int col, int row)
        {
            if (gates == null) return false;
            Vector2Int coord = new Vector2Int(col, row);
            for (int i = 0; i < gates.Count; i++)
            {
                var g = gates[i];
                if (g != null && g.GetGateCoords().Contains(coord)) return true;
            }
            return false;
        }

        public bool IsValidCoord(int col, int row)
        {
            return IsPlayableCell(col, row) || IsGateCell(col, row);
        }

        public void GetActiveBounds(out int minCol, out int maxCol, out int minRow, out int maxRow)
        {
            minCol = int.MaxValue;
            maxCol = int.MinValue;
            minRow = int.MaxValue;
            maxRow = int.MinValue;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (IsValidCoord(c, r))
                    {
                        if (c < minCol) minCol = c;
                        if (c > maxCol) maxCol = c;
                        if (r < minRow) minRow = r;
                        if (r > maxRow) maxRow = r;
                    }
                }
            }

            if (gates != null)
            {
                for (int i = 0; i < gates.Count; i++)
                {
                    var g = gates[i];
                    if (g == null) continue;
                    var coords = g.GetGateCoords();
                    for (int j = 0; j < coords.Count; j++)
                    {
                        var gc = coords[j];
                        if (gc.x < minCol) minCol = gc.x;
                        if (gc.x > maxCol) maxCol = gc.x;
                        if (gc.y < minRow) minRow = gc.y;
                        if (gc.y > maxRow) maxRow = gc.y;
                    }
                }
            }

            if (minCol == int.MaxValue)
            {
                minCol = 0;
                maxCol = Mathf.Max(0, columns - 1);
                minRow = 0;
                maxRow = Mathf.Max(0, rows - 1);
            }
        }

        public float ComputeWorldCellSize()
        {
            GetActiveBounds(out int minC, out int maxC, out int minR, out int maxR);
            int activeCols = Mathf.Max(1, maxC - minC + 1);
            int activeRows = Mathf.Max(1, maxR - minR + 1);
            return Mathf.Clamp(Mathf.Min(6.8f / activeCols, 8.4f / activeRows), 0.72f, 1.30f);
        }

        public float ComputeBaseCameraOrthoSize(float screenRatio = ReferencePhoneAspect)
        {
            GetActiveBounds(out int minC, out int maxC, out int minR, out int maxR);
            int activeCols = Mathf.Max(1, maxC - minC + 1);
            int activeRows = Mathf.Max(1, maxR - minR + 1);
            float cellSize = ComputeWorldCellSize();
            const float wallMargin = 0.50f;
            float frameW = (activeCols + wallMargin * 2f) * cellSize;
            float frameH = (activeRows + wallMargin * 2f) * cellSize;
            float sizeByWidth = (frameW * 0.5f + 0.42f) / Mathf.Max(0.1f, screenRatio);
            float sizeByHeight = (frameH * 0.5f) / 0.56f;
            return Mathf.Max(8.70f, Mathf.Max(sizeByWidth, sizeByHeight));
        }

        public float ComputeCameraOrthoSize(float screenRatio = ReferencePhoneAspect)
        {
            float safeScale = (boardScale <= 0.05f) ? 1.0f : Mathf.Clamp(boardScale, 0.45f, 1.80f);
            return ComputeBaseCameraOrthoSize(screenRatio) / safeScale;
        }

        public CustomLevelData Clone()
        {
            var copy = new CustomLevelData
            {
                schemaVersion = schemaVersion,
                levelNumber = levelNumber,
                name = name,
                difficulty = difficulty,
                timeSeconds = timeSeconds,
                columns = columns,
                rows = rows,
                boardScale = boardScale,
                boardOffsetX = boardOffsetX,
                boardOffsetY = boardOffsetY,
                cells = cells != null ? new List<string>(cells) : new List<string>(),
                gates = new List<CustomGateData>(),
                pieces = new List<CustomPieceData>()
            };
            if (gates != null)
            {
                foreach (var g in gates) if (g != null) copy.gates.Add(g.Clone());
            }
            if (pieces != null)
            {
                foreach (var p in pieces) if (p != null) copy.pieces.Add(p.Clone());
            }
            copy.Normalize();
            return copy;
        }

        public bool Validate(out List<string> errors, out List<string> warnings)
        {
            errors = new List<string>();
            warnings = new List<string>();
            Normalize();

            int playableCount = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (IsPlayableCell(c, r)) playableCount++;

            if (playableCount == 0)
            {
                errors.Add("Map chưa có ô sàn chơi nào ('1').");
            }

            if (gates == null || gates.Count == 0)
            {
                errors.Add("Cần ít nhất 1 cổng thoát (Exit Gate).");
            }

            int nonStoneCount = 0;
            if (pieces != null)
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] != null && !pieces[i].isStone) nonStoneCount++;
                }
            }
            if (nonStoneCount == 0)
            {
                errors.Add("Cần ít nhất 1 block di chuyển được (không tính đá cố định).");
            }

            // Validate gates
            var gateOccupied = new Dictionary<Vector2Int, int>();
            if (gates != null)
            {
                for (int i = 0; i < gates.Count; i++)
                {
                    var g = gates[i];
                    if (g == null) continue;
                    var coords = g.GetGateCoords();
                    var approach = g.GetApproachCoords();
                    for (int k = 0; k < coords.Count; k++)
                    {
                        Vector2Int gc = coords[k];
                        if (gc.x < 0 || gc.x >= columns || gc.y < 0 || gc.y >= rows)
                        {
                            errors.Add($"Cổng #{i + 1} ({g.color} [{g.requiredValue}]) nằm ngoài kích thước lưới ({gc.x},{gc.y}).");
                        }
                        if (IsPlayableCell(gc.x, gc.y))
                        {
                            errors.Add($"Cổng #{i + 1} ({g.color} [{g.requiredValue}]) đè lên ô sàn chơi tại ({gc.x},{gc.y}). Cổng phải nằm trên ô viền ('0').");
                        }
                        if (gateOccupied.TryGetValue(gc, out int prevIdx))
                        {
                            errors.Add($"Cổng #{i + 1} bị chồng lên Cổng #{prevIdx + 1} tại ({gc.x},{gc.y}).");
                        }
                        else
                        {
                            gateOccupied[gc] = i;
                        }

                        Vector2Int ap = approach[k];
                        if (!IsPlayableCell(ap.x, ap.y))
                        {
                            errors.Add($"Cổng #{i + 1} ({g.color} [{g.requiredValue}] - {g.side}) tại ({gc.x},{gc.y}) không giáp ô sàn chơi ở hướng vào ({ap.x},{ap.y}).");
                        }
                    }
                }
            }

            // Validate pieces placement & overlaps
            var pieceOccupied = new Dictionary<Vector2Int, int>();
            if (pieces != null)
            {
                for (int i = 0; i < pieces.Count; i++)
                {
                    var p = pieces[i];
                    if (p == null) continue;
                    var cellsOccupied = p.GetOccupiedCells();
                    for (int k = 0; k < cellsOccupied.Count; k++)
                    {
                        Vector2Int c = cellsOccupied[k];
                        if (c.x < 0 || c.x >= columns || c.y < 0 || c.y >= rows)
                        {
                            errors.Add($"Block #{i + 1} ({p.color} {p.shape}) vượt ngoài lưới tại ({c.x},{c.y}).");
                        }
                        else if (!IsPlayableCell(c.x, c.y))
                        {
                            errors.Add($"Block #{i + 1} ({p.color} {p.shape}) nằm trên ô trống/cổng tại ({c.x},{c.y}).");
                        }

                        if (pieceOccupied.TryGetValue(c, out int otherIdx))
                        {
                            errors.Add($"Block #{i + 1} chồng lên Block #{otherIdx + 1} tại ({c.x},{c.y}).");
                        }
                        else
                        {
                            pieceOccupied[c] = i;
                        }
                    }

                    if (!p.isStone && p.iceCount > 0 && p.iceCount >= nonStoneCount)
                    {
                        errors.Add($"Block #{i + 1} đóng băng ({p.iceCount}) >= tổng số block thường khác ({Mathf.Max(0, nonStoneCount - 1)}), không thể phá băng!");
                    }
                }
            }

            // Cross-check piece <-> gate match, merge power, span, axis
            if (pieces != null && gates != null)
            {
                var pieceGroups = new Dictionary<(PieceColor, PieceShape), List<CustomPieceData>>();
                for (int i = 0; i < pieces.Count; i++)
                {
                    var p = pieces[i];
                    if (p == null || p.isStone) continue;
                    var key = (p.GetPieceColor(), p.GetPieceShape());
                    if (!pieceGroups.TryGetValue(key, out var list))
                    {
                        list = new List<CustomPieceData>();
                        pieceGroups[key] = list;
                    }
                    list.Add(p);
                }

                for (int gi = 0; gi < gates.Count; gi++)
                {
                    var g = gates[gi];
                    if (g == null) continue;
                    PieceColor gCol = g.GetPieceColor();
                    bool hasColorPiece = false;
                    foreach (var kv in pieceGroups)
                    {
                        if (kv.Key.Item1 == gCol) { hasColorPiece = true; break; }
                    }
                    if (!hasColorPiece)
                    {
                        warnings.Add($"Cổng #{gi + 1} ({g.color} [{g.requiredValue}]) chưa có block màu {g.color} nào.");
                    }
                }

                foreach (var kv in pieceGroups)
                {
                    PieceColor pCol = kv.Key.Item1;
                    PieceShape pShape = kv.Key.Item2;
                    List<CustomPieceData> list = kv.Value;

                    List<CustomGateData> matchingGates = new List<CustomGateData>();
                    for (int gi = 0; gi < gates.Count; gi++)
                    {
                        if (gates[gi] != null && gates[gi].GetPieceColor() == pCol) matchingGates.Add(gates[gi]);
                    }

                    if (matchingGates.Count == 0)
                    {
                        errors.Add($"Có block màu {pCol} ({pShape}) nhưng chưa có cổng thoát màu {pCol}.");
                        continue;
                    }

                    Vector2Int bSize = GameConfig.GetShapeBoundingSize(pShape);
                    bool anyGateWideEnough = false;
                    int minGateReq = int.MaxValue;
                    for (int gi = 0; gi < matchingGates.Count; gi++)
                    {
                        var g = matchingGates[gi];
                        GateSide side = g.GetGateSide();
                        int neededSpan = (side == GateSide.Top || side == GateSide.Bottom) ? bSize.x : bSize.y;
                        if (g.span >= neededSpan) anyGateWideEnough = true;
                        if (g.requiredValue < minGateReq) minGateReq = g.requiredValue;
                    }

                    if (!anyGateWideEnough)
                    {
                        errors.Add($"Block {pCol} {pShape} (rộng {bSize.x}, cao {bSize.y}) lớn hơn span của mọi cổng màu {pCol}.");
                    }

                    // Binary merge-power solvability check: sum(2^(v-1)) vs target gate 2^(req-1)
                    double totalPower = 0;
                    foreach (var p in list)
                    {
                        totalPower += Math.Pow(2.0, Mathf.Clamp(p.value - 1, 0, 20));
                    }
                    double neededPower = Math.Pow(2.0, Mathf.Clamp(minGateReq - 1, 0, 20));
                    if (totalPower + 0.0001 < neededPower)
                    {
                        warnings.Add($"Nhóm {pCol} {pShape} tổng lực hợp nhất chưa đủ đạt số cổng nhỏ nhất ({minGateReq}).");
                    }
                }
            }

            return errors.Count == 0;
        }

        public string ToJson(bool prettyPrint = true)
        {
            Normalize();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CustomLevelData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            string cleaned = json.Trim().TrimStart('\uFEFF');
            try
            {
                CustomLevelData parsed = JsonUtility.FromJson<CustomLevelData>(cleaned);
                if (parsed == null) return null;
                parsed.Normalize();
                return parsed;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
