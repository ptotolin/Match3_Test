using System.Threading.Tasks;
using UnityEngine;

public class DelayCommand : IGameBoardCommand
{
    public string Name => "DelayCommand";
    
    public string Details => $"Delay {delaySeconds}s";
    
    private readonly float delaySeconds;
    
    public DelayCommand(float delaySeconds)
    {
        this.delaySeconds = delaySeconds;
    }
    
    public async Task ExecuteAsync()
    {
        float elapsed = 0f;
        while (elapsed < delaySeconds)
        {
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
    }
}