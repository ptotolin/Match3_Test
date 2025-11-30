using System.Threading.Tasks;

public interface IGameBoardCommand
{
    string Name { get; }
    string Details { get; }
    Task ExecuteAsync();
}