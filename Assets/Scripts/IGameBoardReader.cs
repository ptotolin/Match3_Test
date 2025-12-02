using UnityEngine;

public interface IGameBoardReader
{
    int Width { get; }
    int Height { get; }
    SC_Gem GetGem(int x, int y);
    bool TryGetGemPos(SC_Gem gem, out Vector2Int pos);
}