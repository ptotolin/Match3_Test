using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SwapGemsCommand : IGameBoardCommand
{
    public string Name => "SwapGemsCommand";

    public string Details {
        get {
            return $"({gem1}, {gem1Pos}) <--> ({gem2}, {gem2Pos})";
        }
    }
    
    // dependencies
    private readonly ICellModelToWorldConverter cellModelToWorldConverter;

    // locals
    private CompositeCommand compositeCommand;
    private SC_Gem gem1;
    private SC_Gem gem2;
    private Vector2Int gem1Pos;
    private Vector2Int gem2Pos;
    
    public SwapGemsCommand(SC_Gem gem1, SC_Gem gem2, Vector2Int gem1Pos, Vector2Int gem2Pos, 
        ICellModelToWorldConverter cellModelToWorldConverter, GameBoardPresenter gameBoardPresenter)
    {
        this.gem1 = gem1;
        this.gem2 = gem2;
        this.gem1Pos = gem1Pos;
        this.gem2Pos = gem2Pos;
        
        var gem1Move = new MoveGemCommand(gem1, gem2Pos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, gameBoardPresenter);
        var gem2Move = new MoveGemCommand(gem2, gem1Pos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, gameBoardPresenter);
        
        GameLogger.Log($"Swapping {gem1} to {gem2} with pos {gem1Pos} to {gem2Pos}");

        var commands = new List<IGameBoardCommand>() { gem1Move, gem2Move };
        compositeCommand = new CompositeCommand(commands);
    }
    
    public async Task ExecuteAsync()
    {
        await compositeCommand.ExecuteAsync();
    }
}