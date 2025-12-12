using System.Threading.Tasks;

public class ScoreUpdateCommand : IGameBoardCommand
{
    public string Name => "Score Update";
    public string Details => "Not implemented";

    private IEventBus eventBus;
    private int scoreOld;
    private int scoreNew;

    public ScoreUpdateCommand(IEventBus eventBus, int scoreOld, int scoreNew)
    {
        this.eventBus = eventBus;
        this.scoreOld = scoreOld;
        this.scoreNew = scoreNew;
    }
    
    public async Task ExecuteAsync()
    {
        eventBus.Publish(new ScoreEventData()
        {
            ScoreOld = scoreOld,
            ScoreNew = scoreNew
        });
    }
}