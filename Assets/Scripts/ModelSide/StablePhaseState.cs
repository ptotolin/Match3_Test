public class StablePhaseState : IPhaseState
{
    // dependencies
    private readonly GameState gameState;
    
    // properties
    public string Name => "Stable";

    public StablePhaseState(GameState gameState)
    {
        this.gameState = gameState;
    }
    
    public void Execute()
    {
        gameState.IsStable = true;
        gameState.NeedStable = false;
    }
}