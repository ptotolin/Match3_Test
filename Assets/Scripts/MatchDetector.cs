using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MatchDetector
{
    private readonly IGameBoardReader gameBoard;
    private List<SC_Gem> currentMatches = new();
    
    public List<SC_Gem> CurrentMatches => currentMatches;
    
    public MatchDetector(IGameBoardReader gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public void FindAllMatches()
    {
        currentMatches.Clear();

        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++) {
            SC_Gem currentGem = gameBoard.GetGem(x, y);
            if (currentGem != null) {
                if (x > 0 && x < gameBoard.Width - 1) {
                    SC_Gem leftGem = gameBoard.GetGem(x - 1, y);
                    SC_Gem rightGem = gameBoard.GetGem(x + 1, y);
                    //checking no empty spots
                    if (leftGem != null && rightGem != null) {
                        //Match
                        if (leftGem.Type == currentGem.Type && rightGem.Type == currentGem.Type) {
                            currentGem.IsMatch = true;
                            leftGem.IsMatch = true;
                            rightGem.IsMatch = true;
                            currentMatches.Add(currentGem);
                            currentMatches.Add(leftGem);
                            currentMatches.Add(rightGem);
                        }
                    }
                }

                if (y > 0 && y < gameBoard.Height - 1) {
                    SC_Gem aboveGem = gameBoard.GetGem(x, y - 1);
                    SC_Gem bellowGem = gameBoard.GetGem(x, y + 1);
                    //checking no empty spots
                    if (aboveGem != null && bellowGem != null) {
                        //Match
                        if (aboveGem.Type == currentGem.Type && bellowGem.Type == currentGem.Type) {
                            currentGem.IsMatch = true;
                            aboveGem.IsMatch = true;
                            bellowGem.IsMatch = true;
                            currentMatches.Add(currentGem);
                            currentMatches.Add(aboveGem);
                            currentMatches.Add(bellowGem);
                        }
                    }
                }
            }
        }

        if (currentMatches.Count > 0)
            currentMatches = currentMatches.Distinct().ToList();

        CheckForBombs();
    }

    public void CheckForBombs()
    {
        for (int i = 0; i < currentMatches.Count; i++) {
            SC_Gem gem = currentMatches[i];
            gameBoard.TryGetGemPos(gem, out var gemPos);
            int x = gemPos.x;
            int y = gemPos.y;

            if (x > 0) {
                if (gameBoard.GetGem(x - 1, y) != null && gameBoard.GetGem(x - 1, y).Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x - 1, y), gameBoard.GetGem(x - 1, y).BlastSize);
            }

            if (x + 1 < gameBoard.Width) {
                if (gameBoard.GetGem(x + 1, y) != null && gameBoard.GetGem(x + 1, y).Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x + 1, y), gameBoard.GetGem(x + 1, y).BlastSize);
            }

            if (y > 0) {
                if (gameBoard.GetGem(x, y - 1) != null && gameBoard.GetGem(x, y - 1).Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y - 1), gameBoard.GetGem(x, y - 1).BlastSize);
            }

            if (gemPos.y + 1 < gameBoard.Height) {
                if (gameBoard.GetGem(x, y + 1) != null && gameBoard.GetGem(x, y + 1).Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y + 1), gameBoard.GetGem(x, y + 1).BlastSize);
            }
        }
    }

    public void MarkBombArea(Vector2Int bombPos, int _BlastSize)
    {
        string _print = "";
        for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++) {
            for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++) {
                if (x >= 0 && x < gameBoard.Width && y >= 0 && y < gameBoard.Height) {
                    if (gameBoard.GetGem(x, y) != null) {
                        _print += "(" + x + "," + y + ")" + System.Environment.NewLine;
                        gameBoard.GetGem(x, y).IsMatch = true;
                        currentMatches.Add(gameBoard.GetGem(x, y));
                    }
                }
            }
        }

        currentMatches = currentMatches.Distinct().ToList();
    }
    
    
    /// <summary>
    /// Проверяет, будет ли матч если поставить гем в указанную позицию
    /// </summary>
    public bool MatchesAt(Vector2Int _PositionToCheck, SC_Gem _GemToCheck)
    {
        if (_PositionToCheck.x > 1) {
            if (gameBoard.GetGem(_PositionToCheck.x - 1, _PositionToCheck.y).Type == _GemToCheck.Type &&
                gameBoard.GetGem(_PositionToCheck.x - 2, _PositionToCheck.y).Type == _GemToCheck.Type) {
                return true;
            }
        }

        if (_PositionToCheck.y > 1) {
            if (gameBoard.GetGem(_PositionToCheck.x, _PositionToCheck.y - 1).Type == _GemToCheck.Type &&
                gameBoard.GetGem(_PositionToCheck.x, _PositionToCheck.y - 2).Type == _GemToCheck.Type) {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Находит все связанные гемы того же типа, используя BFS (поиск в ширину)
    /// </summary>
    private List<Vector2Int> FindConnectedGemsOfSameType(Vector2Int startPos, GlobalEnums.GemType targetType)
    {
        List<Vector2Int> connectedGroup = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0) {
            Vector2Int currentPos = queue.Dequeue();
            connectedGroup.Add(currentPos);

            // Проверяем всех соседей (вверх, вниз, влево, вправо)
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(currentPos.x - 1, currentPos.y), // влево
                new Vector2Int(currentPos.x + 1, currentPos.y), // вправо
                new Vector2Int(currentPos.x, currentPos.y - 1), // вверх
                new Vector2Int(currentPos.x, currentPos.y + 1) // вниз
            };

            foreach (var neighborPos in neighbors) {
                // Проверяем границы
                if (neighborPos.x < 0 || neighborPos.x >= gameBoard.Width ||
                    neighborPos.y < 0 || neighborPos.y >= gameBoard.Height)
                    continue;

                // Пропускаем уже посещенные
                if (visited.Contains(neighborPos))
                    continue;

                SC_Gem neighborGem = gameBoard.GetGem(neighborPos.x, neighborPos.y);

                // Проверяем, что гем того же типа
                if (neighborGem != null && neighborGem.Type == targetType) {
                    queue.Enqueue(neighborPos);
                    visited.Add(neighborPos);
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        foreach (var connectedElement in connectedGroup) {
            if (connectedElement != connectedGroup.Last()) {
                sb.Append($"{connectedElement}, ");
            }
            else {
                sb.Append($"{connectedElement}");
            }
        }

        Debug.Log($"<color=blue>FindConnectedGemsOfSameType({startPos}, {targetType}) => {sb.ToString()}</color>");

        return connectedGroup;
    }
    
    /// <summary>
    /// Проверяет, является ли группа матчем 4+:
    /// 1. Есть прямая линия 4+ (горизонтальная или вертикальная)
    /// 2. ИЛИ есть пересекающиеся линии: горизонтальная 3+ И вертикальная 3+
    /// </summary>
    private bool HasStraightLineOfFourOrMore(List<Vector2Int> group)
    {
        if (group.Count < 4)
            return false;

        // Находим все горизонтальные линии 3+
        List<List<Vector2Int>> horizontalLines = new List<List<Vector2Int>>();
        Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

        // Группируем по строкам (y координата)
        foreach (var pos in group) {
            if (!rows.ContainsKey(pos.y))
                rows[pos.y] = new List<int>();
            rows[pos.y].Add(pos.x);
        }

        // Находим последовательные горизонтальные линии
        foreach (var rowPair in rows) {
            rowPair.Value.Sort();
            List<int> currentLine = new List<int> { rowPair.Value[0] };

            for (int i = 1; i < rowPair.Value.Count; i++) {
                if (rowPair.Value[i] == rowPair.Value[i - 1] + 1) {
                    // Последовательные гемы
                    currentLine.Add(rowPair.Value[i]);
                }
                else {
                    // Конец линии
                    if (currentLine.Count >= 3) {
                        List<Vector2Int> line = new List<Vector2Int>();
                        foreach (var x in currentLine)
                            line.Add(new Vector2Int(x, rowPair.Key));
                        horizontalLines.Add(line);
                    }

                    currentLine = new List<int> { rowPair.Value[i] };
                }
            }

            // Проверяем последнюю линию в строке
            if (currentLine.Count >= 3) {
                List<Vector2Int> line = new List<Vector2Int>();
                foreach (var x in currentLine)
                    line.Add(new Vector2Int(x, rowPair.Key));
                horizontalLines.Add(line);
            }
        }

        // Проверяем, есть ли горизонтальная линия 4+
        foreach (var line in horizontalLines) {
            if (line.Count >= 4) {
                Debug.Log($"<color=green>Found horizontal line of {line.Count} gems!</color>");
                return true;
            }
        }

        // Находим все вертикальные линии 3+
        List<List<Vector2Int>> verticalLines = new List<List<Vector2Int>>();
        Dictionary<int, List<int>> cols = new Dictionary<int, List<int>>();

        // Группируем по столбцам (x координата)
        foreach (var pos in group) {
            if (!cols.ContainsKey(pos.x))
                cols[pos.x] = new List<int>();
            cols[pos.x].Add(pos.y);
        }

        // Находим последовательные вертикальные линии
        foreach (var colPair in cols) {
            colPair.Value.Sort();
            List<int> currentLine = new List<int> { colPair.Value[0] };

            for (int i = 1; i < colPair.Value.Count; i++) {
                if (colPair.Value[i] == colPair.Value[i - 1] + 1) {
                    // Последовательные гемы
                    currentLine.Add(colPair.Value[i]);
                }
                else {
                    // Конец линии
                    if (currentLine.Count >= 3) {
                        List<Vector2Int> line = new List<Vector2Int>();
                        foreach (var y in currentLine)
                            line.Add(new Vector2Int(colPair.Key, y));
                        verticalLines.Add(line);
                    }

                    currentLine = new List<int> { colPair.Value[i] };
                }
            }

            // Проверяем последнюю линию в столбце
            if (currentLine.Count >= 3) {
                List<Vector2Int> line = new List<Vector2Int>();
                foreach (var y in currentLine)
                    line.Add(new Vector2Int(colPair.Key, y));
                verticalLines.Add(line);
            }
        }

        // Проверяем, есть ли вертикальная линия 4+
        foreach (var line in verticalLines) {
            if (line.Count >= 4) {
                Debug.Log($"<color=green>Found vertical line of {line.Count} gems!</color>");
                return true;
            }
        }

        // Проверяем пересечение: есть ли горизонтальная линия 3+ И вертикальная линия 3+ которые пересекаются
        foreach (var hLine in horizontalLines) {
            if (hLine.Count < 3)
                continue;

            foreach (var vLine in verticalLines) {
                if (vLine.Count < 3)
                    continue;

                // Проверяем пересечение линий
                foreach (var hPos in hLine) {
                    if (vLine.Contains(hPos)) {
                        // Линии пересекаются! Это матч 4+
                        Debug.Log(
                            $"<color=green>Found intersecting lines: horizontal {hLine.Count} and vertical {vLine.Count}!</color>");
                        return true;
                    }
                }
            }
        }

        return false;
    }
    
    /// <summary>
    /// Находит все группы из 4 или более гемов, где есть матч 4+:
    /// - Прямая линия 4+ (горизонтальная или вертикальная)
    /// - ИЛИ пересекающиеся линии: горизонтальная 3+ И вертикальная 3+
    /// </summary>
    private List<MatchGroup> FindMatchesOfFourOrMore()
    {
        List<MatchGroup> matchGroups = new List<MatchGroup>();
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        // Проходим по всем гемам на доске
        for (int x = 0; x < gameBoard.Width; x++) {
            for (int y = 0; y < gameBoard.Height; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);

                // Пропускаем уже обработанные позиции
                if (processedPositions.Contains(currentPos))
                    continue;

                SC_Gem currentGem = gameBoard.GetGem(x, y);
                if (currentGem == null || currentGem.Type == GlobalEnums.GemType.bomb)
                    continue;

                // Ищем связанную группу гемов того же типа
                List<Vector2Int> group = FindConnectedGemsOfSameType(currentPos, currentGem.Type);

                // Проверяем, есть ли в группе матч 4+ (прямая линия 4+ или пересекающиеся линии 3+)
                if (group.Count >= 4 && HasStraightLineOfFourOrMore(group)) {
                    matchGroups.Add(new MatchGroup { Positions = group, Count = group.Count });

                    // Отмечаем все позиции как обработанные
                    foreach (var pos in group) {
                        processedPositions.Add(pos);
                    }
                }
            }
        }

        return matchGroups;
    }

    
    /// <summary>
    /// Проверяет, участвует ли позиция свопа в матче 4+ (без постановки бомбы)
    /// </summary>
    /// <returns>true если найден матч 4+, и возвращает позицию для бомбы</returns>
    public bool HasMatchOfFourOrMoreInSwapPosition(Vector2Int swapPos1, Vector2Int swapPos2,
        out Vector2Int bombPosition)
    {
        bombPosition = Vector2Int.zero;
        List<MatchGroup> matchGroups = FindMatchesOfFourOrMore();

        if (matchGroups.Count == 0)
            return false;

        int maxGroupSize = 0;

        foreach (var group in matchGroups) {
            // Проверяем, содержит ли группа позицию свопа
            bool containsPos1 = group.Positions.Contains(swapPos1);
            bool containsPos2 = group.Positions.Contains(swapPos2);

            if (containsPos1 || containsPos2) {
                // Выбираем позицию свопа с наибольшей группой
                if (containsPos1 && group.Count > maxGroupSize) {
                    bombPosition = swapPos1;
                    maxGroupSize = group.Count;
                }
                else if (containsPos2 && group.Count > maxGroupSize) {
                    bombPosition = swapPos2;
                    maxGroupSize = group.Count;
                }
            }
        }

        return maxGroupSize >= 4;
    }
}