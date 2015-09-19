using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class QuitMediator : Mediator
    {
        [Inject]
        public QuitView view { get; set; }

        [Inject]
        public QuitSignal quitSignal { get; set; }

        public override void OnRegister()
        {
            view.quit.AddListener(() => quitSignal.Dispatch());
        }

    }
}

