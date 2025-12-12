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
            var easedTime = EaseOutBack(normalizedTime);
            gemViewTransform.position = startPos + easedTime * moveVec;
            await Task.Yield();
        }
        GameLogger.Log($"===== Ended executing ======");

        gemViewTransform.position = destinationPoint;
        
    }
    
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
    
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}