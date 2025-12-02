using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SequentialCompositeCommand : IGameBoardCommand
{
    public string Name => "SequentialCompositeCommand";
    
    public string Details 
    {
        get 
        {
            var sb = new StringBuilder();
            for (int i = 0; i < commandGroups.Count; i++)
            {
                sb.AppendLine($"Group {i}:");
                foreach (var cmd in commandGroups[i])
                {
                    sb.AppendLine($"  - {cmd.Details}");
                }
            }
            return sb.ToString();
        }
    }
    
    private readonly List<List<IGameBoardCommand>> commandGroups;
    private readonly float delayBetweenGroups;
    
    public SequentialCompositeCommand(List<List<IGameBoardCommand>> commandGroups, float delayBetweenGroups = 0.1f)
    {
        this.commandGroups = commandGroups;
        this.delayBetweenGroups = delayBetweenGroups;
    }
    
    public async Task ExecuteAsync()
    {
        for (int i = 0; i < commandGroups.Count; i++)
        {
            // Parallel
            var group = commandGroups[i];
            var tasks = group.Select(cmd => cmd.ExecuteAsync());
            await Task.WhenAll(tasks);
            
            // Delay for every command except last one
            if (i < commandGroups.Count - 1)
            {
                await new DelayCommand(delayBetweenGroups).ExecuteAsync();
            }
        }
    }
}