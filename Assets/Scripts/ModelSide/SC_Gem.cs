using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SC_Gem
{
    [SerializeField] private GlobalEnums.GemType type;
    [SerializeField] private int score;

    private List<IGemComponent> components = new List<IGemComponent>();
    
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
    
    public T GetComponent<T>() where T : class, IGemComponent
    {
        return components.OfType<T>().FirstOrDefault();
    }
    
    public void AddComponent(IGemComponent component)
    {
        components.Add(component);
    }
    
    public bool HasComponent<T>() where T : class, IGemComponent
    {
        return components.OfType<T>().Any();
    }
    
    public override string ToString()
    {
        return type.ToString();
    }
}
