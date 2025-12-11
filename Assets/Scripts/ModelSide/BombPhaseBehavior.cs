public class BombPhaseBehavior : IGemPhaseBehavior
{
    private readonly SC_Gem gem;
    private readonly GameState gameState;
    private readonly IEventBus eventBus;
    private readonly GameBoard gameBoard;

    public string ComponentType => "BombPhaseBehavior";

    public BombPhaseBehavior(SC_Gem gem, GameState gameState, IEventBus eventBus, GameBoard gameBoard)
    {
        this.gem = gem;
        this.gameState = gameState;
        this.eventBus = eventBus;
        this.gameBoard = gameBoard;

        //eventBus.Subscribe<PhaseEnteredEventData>(OnPhaseEntered);
        eventBus.Subscribe<PhaseExitedEventData>(OnPhaseExited);
    }

    private void Cleanup()
    {
        //eventBus.Unsubscribe<PhaseEnteredEventData>(OnPhaseEntered);
        eventBus.Unsubscribe<PhaseExitedEventData>(OnPhaseExited);
    }

    private void OnPhaseExited(PhaseExitedEventData eventData)
    {
        if (eventData.Phase is MatchPhaseState)
        {
            if (gameState.CurrentMatches.Contains(gem))
            {
                bool wasSwapped = IsGemInSwap();
                
                if (wasSwapped)
                {
                    // Бомба была свопнута - должна была уже взорваться в CheckSwapValidity
                    // Или взрываем сейчас
                    GameLogger.Log($"<color=orange>Bomb was swapped - exploding immediately</color>");
                    gem.SpecialAbility?.Execute();
                    Cleanup();
                }
                else
                {
                    // Бомба попала в матч (не swap) - отложенный взрыв
                    if (!gameState.DelayedGems.Contains(gem))
                    {
                        gameState.DelayedGems.Add(gem);
                        GameLogger.Log($"<color=orange>Bomb matched (not swapped) - delaying explosion</color>");
                    }
                }
            }
        }
        else if (eventData.Phase is StablePhaseState)
        {
            // В стабильной фазе взрываем отложенные бомбы
            if (gameState.DelayedGems.Contains(gem))
            {
                GameLogger.Log($"<color=orange>Bomb: Exploding delayed bomb in stable phase</color>");
                gem.SpecialAbility?.Execute();
                gameState.DelayedGems.Remove(gem);
                Cleanup();
            }
        }
    }
    
    private bool IsGemInSwap()
    {
        if (!gameState.SwapHappened)
            return false;

        if (gameBoard.TryGetGemPos(gem, out var gemPos)) {
            return gemPos == gameState.LastSwapPos1 || gemPos == gameState.LastSwapPos2;
        }

        return false;
    }
}