using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistinctGemGenerator : IGemGenerator
{
    private readonly GameBoard gameBoard;
    
    public DistinctGemGenerator(GameBoard gameBoard)
    {
        this.gameBoard = gameBoard;
    }
    
    public SC_Gem GenerateGem(Vector2Int position)
    {
        var availableGems = SC_GameVariables.Instance.GemsInfo;
        if (availableGems == null || availableGems.Count == 0) {
            GameLogger.LogError("No gems available for generation!");
            return null;
        }
        
        HashSet<GlobalEnums.GemType> forbiddenTypes = new HashSet<GlobalEnums.GemType>();

        int x = position.x;
        int y = position.y;

        // Horizontal
        // [x-2, x-1, x]
        if (x >= 2) {
            var left1 = gameBoard.GetGem(x - 1, y);
            var left2 = gameBoard.GetGem(x - 2, y);
            if (left1 != null && left2 != null && left1.Type == left2.Type) {
                forbiddenTypes.Add(left1.Type);
            }
        }

        // [x, x+1, x+2]
        if (x < gameBoard.Width - 2) {
            var right1 = gameBoard.GetGem(x + 1, y);
            var right2 = gameBoard.GetGem(x + 2, y);
            if (right1 != null && right2 != null && right1.Type == right2.Type) {
                forbiddenTypes.Add(right1.Type);
            }
        }

        // [x-1, x, x+1]
        if (x > 0 && x < gameBoard.Width - 1) {
            var left = gameBoard.GetGem(x - 1, y);
            var right = gameBoard.GetGem(x + 1, y);
            if (left != null && right != null && left.Type == right.Type) {
                forbiddenTypes.Add(left.Type);
            }
        }

        // Verticals
        // [y-2, y-1, y]
        if (y >= 2) {
            var up1 = gameBoard.GetGem(x, y - 1);
            var up2 = gameBoard.GetGem(x, y - 2);
            if (up1 != null && up2 != null && up1.Type == up2.Type) {
                forbiddenTypes.Add(up1.Type);
            }
        }

        // [y, y+1, y+2] 
        if (y < gameBoard.Height - 2) {
            var down1 = gameBoard.GetGem(x, y + 1);
            var down2 = gameBoard.GetGem(x, y + 2);
            if (down1 != null && down2 != null && down1.Type == down2.Type) {
                forbiddenTypes.Add(down1.Type);
            }
        }

        // [y-1, y, y+1] 
        if (y > 0 && y < gameBoard.Height - 1) {
            var up = gameBoard.GetGem(x, y - 1);
            var down = gameBoard.GetGem(x, y + 1);
            if (up != null && down != null && up.Type == down.Type) {
                forbiddenTypes.Add(up.Type);
            }
        }

        // no bombs
        forbiddenTypes.Add(GlobalEnums.GemType.bomb);

        // Final filtration
        var safeGems = new List<GemInfo>();
        foreach (var gemInfo in availableGems) {
            if (!forbiddenTypes.Contains(gemInfo.Gem.Type)) {
                safeGems.Add(gemInfo);
            }
        }

        // if all the types are forbidden (except bomb)
        if (safeGems.Count == 0) {
            safeGems = availableGems.Where(g => g.Gem.Type != GlobalEnums.GemType.bomb).ToList();
        }

        // Final decision
        int randomIndex = Random.Range(0, safeGems.Count);
        return safeGems[randomIndex].Gem.Clone();
    }
}