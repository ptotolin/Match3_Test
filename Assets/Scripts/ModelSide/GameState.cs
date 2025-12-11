using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    // dependencies
    private IEventBus eventBus;
    
    // locals
    private int score;
    
    // properties
    public List<SC_Gem> DelayedGems { get; } = new();
    public List<SC_Gem> GemsMarkedForActivation { get; } = new();
    public List<SC_Gem> CurrentMatches { get; set; } = new();
    public List<(Vector2Int bombPosition, GlobalEnums.GemType bombType)> BombPlacements { get; set; } 
        = new List<(Vector2Int, GlobalEnums.GemType)>();
    public bool HasMatches { get; set; }
    public bool NeedStable { get; set; }
    public bool IsStable { get; set; }
    public Vector2Int? LastSwapPos1 { get; set; }
    public Vector2Int? LastSwapPos2 { get; set; }
    public bool SwapHappened { get; set; }
    public int Score {
        get => score;
        set {
            if (score != value) {
                eventBus.Publish(new ScoreEventData()
                {
                    ScoreOld = score, ScoreNew = value
                });
                
                score = value;
            }
        }
    }

    public GameState(IEventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void Reset()
    {
        DelayedGems.Clear();
        GemsMarkedForActivation.Clear();
        CurrentMatches.Clear();
        BombPlacements.Clear();
        HasMatches = false;
        NeedStable = false;
        IsStable = false;
        SwapHappened = false;
    }
}