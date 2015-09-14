using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class MinionSelectMediator : Mediator
    {
        [Inject]
        public MinionSelectView view { get; set; }
        
        [Inject]
        public MinionSelectedSignal tileHover { get; set; }

        public override void OnRegister()
        {
        }

        public override void onRemove()
        {
        }

    }
}

