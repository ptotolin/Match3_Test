using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombSpecialAbility : IGemSpecialAbility
{
    public string AbilityType => "Bomb";

    private GameBoard gameBoard; // TODO: Problem: we have IGameBoardReader and GameBoard.. Should we introduce IGameBoardWriter ? Or remove the interface ? 
    private MatchDetector matchDetector;
    //private readonly Vector2Int bombPos;
    private readonly IEventBus eventBus;
    private readonly float neighborDestroyDelay;
    private readonly float bombDestroyDelay;
    private readonly SC_Gem gem;
    private Vector2Int gemPos;
    private List<SC_Gem> affectedGems;
    private List<Vector2Int> affectedPositions;
    private GameState gameState;
    
    public BombSpecialAbility(
        GameBoard gameBoard,
        MatchDetector matchDetector,
        SC_Gem gem, 
        IEventBus eventBus,
        GameState gameState,
        float neighborDestroyDelay = 0.3f,
        float bombDestroyDelay = 0.2f)
    {
        this.gameBoard = gameBoard;
        this.matchDetector = matchDetector;
        this.eventBus = eventBus;
        this.neighborDestroyDelay = neighborDestroyDelay;
        this.bombDestroyDelay = bombDestroyDelay;
        this.gem = gem;
        this.gameState = gameState;
    }
    
    public void Execute()
    {
        // TODO: Optimize
        // 1. Get cross pattern 
        gameBoard.TryGetGemPos(gem, out gemPos);
        affectedPositions = matchDetector.GetBombCrossPattern(gemPos);
        affectedGems = affectedPositions.Select(t => gameBoard.GetGem(t.x, t.y)).Where(t => t != null).ToList();
        var specialGems = affectedGems.Where(t => t.SpecialAbility != null && t != gem).ToList();
        var filteredGems = affectedGems.Except(specialGems).ToList();

        // var specialGemsAffectedEventData =
        //     new SpecialGemsAffectedEventData(specialGems, AbilityType);
        // eventBus.Publish(specialGemsAffectedEventData);

        // TODO: I believe this is temporary solution
        foreach (var specialGem in specialGems) {
            if (!gameState.DelayedGems.Contains(specialGem)) {
                gameState.DelayedGems.Add(specialGem);
            }
        }
        
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
}