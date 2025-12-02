using Unity.VisualScripting;
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

    private void Awake()
    {
        gameBoard = new GameBoard(width, height);
        GameBoardSetup();
        
        // Create phase
        gameLogic = Instantiate(gameLogicPrefab);
        gameLogic.gameObject.name = "Game Logic";
        
        gemInputHandler = Instantiate(gemInputHandlerPrefab);
        gemInputHandler.gameObject.name = "Input handler";
        
        gameBoardPresenter = Instantiate(gameBoardPresenterPrefab);
        gameBoardPresenter.gameObject.name = "GameBoard Presenter";

        gameBoardEventsAdapter = new GameBoardEventsAdapter(gameBoard);
        
        // Initialize phase
        gameLogic.Initialize(gemInputHandler, gameBoard);
        gameBoardPresenter.Initialize(gameBoard, gameBoardEventsAdapter);
        gemInputHandler.Initialize(gameBoard, gameBoardPresenter);
    }
    
    private void GameBoardSetup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++) {
            // TODO: Remove that from here 
            if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance) {
                gameBoard.SetGem(x, y, SC_GameVariables.Instance.bomb.Clone(), GlobalEnums.GemSpawnType.Instant);
            }
            else {
                int gemToUse = Random.Range(0, SC_GameVariables.Instance.GemsInfo.Count);

                int iterations = 0;
                while (gameBoard.MatchesAt(new Vector2Int(x, y), SC_GameVariables.Instance.GemsInfo[gemToUse].Gem) &&
                       iterations < 100) {
                    gemToUse = Random.Range(0, SC_GameVariables.Instance.GemsInfo.Count);
                    iterations++;
                }

                gameBoard.SetGem(x, y, SC_GameVariables.Instance.GemsInfo[gemToUse].Gem.Clone(), GlobalEnums.GemSpawnType.Instant);
            }
        }
    }
}