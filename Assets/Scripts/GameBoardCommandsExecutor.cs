using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameBoardCommandsExecutor
{
    public event Action EventLastCommandExecuted;
    
    private Queue<IGameBoardCommand> commands = new();
    private bool isExecuting;

    public void AddCommand(IGameBoardCommand command)
    {
        Debug.Log($"<color=green>[Command] Added Command '{command.Name}' to queue. Details:\n {command.Details} </color>");
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
            Debug.Log($"[Command] Executing {command.Name}");
            await command.ExecuteAsync();
            Debug.Log($"[Command] Executing finished {command.Name}");
            if (commands.Count == 0) {
                break;
            }
        }

        isExecuting = false;
        EventLastCommandExecuted?.Invoke();
    }
}