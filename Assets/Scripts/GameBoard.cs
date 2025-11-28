using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard
{
    public event Action<Vector2Int, Vector2Int> EventGemMoved;
    public event Action<Vector2Int> EventGemDestroy;
    
    // locals
    private int height = 0;
    private int width = 0;
    private SC_Gem[,] allGems;
    private List<SC_Gem> currentMatches = new();
    
    // to move outside
    private int score = 0; // TODO: Move to other module

    // properties
    public int Height => height;
    public int Width => width;
    // TODO: Remove score from here
    public int Score 
    {
        get => score;
        set => score = value;
    }

    public List<SC_Gem> CurrentMatches => currentMatches;


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

    public void MoveGem(Vector2Int fromPos, Vector2Int toPos)
    {
        (allGems[fromPos.x, fromPos.y], allGems[toPos.x, toPos.y]) = 
            (allGems[toPos.x, toPos.y], allGems[fromPos.x, fromPos.y]);
        
        EventGemMoved?.Invoke(fromPos, toPos);
    }

    public void DestroyGem(Vector2Int gemPos)
    {
        allGems[gemPos.x, gemPos.y] = null;
        
        EventGemDestroy?.Invoke(gemPos);
    }
    
    public bool MatchesAt(Vector2Int _PositionToCheck, SC_Gem _GemToCheck)
    {
        if (_PositionToCheck.x > 1)
        {
            if (allGems[_PositionToCheck.x - 1, _PositionToCheck.y].Type == _GemToCheck.Type &&
                allGems[_PositionToCheck.x - 2, _PositionToCheck.y].Type == _GemToCheck.Type)
                return true;
        }

        if (_PositionToCheck.y > 1)
        {
            if (allGems[_PositionToCheck.x, _PositionToCheck.y - 1].Type == _GemToCheck.Type &&
                allGems[_PositionToCheck.x, _PositionToCheck.y - 2].Type == _GemToCheck.Type)
                return true;
        }

        return false;
    }

    public void SetGem(int _X, int _Y, SC_Gem _Gem)
    {
        allGems[_X, _Y] = _Gem;
    }
    public SC_Gem GetGem(int _X,int _Y)
    {
       return allGems[_X, _Y];
    }

    public void FindAllMatches()
    {
        currentMatches.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                SC_Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    if (x > 0 && x < width - 1)
                    {
                        SC_Gem leftGem = allGems[x - 1, y];
                        SC_Gem rightGem = allGems[x + 1, y];
                        //checking no empty spots
                        if (leftGem != null && rightGem != null)
                        {
                            //Match
                            if (leftGem.Type == currentGem.Type && rightGem.Type == currentGem.Type)
                            {
                                currentGem.IsMatch = true;
                                leftGem.IsMatch = true;
                                rightGem.IsMatch = true;
                                currentMatches.Add(currentGem);
                                currentMatches.Add(leftGem);
                                currentMatches.Add(rightGem);
                            }
                        }
                    }

                    if (y > 0 && y < height - 1)
                    {
                        SC_Gem aboveGem = allGems[x, y - 1];
                        SC_Gem bellowGem = allGems[x, y + 1];
                        //checking no empty spots
                        if (aboveGem != null && bellowGem != null)
                        {
                            //Match
                            if (aboveGem.Type == currentGem.Type && bellowGem.Type == currentGem.Type)
                            {
                                currentGem.IsMatch = true;
                                aboveGem.IsMatch = true;
                                bellowGem.IsMatch = true;
                                currentMatches.Add(currentGem);
                                currentMatches.Add(aboveGem);
                                currentMatches.Add(bellowGem);
                            }
                        }
                    }
                }
            }

        if (currentMatches.Count > 0)
            currentMatches = currentMatches.Distinct().ToList();

        CheckForBombs();
    }

    public void CheckForBombs()
    {
        for (int i = 0; i < currentMatches.Count; i++)
        {
            SC_Gem gem = currentMatches[i];
            TryGetGemPos(gem, out var gemPos);
            int x = gemPos.x;
            int y = gemPos.y;

            if (x > 0)
            {
                if (allGems[x - 1, y] != null && allGems[x - 1, y].Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x - 1, y), allGems[x - 1, y].BlastSize);
            }

            if (x + 1 < width)
            {
                if (allGems[x + 1, y] != null && allGems[x + 1, y].Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x + 1, y), allGems[x + 1, y].BlastSize);
            }

            if (y > 0)
            {
                if (allGems[x, y - 1] != null && allGems[x, y - 1].Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y - 1), allGems[x, y - 1].BlastSize);
            }

            if (gemPos.y + 1 < height)
            {
                if (allGems[x, y + 1] != null && allGems[x, y + 1].Type == GlobalEnums.GemType.bomb)
                    MarkBombArea(new Vector2Int(x, y + 1), allGems[x, y + 1].BlastSize);
            }
        }
    }

    public void MarkBombArea(Vector2Int bombPos, int _BlastSize)
    {
        string _print = "";
        for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++)
        {
            for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (allGems[x, y] != null)
                    {
                        _print += "(" + x + "," + y + ")" + System.Environment.NewLine;
                        allGems[x, y].IsMatch = true;
                        currentMatches.Add(allGems[x, y]);
                    }
                }
            }
        }
        currentMatches = currentMatches.Distinct().ToList();
    }
}

