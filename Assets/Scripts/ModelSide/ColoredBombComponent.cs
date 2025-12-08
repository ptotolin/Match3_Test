public class ColoredBombComponent : IGemComponent
{
    public string ComponentType => "ColoredBomb";
    public GlobalEnums.GemType MatchColor { get; }
    
    public ColoredBombComponent(GlobalEnums.GemType matchColor)
    {
        MatchColor = matchColor;
    }
}