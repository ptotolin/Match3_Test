using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Structure for storing information about a match group of 4+ gems
/// </summary>
public struct MatchGroup
{
    public List<Vector2Int> Positions; // All positions in the group
    public int Count; // Number of gems in the group

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