using System;
using UnityEngine;

public class MonsterPathFollower : MonoBehaviour
{
    [IoC.Inject]
    public PathController pathController { set; private get; }

    void Start()
    {
        transform.position = pathController.CheckPoint(currentCheckPoint);

        MoveNext();
    }

    void MoveNext()
    {
        if (pathController.IsEndReached(currentCheckPoint) == false)
        {

            currentCheckPoint++;
        }
        else
        {
            GetComponent<Monster>().CommitSuicide();
        }

    }

    private int currentCheckPoint = 0;
}

