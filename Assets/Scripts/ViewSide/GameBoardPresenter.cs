using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardPresenter : MonoBehaviour
{
    [SerializeField] private Transform gemsHolder;
    
    // dependencies
    private GameBoard gameboard;
    private GameBoardEventsAdapter gameBoardEventsAdapter;
    private IEventBus eventBus;
    
    // locals
    private Dictionary<SC_Gem, SC_GemView> gemViewsDict = new();
    private ICellModelToWorldConverter cellModelToWorldConverter;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    
    // properties
    public GlobalEnums.GameState CurrentState => currentState;

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void Initialize(GameBoard gameboard, GameBoardEventsAdapter gameBoardEventsAdapter, IEventBus eventBus)
    {
        this.gameboard = gameboard;
        this.gameBoardEventsAdapter = gameBoardEventsAdapter;
        this.eventBus = eventBus;

        cellModelToWorldConverter = new CellModelToWorldConverter();
        SubscribeToEvents();
        
        gameboard.InvokeBatchStart();
        for (var x = 0; x < this.gameboard.Width; ++x) {
            for (var y = 0; y < this.gameboard.Height; ++y) {
                var gem = this.gameboard.GetGem(x, y);
                var spawnGemCommand = new SpawnGemCommand(new Vector2Int(x, y), gem, gemsHolder, cellModelToWorldConverter, this);
                gameBoardEventsAdapter.AddGlobalCommand(spawnGemCommand);
            }
        }
        
        gameboard.InvokeBatchEnd();
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
            GameLogger.LogError($"DestroyGemAt curGem == null");
        }
        
        GameLogger.Log($"Destroy gem {curGem} at {_Pos}");

        var destroyGemCommand = new DestroyGemCommand(this, curGem);
        gameBoardEventsAdapter.AddGlobalCommand(destroyGemCommand);
    }
    
    private void OnGemMoved(Vector2Int fromPos, Vector2Int toPos)
    {
        GameLogger.Log($"[Client] GemMove({fromPos}, {toPos})");

        var gem = gameboard.GetGem(fromPos.x, fromPos.y);
        
        var moveCommand = new MoveGemCommand(gem, fromPos, toPos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, this);
        gameBoardEventsAdapter.AddColumnCommand(moveCommand, fromPos.x);
    }

    private void OnGemsSwapped(Vector2Int gem1Pos, Vector2Int gem2Pos, SC_Gem gem1, SC_Gem gem2)
    {
        GameLogger.Log($"[Client] Swap({gem1Pos}, {gem2Pos})");

        var swapGemCommand = new SwapGemsCommand(gem1, gem2, gem1Pos, gem2Pos, cellModelToWorldConverter, this);
        gameBoardEventsAdapter.AddGlobalCommand(swapGemCommand);

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

    private List<SC_Gem> gemsMarkedForDestroy = new();
    
    private void OnBombExploded(BombExplosionEventData eventData)
    {
        GameLogger.Log($"<color=orange>Bomb exploded at {eventData.Bomb}! Affected: {eventData.AffectedGems.Count} positions</color>");

        gemsMarkedForDestroy.AddRange(eventData.AffectedGems);
        
        // Create bomb explosion command with correct sequence
        var bombExplosionCommand = new BombExplosionCommand(
            eventData.Bomb,
            eventData.AffectedGems,
            eventData.NeighborDestroyDelay,
            eventData.BombDestroyDelay,
            this,
            gameboard // GameBoard implements IGameBoardReader
        );
    
        // Add command to adapter for execution
        gameBoardEventsAdapter.AddGlobalCommand(bombExplosionCommand);
    }

    private void SpawnGemInstant(Vector2Int gemPos)
    {
        var gem = gameboard.GetGem(gemPos.x, gemPos.y);
        var spawnGemCommand = new SpawnGemCommand(gemPos, gem, gemsHolder, cellModelToWorldConverter, this);
        gameBoardEventsAdapter.AddColumnCommand(spawnGemCommand, gemPos.x);
    }

    private void SpawnGemFallFromTop(Vector2Int gemPos)
    {
        var gem = gameboard.GetGem(gemPos.x, gemPos.y);
        var spawnPos = new Vector2Int(gemPos.x, gameboard.Height);
        var spawnGemCommand = new SpawnGemCommand(spawnPos, gem, gemsHolder, cellModelToWorldConverter, this);
        var moveCommand = new MoveGemCommand(gem, spawnPos, gemPos, SC_GameVariables.Instance.BlockSpeed, cellModelToWorldConverter, this);
        gameBoardEventsAdapter.AddColumnCommand(spawnGemCommand, gemPos.x);
        gameBoardEventsAdapter.AddColumnCommand(moveCommand, gemPos.x);
    }
    
    private void OnGemDestroy(Vector2Int gemPos)
    {
        var gemToDestroy = gameboard.GetGem(gemPos.x, gemPos.y);
        if (gemsMarkedForDestroy.Contains(gemToDestroy)) {
            gemsMarkedForDestroy.Remove(gemToDestroy);
            return;
        }
        
        DestroyGemAt(gemPos);
    }

    private void SubscribeToEvents()
    {
        gameboard.EventGemMoved += OnGemMoved;
        gameboard.EventGemsSwapped += OnGemsSwapped;
        gameboard.EventGemDestroy += OnGemDestroy;
        gameboard.EventGemSpawned += OnGemSpawned;

        //gameBoardCommandsExecutor.EventLastCommandExecuted += OnLastCommandExecuted;
        gameBoardEventsAdapter.EventLastCommandExecuted += OnLastCommandExecuted;
        
        eventBus.Subscribe<BombExplosionEventData>(OnBombExploded);
    }

    private void UnsubscribeFromEvents()
    {
        gameboard.EventGemMoved -= OnGemMoved;
        gameboard.EventGemsSwapped -= OnGemsSwapped;
        gameboard.EventGemDestroy -= OnGemDestroy;
        gameboard.EventGemSpawned -= OnGemSpawned;
        
        //gameBoardCommandsExecutor.EventLastCommandExecuted -= OnLastCommandExecuted;
        gameBoardEventsAdapter.EventLastCommandExecuted -= OnLastCommandExecuted;
        eventBus.Unsubscribe<BombExplosionEventData>(OnBombExploded);

    }
}