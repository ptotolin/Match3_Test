using System;
using UnityEngine;

public class GameBoard : IGameBoardReader
{
    public event Action<Vector2Int, Vector2Int, SC_Gem, SC_Gem> EventGemsSwapped;
    public event Action<Vector2Int, Vector2Int> EventGemMoved;
    public event Action<Vector2Int> EventGemDestroy;
    public event Action<Vector2Int, GlobalEnums.GemSpawnType> EventGemSpawned;
    public event Action EventBatchOperationStarted;
    public event Action EventBatchOperationEnded;

    // locals
    private int height = 0;
    private int width = 0;
    private SC_Gem[,] allGems;
    private bool dirty;

    // to move outside
    private int score = 0; // TODO: Move to other module

    // properties
    public int Height => height;

    public int Width => width;

    public GameBoard(int _Width, int _Height)
    {
        height = _Height;
        width = _Width;
        allGems = new SC_Gem[width, height];
    }

    public bool TryGetGemPos(SC_Gem gem, out Vector2Int pos)
    {
        for (var x = 0; x < width; ++x) {
            for (var y = 0; y < height; ++y) {
                if (allGems[x, y] == gem) {
                    pos = new Vector2Int(x, y);
                    return true;
                }
            }
        }

        pos = Vector2Int.zero;
        return false;
    }

    public void SwapGems(Vector2Int fromPos, Vector2Int toPos)
    {
        // Save gems BEFORE swap
        var gemFrom = allGems[fromPos.x, fromPos.y];
        var gemTo = allGems[toPos.x, toPos.y];

        EventBatchOperationStarted?.Invoke();
        EventGemsSwapped?.Invoke(fromPos, toPos, gemFrom, gemTo);
        EventBatchOperationEnded?.Invoke();

        (allGems[fromPos.x, fromPos.y], allGems[toPos.x, toPos.y]) =
            (allGems[toPos.x, toPos.y], allGems[fromPos.x, fromPos.y]);

        dirty = true;
    }

    public void MoveGem(Vector2Int fromPos, Vector2Int toPos)
    {
        EventGemMoved?.Invoke(fromPos, toPos);

        var gem = allGems[fromPos.x, fromPos.y];

        allGems[fromPos.x, fromPos.y] = null;
        allGems[toPos.x, toPos.y] = gem;

        dirty = true;
    }

    public void DestroyGem(Vector2Int gemPos)
    {
        EventGemDestroy?.Invoke(gemPos);

        allGems[gemPos.x, gemPos.y] = null;

        dirty = true;
    }

    public void SetGem(int x, int y, SC_Gem gem, GlobalEnums.GemSpawnType gemSpawnType)
    {
        allGems[x, y] = gem;

        dirty = true;

        if (gem != null) {
            EventGemSpawned?.Invoke(new Vector2Int(x, y), gemSpawnType);
        }
    }

    public SC_Gem GetGem(int _X, int _Y)
    {
        return allGems[_X, _Y];
    }
    
    public void InvokeBatchStart()
    {
        EventBatchOperationStarted?.Invoke();
    }

    public void InvokeBatchEnd()
    {
        EventBatchOperationEnded?.Invoke();
    }

    public void SetDirty(bool flag)
    {
        dirty = flag;
    }
    
    public bool IsDirty()
    {
        return dirty;
    }
}