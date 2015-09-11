using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using strange.examples.signals;
using ctac.services;

namespace ctac
{
    public class SignalsContext : MVCSContext
    {

        public SignalsContext(MonoBehaviour view) : base(view)
        {
        }

        public SignalsContext(MonoBehaviour view, ContextStartupFlags flags) : base(view, flags)
        {
        }

        // Unbind the default EventCommandBinder and rebind the SignalCommandBinder
        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            injectionBinder.Unbind<ICommandBinder>();
            injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
        }

        // Override Start so that we can fire the StartSignal 
        override public IContext Start()
        {
            base.Start();
            StartSignal startSignal = (StartSignal)injectionBinder.GetInstance<StartSignal>();
            startSignal.Dispatch();
            return this;
        }

        protected override void mapBindings()
        {
            injectionBinder.Bind<IExampleModel>().To<ExampleModel>().ToSingleton();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton();

            mediationBinder.Bind<ExampleView>().To<ExampleMediator>();

            commandBinder.Bind<CallWebServiceSignal>().To<CallWebServiceCommand>();

            //StartSignal is now fired instead of the START event.
            //Note how we've bound it "Once". This means that the mapping goes away as soon as the command fires.
            commandBinder.Bind<StartSignal>().To<StartCommand>().Once();

            //In MyFirstProject, there's are SCORE_CHANGE and FULFILL_SERVICE_REQUEST Events.
            //Here we change that to a Signal. The Signal isn't bound to any Command,
            //so we map it as an injection so a Command can fire it, and a Mediator can receive it
            injectionBinder.Bind<FulfillWebServiceRequestSignal>().ToSingleton();
        }
    }
}

