using UnityEngine;

public class GameBoardInitializer
{
    // dependencies
    private readonly GameBoard gameBoard;
    private readonly IGemGenerator gemGenerator;
    private readonly MatchDetector matchDetector;
    
    public GameBoardInitializer(GameBoard gameBoard, IGemGenerator gemGenerator, MatchDetector matchDetector)
    {
        this.gameBoard = gameBoard;
        this.gemGenerator = gemGenerator;
        this.matchDetector = matchDetector;
        
        GameBoardSetup();
    }
    
    private void GameBoardSetup()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        for (int y = 0; y < gameBoard.Height; y++) {
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