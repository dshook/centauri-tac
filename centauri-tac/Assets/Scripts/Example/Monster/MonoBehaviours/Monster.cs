using UnityEngine;

enum MonsterState
{
    Move,
    Hit
};

public class Monster : MonoBehaviour, IMonster
{
    public event System.Action OnKilled;

    void Start()
    {
        state = MonsterState.Move;
    }

    public void CommitSuicide()
    {
        Killed();
    }

    public void StartBeingHit(float energyPerSecond)
    {
        hitEnergy += energyPerSecond;
        state = MonsterState.Hit;
    }

    public void StopBeingHit(float energyPerSecond)
    {
        hitEnergy -= energyPerSecond;
        state = MonsterState.Move;
    }

    void Update()
    {
        if (state == MonsterState.Hit)
        {
            energy -= Time.deltaTime * hitEnergy;

            if (energy <= 0)
                Killed();
        }
    }

    void Killed()
    {
        OnKilled();

        Destroy(this.gameObject);
    }

    private float energy = 1.0f;
    private float hitEnergy = 0.0f;
    private MonsterState state;
}

