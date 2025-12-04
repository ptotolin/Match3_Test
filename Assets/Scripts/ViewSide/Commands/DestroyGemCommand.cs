using System.Threading.Tasks;
using UnityEngine;

public class DestroyGemCommand : IGameBoardCommand
{
    public string Name => "DestroyCommand";

    public string Details {
        get {
            return gem.ToString();
        }
    }
    
    private readonly GameBoardPresenter gameBoardPresenter;
    
    private SC_Gem gem;
    
    public DestroyGemCommand(GameBoardPresenter gameBoardPresenter, SC_Gem gem)
    {
        this.gameBoardPresenter = gameBoardPresenter;
        this.gem = gem;
    }
    
    public async Task ExecuteAsync()
    {
        var gemView = gameBoardPresenter.GetGemView(gem);
        await gemView.ShowDestroyEffect();
        
        gameBoardPresenter.UnregisterGemViewByView(gemView);

        var pooledObject = gemView.GetComponent<PooledObject>();
        if (pooledObject != null) {
            pooledObject.Despawn();
        }
        else {
            GameLogger.LogWarning($"Object {gemView.gameObject.name} is not pooled (doesn't have PooledObject component)");
            GameObject.Destroy(gemView.gameObject);
        }
        
    }
}