using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Структура для хранения информации о группе матча 4+ гемов
/// </summary>
public struct MatchGroup
{
    public List<Vector2Int> Positions; // Все позиции в группе
    public int Count; // Количество гемов в группе

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var pos in Positions) {
            if (pos != Positions.Last()) {
                sb.Append($"{pos},");
            }
            else {
                sb.Append($"pos");
            }
        }

        sb.Append($" Count = {Count}");

        return sb.ToString();
    }
}