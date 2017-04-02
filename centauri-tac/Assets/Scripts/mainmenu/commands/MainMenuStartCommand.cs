using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;

namespace ctac
{
    public class MainMenuStartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            debug.Log("Hi from Main Menu");
        }
    }
}

