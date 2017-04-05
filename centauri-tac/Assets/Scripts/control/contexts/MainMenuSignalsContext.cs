using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System.Reflection;

namespace ctac
{
    public class MainMenuSignalsContext : MVCSContextBase
    {
        public MainMenuSignalsContext(MonoBehaviour view) : base(view)
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

            var startSignal = injectionBinder.GetInstance<MainMenuStartSignal>();
            startSignal.Dispatch();

            return this;
        }


        protected override void mapBindings()
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            BindViews(assemblyTypes);
            BindSignals(assemblyTypes);

            commandBinder.Bind<MainMenuStartSignal>().To<MainMenuStartCommand>().Once();

            commandBinder.Bind<LoggedInSignal>().To<ComponentLoggedInCommand>();
            commandBinder.Bind<AuthLoggedInSignal>().To<FetchPlayerCommand>();
            commandBinder.Bind<PlayerFetchedSignal>().To<PlayerFetchedCommand>(); 

            commandBinder.Bind<GamelistLoggedInSignal>().To<FetchGamelistCommand>();
            commandBinder.GetBinding<GamelistLoggedInSignal>().To<GamelistCreateGameCommand>().Once();

            commandBinder.Bind<CurrentGameSignal>().To<AuthGameCommand>();
            commandBinder.Bind<PlayerJoinedSignal>().To<StartGameCommand>();

        }
    }
}

