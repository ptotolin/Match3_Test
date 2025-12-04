using System.Collections.Generic;
using UnityEngine;

public class BombExplosionEventData : ISpecialAbilityEventData
{
    public string AbilityType => "Bomb";
    
    public SC_Gem Bomb { get; }
    public List<SC_Gem> AffectedGems { get; }
    public float NeighborDestroyDelay { get; }
    public float BombDestroyDelay { get; }
    
    public BombExplosionEventData(
        SC_Gem bomb, 
        List<SC_Gem> affectedGems, 
        float neighborDelay = 0.5f, 
        float bombDelay = 0.3f)
    {
        Bomb = bomb;
        AffectedGems = affectedGems;
        NeighborDestroyDelay = neighborDelay;
        BombDestroyDelay = bombDelay;
    }
}