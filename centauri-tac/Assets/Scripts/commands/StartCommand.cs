using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.impl;

namespace ctac
{
    public class StartCommand : Command
    {

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public StartConnectSignal startConnect { get; set; }

        public override void Execute()
        {
            startConnect.Dispatch();
        }
    }
}

