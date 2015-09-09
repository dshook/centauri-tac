using Svelto.IoC;
using System;
using UnityEngine;

enum MonsterState
{
    Move,
    Hit
};

public class MonsterPresenter
{
    const float DAMAGE = 0.25f;

    float _energy = 1.0f;
    float _hitEnergy = 0.0f;
    MonsterView _view;
    
    [Inject]
    public UnderAttackSystem attackSystem { set; private get; }

    public event System.Action<MonsterPresenter> OnKilled;

    public Transform target { get { return _view.transform; } }
    public float energy { get { return _energy; } }

    public MonsterPresenter()
    {
    }

    public void SetView(MonsterView view)
    {
        _view = view;

        attackSystem.AddMonster(this);
    }

    public void CommitSuicide()
    {
        Killed();
    }

    public void StartBeingHit()
    {
        _hitEnergy += DAMAGE;
    }

    public void StopBeingHit()
    {
        _hitEnergy -= DAMAGE;
    }

    public void Update(float deltaTime)
    {
        _energy -= deltaTime * _hitEnergy;

        if (_energy <= 0)
        {
            Killed();
        }
    }

    void Killed()
    {
        if (OnKilled != null)
            OnKilled(this);

        _view.Killed();
    }
}
