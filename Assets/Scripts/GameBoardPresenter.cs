using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardPresenter : MonoBehaviour
{
    [SerializeField] private Transform gemsHolder;
    
    // dependencies
    private GameBoard gameboard;
    
    // locals
    private Dictionary<SC_Gem, SC_GemView> gemViewsDict = new();
    private ICellModelToWorldConverter cellModelToWorldConverter;
    private GameBoardCommandsExecutor gameBoardCommandsExecutor;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    
    // properties
    public GlobalEnums.GameState CurrentState => currentState;

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void Initialize(GameBoard gameboard)
    {
        this.gameboard = gameboard;

        cellModelToWorldConverter = new CellModelToWorldConverter();
        gameBoardCommandsExecutor = new GameBoardCommandsExecutor();
        
        for (var x = 0; x < this.gameboard.Width; ++x) {
            for (var y = 0; y < this.gameboard.Height; ++y) {
                var gem = this.gameboard.GetGem(x, y);
                var spawnGemCommand = new SpawnGemCommand(new Vector2Int(x, y), gem, gemsHolder, cellModelToWorldConverter, this);
                gameBoardCommandsExecutor.AddCommand(spawnGemCommand);
            }
        }
        
        SubscribeToEvents();
    }

    public void RegisterGemView(SC_Gem gem, SC_GemView gemView)
    {
        gemViewsDict.Add(gem, gemView);
    }

    public void UnregisterGemView(SC_Gem gem)
    {
        gemViewsDict.Remove(gem);
    }

    public void UnregisterGemViewByView(SC_GemView gemView)
    {
        var rec = gemViewsDict.First(t => t.Value == gemView);
        
        // -- DEBUG
        if (gameboard.TryGetGemPos(rec.Key, out var pos)) {
            Debug.Log($"Unregister gem at pos: {pos}");
        }
        else {
            Debug.Log($"CAN'T FIND gem {rec.Key}. View at position: {gemView.transform.position}");
        }
        // -- END 
        
        gemViewsDict.Remove(rec.Key);
    }

    public SC_Gem GetGemByView(SC_GemView view)
    {
        return gemViewsDict.FirstOrDefault(t => t.Value == view).Key;
    }

    public SC_GemView GetGemView(SC_Gem gem)
    {
        return gemViewsDict[gem];
    }
    
    private void DestroyGemAt(Vector2Int _Pos)
    {
        SC_Gem curGem = gameboard.GetGem(_Pos.x,_Pos.y);
        if (curGem == null) {
            Debug.LogError($"DestroyGemAt curGem == null");
        }
        
        Debug.Log($"Destroy gem {curGem} at {_Pos}");

        var destroyGemCommand = new DestroyGemCommand(this, curGem);
        gameBoardCommandsExecutor.AddCommand(destroyGemCommand);
    }
    
    private void OnGemMoved(Vector2Int fromPos, Vector2Int toPos)
    {
        Debug.Log($"[Client] GemMove({fromPos}, {toPos})");

        var gem = gameboard.GetGem(fromPos.x, fromPos.y);
        
        var moveCommand = new MoveGemCommand(gem, toPos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, this);
        gameBoardCommandsExecutor.AddCommand(moveCommand);
    }

    private void OnGemsSwapped(Vector2Int gem1Pos, Vector2Int gem2Pos, SC_Gem gem1, SC_Gem gem2)
    {
        Debug.Log($"[Client] Swap({gem1Pos}, {gem2Pos})");

        // var gem1 = gameboard.GetGem(gem1Pos.x, gem1Pos.y);
        // var gem2 = gameboard.GetGem(gem2Pos.x, gem2Pos.y);

        var swapGemCommand = new SwapGemsCommand(gem1, gem2, gem1Pos, gem2Pos, cellModelToWorldConverter, this);
        gameBoardCommandsExecutor.AddCommand(swapGemCommand);

        currentState = GlobalEnums.GameState.wait;

    }

    private void OnGemSpawned(Vector2Int gemPos, GlobalEnums.GemSpawnType gemSpawnType)
    {
        switch (gemSpawnType) {
            case GlobalEnums.GemSpawnType.Instant:
                SpawnGemInstant(gemPos);
                break;
            case GlobalEnums.GemSpawnType.FallFromTop:
                SpawnGemFallFromTop(gemPos);
                break;
            case GlobalEnums.GemSpawnType.Appear:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gemSpawnType), gemSpawnType, null);
        }
    }
    
    private void OnLastCommandExecuted()
    {
        currentState = GlobalEnums.GameState.move;
    }

    private void SpawnGemInstant(Vector2Int gemPos)
    {
        var gem = gameboard.GetGem(gemPos.x, gemPos.y);
        var spawnGemCommand = new SpawnGemCommand(gemPos, gem, gemsHolder, cellModelToWorldConverter, this);
        gameBoardCommandsExecutor.AddCommand(spawnGemCommand);
    }

    private void SpawnGemFallFromTop(Vector2Int gemPos)
    {
        var gem = gameboard.GetGem(gemPos.x, gemPos.y);
        var spawnGemCommand = new SpawnGemCommand(new Vector2Int(gemPos.x, gameboard.Height), gem, gemsHolder, cellModelToWorldConverter, this);
        var moveCommand = new MoveGemCommand(gem, gemPos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, this);
        gameBoardCommandsExecutor.AddCommand(spawnGemCommand);
        gameBoardCommandsExecutor.AddCommand(moveCommand);
    }
    
    private void OnGemDestroy(Vector2Int gemPos)
    {
        DestroyGemAt(gemPos);
    }

    private void SubscribeToEvents()
    {
        gameboard.EventGemMoved += OnGemMoved;
        gameboard.EventGemsSwapped += OnGemsSwapped;
        gameboard.EventGemDestroy += OnGemDestroy;
        gameboard.EventGemSpawned += OnGemSpawned;

        gameBoardCommandsExecutor.EventLastCommandExecuted += OnLastCommandExecuted;
    }

    private void UnsubscribeFromEvents()
    {
        gameboard.EventGemMoved -= OnGemMoved;
        gameboard.EventGemsSwapped -= OnGemsSwapped;
        gameboard.EventGemDestroy -= OnGemDestroy;
        gameboard.EventGemSpawned -= OnGemSpawned;
        
        gameBoardCommandsExecutor.EventLastCommandExecuted -= OnLastCommandExecuted;
    }
}