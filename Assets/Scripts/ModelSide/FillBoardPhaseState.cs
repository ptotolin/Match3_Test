using UnityEngine;

public class FillBoardPhaseState : IPhaseState
{
    public string Name => "FillBoard";
    
    private readonly GameBoard gameBoard;
    private readonly IGemGenerator gemGenerator;
    
    public FillBoardPhaseState(
        GameBoard gameBoard, 
        IGemGenerator gemGenerator)
    {
        this.gameBoard = gameBoard;
        this.gemGenerator = gemGenerator;
    }
    
    public void Execute()
    {
        gameBoard.InvokeBatchStart();
        DecreaseRows(gameBoard);
        RefillBoard(gameBoard, gemGenerator);
        gameBoard.InvokeBatchEnd();
    }
    
    private void DecreaseRows(GameBoard gameBoard)
    {
        var nullCounter = 0;
        for (var x = 0; x < gameBoard.Width; x++)
        {
            for (var y = 0; y < gameBoard.Height; y++)
            {
                var curGem = gameBoard.GetGem(x, y);
                if (curGem == null) 
                {
                    nullCounter++;
                }
                else if (nullCounter > 0) 
                {
                    gameBoard.MoveGem(new Vector2Int(x, y), new Vector2Int(x, y - nullCounter));
                }
            }
            nullCounter = 0;
        }
    }
    
    private void RefillBoard(GameBoard gameBoard, IGemGenerator generator)
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem curGem = gameBoard.GetGem(x, y);
                if (curGem == null)
                {
                    var newGem = generator.GenerateGem(new Vector2Int(x, y));
                    gameBoard.SetGem(x, y, newGem, GlobalEnums.GemSpawnType.FallFromTop);
                }
            }
        }
    }
}