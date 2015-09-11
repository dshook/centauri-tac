/// The only change in StartCommand is that we extend Command, not EventCommand
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
        public StartAuthSignal startAuth { get; set; }

        public override void Execute()
        {
            //var startAuth = (StartAuthSignal)injectionBinder.GetInstance<StartAuthSignal>();
            startAuth.Dispatch("");
            //GameObject go = new GameObject();
            //go.name = "ExampleView";
            //go.AddComponent<ExampleView>();
            //go.transform.parent = contextView.transform;
        }
    }
}

