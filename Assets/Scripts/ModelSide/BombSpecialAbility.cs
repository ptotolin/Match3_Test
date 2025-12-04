using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombSpecialAbility : IGemSpecialAbility
{
    public string AbilityType => "Bomb";

    private GameBoard gameBoard; // TODO: Problem: we have IGameBoardReader and GameBoard.. Should we introduce IGameBoardWriter ? Or remove the interface ? 
    //private readonly Vector2Int bombPos;
    private readonly IEventBus eventBus;
    private readonly float neighborDestroyDelay;
    private readonly float bombDestroyDelay;
    private readonly SC_Gem gem;
    private Vector2Int gemPos;
    private List<SC_Gem> affectedGems;
    private List<Vector2Int> affectedPositions;
    
    public BombSpecialAbility(
        GameBoard gameBoard,
        SC_Gem gem, 
        IEventBus eventBus,
        float neighborDestroyDelay = 0.3f,
        float bombDestroyDelay = 0.2f)
    {
        this.gameBoard = gameBoard;
        this.eventBus = eventBus;
        this.neighborDestroyDelay = neighborDestroyDelay;
        this.bombDestroyDelay = bombDestroyDelay;
        this.gem = gem;
    }
    
    public void Execute()
    {
        // TODO: Optimize
        // 1. Get cross pattern 
        gameBoard.TryGetGemPos(gem, out gemPos);
        affectedPositions = GetBombCrossPattern(gemPos);
        affectedGems = affectedPositions.Select(t => gameBoard.GetGem(t.x, t.y)).Where(t => t != null).ToList();
        var specialGems = affectedGems.Where(t => t.SpecialAbility != null && t != gem).ToList();
        var filteredGems = affectedGems.Except(specialGems).ToList();

        var specialGemsAffectedEventData =
            new SpecialGemsAffectedEventData(specialGems, AbilityType);
        eventBus.Publish(specialGemsAffectedEventData);
        
        // 2. Publish event for presenter
        var eventData = new BombExplosionEventData(
            gem, 
            filteredGems, 
            neighborDestroyDelay, 
            bombDestroyDelay);
        
        eventBus.Publish(eventData);
        
        // 3. Remove all gems in pattern (including bomb itself)
        foreach (var filteredGem in filteredGems)
        {
            if (filteredGem != null)
            {
                gameBoard.TryGetGemPos(filteredGem, out var pos);
                gameBoard.DestroyGem(pos);
            }
        }
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
    private List<Vector2Int> GetBombCrossPattern(Vector2Int bombPos)
    {
        List<Vector2Int> pattern = new List<Vector2Int>();
        
        // Add bomb position itself
        pattern.Add(bombPos);
        
        // Add 3x3 square neighbors (8 cells around bomb)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip bomb position
                
                Vector2Int pos = new Vector2Int(bombPos.x + dx, bombPos.y + dy);
                if (IsValidPosition(pos))
                {
                    pattern.Add(pos);
                }
            }
        }
        
        // Add 4 cells 2 steps away in cardinal directions
        Vector2Int[] cardinalDirections = new Vector2Int[]
        {
            new Vector2Int(0, -2),  // up
            new Vector2Int(0, 2),    // down
            new Vector2Int(-2, 0),   // left
            new Vector2Int(2, 0)     // right
        };
        
        foreach (var offset in cardinalDirections)
        {
            Vector2Int pos = bombPos + offset;
            if (IsValidPosition(pos))
            {
                pattern.Add(pos);
            }
        }
        
        return pattern;
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gameBoard.Width && 
               pos.y >= 0 && pos.y < gameBoard.Height;
    }
}