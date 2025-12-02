using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GemInputHandler gemInputHandlerPrefab;
    [SerializeField] private GameBoardPresenter gameBoardPresenterPrefab;
    [SerializeField] private SC_GameLogic gameLogicPrefab;
    
    [Header("Gameboard")] 
    [SerializeField] private int width;
    [SerializeField] private int height;

    private SC_GameLogic gameLogic;
    private GemInputHandler gemInputHandler;
    private GameBoardPresenter gameBoardPresenter;
    private GameBoard gameBoard;
    private GameBoardEventsAdapter gameBoardEventsAdapter;
    private MatchDetector matchDetector;

    private void Awake()
    {
        gameBoard = new GameBoard(width, height);
        var gemGenerator = new DistinctGemGenerator(gameBoard);
        matchDetector = new MatchDetector(gameBoard);
        GameBoardSetup(gemGenerator);
        
        // Create phase
        gameLogic = Instantiate(gameLogicPrefab);
        gameLogic.gameObject.name = "Game Logic";
        
        gemInputHandler = Instantiate(gemInputHandlerPrefab);
        gemInputHandler.gameObject.name = "Input handler";
        
        gameBoardPresenter = Instantiate(gameBoardPresenterPrefab);
        gameBoardPresenter.gameObject.name = "GameBoard Presenter";

        gameBoardEventsAdapter = new GameBoardEventsAdapter(gameBoard);
        
        // Initialize phase
        gameLogic.Initialize(gemInputHandler, gameBoard, gemGenerator, matchDetector);
        gameBoardPresenter.Initialize(gameBoard, gameBoardEventsAdapter);
        gemInputHandler.Initialize(gameBoard, gameBoardPresenter);
    }

    private void GameBoardSetup(IGemGenerator gemGenerator)
    {
        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++) {
            // TODO: Remove that from here 
            var gem = gemGenerator.GenerateGem(new Vector2Int(x, y));

            int iterations = 0;
            while (matchDetector.MatchesAt(new Vector2Int(x, y), gem) &&
                   iterations < 100) {
                gem = gemGenerator.GenerateGem(new Vector2Int(x, y));
                iterations++;
            }

            gameBoard.SetGem(x, y, gem, GlobalEnums.GemSpawnType.Instant);
        }
    }
}