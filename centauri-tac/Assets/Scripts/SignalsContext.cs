using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System;
using System.Reflection;
using System.Linq;
using strange.extensions.injector.api;

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
            var startSignal = injectionBinder.GetInstance<StartSignal>();
            startSignal.Dispatch();
            return this;
        }


        protected override void mapBindings()
        {
            injectionBinder.Bind<IDebugService>().To<UnityDebugService>().ToSingleton();
            injectionBinder.Bind<ICrossContextInjectionBinder>().To(injectionBinder);

            //bind up all the singleton signals
            //note that this will not inject any types that are bound so these should be plain data classes
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in assemblyTypes)
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

            //special injection of a direct view since the dispatcher service needs to be a mono behavior
            var signalDispatch = GameObject.FindObjectOfType<SignalDispatcherService>();
            injectionBinder.Bind<SignalDispatcherService>().To(signalDispatch).ToSingleton();

            injectionBinder.Bind<ComponentModel>().To<ComponentModel>().ToSingleton();
            injectionBinder.Bind<IJsonNetworkService>().To<JsonNetworkService>().ToSingleton();
            injectionBinder.Bind<ISocketService>().To<SocketService>().ToSingleton();
            injectionBinder.Bind<IMapCreatorService>().To<MapCreatorService>().ToSingleton();
            injectionBinder.Bind<IMapService>().To<MapService>().ToSingleton();


            //bind views
            mediationBinder.Bind<LoginView>().To<LoginMediator>();
            mediationBinder.Bind<TileHighlightView>().To<TileHighlightMediator>();
            mediationBinder.Bind<TileClickView>().To<TileClickMediator>();
            mediationBinder.Bind<MinionView>().To<MinionMediator>();
            mediationBinder.Bind<QuitView>().To<QuitMediator>();
            mediationBinder.Bind<EndTurnView>().To<EndTurnMediator>();
            mediationBinder.Bind<LeaveGameView>().To<LeaveGameMediator>();

            //bind signals to commands
            foreach (Type type in assemblyTypes.Where(x => x.Name.EndsWith("Signal")))
            {
                if(type.IsInterface) continue;
                if (type.GetCustomAttributes(typeof(ManualMapSignalAttribute), true).Length > 0)
                {
                    continue;
                }

                var commandName = type.Name.Replace("Signal", "Command");
                var commandType = assemblyTypes.Where(x => x.Name == commandName).FirstOrDefault();
                if (commandType != null)
                {
                    commandBinder.Bind(type).To(commandType);
                }
            }

            //StartSignal is now fired instead of the START event.
            //Note how we've bound it "Once". This means that the mapping goes away as soon as the command fires.

            commandBinder.Bind<StartSignal>().To<StartCommand>().Once();

            //commandBinder.Bind<FetchComponentsSignal>().To<FetchComponentsCommand>();
            commandBinder.Bind<ComponentsFetchedSignal>().To<ServerAuthCommand>();

            //commandBinder.Bind<TryLoginSignal>().To<TryLoginCommand>();
            commandBinder.Bind<LoggedInSignal>().To<ComponentLoggedInCommand>();
            commandBinder.Bind<AuthLoggedInSignal>().To<FetchPlayerCommand>();
            //commandBinder.Bind<TokenSignal>().To<TokenCommand>();
            commandBinder.Bind<PingSignal>().To<PongCommand>();
            commandBinder.Bind<PlayerFetchedSignal>().To<PlayerFetchedCommand>().To<AuthMatchmakerCommand>();

            commandBinder.Bind<GamelistLoggedInSignal>().To<FetchGamelistCommand>();
            commandBinder.GetBinding<GamelistLoggedInSignal>().To<GamelistCreateGameCommand>().Once();
            //commandBinder.Bind<GamelistGameSignal>().To<GamelistGameCommand>();

            commandBinder.Bind<MatchmakerLoggedInSignal>().To<MatchmakerQueueCommand>();

            commandBinder.Bind<CurrentGameSignal>().To<AuthGameCommand>();
            commandBinder.Bind<GameLoggedInSignal>().To<JoinGameCommand>();
            commandBinder.Bind<PlayerJoinedSignal>().To<StartGameCommand>();
            //commandBinder.Bind<PlayerConnectSignal>().To<PlayerConnectCommand>();
            //commandBinder.Bind<PlayerPartSignal>().To<PlayerPartCommand>();
            //commandBinder.Bind<LeaveGameSignal>().To<LeaveGameCommand>();

            //commandBinder.Bind<PieceDiedSignal>().To<PieceDiedCommand>();

            commandBinder.Bind<ActionPassTurnSignal>().To<EndTurnCommand>();
            commandBinder.Bind<ActionSpawnPieceSignal>().To<SpawnPieceCommand>();

            commandBinder.Bind<MoveMinionSignal>().To<MovePieceCommand>();
            commandBinder.Bind<ActionMovePieceSignal>().To<PieceMovedCommand>();

            commandBinder.Bind<AttackMinionSignal>().To<AttackPieceCommand>();
            commandBinder.Bind<ActionAttackPieceSignal>().To<PieceAttackedCommand>();

        }
    }
}

