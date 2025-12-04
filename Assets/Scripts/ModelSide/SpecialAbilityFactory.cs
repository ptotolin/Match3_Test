using UnityEngine;

public static class SpecialAbilityFactory
{
    /// <summary>
    /// Creates a special ability for a gem based on its type
    /// </summary>
    public static IGemSpecialAbility CreateAbility(
        GlobalEnums.GemType gemType, 
        GameBoard gameBoard, 
        SC_Gem gem,
        IEventBus eventBus,
        MatchDetector matchDetector = null)
    {
        switch (gemType)
        {
            case GlobalEnums.GemType.bomb:
                if (matchDetector == null)
                {
                    GameLogger.LogError("MatchDetector is required for BombSpecialAbility!");
                    return null;
                }
                return new BombSpecialAbility(gameBoard, gem, eventBus);
            
            default:
                return null; // Regular gems have no ability
        }
    }
}