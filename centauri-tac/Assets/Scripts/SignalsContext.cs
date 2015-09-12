using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;

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
            injectionBinder.Bind<IConfigModel>().To<ConfigModel>().ToSingleton();
            injectionBinder.Bind<IAuthModel>().To<AuthModel>().ToSingleton();
            injectionBinder.Bind<IComponentModel>().To<ComponentModel>().ToSingleton();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton();
            injectionBinder.Bind<TryLoginSignal>().To<TryLoginSignal>().ToSingleton();
            injectionBinder.Bind<LoggedInSignal>().To<LoggedInSignal>().ToSingleton();

            mediationBinder.Bind<LoginView>().To<LoginMediator>();

            //StartSignal is now fired instead of the START event.
            //Note how we've bound it "Once". This means that the mapping goes away as soon as the command fires.
            commandBinder.Bind<StartSignal>().To<StartCommand>().Once();
            commandBinder.Bind<StartConnectSignal>().To<ServerConnnectCommand>();
            commandBinder.Bind<ConnectedSignal>().To<ServerAuthCommand>();

            injectionBinder.Bind<FulfillWebServiceRequestSignal>().ToSingleton();
        }
    }
}

