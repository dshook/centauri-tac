using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System;
using System.Reflection;
using System.Linq;
using strange.extensions.signal.impl;

namespace ctac
{
    public class GameSignalsContext : MVCSContextBase
    {
        public GameSignalsContext(MonoBehaviour view) : base(view)
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

            var contextViewGo = base.contextView as GameObject;
            var signalRoot = contextViewGo == null ? null : contextViewGo.GetComponent<GameSignalsRoot>();

            if (signalRoot == null || String.IsNullOrEmpty(signalRoot.startSignalName)){
                var startSignal = injectionBinder.GetInstance<StartSignal>();
                startSignal.Dispatch();
            } else {
                var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
                var startType = assemblyTypes.FirstOrDefault(t => t.Name == signalRoot.startSignalName);
                if (startType == null)
                {
                    throw new Exception("Could not find type for start signal " + signalRoot.startSignalName);
                }
                if (startType.BaseType != typeof(Signal)) {
                    throw new Exception("Start Signal is not of base type Signal");
                }

                var startSignal = injectionBinder.GetInstance(startType);
                (startSignal as Signal).Dispatch();
            }
            return this;
        }


        protected override void mapBindings()
        {
            var contextViewGo = base.contextView as GameObject;
            injectionBinder.Bind<GameObject>().To(contextViewGo).ToName(InjectionKeys.GameSignalsRoot);

            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            injectionBinder.Bind<IMapCreatorService>().To<MapCreatorService>().ToSingleton();
            injectionBinder.Bind<IMapService>().To<MapService>().ToSingleton();
            injectionBinder.Bind<ICardService>().To<CardService>().ToSingleton();
            injectionBinder.Bind<IPieceService>().To<PieceService>().ToSingleton();

            BindViews(assemblyTypes);
            BindSignals(assemblyTypes);

            commandBinder.Bind<StartSignal>().To<StartCommand>().Once();
            commandBinder.Bind<PiecesStartSignal>().To<PiecesStartCommand>().Once();

            //signals for the standalone debug game launch
            commandBinder.Bind<PlayerFetchedSignal>().To<PlayerFetchedCommand>().To<AuthLobbyFromGameCommand>(); 

            commandBinder.Bind<CurrentGameSignal>().To<AuthGameCommand>();
            commandBinder.Bind<GameLoggedInSignal>().To<JoinGameCommand>();
            commandBinder.Bind<PlayerJoinedSignal>().To<StartGameCommand>();

        }
    }
}

