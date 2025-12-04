using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CompositeCommand : IGameBoardCommand
{
    public string Name => "CompositeCommand";

    public string Details {
        get {
            StringBuilder sb = new();
            foreach (var command in commands) {
                sb.AppendLine(command.Details);
            }

            return sb.ToString();
        }
    }
    
    private readonly List<IGameBoardCommand> commands;
    
    
    public CompositeCommand(List<IGameBoardCommand> commands)
    {
        this.commands = commands;
    }
    
    public async Task ExecuteAsync()
    {
        var tasks = commands.Select(t => t.ExecuteAsync());
        await Task.WhenAll(tasks);
    }
}