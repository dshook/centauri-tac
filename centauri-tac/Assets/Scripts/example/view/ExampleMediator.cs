/// Example mediator
/// =====================
/// Note how we no longer extend EventMediator, and inject Signals instead

using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;

namespace strange.examples.signals
{
    public class ExampleMediator : Mediator
    {
        [Inject]
        public ExampleView view { get; set; }

        //Injecting this one because we want to fire it
        [Inject]
        public CallWebServiceSignal callWebServiceSignal { get; set; }

        public override void OnRegister()
        {
            view.clickSignal.AddListener(onViewClicked);

            view.init();
        }

        public override void OnRemove()
        {
            //Clean up listeners just as you do with EventDispatcher
            view.clickSignal.RemoveListener(onViewClicked);
            Debug.Log("Mediator OnRemove");
        }

        private void onViewClicked()
        {
            Debug.Log("View click detected");
            //Dispatch a Signal. We're adding a string value (different from MyFirstContext,
            //just to show how we can Inject values into commands)
            callWebServiceSignal.Dispatch(view.currentText);
        }
    }
}

