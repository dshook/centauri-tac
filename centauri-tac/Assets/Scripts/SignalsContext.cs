using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System;
using System.Reflection;
using System.Linq;

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
            //bind up all the singleton signals
            //note that this will not inject any types that are bound so these should be plain data classes
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttributes(typeof(SingletonAttribute), true).Length > 0)
                {
                    var interfaceName = "I" + type.Name;
                    var implementedInterface = type.GetInterfaces().Where(x => x.Name == interfaceName).FirstOrDefault();
                    if (implementedInterface != null)
                    {
                        injectionBinder.Bind(implementedInterface).To(Activator.CreateInstance(type)).ToSingleton();
                    }
                    else
                    {
                        injectionBinder.Bind(type).To(Activator.CreateInstance(type)).ToSingleton();
                    }
                }
            }

            injectionBinder.Bind<IComponentModel>().To<ComponentModel>().ToSingleton();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton();
            injectionBinder.Bind<ISocketService>().Named(InjectKeys.authSocketService).To(new SocketService());
            injectionBinder.Bind<ISocketService>().Named(InjectKeys.gameSocketService).To(new SocketService());
            injectionBinder.Bind<IMapCreatorService>().To<MapCreatorService>().ToSingleton();


            mediationBinder.Bind<LoginView>().To<LoginMediator>();
            mediationBinder.Bind<TileHighlightView>().To<TileHighlightMediator>();
            mediationBinder.Bind<TileClickView>().To<TileClickMediator>();
            mediationBinder.Bind<MinionView>().To<MinionMediator>();

            //StartSignal is now fired instead of the START event.
            //Note how we've bound it "Once". This means that the mapping goes away as soon as the command fires.
            commandBinder.Bind<StartSignal>().To<StartCommand>().Once();
            commandBinder.Bind<StartConnectSignal>().To<ServerConnnectCommand>();
            commandBinder.Bind<ConnectedSignal>().To<ServerAuthCommand>();
            commandBinder.Bind<LoggedInSignal>().To<FetchPlayerCommand>();

            injectionBinder.Bind<FulfillWebServiceRequestSignal>().ToSingleton();
        }
    }
}

