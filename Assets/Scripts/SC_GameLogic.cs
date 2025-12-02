using UnityEngine;
using Random = UnityEngine.Random;

public class SC_GameLogic : MonoBehaviour
{
    // dependencies
    private GemInputHandler gemInputHandler;
    private GameBoard gameBoard;
    private IGemGenerator gemGenerator;

    // locals
    private int score = 0;
    
    // move outside
    private float displayScore = 0;
    
    #region MonoBehaviour
    private void OnEnable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected += OnSwipe;
        }
    }

    private void OnDisable()
    {
        if (gemInputHandler != null) {
            gemInputHandler.EventSwipeDetected -= OnSwipe;
        }
    }

    private void Start()
    {
        StartGame();
    }

    public void Initialize(GemInputHandler gemInputHandler, GameBoard gameBoard, IGemGenerator gemGenerator)
    {
        this.gemInputHandler = gemInputHandler;
        this.gemInputHandler.EventSwipeDetected += OnSwipe;
        this.gameBoard = gameBoard;
        this.gemGenerator = gemGenerator;
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
    
    public SC_Gem GetGem(int _X, int _Y)
    {
        return gameBoard.GetGem(_X, _Y);
    }

    public void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.ScoreValue;
    }
    
    private void OnSwipe(Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        var gem1 = GetGem(gem1Pos.x, gem1Pos.y);
        var gem2 = GetGem(gem2Pos.x, gem2Pos.y);
        
        gameBoard.SwapGems(gem1Pos, gem2Pos);
        
        CheckMoveCo(gem1, gem2, gem1Pos, gem2Pos);
    }
    
    private void CheckMoveCo(SC_Gem gem1, SC_Gem gem2, Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        FindAllMatches();
        
        if (gem1 != null && gem2 != null)
        {
            if (!gem1.IsMatch && !gem2.IsMatch)
            {
                Debug.Log($"[Client] Swap back ({gem1Pos}, {gem2Pos})");
                gameBoard.InvokeBatchStart();
                // swap back
                gameBoard.SwapGems(gem2Pos, gem1Pos);
                gameBoard.InvokeBatchEnd();
            }
            else 
            {
                DestroyMatches();
            }
        }
    }

    
    private void DestroyMatches()
    {
        gameBoard.InvokeBatchStart();
        // TODO: We may form matches here like Match3, Match4, Match5
        Debug.Log($"<color=white>Matches count:{gameBoard.CurrentMatches.Count}</color>");
        foreach (var match in gameBoard.CurrentMatches) {
            gameBoard.TryGetGemPos(match, out var gemPos);
            Debug.Log($"<color=yellow>[DELETE] {match} at {gemPos}</color>");
        }
        //Debug.Break();
        for (int i = 0; i < gameBoard.CurrentMatches.Count; i++) {
            if (gameBoard.CurrentMatches[i] != null) {
                ScoreCheck(gameBoard.CurrentMatches[i]);
                if (gameBoard.TryGetGemPos(gameBoard.CurrentMatches[i], out var gemPos)) {
                    Debug.Log($"<color=white>Match {gemPos}</color>");

                    gameBoard.DestroyGem(gemPos);
                }
            }
        }
        gameBoard.InvokeBatchEnd();
        
        DecreaseRowCo();
        
    }
    
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

        // TODO: Fillboard command ? 
        FilledBoardCo();
    }

    private void FilledBoardCo()
    {
        gameBoard.InvokeBatchStart();
        RefillBoard();
        gameBoard.InvokeBatchEnd();
        
        gameBoard.FindAllMatches();
        if (gameBoard.CurrentMatches.Count > 0) {
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
        gameBoard.FindAllMatches();
    }

    #endregion
}
