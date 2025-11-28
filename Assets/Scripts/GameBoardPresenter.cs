using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameBoardPresenter : MonoBehaviour
{
    // TODO: Move to some other place
    private const float BlockSize = 1.0f;

    [SerializeField] private Transform gemsHolder;
    
    // dependencies
    private GameBoard gameboard;
    
    // locals
    private Dictionary<SC_Gem, SC_GemView> gemViewsDict = new();

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void Initialize(GameBoard gameboard)
    {
        this.gameboard = gameboard;

        for (var x = 0; x < this.gameboard.Width; ++x) {
            for (var y = 0; y < this.gameboard.Height; ++y) {
                var gem = this.gameboard.GetGem(x, y);
                var gemView = SpawnGem(new Vector2Int(x,y), gem.Type);
                if (gemView != null) {
                    gemViewsDict.Add(gem, gemView);
                }
            }
        }
        
        SubscribeToEvents();
    }

    private async Task MoveGem(Vector2Int fromPos, Vector2Int toPos, float blockPerSecSpeed)
    {
        var gem = gameboard.GetGem(fromPos.x, fromPos.y);
        
        // converting cellPos to WorldPos
        var newPos = CellPosToWorldPos(toPos);
        
        if (gemViewsDict.TryGetValue(gem, out var gemView)) {
            var timer = 0.0f;
            var gemViewTransform = gemView.transform;
            Vector2 startPos = gemViewTransform.position;
            var moveVec = newPos - startPos;
            var duration = (moveVec.magnitude / BlockSize) / blockPerSecSpeed;
            while (timer < duration) {
                timer += Time.deltaTime;
                var normalizedTime = timer / duration;
                gemViewTransform.position = startPos + normalizedTime * moveVec;
                await Task.Yield();
            }

            gemViewTransform.position = newPos;
        }
    }

    private SC_GemView SpawnGem(Vector2Int pos, GlobalEnums.GemType gemType)
    {
        var gemInfo = SC_GameVariables.Instance.GemsInfo.FirstOrDefault(t => t.Gem.Type == gemType);
        if (gemInfo.GemView == null) {
            Debug.LogError($"Can't find gem with such a gemType: {gemType}");
            return null;
        }

        var gemViewPrefab = gemInfo.GemView;
        var gemView = Instantiate(gemViewPrefab, new Vector2(pos.x, pos.y + SC_GameVariables.Instance.dropHeight), Quaternion.identity);
        gemView.transform.SetParent(gemsHolder);
        gemView.name = "Gem - " + pos.x + ", " + pos.y;

        return gemView;
    }
    
    private void DestroyGemAt(Vector2Int _Pos)
    {
        SC_Gem curGem = gameboard.GetGem(_Pos.x,_Pos.y);
        if (curGem != null && gemViewsDict.TryGetValue(curGem, out var gemView)) {
            gameboard.SetGem(_Pos.x,_Pos.y, null);
            gemView.ShowDestroyEffect();

            gemViewsDict.Remove(curGem);
        }
    }
    
    private void OnGemMoved(Vector2Int fromPos, Vector2Int toPos)
    {
        MoveGem(fromPos, toPos, 1);
    }
    
    private void OnGemDestroy(Vector2Int gemPos)
    {
        DestroyGemAt(gemPos);
    }

    private Vector2 CellPosToWorldPos(Vector2 cellPos)
    {
        return new Vector2(cellPos.x, cellPos.y - SC_GameVariables.Instance.dropHeight);
    }

    private void SubscribeToEvents()
    {
        gameboard.EventGemMoved += OnGemMoved;
        gameboard.EventGemDestroy += OnGemDestroy;
    }

    private void UnsubscribeFromEvents()
    {
        gameboard.EventGemMoved -= OnGemMoved;
        gameboard.EventGemDestroy -= OnGemDestroy;
    }
}