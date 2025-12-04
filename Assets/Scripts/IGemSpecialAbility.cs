public interface IGemSpecialAbility
{
    /// <summary>
    /// Executes the ability (removes gems, changes types, etc.)
    /// </summary>
    void Execute();
    
    /// <summary>
    /// Ability type identifier (for visualization)
    /// </summary>
    string AbilityType { get; }
}