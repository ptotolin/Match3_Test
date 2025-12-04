using System.Collections.Generic;

public class SpecialGemsAffectedEventData : ISpecialAbilityEventData
{
    public string AbilityType => "SpecialGemsAffected";
    
    public List<SC_Gem> AffectedSpecialGems { get; }
    public string SourceAbilityType { get; }
    
    
    public SpecialGemsAffectedEventData(List<SC_Gem> affectedSpecialGems, string sourceAbilityType)
    {
        AffectedSpecialGems = affectedSpecialGems;
        SourceAbilityType = sourceAbilityType;
    }
}