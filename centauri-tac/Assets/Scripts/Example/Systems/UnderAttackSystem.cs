using System.Collections.Generic;
using UnityEngine;
using Svelto.Ticker;
using Svelto.IoC;

public class UnderAttackSystem : ITickable, IInitialize
{
    List<WeaponPresenter> _freeWeapons;
    Dictionary<Transform, MonsterPresenter> _monstersDic;

    [Inject]
    public IMonsterCountHolder monsterCounter { set; private get; }

    public UnderAttackSystem()
    {
        _freeWeapons = new List<WeaponPresenter>();
        _monstersDic = new Dictionary<Transform, MonsterPresenter>();
    }

    public void OnDependenciesInjected()
    {
        DesignByContract.Check.Require(monsterCounter != null);
    }

    //ITickable Interface

    public void Tick(float delta)
    {
        CheckTargets();
    }

    /// <summary>
    /// public methods
    /// </summary>

    public void AddFreeWeapon(WeaponPresenter weapon)
    {
        _freeWeapons.Add(weapon);
    }

    public void AddMonster(MonsterPresenter monster)
    {
        _monstersDic[monster.target] = monster;

        monsterCounter.AddMonster();

        monster.OnKilled += OnMonsterKilled;
    }

    void CheckTargets()
    {
        for (var monsterEnumerator = _monstersDic.Values.GetEnumerator(); monsterEnumerator.MoveNext();)
        {
            var currentMonster = monsterEnumerator.Current;

            for (int i = 0; i < _freeWeapons.Count; i++)
            {
                WeaponPresenter currentWeapon = _freeWeapons[i];

                if (currentWeapon.CheckAndAcquireTarget(currentMonster.target) == true)
                {
                    currentMonster.StartBeingHit();

                    currentWeapon.OnTargetNotFound += TargetOutOfRange;

                    _freeWeapons.RemoveAt(i);

                    return;
                }
            }
        }
    }

    void OnMonsterKilled(MonsterPresenter monster)
    {
        monster.OnKilled -= OnMonsterKilled;

        monsterCounter.RemoveMonster();

        _monstersDic.Remove(monster.target);
    }

    void TargetOutOfRange(WeaponPresenter weapon, bool targetIsDead)
    {
        weapon.OnTargetNotFound -= TargetOutOfRange;

        if (targetIsDead == false)
            _monstersDic[weapon.target].StopBeingHit();

        AddFreeWeapon(weapon);
    }
}
