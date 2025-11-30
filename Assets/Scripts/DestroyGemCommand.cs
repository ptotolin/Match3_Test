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
        
        GameObject.Destroy(gemView.gameObject);
    }
}