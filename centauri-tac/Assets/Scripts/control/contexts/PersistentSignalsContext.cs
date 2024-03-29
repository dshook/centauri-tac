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
            //check to see if this is a duplicate persistent context (from loading another scene into this one)
            //if it is, destroy this one before it gets a chance to wreck havok
            var contexts = GameObject.FindGameObjectsWithTag("PersistentContext");
            if (contexts.Length > 1)
            {
                GameObject.DestroyImmediate(base.contextView as GameObject);
                return null;
            }

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
            BindSingletons(assemblyTypes, typeof(SingletonAttribute), true);

            //special injection of a direct view since the dispatcher service needs to be a mono behavior
            var signalDispatch = GameObject.FindObjectOfType<SignalDispatcherService>();
            injectionBinder.Bind<SignalDispatcherService>().To(signalDispatch).ToSingleton().CrossContext();

            injectionBinder.Bind<ComponentModel>().To<ComponentModel>().ToSingleton().CrossContext();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton().CrossContext();
            injectionBinder.Bind<ISocketService>().To<SocketService>().ToSingleton().CrossContext();
            injectionBinder.Bind<IResourceLoaderService>().To<ResourceLoaderService>().ToSingleton().CrossContext();
            injectionBinder.Bind<ISoundService>().To<SoundService>().ToSingleton().CrossContext();

            commandBinder.Bind<PingSignal>().To<PongCommand>();

            //this should be moved to a lobby specific context at some point I think
            injectionBinder.Bind<LobbyModel>().To<LobbyModel>().ToSingleton().CrossContext();
        }

        protected override void postBindings()
        {
            GameObject.DontDestroyOnLoad(base.contextView as GameObject);
        }
    }
}

