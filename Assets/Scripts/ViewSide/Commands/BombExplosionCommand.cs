using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BombExplosionCommand : IGameBoardCommand
{
    public string Name => "BombExplosionCommand";
    
    public string Details => $"Bomb at {bomb}, affected: {neighborGems.Count} neighbors";

    // dependencies
    private readonly GameBoardPresenter presenter;
    private readonly SC_Gem bomb;
    private readonly List<SC_Gem> neighborGems;
    private readonly float neighborDestroyDelay;
    private readonly float bombDestroyDelay;
    private readonly IGameBoardReader gameBoard;
    
    public BombExplosionCommand(
        SC_Gem bomb,
        List<SC_Gem> affectedGems,
        float neighborDestroyDelay,
        float bombDestroyDelay,
        GameBoardPresenter presenter,
        IGameBoardReader gameBoard)
    {
        this.bomb = bomb;
        this.presenter = presenter;
        this.neighborDestroyDelay = neighborDestroyDelay;
        this.bombDestroyDelay = bombDestroyDelay;
        this.gameBoard = gameBoard;
        
        // Separate neighbors from the bomb itself
        
        neighborGems = affectedGems.Where(gem => gem != bomb).ToList();
    }
    
    public async Task ExecuteAsync()
    {
        // 1. Delay before destroying neighbors
        if (neighborDestroyDelay > 0)
        {
            await new DelayCommand(neighborDestroyDelay).ExecuteAsync();
        }
        
        // 2. Destroy all neighbors in parallel
        var destroyTasks = new List<Task>();
        foreach (var neighbourGem in neighborGems)
        {
            if (neighbourGem != null)
            {
                var destroyCommand = new DestroyGemCommand(presenter, neighbourGem);
                destroyTasks.Add(destroyCommand.ExecuteAsync());
            }
        }
        
        await Task.WhenAll(destroyTasks);
        
        // 3. Delay before destroying the bomb
        if (bombDestroyDelay > 0)
        {
            await new DelayCommand(bombDestroyDelay).ExecuteAsync();
        }
        
        // 4. Destroy the bomb itself
        if (bomb != null)
        {
            var bombDestroyCommand = new DestroyGemCommand(presenter, bomb);
            await bombDestroyCommand.ExecuteAsync();
        }
    }
}