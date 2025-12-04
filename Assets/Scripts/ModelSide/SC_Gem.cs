using System;
using UnityEngine;

using UnityEngine;

[Serializable]
public class SC_Gem
{
    [SerializeField] private GlobalEnums.GemType type;
    [SerializeField] private int score;

    public GlobalEnums.GemType Type => type;
    
    public IGemSpecialAbility SpecialAbility { get; set; }
    
    public bool IsMatch { get; set; }
    public int ScoreValue => score;

    // TODO: remove outside
    public int BlastSize { get; set; } = 2;

    public SC_Gem(GlobalEnums.GemType gemType, int scoreValue)
    {
        type = gemType;
        score = scoreValue;
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
