using UnityEngine;

public enum WeaponState
{
    idle,
    fire
}

public class WeaponAI : MonoBehaviour
{
    WeaponState _currentState;
    GameObject _lockedTarget;
    [IoC.Inject]
    public IMonsterSystem monsterSystem { set; private get; }

    // Use this for initialization
    void Start()
    {
        this.Inject();

        DesignByContract.Check.Require(monsterSystem != null);

        _lockedTarget = null;

        _currentState = WeaponState.idle;

        PlayIdle();
    }

    void PlayIdle()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (_lockedTarget == null)
        {
            Fire();

            _lockedTarget = other.gameObject;

            IMonster monster = monsterSystem.SetUnderFire(_lockedTarget);

            if (monster != null)
                monster.OnKilled += () => { TargetKilledOrEscaped(_lockedTarget); };
        }
    }

    void OnTriggerExit(Collider other)
    {
        TargetKilledOrEscaped(other.gameObject);
    }

    void TargetKilledOrEscaped(GameObject target)
    {
        if (_lockedTarget != null && Object.ReferenceEquals(_lockedTarget, target))
        {
            //Check if there are other targets
            var collider = GetComponent<SphereCollider>();
            RaycastHit[] hits = Physics.SphereCastAll(this.transform.position, collider.radius, Vector3.zero);

            if (hits.Length > 0)
                _lockedTarget = hits[0].transform.gameObject;
            else
            {
                monsterSystem.EscapeFromFire(_lockedTarget);

                _lockedTarget = null;

                Idle();
            }
        }
    }

    void Update()
    {
        if (_currentState == WeaponState.fire)
        {
            transform.LookAt(_lockedTarget.transform);
        }
    }

    void Fire()
    {
        _currentState = WeaponState.fire;
    }

    void Idle()
    {
        _currentState = WeaponState.idle;

        PlayIdle();
    }
}
