public class PhaseEnteredEventData
{
    public IPhaseState Phase { get; set; }
    public GameState GameState { get; set; }
    public IPhaseState PreviousPhase { get; set; } 
}