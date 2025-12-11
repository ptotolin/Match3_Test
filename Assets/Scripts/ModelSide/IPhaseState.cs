public interface IPhaseState
{
    string Name { get; }
    void Execute();
}