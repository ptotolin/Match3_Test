using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [Header("Scene objects:")]
    [SerializeField] private GameScreen gameScreen;
    
    [Header("Prefabs")]
    [SerializeField] private GemInputHandler gemInputHandlerPrefab;
    [SerializeField] private GameBoardPresenter gameBoardPresenterPrefab;
    [SerializeField] private SC_GameLogic gameLogicPrefab;
    
    [Header("GameBoard settings:")]
    [SerializeField] private int gameBoardWidth;
    [SerializeField] private int gameBoardHeight;
    
    public override void InstallBindings()
    {
        Debug.Log($"Scene Context: Install Bindings");
        
        var gameBoard = new GameBoard(gameBoardWidth, gameBoardHeight);
        Container.BindInstance(gameBoard).AsSingle();

        Container.Bind<IGameBoardReader>().To<GameBoard>().FromResolve();

        Container.Bind<IGemGenerator>().To<DistinctGemGenerator>().AsSingle();
        Container.Bind<MatchDetector>().AsSingle();
        Container.Bind<IEventBus>().FromInstance(new EventBus());
        Container.Bind<GameBoardEventsAdapter>().AsSingle();
        Container.Bind<GameState>().AsSingle();
        Container.Bind<PhaseContext>().AsSingle();
        Container.Bind<GameBoardInitializer>().AsSingle().NonLazy();
        
        Container.Bind<SC_GameLogic>()
            .FromComponentInNewPrefab(gameLogicPrefab)
            .WithGameObjectName("Game logic")
            .AsSingle()
            .NonLazy(); 
        
        Container.Bind<GemInputHandler>()
            .FromComponentInNewPrefab(gemInputHandlerPrefab)
            .WithGameObjectName("Input handler")
            .AsSingle()
            .NonLazy();
        
        Container.Bind<GameBoardPresenter>()
            .FromComponentInNewPrefab(gameBoardPresenterPrefab)
            .WithGameObjectName("GameBoard presenter")
            .AsSingle()
            .NonLazy();

        Container.Bind<GameScreen>()
            .FromComponentOn(gameScreen.gameObject)
            .AsSingle();
    }
}
