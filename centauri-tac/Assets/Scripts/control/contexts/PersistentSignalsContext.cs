using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System.Reflection;
using strange.extensions.injector.api;

namespace ctac
{
    public class PersistentSignalsContext : MVCSContextBase
    {
        public PersistentSignalsContext(MonoBehaviour view) : base(view)
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

            //no start signal dispatching here, should be done in a scene specific context
            return this;
        }


        protected override void mapBindings()
        {
            var contextViewGo = base.contextView as GameObject;
            injectionBinder.Bind<GameObject>().To(contextViewGo).ToName(InjectionKeys.PersistentSignalsRoot).CrossContext();

            injectionBinder.Bind<IDebugService>().To<UnityDebugService>().ToSingleton().CrossContext();
            injectionBinder.Bind<ICrossContextInjectionBinder>().To(injectionBinder).CrossContext();

            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            BindSingletons(assemblyTypes);

            //special injection of a direct view since the dispatcher service needs to be a mono behavior
            var signalDispatch = GameObject.FindObjectOfType<SignalDispatcherService>();
            injectionBinder.Bind<SignalDispatcherService>().To(signalDispatch).ToSingleton().CrossContext();

            injectionBinder.Bind<ComponentModel>().To<ComponentModel>().ToSingleton().CrossContext();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton().CrossContext();
            injectionBinder.Bind<ISocketService>().To<SocketService>().ToSingleton().CrossContext();
            injectionBinder.Bind<IResourceLoaderService>().To<ResourceLoaderService>().ToSingleton().CrossContext();

            commandBinder.Bind<PingSignal>().To<PongCommand>();
        }

        protected override void postBindings()
        {
            GameObject.DontDestroyOnLoad(base.contextView as GameObject);
        }
    }
}

