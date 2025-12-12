using UnityEngine;
using Zenject;

public class SC_GameLogic : MonoBehaviour
{
    // dependencies
    private GemInputHandler gemInputHandler;
    private GameBoard gameBoard;
    private IGemGenerator gemGenerator;
    private MatchDetector matchDetector;
    private IEventBus eventBus;
    
    // Phase management
    private PhaseContext phaseContext;
    private GameState gameState;

    
    #region MonoBehaviour
    private void OnEnable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected += OnSwipe;
        }
    }

    private void OnDisable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected -= OnSwipe;
        }
    }

    [Inject]
    public void Initialize(
        GemInputHandler gemInputHandler, 
        GameBoard gameBoard, 
        IGemGenerator gemGenerator, 
        MatchDetector matchDetector,
        IEventBus eventBus,
        GameState gameState,
        PhaseContext phaseContext)
    {
        this.gemInputHandler = gemInputHandler;
        //this.gemInputHandler.EventSwipeDetected += OnSwipe;
        this.gameBoard = gameBoard;
        this.gemGenerator = gemGenerator;
        this.matchDetector = matchDetector;
        this.eventBus = eventBus;
        this.gameState = gameState;
        this.phaseContext = phaseContext;
    }
    
    #endregion

    #region Logic
    
    private SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }
    
    private void OnSwipe(Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        var gem1 = GetGem(gem1Pos.x, gem1Pos.y);
        var gem2 = GetGem(gem2Pos.x, gem2Pos.y);
        
        gameState.LastSwapPos1 = gem1Pos;
        gameState.LastSwapPos2 = gem2Pos;
        
        gameBoard.SwapGems(gem1Pos, gem2Pos);

        gameState.SwapHappened = true;
        
        if (!IsSwapProcessed(gem1, gem2, gem1Pos, gem2Pos)) {
            GameLogger.Log($"[Client] Swap back ({gem1Pos}, {gem2Pos})");
            gameBoard.SwapGems(gem2Pos, gem1Pos);
            gameState.SwapHappened = false;
        }
        else {
            ProcessMatches();
        }
    }
    
    private bool IsSwapProcessed(SC_Gem gem1, SC_Gem gem2, Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        matchDetector.FindAllMatches();

        var canGemsMatch = matchDetector.CanGemsMatch(gem1, gem2);
        var gem1Matches = gem1.IsMatch || canGemsMatch;
        var gem2Matches = gem2.IsMatch || canGemsMatch;
        
        var gem1HasAbility = gem1.SpecialAbility != null;
        var gem2HasAbility = gem2.SpecialAbility != null;
        var bothHaveAbilities = gem1HasAbility && gem2HasAbility;
        
        if (!gem1Matches && !gem2Matches) {
            if (!bothHaveAbilities) {
                return false;
            }
        }
        
        if (gem1HasAbility) {
            if (gem1Matches) {
                gameBoard.InvokeBatchStart();
                gem1.SpecialAbility.Execute();
                gameBoard.InvokeBatchEnd();
            }
        }

        if (gem2HasAbility) {
            if (gem2Matches) {
                gameBoard.InvokeBatchStart();
                gem2.SpecialAbility.Execute();
                gameBoard.InvokeBatchEnd();
            }
        }

        if (!gem1Matches && !gem2Matches) {
            gameBoard.InvokeBatchStart();
            gem1.SpecialAbility.Execute();
            gem2.SpecialAbility.Execute();
            gameBoard.InvokeBatchEnd();
        }
        
        return true;
    }
    
    private void ProcessMatches()
    {
        do
        {
            gameBoard.SetDirty(false);
            
            // Match
            var matchPhase = new MatchPhaseState(matchDetector, gameState);
            phaseContext.ExecutePhase(matchPhase);
            // OnExitState => BombPhaseBehaviour - mark bombs as delayed
            
            // Destroy
            var destroyPhase = new DestroyPhaseState(gameBoard, matchDetector, eventBus, gameState);
            phaseContext.ExecutePhase(destroyPhase);
            
            // FillBoard
            var fillBoardPhase = new FillBoardPhaseState(gameBoard, gemGenerator);
            phaseContext.ExecutePhase(fillBoardPhase);
            
        } while (gameState.HasMatches || gameBoard.IsDirty());
        
        // StablePhase
        var stablePhase = new StablePhaseState(gameState);
        phaseContext.ExecutePhase(stablePhase);
        // OnExitState => BombPhaseBehaviour - destroy delayed bombs

        if (gameBoard.IsDirty()) {
            ProcessMatches();
        }
        
        gameState.SwapHappened = false;
    }
    
    #endregion
}