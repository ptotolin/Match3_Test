using System.Collections.Generic;
using System.Threading.Tasks;


public class Match3Command : IGameBoardCommand
{
    public string Name => "Math3Command";

    public string Details {
        get {
            return "not implemented";
        }
    }
    
    private CompositeCommand matchCommand;
    
    public Match3Command(GameBoardPresenter gameBoardPresenter, SC_Gem gem1, SC_Gem gem2, SC_Gem gem3)
    {
        var commands = new List<IGameBoardCommand>();
        
        commands.Add(new DestroyGemCommand(gameBoardPresenter, gem1));
        commands.Add(new DestroyGemCommand(gameBoardPresenter, gem2));
        commands.Add(new DestroyGemCommand(gameBoardPresenter, gem3));
        
        matchCommand = new CompositeCommand(commands);

    }
    
    public async Task ExecuteAsync()
    {
        await matchCommand.ExecuteAsync();
    }
}