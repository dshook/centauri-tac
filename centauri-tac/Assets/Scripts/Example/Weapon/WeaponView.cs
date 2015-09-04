using UnityEngine;
using Svelto.IoC;

public class WeaponView : MonoBehaviour
{
    [Inject]
    public WeaponPresenter weapon { private get; set; }
    // Use this for initialization
    void Start()
    {

        weapon.SetView(this);
    }

    public void PlayIdle()
    {
    }

    void Update()
    {
        weapon.Update();
    }

    public void PauseTweener()
    {
    }

    WeaponState _currentViewState;
    GameObject _lockedTarget;
}
