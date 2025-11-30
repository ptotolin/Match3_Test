using System;
using UnityEngine;

[Serializable]
public class SC_Gem
{
    [SerializeField] private GlobalEnums.GemType type;

    public GlobalEnums.GemType Type => type;
    
    public bool IsMatch { get; set; }
    public int ScoreValue { get; }

    // TODO: remove outside
    public int BlastSize { get; set; } = 2;

    public SC_Gem(GlobalEnums.GemType gemType, int scoreValue)
    {
        type = gemType;
        ScoreValue = scoreValue;
    }

    public SC_Gem Clone()
    {
        return new SC_Gem(type, ScoreValue);
    }

    public override string ToString()
    {
        return type.ToString();
    }
}
