using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SC_GameLogic : MonoBehaviour
{
    // dependencies
    private GemInputHandler gemInputHandler;
    private GameBoard gameBoard;

    // locals
    private int score = 0;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;
    
    // move outside
    private float displayScore = 0;
    
    public GlobalEnums.GameState CurrentState => currentState;

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

    public void Initialize(GemInputHandler gemInputHandler, GameBoard gameBoard)
    {
        gemInputHandler.EventSwipeDetected += OnSwipe;
        this.gameBoard = gameBoard;
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
    
    public void SetState(GlobalEnums.GameState _CurrentState)
    {
        currentState = _CurrentState;
    }

    public void ScoreCheck(SC_Gem gemToCheck)
    {
        gameBoard.Score += gemToCheck.ScoreValue;
    }
    
    private void OnSwipe(Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        if (currentState != GlobalEnums.GameState.move) {
            return;
        }

        var gem1 = GetGem(gem1Pos.x, gem1Pos.y);
        var gem2 = GetGem(gem2Pos.x, gem2Pos.y);
        
        gameBoard.SetGem(gem2Pos.x, gem2Pos.y, gem1);
        gameBoard.SetGem(gem1Pos.x, gem1Pos.y, gem2);
        
        // Temporary
        StartCoroutine(CheckMoveCo(gem1, gem2, gem1Pos, gem2Pos));
    }
    
    private void DestroyMatches()
    {
        for (int i = 0; i < gameBoard.CurrentMatches.Count; i++) {
            if (gameBoard.CurrentMatches[i] != null) {
                ScoreCheck(gameBoard.CurrentMatches[i]);
                if (gameBoard.TryGetGemPos(gameBoard.CurrentMatches[i], out var gemPos)) {
                    gameBoard.DestroyGem(gemPos);
                }
            }
        }

        StartCoroutine(DecreaseRowCo());
    }
    
    // TODO: Move to board presenter
    private IEnumerator CheckMoveCo(SC_Gem gem1, SC_Gem gem2, Vector2Int gem1Pos, Vector2Int gem2Pos)
    {
        SetState(GlobalEnums.GameState.wait);

        yield return new WaitForSeconds(.5f);
        
        FindAllMatches();

        if (gem1 != null && gem2 != null)
        {
            if (!gem1.IsMatch && !gem2.IsMatch)
            {
                // TODO: recheck
                gameBoard.SetGem(gem1Pos.x, gem1Pos.y, gem2);
                gameBoard.SetGem(gem2Pos.x, gem2Pos.y, gem1);

                yield return new WaitForSeconds(.5f);
                
                SetState(GlobalEnums.GameState.move);
            }
            else
            {
                DestroyMatches();
            }
        }
    }
    
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.2f);

        int nullCounter = 0;
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem curGem = gameBoard.GetGem(x, y);
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

        // TODO: Fillboard command ? 
        StartCoroutine(FilledBoardCo());
    }

    private IEnumerator FilledBoardCo()
    {
        yield return new WaitForSeconds(0.5f);
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
        gameBoard.FindAllMatches();
        if (gameBoard.CurrentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            currentState = GlobalEnums.GameState.move;
        }
    }
    private void RefillBoard()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                SC_Gem _curGem = gameBoard.GetGem(x,y);
                if (_curGem == null)
                {
                    int gemToUse = Random.Range(0, SC_GameVariables.Instance.GemsInfo.Count);
                    
                    // TODO: Think about commands
                    gameBoard.SetGem(x, y, SC_GameVariables.Instance.GemsInfo[gemToUse].Gem.Clone());
                }
            }
        }
        
        //CheckMisplacedGems();
    }
    // private void CheckMisplacedGems()
    // {
    //     List<SC_Gem> foundGems = new List<SC_Gem>();
    //     foundGems.AddRange(FindObjectsOfType<SC_Gem>());
    //     for (int x = 0; x < gameBoard.Width; x++)
    //     {
    //         for (int y = 0; y < gameBoard.Height; y++)
    //         {
    //             SC_Gem _curGem = gameBoard.GetGem(x, y);
    //             if (foundGems.Contains(_curGem))
    //                 foundGems.Remove(_curGem);
    //         }
    //     }
    //
    //     foreach (SC_Gem g in foundGems)
    //         Destroy(g.gameObject);
    // }
    public void FindAllMatches()
    {
        gameBoard.FindAllMatches();
    }

    #endregion
}
