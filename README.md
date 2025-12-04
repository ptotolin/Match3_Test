# Match-3 Game - Project Architecture

## Overview

This project is a Match-3 game (similar to Bejeweled/Candy Crush) developed in Unity. The project follows clean architecture principles with clear separation of responsibilities between components.

## Architectural Pattern

The project uses **Model-View-Presenter (MVP)** architecture with elements of **Event-Driven** approach and **Command Pattern** for animation management.

### Core Principles:
- **Model** - Pure business logic, independent of Unity
- **View** - Visual Unity components (MonoBehaviour)
- **Presenter** - Bridge between Model and View
- **Events** - Communication between components without tight coupling

## Project Structure

```
Assets/Scripts/
├── Core/                          # Common infrastructure components
│   ├── GameBootstrap.cs          # Entry point, Dependency Injection
│   ├── EventBus.cs               # Event system for communication
│   ├── GameLogger.cs             # Centralized logging
│   ├── SC_GameVariables.cs       # Game configuration (needs optimization)
│   └── SingletonMonobehaviour.cs # Base Singleton class
│
├── ModelSide/                     # Model layer (pure business logic)
│   ├── GameBoard.cs              # Game board - state storage
│   ├── SC_Gem.cs                 # Gem model
│   ├── SC_GameLogic.cs           # Main game logic
│   ├── MatchDetector.cs          # Match detection service
│   ├── DistinctGemGenerator.cs   # Safe gem generation
│   ├── BombSpecialAbility.cs     # Bomb ability
│   └── EventBusEvents/           # Model events
│       ├── BombExplosionEventData.cs
│       └── SpecialGemsAffectedEventData.cs
│
└── ViewSide/                      # View layer (visualization)
    ├── GameBoardPresenter.cs     # Game board presenter
    ├── SC_GemView.cs             # Visual gem representation
    ├── GemInputHandler.cs        # User input handling
    ├── GameBoardEventsAdapter.cs # Event batching adapter
    ├── ObjectPool.cs             # Object pooling for optimization
    └── Commands/                  # Animation commands
        ├── MoveGemCommand.cs
        ├── SpawnGemCommand.cs
        ├── DestroyGemCommand.cs
        ├── SwapGemsCommand.cs
        └── ...
```

## Key Architectural Decisions

### 1. Model and View Separation

**Model (ModelSide)**:
- Pure C# logic without Unity dependencies
- `GameBoard` - stores game board state
- `SC_GameLogic` - manages game flow
- `SC_Gem` - simple gem data model

**View (ViewSide)**:
- Unity-specific components (MonoBehaviour)
- `SC_GemView` - visual gem representation
- `GameBoardPresenter` - manages visualization

**Separation ensures**:
- Model can be moved to a separate Assembly
- Easy testing of business logic
- Independent development of logic and visualization

### 2. Event-Driven Communication

Model publishes events, presenter subscribes to them:

```csharp
// Model publishes events
gameBoard.EventGemMoved?.Invoke(fromPos, toPos);
gameBoard.EventGemDestroy?.Invoke(gemPos);

// Presenter subscribes
gameboard.EventGemMoved += OnGemMoved;
gameboard.EventGemDestroy += OnGemDestroy;
```

**Advantages**:
- Loose coupling between components
- Model doesn't know about view
- Easy to add new subscribers (logging, analytics)

### 3. Command Pattern for Animations

All visual operations are executed through commands:

```csharp
interface IGameBoardCommand
{
    Task ExecuteAsync();
}
```

**Command Types**:
- `MoveGemCommand` - gem movement animation
- `SpawnGemCommand` - visual gem creation
- `DestroyGemCommand` - destruction with effect
- `SwapGemsCommand` - swap animation
- `CompositeCommand` - parallel execution
- `SequentialCompositeCommand` - sequential execution

**Advantages**:
- All animations execute sequentially through a queue
- Easy to combine commands
- Can add delays between commands

### 4. GameBoardEventsAdapter - Event Batching

Problem: Model generates many events synchronously (e.g., all gems fall at once), but they need to be visualized with delays.

**Solution**: `GameBoardEventsAdapter`:
- Buffers events from model
- Groups them into batches (rounds)
- Executes commands with proper synchronization:
  - Commands in the same column - sequentially with delays (staggered)
  - Commands in different columns - in parallel
  - Global commands (swaps, explosions) - sequentially

### 5. Special Abilities

Gems can have abilities that activate on match:

```csharp
interface IGemSpecialAbility
{
    void Execute();
    string AbilityType { get; }
}
```

**Implementations**:
- `BombSpecialAbility` - explodes gems in cross pattern

**Activation**:
1. On match, gem with ability is marked
2. After fall and refill, ability activates
3. EventBus publishes event for visualization

### 6. EventBus for Inter-Module Communication

For communication between Model and View, `EventBus` is used:

```csharp
interface IEventBus
{
    void Publish<T>(T eventData);
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
}
```

**Usage**:
- Model publishes ability events
- Presenter subscribes and creates visual effects

### 7. Dependency Injection via GameBootstrap

All dependencies are created and configured in `GameBootstrap`:

```csharp
// Create all components
gameBoard = new GameBoard(width, height);
matchDetector = new MatchDetector(gameBoard);
eventBus = new EventBus();

// Initialize with dependencies
gameLogic.Initialize(..., eventBus);
gameBoardPresenter.Initialize(gameBoard, adapter, eventBus);
```

**Advantages**:
- Centralized dependency management
- Easy to test (can replace dependencies)
- Clear entry point

### 8. Service Classes

Complex logic is extracted into separate services:

- **MatchDetector** - match detection (3-in-a-row, 4+, intersections)
- **DistinctGemGenerator** - safe gem generation (no immediate matches)
- **IGameBoardReader** - interface for read-only board access

**Advantages**:
- `GameBoard` is not overloaded with logic
- Easy to test services separately
- Can swap implementations

### 9. Staggered Drop Animation

To create cascading fall effect:
- Gems in the same column fall sequentially with delay
- Gems in different columns fall in parallel
- Implemented via `DelayedStartCommand` in `GameBoardEventsAdapter`

### 10. Centralized Logging

All logging goes through `GameLogger`:

```csharp
GameLogger.IsEnabled = false; // Disable all logs with one line
```

## Data Flow

### Game Loop:

1. **Input** → `GemInputHandler` detects swipe
2. **Model** → `SC_GameLogic` receives event, calls `gameBoard.SwapGems()`
3. **Model Events** → `GameBoard` publishes `EventGemsSwapped`
4. **Adapter** → `GameBoardEventsAdapter` buffers event
5. **Presenter** → `GameBoardPresenter` creates `SwapGemsCommand`
6. **Commands** → Command executes asynchronously, animating swap
7. **Logic** → After animation, matches are checked
8. **Cascade** → Process repeats for cascade matches

### Cascade Matches:

1. Destroy matches → `EventGemDestroy`
2. Gems fall → `EventGemMoved` (multiple events)
3. Refill → `EventGemSpawned` (multiple events)
4. New matches → cycle repeats

## Design Decisions

### Why not Commands on Model side?

Commands are used only for animations (View side). Model works synchronously and publishes events - this is simpler and clearer for business logic.

### Why GameBoardEventsAdapter?

Model generates events synchronously, but visualization must be asynchronous with delays. The adapter solves this by grouping events and managing their execution.

### Why separate EventBus instead of built-in GameBoard events?

EventBus is used for communication between abilities (Model) and visualization (View), which may be in different layers. This is a more universal solution.

## Known Issues / TODO

- **SC_GameVariables** - Needs optimization (currently uses Singleton pattern, should be refactored to use Dependency Injection)

## Extension Possibilities

- **New gem types**: Add new types to `GlobalEnums.GemType`
- **New abilities**: Implement `IGemSpecialAbility`
- **New effects**: Create new commands in `Commands/`
- **Different visual styles**: Replace `SC_GemView` without changing logic

## Dependencies

- Unity (MonoBehaviour, Transform, Vector2Int, etc.)
- System.Threading.Tasks (for async/await commands)

