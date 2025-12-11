public class PhaseContext
{
    private readonly GameState gameState;
    private readonly GameBoard gameBoard;
    private readonly IEventBus eventBus;
    private IPhaseState currentPhase;
    
    public PhaseContext(GameState gameState, GameBoard gameBoard, IEventBus eventBus)
    {
        this.gameState = gameState;
        this.gameBoard = gameBoard;
        this.eventBus = eventBus;
    }
    
    public void ExecutePhase(IPhaseState newPhase)
    {
        var oldPhase = currentPhase;
        
        eventBus.Publish(new PhaseEnteredEventData 
        { 
            Phase = newPhase,
            GameState = gameState,
            PreviousPhase = oldPhase
        });
        
        newPhase.Execute();
        
        eventBus.Publish(new PhaseExitedEventData 
        { 
            Phase = newPhase,
            GameState = gameState,
        });
        
        currentPhase = newPhase;
    }
}