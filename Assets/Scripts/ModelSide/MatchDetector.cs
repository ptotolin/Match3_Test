using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MatchDetector
{
    private readonly IGameBoardReader gameBoard;
    private List<SC_Gem> currentMatches = new();
    // Field for storing gems with abilities that need to be activated
    private List<SC_Gem> gemsWithAbilitiesToActivate = new();

    public List<SC_Gem> GemsWithAbilitiesToActivate => gemsWithAbilitiesToActivate;

    public List<SC_Gem> CurrentMatches => currentMatches;

    public MatchDetector(IGameBoardReader gameBoard)
    {
        this.gameBoard = gameBoard;
    } 
    
    public void FindAllMatches()
    {
        currentMatches.Clear();
        gemsWithAbilitiesToActivate.Clear(); 

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
                        if (CanGemsMatch(leftGem, currentGem) && 
                            CanGemsMatch(rightGem, currentGem)) {
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
                        if (CanGemsMatch(aboveGem, currentGem) && 
                            CanGemsMatch(bellowGem, currentGem)) {
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
    }
    
    /// <summary>
    /// Checks if placing a gem at the specified position will create a match
    /// </summary>
    public bool MatchesAt(Vector2Int checkPosition, SC_Gem gemToCheck)
    {
        if (checkPosition.x > 1) {
            var gem1 = gameBoard.GetGem(checkPosition.x - 1, checkPosition.y);
            var gem2 = gameBoard.GetGem(checkPosition.x - 2, checkPosition.y);
            if (CanGemsMatch(gem1, gemToCheck) &&
                CanGemsMatch(gem2, gemToCheck)) {
                return true;
            }
        }

        if (checkPosition.y > 1) {
            var gem1 = gameBoard.GetGem(checkPosition.x, checkPosition.y - 1);
            var gem2 = gameBoard.GetGem(checkPosition.x, checkPosition.y - 2);
            if (CanGemsMatch(gem1, gemToCheck) &&
                CanGemsMatch(gem2, gemToCheck)) {
                return true;
            }
        }

        return false;
    }
    
    public bool HasMatchOfFourOrMore(out Vector2Int bombPosition, out GlobalEnums.GemType matchedGemType)
    {
        matchedGemType = GlobalEnums.GemType.blue;
        bombPosition = Vector2Int.zero;
        List<MatchGroup> matchGroups = FindMatchesOfFourOrMore();

        if (matchGroups.Count == 0)
            return false;

        // Find the largest match group
        MatchGroup largestGroup = matchGroups[0];
        foreach (var group in matchGroups)
        {
            if (group.Count > largestGroup.Count)
            {
                largestGroup = group;
            }
        }

        // Choose bomb position: take first position from the group (guaranteed to exist)
        // This is similar to how swap position works - it's a real position from the match
        if (largestGroup.Positions.Count > 0)
        {
            bombPosition = largestGroup.Positions[0];
            var gem = gameBoard.GetGem(bombPosition.x, bombPosition.y);
            matchedGemType = gem.Type;
            return true;
        }

        return false;
    }


    /// <summary>
    /// Checks if the swap position participates in a match of 4+ (without placing a bomb)
    /// </summary>
    /// <returns>true if a match of 4+ is found, and returns the position for the bomb</returns>
    public bool HasMatchOfFourOrMoreInSwapPosition(Vector2Int swapPos1, Vector2Int swapPos2,
        out Vector2Int bombPosition, out GlobalEnums.GemType matchedGemType)
    {
        matchedGemType = GlobalEnums.GemType.blue;
        bombPosition = Vector2Int.zero;
        List<MatchGroup> matchGroups = FindMatchesOfFourOrMore();

        if (matchGroups.Count == 0)
            return false;

        int maxGroupSize = 0;

        foreach (var group in matchGroups) {
            // Check if the group contains the swap position
            bool containsPos1 = group.Positions.Contains(swapPos1);
            bool containsPos2 = group.Positions.Contains(swapPos2);

            if (containsPos1 || containsPos2) {
                // Select the swap position with the largest group
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

        matchedGemType = gameBoard.GetGem(bombPosition.x, bombPosition.y).Type;
        return maxGroupSize >= 4;
    }

    /// <summary>
    /// Generates cross pattern for bomb explosion (13 cells: 3x3 + 4 cardinal directions)
    /// Pattern:
    ///      x
    ///      x
    /// x x x B x x x
    ///      x
    ///      x
    /// </summary>
    public List<Vector2Int> GetBombCrossPattern(Vector2Int bombPos)
    {
        List<Vector2Int> pattern = new List<Vector2Int>();

        // Add bomb position itself
        pattern.Add(bombPos);

        // Add 3x3 square neighbors (8 cells around bomb)
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if (dx == 0 && dy == 0) continue; // Skip bomb position

                Vector2Int pos = new Vector2Int(bombPos.x + dx, bombPos.y + dy);
                if (IsValidPosition(pos)) {
                    pattern.Add(pos);
                }
            }
        }

        // Add 4 cells 2 steps away in cardinal directions
        Vector2Int[] cardinalDirections = new Vector2Int[]
        {
            new Vector2Int(0, -2), // up
            new Vector2Int(0, 2), // down
            new Vector2Int(-2, 0), // left
            new Vector2Int(2, 0) // right
        };

        foreach (var offset in cardinalDirections) {
            Vector2Int pos = bombPos + offset;
            if (IsValidPosition(pos)) {
                pattern.Add(pos);
            }
        }

        return pattern;
    }

    public bool CanGemsMatch(SC_Gem gem1, SC_Gem gem2)
    {
        if (gem1 == null || gem2 == null) return false;

        var colored1 = gem1.GetComponent<ColoredBombComponent>();
        var colored2 = gem2.GetComponent<ColoredBombComponent>();

        // Bomb & bomb
        if (colored1 != null && colored2 != null) {
            return colored1.MatchColor == colored2.MatchColor;
        }

        // Bomb & non-bomb
        if (colored1 != null && gem1.Type == GlobalEnums.GemType.bomb) {
            return colored1.MatchColor == gem2.Type;
        }

        if (colored2 != null && gem2.Type == GlobalEnums.GemType.bomb) {
            return colored2.MatchColor == gem1.Type;
        }

        // ordinary gems
        return gem1.Type == gem2.Type;
    }

    /// <summary>
    /// Finds all connected gems of the same type using BFS (breadth-first search)
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

            // Check all neighbors (up, down, left, right)
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(currentPos.x - 1, currentPos.y), // left
                new Vector2Int(currentPos.x + 1, currentPos.y), // right
                new Vector2Int(currentPos.x, currentPos.y - 1), // up
                new Vector2Int(currentPos.x, currentPos.y + 1) // down
            };

            foreach (var neighborPos in neighbors) {
                // Check bounds
                if (neighborPos.x < 0 || neighborPos.x >= gameBoard.Width ||
                    neighborPos.y < 0 || neighborPos.y >= gameBoard.Height)
                    continue;

                // Skip already visited
                if (visited.Contains(neighborPos))
                    continue;

                SC_Gem neighborGem = gameBoard.GetGem(neighborPos.x, neighborPos.y);

                // Check if gem is of the same type
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

        GameLogger.Log($"<color=blue>FindConnectedGemsOfSameType({startPos}, {targetType}) => {sb.ToString()}</color>");

        return connectedGroup;
    }

    /// <summary>
    /// Checks if a group is a match of 4+:
    /// 1. Has a straight line of 4+ (horizontal or vertical)
    /// 2. OR has intersecting lines: horizontal 3+ AND vertical 3+
    /// </summary>
    private bool HasStraightLineOfFourOrMore(List<Vector2Int> group)
    {
        if (group.Count < 4)
            return false;

        // Find all horizontal lines of 3+
        List<List<Vector2Int>> horizontalLines = new List<List<Vector2Int>>();
        Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

        // Group by rows (y coordinate)
        foreach (var pos in group) {
            if (!rows.ContainsKey(pos.y))
                rows[pos.y] = new List<int>();
            rows[pos.y].Add(pos.x);
        }

        // Find consecutive horizontal lines
        foreach (var rowPair in rows) {
            rowPair.Value.Sort();
            List<int> currentLine = new List<int> { rowPair.Value[0] };

            for (int i = 1; i < rowPair.Value.Count; i++) {
                if (rowPair.Value[i] == rowPair.Value[i - 1] + 1) {
                    // Consecutive gems
                    currentLine.Add(rowPair.Value[i]);
                }
                else {
                    // End of line
                    if (currentLine.Count >= 3) {
                        List<Vector2Int> line = new List<Vector2Int>();
                        foreach (var x in currentLine)
                            line.Add(new Vector2Int(x, rowPair.Key));
                        horizontalLines.Add(line);
                    }

                    currentLine = new List<int> { rowPair.Value[i] };
                }
            }

            // Check the last line in the row
            if (currentLine.Count >= 3) {
                List<Vector2Int> line = new List<Vector2Int>();
                foreach (var x in currentLine)
                    line.Add(new Vector2Int(x, rowPair.Key));
                horizontalLines.Add(line);
            }
        }

        // Check if there is a horizontal line of 4+
        foreach (var line in horizontalLines) {
            if (line.Count >= 4) {
                GameLogger.Log($"<color=green>Found horizontal line of {line.Count} gems!</color>");
                return true;
            }
        }

        // Find all vertical lines of 3+
        List<List<Vector2Int>> verticalLines = new List<List<Vector2Int>>();
        Dictionary<int, List<int>> cols = new Dictionary<int, List<int>>();

        // Group by columns (x coordinate)
        foreach (var pos in group) {
            if (!cols.ContainsKey(pos.x))
                cols[pos.x] = new List<int>();
            cols[pos.x].Add(pos.y);
        }

        // Find consecutive vertical lines
        foreach (var colPair in cols) {
            colPair.Value.Sort();
            List<int> currentLine = new List<int> { colPair.Value[0] };

            for (int i = 1; i < colPair.Value.Count; i++) {
                if (colPair.Value[i] == colPair.Value[i - 1] + 1) {
                    // Consecutive gems
                    currentLine.Add(colPair.Value[i]);
                }
                else {
                    // End of line
                    if (currentLine.Count >= 3) {
                        List<Vector2Int> line = new List<Vector2Int>();
                        foreach (var y in currentLine)
                            line.Add(new Vector2Int(colPair.Key, y));
                        verticalLines.Add(line);
                    }

                    currentLine = new List<int> { colPair.Value[i] };
                }
            }

            // Check the last line in the column
            if (currentLine.Count >= 3) {
                List<Vector2Int> line = new List<Vector2Int>();
                foreach (var y in currentLine)
                    line.Add(new Vector2Int(colPair.Key, y));
                verticalLines.Add(line);
            }
        }

        // Check if there is a vertical line of 4+
        foreach (var line in verticalLines) {
            if (line.Count >= 4) {
                GameLogger.Log($"<color=green>Found vertical line of {line.Count} gems!</color>");
                return true;
            }
        }

        // Check intersection: is there a horizontal line of 3+ AND a vertical line of 3+ that intersect
        foreach (var hLine in horizontalLines) {
            if (hLine.Count < 3)
                continue;

            foreach (var vLine in verticalLines) {
                if (vLine.Count < 3)
                    continue;

                // Check line intersection
                foreach (var hPos in hLine) {
                    if (vLine.Contains(hPos)) {
                        // Lines intersect! This is a match of 4+
                        GameLogger.Log(
                            $"<color=green>Found intersecting lines: horizontal {hLine.Count} and vertical {vLine.Count}!</color>");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Finds all groups of 4 or more gems where there is a match of 4+:
    /// - Straight line of 4+ (horizontal or vertical)
    /// - OR intersecting lines: horizontal 3+ AND vertical 3+
    /// </summary>
    private List<MatchGroup> FindMatchesOfFourOrMore()
    {
        List<MatchGroup> matchGroups = new List<MatchGroup>();
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        // Iterate through all gems on the board
        for (int x = 0; x < gameBoard.Width; x++) {
            for (int y = 0; y < gameBoard.Height; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);

                // Skip already processed positions
                if (processedPositions.Contains(currentPos))
                    continue;

                SC_Gem currentGem = gameBoard.GetGem(x, y);
                if (currentGem == null || currentGem.Type == GlobalEnums.GemType.bomb)
                    continue;

                // Find connected group of gems of the same type
                List<Vector2Int> group = FindConnectedGemsOfSameType(currentPos, currentGem.Type);

                // Check if the group has a match of 4+ (straight line of 4+ or intersecting lines of 3+)
                if (group.Count >= 4 && HasStraightLineOfFourOrMore(group)) {
                    matchGroups.Add(new MatchGroup { Positions = group, Count = group.Count });

                    // Mark all positions as processed
                    foreach (var pos in group) {
                        processedPositions.Add(pos);
                    }
                }
            }
        }

        return matchGroups;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gameBoard.Width &&
               pos.y >= 0 && pos.y < gameBoard.Height;
    }
}