using System.Threading.Tasks;
using UnityEngine;

public class MarkGemForActivationCommand : IGameBoardCommand
{
    public string Name => "MarkGemForActivationCommand";
    
    public string Details => $"Marking gem with ability '{abilityType}' at {gemPos}";
    
    private readonly GameBoardPresenter presenter;
    private readonly SC_Gem gem;
    private readonly Vector2Int gemPos;
    private readonly string abilityType;
    
    public MarkGemForActivationCommand(
        GameBoardPresenter presenter, 
        SC_Gem gem, 
        Vector2Int gemPos,
        string abilityType)
    {
        this.presenter = presenter;
        this.gem = gem;
        this.gemPos = gemPos;
        this.abilityType = abilityType;
    }
    
    public async Task ExecuteAsync()
    {
        var gemView = presenter.GetGemView(gem);
        if (gemView != null)
        {
            // TODO: Show visual effect for marking (e.g., pulse, glow, etc.)
            // gemView.ShowActivationMark();
            Debug.Log($"<color=yellow>Visual: Gem at {gemPos} marked for {abilityType} activation</color>");
        }
        await Task.Yield();
    }
}