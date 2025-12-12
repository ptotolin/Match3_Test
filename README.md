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
│   ├── ProjectInstaller.cs       # Zenject installer for project-wide dependencies
│   ├── SceneInstaller.cs         # Zenject installer for scene-specific dependencies
│   ├── GameBoardInitializer.cs   # Initial game board setup
│   ├── EventBus.cs               # Event system for communication
│   ├── GameLogger.cs             # Centralized logging
│   ├── SC_GameVariables.cs       # Game configuration (needs optimization)
│   └── SingletonMonobehaviour.cs # Base Singleton class
│
├── ModelSide/                     # Model layer (pure business logic)
│   ├── GameBoard.cs              # Game board - state storage
│   ├── SC_Gem.cs                 # Gem model with component system
│   ├── SC_GameLogic.cs           # Main game logic
│   ├── GameState.cs              # Shared mutable game state
│   ├── MatchDetector.cs          # Match detection service
│   ├── DistinctGemGenerator.cs   # Safe gem generation
│   ├── BombSpecialAbility.cs     # Bomb ability
│   ├── IPhaseState.cs            # Phase state interface
│   ├── PhaseContext.cs           # Phase execution and transitions
│   ├── MatchPhaseState.cs        # Match detection phase
│   ├── DestroyPhaseState.cs      # Gem destruction phase
│   ├── FillBoardPhaseState.cs    # Board filling phase
│   ├── StablePhaseState.cs       # Stable state phase
│   ├── IGemComponent.cs          # Gem component interface
│   ├── IGemPhaseBehavior.cs      # Phase-aware component interface
│   ├── BombPhaseBehavior.cs      # Bomb phase behavior component
│   ├── ColoredBombComponent.cs   # Colored bomb component
│   └── EventBusEvents/           # Model events
│       ├── BombExplosionEventData.cs
│       ├── PhaseEnteredEventData.cs
│       ├── PhaseExitedEventData.cs
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
- `GameState` - shared mutable state across phases
- `SC_GameLogic` - manages game flow and phase transitions
- `SC_Gem` - gem data model with component system
- `IPhaseState` implementations - phase-specific game logic

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

**Component System**:
Gems can have components that extend their behavior:
- `IGemComponent` - base interface for gem components
- `IGemPhaseBehavior` - phase-aware behavior components
- `BombPhaseBehavior` - handles delayed/immediate bomb explosions based on phase
- `ColoredBombComponent` - stores the color a bomb matches with

**Activation**:
- **On Swap**: Abilities activate immediately in `IsSwapProcessed()`
- **On Match**: Abilities activate based on phase behavior (see Delayed Bomb Explosions section)
- EventBus publishes events for visualization

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

### 7. Dependency Injection via Zenject

The project uses **Zenject** (Extenject) for Dependency Injection, replacing the previous manual dependency wiring in `GameBootstrap`.

**Dependency Registration**:

- **ProjectInstaller** (`ProjectInstaller.cs`) - Project-wide dependencies:
  - `GameLogger` configuration

- **SceneInstaller** (`SceneInstaller.cs`) - Scene-specific dependencies:
  - Game services: `GameBoard`, `GameState`, `PhaseContext`, `MatchDetector`, `IGemGenerator`, `IEventBus`, `GameBoardEventsAdapter`
  - MonoBehaviour components from prefabs: `SC_GameLogic`, `GameBoardPresenter`, `GemInputHandler`
  - Scene objects: `GameScreen`
  - `GameBoardInitializer` (with `.NonLazy()`) - initializes the game board on startup

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

### 11. Phase-Based State Machine

The game logic is organized into distinct phases using a state machine pattern. This allows for clear separation of concerns and better control over the game flow.

**Phase Interface**:
```csharp
public interface IPhaseState
{
    string Name { get; }
    void Execute();
}
```

**Phases**:
1. **MatchPhaseState** - Detects all matches on the board
   - Calls `MatchDetector.FindAllMatches()`
   - Determines positions for creating new bombs (from 4+ matches)
   - Stores matches in `GameState.CurrentMatches`

2. **DestroyPhaseState** - Destroys matched gems and creates new bombs
   - Destroys gems from `CurrentMatches` (except those in `DelayedGems`)
   - Creates new bombs at positions determined in MatchPhase
   - Calculates score

3. **FillBoardPhaseState** - Makes gems fall and refills empty spaces
   - Simulates gravity (gems fall down)
   - Generates new gems for empty cells

4. **StablePhaseState** - Final phase when no matches or movements occur
   - Marks the board as stable
   - Triggers delayed actions (e.g., delayed bomb explosions)

**Phase Context**:
`PhaseContext` manages phase transitions and publishes events:

```csharp
// Before phase execution
eventBus.Publish(new PhaseEnteredEventData { Phase = newPhase, ... });

newPhase.Execute();

// After phase execution
eventBus.Publish(new PhaseExitedEventData { Phase = newPhase, ... });
```

**Game Loop**:
```csharp
do {
    phaseContext.ExecutePhase(new MatchPhaseState(...));      // Find matches
    phaseContext.ExecutePhase(new DestroyPhaseState(...));    // Destroy & create bombs
    phaseContext.ExecutePhase(new FillBoardPhaseState(...));  // Fall & refill
} while (gameState.HasMatches || gameBoard.IsDirty());

phaseContext.ExecutePhase(new StablePhaseState(...));         // Stable state
```

**Advantages**:
- Clear separation of responsibilities
- Easy to test individual phases
- Components can react to phase changes via events
- Extensible - easy to add new phases

### 12. Delayed Bomb Explosions

Bombs have special behavior based on how they enter a match:

**Component Pattern**:
Bombs use `BombPhaseBehavior` component that reacts to phase events:

```csharp
public class BombPhaseBehavior : IGemPhaseBehavior
{
    // Subscribes to PhaseExitedEventData
    // Decides when to explode based on phase and context
}
```

**Behavior Rules**:

1. **Immediate Explosion** - When a bomb is swapped by the player:
   - Bomb explodes immediately in `IsSwapProcessed()` method
   - No delay, standard bomb explosion pattern

2. **Delayed Explosion** - When a bomb is matched automatically (e.g., falling gems form a match):
   - Matched gems are destroyed immediately
   - Bomb is added to `GameState.DelayedGems` list
   - Bomb is **not** destroyed in `DestroyPhaseState` (checked via `DelayedGems.Contains()`)
   - After all cascades complete and board is stable, bomb explodes in `StablePhaseState`

**Implementation Flow**:

```
MatchPhaseState.Execute()
  → Finds matches (bomb in CurrentMatches)
  → PhaseExitedEventData published
  
BombPhaseBehavior.OnPhaseExited(MatchPhaseState)
  → Checks: was bomb swapped?
  → If NO: adds bomb to GameState.DelayedGems
  
DestroyPhaseState.Execute()
  → Destroys all gems in CurrentMatches
  → Skips gems in DelayedGems (bomb survives!)
  
[Game loop continues with cascades...]
  
StablePhaseState.Execute()
  → PhaseExitedEventData published
  
BombPhaseBehavior.OnPhaseExited(StablePhaseState)
  → If bomb in DelayedGems: executes SpecialAbility (explodes)
  → Removes from DelayedGems
```

**GameState Integration**:
`GameState` holds shared mutable state across phases:
- `DelayedGems` - List of gems that should not be destroyed immediately
- `CurrentMatches` - Gems currently in a match
- `BombPlacements` - Positions where new bombs should be created
- `SwapHappened` - Whether current turn started with a swap

**Advantages**:
- Model (BombSpecialAbility) doesn't need to know about delays
- View controls all timing and animations
- Flexible - can add delay behavior to other special gems
- Clear separation: phases handle timing, components handle behavior

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

- **SC_GameVariables** - Needs optimization (currently uses Singleton pattern, should be refactored to use Dependency Injection via Zenject)
- **GameBootstrap** - Legacy entry point, mostly commented out after Zenject migration (can be removed)

## Extension Possibilities

- **New gem types**: Add new types to `GlobalEnums.GemType`
- **New abilities**: Implement `IGemSpecialAbility`
- **New effects**: Create new commands in `Commands/`
- **Different visual styles**: Replace `SC_GemView` without changing logic

## Dependencies

- Unity (MonoBehaviour, Transform, Vector2Int, etc.)
- System.Threading.Tasks (for async/await commands)
- Zenject (Extenject) - Dependency Injection framework

