using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameBoardEventsAdapter
{ 
    private class BatchRound
    {
        public int Number = 0;
        public Dictionary<int, Queue<IGameBoardCommand>> ColumnCommands = new();
        public List<IGameBoardCommand> Commands = new();
    }

    public event Action EventLastCommandExecuted;
    
    // dependencies
    private readonly GameBoard gameBoard;
    
    // locals
    private List<BatchRound> batchRounds = new();
    private bool executing;
    
    // debug
    private int roundNum = 0;
    
    public GameBoardEventsAdapter(GameBoard gameBoard)
    {
        this.gameBoard = gameBoard;

        gameBoard.EventBatchOperationStarted += OnBatchStarted;
        gameBoard.EventBatchOperationEnded += OnBatchEnded;
    }
    
    public void AddGlobalCommand(IGameBoardCommand command)
    {
        if (batchRounds.Count == 0) {
            Debug.LogError($"Can't start command since no round found!");
            return;
        }
        
        var currentRound = batchRounds[^1];
        Debug.Log($"<color=green>[Command] Added Command '{command.Name}' to queue of batch round #{currentRound.Number}. Details:\n {command.Details} </color>");
        
        
        currentRound.Commands.Add(command);
    }

    public void AddColumnCommand(IGameBoardCommand command, int column)
    {
        Debug.Log($"<color=green>[COLUMN_COMMAND:{column}] Added Command '{command.Name}' to queue. Details:\n {command.Details} </color>");
        var currentRound = batchRounds[^1];
        if (!currentRound.ColumnCommands.TryGetValue(column, out var queue))
        {
            queue = new Queue<IGameBoardCommand>();
            currentRound.ColumnCommands[column] = queue;
        }
        queue.Enqueue(command);
    }

    public async Task FlushCommands()
    {
        if (executing) {
            return;
        }
        
        executing = true;
        var currentRound = batchRounds[0];
        batchRounds.RemoveAt(0);
        Debug.Log($"<color=cyan>Executing round #{currentRound.Number}!</color>");
        
        // 1. Execute all global commands
        foreach (var globalCommand in currentRound.Commands) {
            Debug.Log($"<color=cyan>Executing global command {globalCommand.Name}. Details: {globalCommand.Details}</color>");
            await globalCommand.ExecuteAsync();
        }
        
        // 2. Execute per-column tasks
        // List<Task> commandsToExecuteInParallel = new();
        // while (currentRound.ColumnCommands.Count > 0) {
        //     for (var col = 0; col < gameBoard.Width; ++col) {
        //         if (currentRound.ColumnCommands.TryGetValue(col, out var commandsInColumn)) {
        //             var command = commandsInColumn.Dequeue();
        //             if (commandsInColumn.Count == 0) {
        //                 currentRound.ColumnCommands.Remove(col);
        //             }
        //
        //             Debug.Log(
        //                 $"<color=cyan>Executing column command {command.Name}. Details: {command.Details}</color>");
        //             commandsToExecuteInParallel.Add(command.ExecuteAsync());
        //         }
        //     }
        //
        //     await Task.WhenAll(commandsToExecuteInParallel);
        //     commandsToExecuteInParallel.Clear();
        //     Debug.Log($"<color=cyan>Finished executing parallel commands. batchRounds = {batchRounds.Count} </color>");
        // }

        // 2. Execute per-column tasks
        List<Task> allColumnTasks = new();

        // For each column, create tasks that launch all commands with delays
        for (var col = 0; col < gameBoard.Width; ++col) {
            if (currentRound.ColumnCommands.TryGetValue(col, out var commandsInColumn) && commandsInColumn.Count > 0) {
                // Create a task for this column that will launch all commands with delays
                var columnTask = ExecuteColumnWithStagger(commandsInColumn, 0.1f);
                allColumnTasks.Add(columnTask);
            }
        }

        await Task.WhenAll(allColumnTasks);
        
        Debug.Log($"<color=cyan>^^^^^^^ FINISHED Executing round #{currentRound.Number}! ^^^^^^^^ </color>");
        if (batchRounds.Count > 0) {
            executing = false;
            FlushCommands();
        }
        else {
            EventLastCommandExecuted?.Invoke();
            executing = false;
        }
    }

    private async Task ExecuteColumnWithStagger(Queue<IGameBoardCommand> commands, float staggerDelay)
    {
        List<Task> columnCommands = new();
        int commandIndex = 0;
    
        // Create all tasks at once (with start delays)
        foreach (var command in commands)
        {
            float startDelay = commandIndex * staggerDelay;
            var delayedCommand = new DelayedStartCommand(command, startDelay);
            columnCommands.Add(delayedCommand.ExecuteAsync());
            commandIndex++;
        }
    
        // Launch all in parallel
        await Task.WhenAll(columnCommands);
    }

    private void OnBatchStarted()
    {
        BatchRound batchRound = new BatchRound() {Number = roundNum++};
        batchRounds.Add(batchRound);
        Debug.Log($"<color=cyan>------- Round #{batchRound.Number} enqueued. Rounds in QUEUE: {batchRounds.Count} ---------</color>");
    }

    private void OnBatchEnded()
    {
        if (!executing) {
            var curRound = batchRounds[^1];
            FlushCommands();
            Debug.Log($"<color=cyan>---------- Round #{curRound.Number} ended. Rounds in queue -----------</color>");
        }
    }
}