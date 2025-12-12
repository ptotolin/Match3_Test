using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO: Remove that class
public class GameBoardCommandsExecutor
{
    public event Action EventLastCommandExecuted;
    
    private Queue<IGameBoardCommand> commands = new();
    private bool isExecuting;

    public void AddCommand(IGameBoardCommand command)
    {
        GameLogger.Log($"<color=green>[Command] Added Command '{command.Name}' to queue. Details:\n {command.Details} </color>");
        commands.Enqueue(command);
        if (!isExecuting) {
            Execute();
        }
    }

    private async Task Execute()
    {
        isExecuting = true;
        while (true) {
            var command = commands.Dequeue();
            GameLogger.Log($"[Command] Executing {command.Name}");
            await command.ExecuteAsync();
            GameLogger.Log($"[Command] Executing finished {command.Name}");
            if (commands.Count == 0) {
                break;
            }
        }

        isExecuting = false;
        EventLastCommandExecuted?.Invoke();
    }
}