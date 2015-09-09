using Svelto.IoC;
using Svelto.Ticker;
using UnityEngine;

public class MonsterSpawner : ITickable, IInitialize
{
    float _frequency;
    float _timeLapsed;
    GameObject _monstersRoot;

    static GameObject _originalGO = Resources.Load("Monster") as GameObject;
    
    [Inject]
    public IGameObjectFactory gameObjectFactory { set; private get; }
    [Inject]
    public IMonsterCounter monsterCounter { set; private get; }

    public MonsterSpawner()
    {
        _monstersRoot = new GameObject("Monsters");
        _frequency = 3;
        _timeLapsed = _frequency;
    }

    public void OnDependenciesInjected()
    {
        DesignByContract.Check.Require(gameObjectFactory != null);
    }

    public void Tick(float delta)
    {
        _timeLapsed += delta;

        if (_timeLapsed >= _frequency)
        {
            if (monsterCounter.monsterCount < 5)
            {
                //the Monster dependencies will be injected by the container inside the factory
                GameObject monster = gameObjectFactory.Build(Monster());

                monster.transform.parent = _monstersRoot.transform;
            }
            _timeLapsed = 0;
            _frequency = Random.Range(0.5f, 4.0f);
        }
    }

    GameObject Monster()
    {
        return _originalGO;
    }
}

