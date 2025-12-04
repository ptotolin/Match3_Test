using System.Threading.Tasks;
using UnityEngine;

public class MoveGemCommand : IGameBoardCommand
{
    public string Name => "MoveGemCommand";

    public string Details {
        get {
            return gem.ToString() + " => " + destPos.ToString();
        }
    }
    
    // dependencies
    private readonly ICellModelToWorldConverter cellModelToWorldConverter;
    private readonly GameBoardPresenter gameBoardPresenter;
    private readonly SC_Gem gem;
    private readonly Vector2Int destPos;
    private readonly float blocksPerSecondSpeed;
    
    // locals
    private float blockSize;
    
    public MoveGemCommand(
        SC_Gem gem, 
        Vector2Int toPos, 
        float blocksPerSecondSpeed, 
        ICellModelToWorldConverter cellModelToWorldConverter,
        GameBoardPresenter gameBoardPresenter)
    {
        this.gem = gem;
        this.destPos = toPos;
        this.cellModelToWorldConverter = cellModelToWorldConverter;
        this.blocksPerSecondSpeed = blocksPerSecondSpeed;
        this.gameBoardPresenter = gameBoardPresenter;

        blockSize = SC_GameVariables.Instance.BlockSize;
    }
    
    public async Task ExecuteAsync()
    {
        Debug.Log($"===== Started executing ======");
        var destinationPoint = cellModelToWorldConverter.Convert(destPos);
        var timer = 0.0f;
        var gemView = gameBoardPresenter.GetGemView(gem);
        var gemViewTransform = gemView.transform;
        Vector2 startPos = gemViewTransform.position;
        var moveVec = destinationPoint - startPos;
        var duration = (moveVec.magnitude / blockSize) / blocksPerSecondSpeed;
        while (timer < duration) {
            timer += Time.deltaTime;
            var normalizedTime = timer / duration;
            gemViewTransform.position = startPos + normalizedTime * moveVec;
            await Task.Yield();
        }
        Debug.Log($"===== Ended executing ======");

        gemViewTransform.position = destinationPoint;
        
    }
}