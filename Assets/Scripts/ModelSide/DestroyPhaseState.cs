public class DestroyPhaseState : IPhaseState
{
    public string Name => "Destroy";

    private readonly GameBoard gameBoard;
    private readonly MatchDetector matchDetector;
    private readonly IEventBus eventBus;
    private readonly GameState gameState;

    public DestroyPhaseState(
        GameBoard gameBoard,
        MatchDetector matchDetector,
        IEventBus eventBus,
        GameState gameState)
    {
        this.gameBoard = gameBoard;
        this.matchDetector = matchDetector;
        this.eventBus = eventBus;
        this.gameState = gameState;
    }

    public void Execute()
    {
        if (!gameState.HasMatches) {
            return;
        }

        gameBoard.InvokeBatchStart();

        foreach (var gem in gameState.CurrentMatches) {
            if (gameState.DelayedGems.Contains(gem)) {
                continue;
            }

            ScoreCheck(gem);
            if (gameBoard.TryGetGemPos(gem, out var gemPos)) {
                gameBoard.DestroyGem(gemPos);
            }
        }

        gameBoard.InvokeBatchEnd();

        // Создаем новые бомбы после уничтожения гемов
        gameBoard.InvokeBatchStart();

        foreach (var (bombPosition, bombType) in gameState.BombPlacements) {
            if (gameBoard.GetGem(bombPosition.x, bombPosition.y) == null) {
                var bombGem = SC_GameVariables.Instance.bomb.Clone();
                bombGem.AddComponent(new ColoredBombComponent(bombType));

                var delayedBehavior = new BombPhaseBehavior(bombGem, gameState, eventBus, gameBoard);
                bombGem.AddComponent(delayedBehavior);

                bombGem.SpecialAbility = SpecialAbilityFactory.CreateAbility(
                    GlobalEnums.GemType.bomb,
                    gameBoard,
                    bombGem,
                    eventBus,
                    matchDetector,
                    gameState
                );

                gameBoard.SetGem(bombPosition.x, bombPosition.y, bombGem, GlobalEnums.GemSpawnType.Instant);
            }
        }

        gameBoard.InvokeBatchEnd();
    }

    private void ScoreCheck(SC_Gem gemToCheck)
    {
        gameState.Score += gemToCheck.ScoreValue;
    }
}