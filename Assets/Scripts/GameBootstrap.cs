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

    private void Awake()
    {
        gameBoard = new GameBoard(width, height);
        GameBoardSetup();
        
        gemInputHandler = Instantiate(gemInputHandlerPrefab);
        gemInputHandler.gameObject.name = "Input handler";
        gemInputHandler.Initialize(gameBoard);
        
        gameLogic = Instantiate(gameLogicPrefab);
        gameLogic.gameObject.name = "Game Logic";
        gameLogic.Initialize(gemInputHandler, gameBoard);

        gameBoardPresenter = Instantiate(gameBoardPresenterPrefab);
        gameBoardPresenter.gameObject.name = "GameBoard Presenter";
        gameBoardPresenter.Initialize(gameBoard);
    }
    
    private void GameBoardSetup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++) {
            // TODO: Remove that from here 
            if (Random.Range(0, 100f) < SC_GameVariables.Instance.bombChance) {
                gameBoard.SetGem(x, y, SC_GameVariables.Instance.bomb);
            }
            else {
                int gemToUse = Random.Range(0, SC_GameVariables.Instance.GemsInfo.Count);

                int iterations = 0;
                while (gameBoard.MatchesAt(new Vector2Int(x, y), SC_GameVariables.Instance.GemsInfo[gemToUse].Gem) &&
                       iterations < 100) {
                    gemToUse = Random.Range(0, SC_GameVariables.Instance.GemsInfo.Count);
                    iterations++;
                }

                gameBoard.SetGem(x, y, SC_GameVariables.Instance.GemsInfo[gemToUse].Gem.Clone());
            }
        }
    }
}