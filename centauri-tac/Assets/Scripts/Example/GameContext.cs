using Svelto.IoC;
using Svelto.Ticker;

//Main is the Application Composition Root.
//Composition Root is the place where the framework can be initialised.

public class Main:ICompositionRoot
{
    public IContainer container { get; private set; }
    
    public Main()
    {
        SetupContainer();
        StartGame();
    }
    
    void SetupContainer()
    {
        container = new Container();
        
        //interface is bound to a specific instance
        container.Bind<IGameObjectFactory>().AsSingle(new GameObjectFactory(container));
        //interfaces are bound to specific implementations, the same instance will be used once created
        container.Bind<IMonsterCounter>().AsSingle<MonsterCountHolder>();
        container.Bind<IMonsterCountHolder>().AsSingle<MonsterCountHolder>();
        //once the dependency is requested, a new instance will be created
        container.Bind<WeaponPresenter>().ToFactory(new MultiProvider<WeaponPresenter>());
        container.Bind<MonsterPresenter>().ToFactory(new MultiProvider<MonsterPresenter>());
        container.Bind<MonsterPathFollower>().ToFactory(new MultiProvider<MonsterPathFollower>());
        //once requested, the same instance will be used
        container.BindSelf<UnderAttackSystem>();
        container.BindSelf<PathController>();
    }
    
    void StartGame()
    {
        UnityTicker tickEngine = new UnityTicker(); //note this object can be safely garbage collected
        
        tickEngine.Add(container.Inject(new MonsterSpawner()));
        tickEngine.Add(container.Build<UnderAttackSystem>());
    }
}

//A GameObject containing GameContext must be present in the scene
//All the monobehaviours present in the scene file that need dependencies 
//injected must be component of GameObjects children of GameContext.

public class GameContext: UnityRoot<Main>
{
}
