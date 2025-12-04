using System.Threading.Tasks;

public class DelayedStartCommand : IGameBoardCommand
{
    private readonly IGameBoardCommand innerCommand;
    private readonly float startDelay;
    
    public DelayedStartCommand(IGameBoardCommand innerCommand, float startDelay)
    {
        this.innerCommand = innerCommand;
        this.startDelay = startDelay;
    }
    
    public string Name => $"DelayedStart({startDelay}s) -> {innerCommand.Name}";
    public string Details => $"Delay {startDelay}s, then: {innerCommand.Details}";
    
    public async Task ExecuteAsync()
    {
        if (startDelay > 0)
        {
            await new DelayCommand(startDelay).ExecuteAsync();
        }
        
        await innerCommand.ExecuteAsync();
    }
}