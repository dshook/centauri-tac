using System;
using UnityEngine;
using Svelto.IoC;

public class MonsterPathFollower
{
    int _currentCheckPoint = 0;
    MonsterPresenter _monster;
    Transform _transform;
    [Inject]
    public PathController pathController { set; private get; }

    public void Start(Transform transform)
    {
        _transform = transform;
        _transform.position = pathController.CheckPoint(_currentCheckPoint);

        MoveNext();
    }

    void MoveNext()
    {
        if (pathController.IsEndReached(_currentCheckPoint) == false)
        {
            _currentCheckPoint++;
        }
        else
        {
            _monster.CommitSuicide(); //in a real project I would have used a command, not directly the presenter
        }
    }

    public void SetMonster(MonsterPresenter monster)
    {
        _monster = monster;
    }

}

