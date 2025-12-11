using System.Collections.Generic;
using UnityEngine;


public class MatchPhaseState : IPhaseState 
{
    public string Name => "Match";
    
    private readonly MatchDetector matchDetector;
    private readonly GameState gameState;

    public MatchPhaseState(
        MatchDetector matchDetector, 
        GameState gameState) 
    {
        this.matchDetector = matchDetector;
        this.gameState = gameState;
    }

    public void Execute()
    {
        matchDetector.FindAllMatches();
        gameState.CurrentMatches = matchDetector.CurrentMatches;
        gameState.HasMatches = gameState.CurrentMatches.Count > 0;

        if (!gameState.HasMatches) {
            gameState.NeedStable = true;
            return;
        }
        
        List<(Vector2Int bombPosition, GlobalEnums.GemType bombType)> bombPlacements;

        if (gameState.SwapHappened) {
            bombPlacements = matchDetector.GetAllMatchGroupsOfFourOrMore(
                gameState.LastSwapPos1,
                gameState.LastSwapPos2);
        }
        else {
            bombPlacements = matchDetector.GetAllMatchGroupsOfFourOrMore();
        }
        
        gameState.BombPlacements = bombPlacements;
    }
}