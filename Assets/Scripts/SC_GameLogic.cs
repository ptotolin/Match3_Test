using System.Collections.Generic;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour
{
    // dependencies
    private GemInputHandler gemInputHandler;
    private GameBoard gameBoard;
    private IGemGenerator gemGenerator;
    private MatchDetector matchDetector;
    private IEventBus eventBus;

    // locals
    private int score = 0;
    private List<SC_Gem> gemsMarkedForAbilityActivation = new List<SC_Gem>();
    private bool swapHappened;
    private Vector2Int lastSwapPos1;
    private Vector2Int lastSwapPos2;
    
    // move outside
    private float displayScore = 0;
    
    #region MonoBehaviour
    private void OnEnable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected += OnSwipe;
        }

        if (eventBus != null) {
            eventBus.Subscribe<SpecialGemsAffectedEventData>(OnSpecialGemAffected);
        }
    }

    private void OnDisable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected -= OnSwipe;
        }

        if (eventBus != null) {
            eventBus.Unsubscribe<SpecialGemsAffectedEventData>(OnSpecialGemAffected);
        }
    }

    private void Start()
    {
        StartGame();
    }

    public void Initialize(
        GemInputHandler gemInputHandler, 
        GameBoard gameBoard, 
        IGemGenerator gemGenerator, 
        MatchDetector matchDetector,
        IEventBus eventBus)
    {
        this.gemInputHandler = gemInputHandler;
        this.gemInputHandler.EventSwipeDetected += OnSwipe;
        this.gameBoard = gameBoard;
        this.gemGenerator = gemGenerator;
        this.matchDetector = matchDetector;
        this.eventBus = eventBus;
        
        eventBus.Subscribe<SpecialGemsAffectedEventData>(OnSpecialGemAffected);
    }

    private void OnSpecialGemAffected(SpecialGemsAffectedEventData eventData)
    {
        if (eventData.SourceAbilityType == "bomb") {
            gemsAwaitingActivation.AddRange(eventData.AffectedSpecialGems);
        }
    }

    // private void Update()
    // {
    //     // TODO: Put to UI
    //     displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
    //     unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    // }
    #endregion

    #region Logic
    
    public void StartGame()
    {
        // TODO: Add UpdateScoreMethod and move it to GameBoardPresenter somehow
        //unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>().text = score.ToString("0");
    }
    
    private SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }

    private void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.ScoreValue;
    }
    
    private void OnSwipe(Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        var gem1 = GetGem(gem1Pos.x, gem1Pos.y);
        var gem2 = GetGem(gem2Pos.x, gem2Pos.y);
        
        lastSwapPos1 = gem1Pos;
        lastSwapPos2 = gem2Pos;
        
        gameBoard.SwapGems(gem1Pos, gem2Pos);

        swapHappened = true;
        
        CheckMoveCo(gem1, gem2, gem1Pos, gem2Pos);
    }
    
    private void CheckMoveCo(SC_Gem gem1, SC_Gem gem2, Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        FindAllMatches();
        
        if (gem1 != null && gem2 != null)
        {
            if (!gem1.IsMatch && !gem2.IsMatch && 
                (gem1.SpecialAbility == null && gem2.SpecialAbility == null))
            {
                Debug.Log($"[Client] Swap back ({gem1Pos}, {gem2Pos})");
                gameBoard.InvokeBatchStart();
                // swap back
                gameBoard.SwapGems(gem2Pos, gem1Pos);
                gameBoard.InvokeBatchEnd();
            }
            else 
            {
                if (gem1.SpecialAbility != null) {
                    gameBoard.InvokeBatchStart();
                    gem1.SpecialAbility.Execute();
                    gameBoard.InvokeBatchEnd();
                }
                
                if (gem2.SpecialAbility != null) {
                    gameBoard.InvokeBatchStart();
                    gem2.SpecialAbility.Execute();
                    gameBoard.InvokeBatchEnd();
                }
                
                DestroyMatches();
                swapHappened = false;
            }
        }
    }
    
    private void DestroyMatches()
    {
        gameBoard.InvokeBatchStart();
        
        // 1. CHECK for 4+ matches BEFORE destruction to know where to place the bomb
        // (but place the bomb AFTER destruction)
        //bool shouldPlaceBomb = matchDetector.HasMatchOfFourOrMoreInSwapPosition(lastSwapPos1, lastSwapPos2, out Vector2Int bombPosition);
        Vector2Int bombPosition;
        bool shouldPlaceBomb = swapHappened ? matchDetector.HasMatchOfFourOrMoreInSwapPosition(lastSwapPos1, lastSwapPos2, out bombPosition) : 
            matchDetector.HasMatchOfFourOrMore(out bombPosition);

        // TODO: We may form matches here like Match3, Match4, Match5
        Debug.Log($"<color=white>Matches count:{matchDetector.CurrentMatches.Count}</color>");
        foreach (var match in matchDetector.CurrentMatches) {
            gameBoard.TryGetGemPos(match, out var gemPos);
            Debug.Log($"<color=yellow>[DELETE] {match} at {gemPos}</color>");
        }
        
        gemsMarkedForAbilityActivation.Clear();
    
        for (int i = 0; i < matchDetector.CurrentMatches.Count; i++) {
            if (matchDetector.CurrentMatches[i] != null) {
                var gem = matchDetector.CurrentMatches[i];
            
                // // Проверяем, есть ли у гема способность
                // if (gem.SpecialAbility != null)
                // {
                //     // Помечаем на активацию (НЕ активируем сразу!)
                //     gemsMarkedForAbilityActivation.Add(gem);
                //     Debug.Log($"<color=orange>Marked gem with ability '{gem.SpecialAbility.AbilityType}' for activation</color>");
                // }
                // else
                {
                    // Обычный гем - удаляем и считаем очки
                    ScoreCheck(gem);
                    if (gameBoard.TryGetGemPos(gem, out var gemPos)) {
                        Debug.Log($"<color=white>Match {gemPos}</color>");
                        gameBoard.DestroyGem(gemPos);
                    }
                }
            }
        }
        gameBoard.InvokeBatchEnd();
        
        gameBoard.InvokeBatchStart();
        
        // 3. PLACE the bomb at the swap position (now it's empty after destruction)
        if (shouldPlaceBomb)
        {
            var bombGem = SC_GameVariables.Instance.bomb.Clone();
            bombGem.SpecialAbility = SpecialAbilityFactory.CreateAbility(
                GlobalEnums.GemType.bomb,
                gameBoard,
                bombGem,
                eventBus,
                matchDetector
            );
            
            gameBoard.SetGem(bombPosition.x, bombPosition.y, bombGem, GlobalEnums.GemSpawnType.Instant);
            Debug.Log($"<color=red>BOMB placed at ({bombPosition.x}, {bombPosition.y}) after 4+ match!</color>");
        }
        gameBoard.InvokeBatchEnd();
        
        DecreaseRowCo();
        
    }

    private List<SC_Gem> gemsAwaitingActivation = new();
    
    private void DecreaseRowCo()
    {
        gameBoard.InvokeBatchStart();
        var nullCounter = 0;
        for (var x = 0; x < gameBoard.Width; x++)
        {
            for (var y = 0; y < gameBoard.Height; y++)
            {
                var curGem = gameBoard.GetGem(x, y);
                if (curGem == null) 
                {
                    nullCounter++;
                }
                else if (nullCounter > 0) 
                {
                    gameBoard.MoveGem(new Vector2Int(x, y), new Vector2Int(x, y - nullCounter));
                }
            }
            nullCounter = 0;
        }
        gameBoard.InvokeBatchEnd();

        if (gemsAwaitingActivation.Count > 0) {
            gameBoard.InvokeBatchStart();
            foreach (var gemAwaitingActivation in gemsAwaitingActivation) {
                gemAwaitingActivation.SpecialAbility.Execute();
            }
            gameBoard.InvokeBatchEnd();
            
            gemsAwaitingActivation.Clear();
        }

        // TODO: Fillboard command ? 
        FilledBoardCo();
    }

    private void FilledBoardCo()
    {
        gameBoard.InvokeBatchStart();
        RefillBoard();
        gameBoard.InvokeBatchEnd();
        
        if (gemsMarkedForAbilityActivation.Count > 0)
        {
            gameBoard.InvokeBatchStart();
        
            // Создаем копию списка, так как Execute() может изменить позиции гемов
            var gemsToActivate = new List<SC_Gem>(gemsMarkedForAbilityActivation);
            gemsMarkedForAbilityActivation.Clear();
        
            foreach (var gem in gemsToActivate)
            {
                if (gem != null && gem.SpecialAbility != null)
                {
                    // Проверяем, что гем еще на доске (не был удален при падении)
                    if (gameBoard.TryGetGemPos(gem, out var gemPos))
                    {
                        Debug.Log($"<color=orange>Activating ability '{gem.SpecialAbility.AbilityType}' for gem at ({gemPos.x}, {gemPos.y})</color>");
                        gem.SpecialAbility.Execute();
                    }
                }
            }
        
            gameBoard.InvokeBatchEnd();
        }
    
        // Проверяем новые матчи
        matchDetector.FindAllMatches();
        if (matchDetector.CurrentMatches.Count > 0) {
            DestroyMatches();
        }
    }
    private void RefillBoard()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x,y);
                if (_curGem == null) {
                    var newGem = gemGenerator.GenerateGem(new Vector2Int(x, y));
                    gameBoard.SetGem(x, y, newGem, GlobalEnums.GemSpawnType.FallFromTop);
                }
            }
        }
    }
    
    public void FindAllMatches()
    {
        matchDetector.FindAllMatches();
    }

    #endregion
}