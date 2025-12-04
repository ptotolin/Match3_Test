using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SpawnGemCommand : IGameBoardCommand
{
    public string Name => "SpawnGemCommand";

    public string Details {
        get {
            return gem.ToString() + " at " + gemPos;
        }
    }
    
    // dependencies
    private GameBoardPresenter gameBoardPresenter;
    
    // locals
    private SC_Gem gem;
    private SC_GemView gemViewPrefab;
    private Transform gemsHolder;
    private Vector2 gemViewPos;
    private Vector2Int gemPos;
    
    public SpawnGemCommand(
        Vector2Int pos, 
        SC_Gem gem, 
        Transform gemsHolder, 
        ICellModelToWorldConverter cellModelToWorldConverter,
        GameBoardPresenter gameBoardPresenter)
    {
        this.gemsHolder = gemsHolder;
        this.gameBoardPresenter = gameBoardPresenter;
        this.gem = gem;
        
        var gemInfo = SC_GameVariables.Instance.GemsInfo.FirstOrDefault(t => t.Gem.Type == gem.Type);
        if (gemInfo.GemView == null) {
            GameLogger.LogError($"Can't find gem with such a gemType: {gem.Type}");
            return;
        }

        gemViewPrefab = gemInfo.GemView;
        gemPos = pos;
        gemViewPos = cellModelToWorldConverter.Convert(gemPos);
    }
    
    public async Task ExecuteAsync()
    {
        //var gemView = GameObject.Instantiate(gemViewPrefab, gemViewPos, Quaternion.identity);
        var gemView = ObjectPool.Instance.Spawn<SC_GemView>(gemViewPrefab.gameObject, gemViewPos, Quaternion.identity);
        gemView.transform.SetParent(gemsHolder);
        gemView.name = "Gem - " + gemPos.x + ", " + gemPos.y;
        gameBoardPresenter.RegisterGemView(gem, gemView);
        await Task.Yield();
    }
}