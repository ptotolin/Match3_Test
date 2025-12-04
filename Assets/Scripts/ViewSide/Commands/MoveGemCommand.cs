using System.Threading.Tasks;
using UnityEngine;

public class MoveGemCommand : IGameBoardCommand
{
    public string Name => "MoveGemCommand";

    public string Details {
        get {
            return gem.ToString() + " from " + fromPos.ToString() + " => " + destPos.ToString();
        }
    }
    
    // dependencies
    private readonly ICellModelToWorldConverter cellModelToWorldConverter;
    private readonly GameBoardPresenter gameBoardPresenter;
    private readonly SC_Gem gem;
    private readonly Vector2Int fromPos;
    private readonly Vector2Int destPos;
    private readonly float blocksPerSecondSpeed;
    
    // locals
    private float blockSize;
    
    public MoveGemCommand(
        SC_Gem gem, 
        Vector2Int fromPos,
        Vector2Int toPos, 
        float blocksPerSecondSpeed, 
        ICellModelToWorldConverter cellModelToWorldConverter,
        GameBoardPresenter gameBoardPresenter)
    {
        this.gem = gem;
        this.fromPos = fromPos;
        this.destPos = toPos;
        this.cellModelToWorldConverter = cellModelToWorldConverter;
        this.blocksPerSecondSpeed = blocksPerSecondSpeed;
        this.gameBoardPresenter = gameBoardPresenter;

        blockSize = SC_GameVariables.Instance.BlockSize;
    }
    
    public async Task ExecuteAsync()
    {
        GameLogger.Log($"===== Started executing ======");
        var destinationPoint = cellModelToWorldConverter.Convert(destPos);
        var startPoint = cellModelToWorldConverter.Convert(fromPos);
        
        var gemView = gameBoardPresenter.GetGemView(gem);
        var gemViewTransform = gemView.transform;
        
        gemViewTransform.position = startPoint;
        
        var timer = 0.0f;
        Vector2 startPos = startPoint;
        var moveVec = destinationPoint - startPos;
        var duration = (moveVec.magnitude / blockSize) / blocksPerSecondSpeed;
        while (timer < duration) {
            timer += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(timer / duration);
            gemViewTransform.position = startPos + normalizedTime * moveVec;
            await Task.Yield();
        }
        GameLogger.Log($"===== Ended executing ======");

        gemViewTransform.position = destinationPoint;
        
    }
}