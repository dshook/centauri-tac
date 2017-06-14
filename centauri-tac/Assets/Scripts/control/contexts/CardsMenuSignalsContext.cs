using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using ctac.signals;
using System.Reflection;

namespace ctac
{
    public class CardsMenuSignalsContext : MVCSContextBase
    {
        public CardsMenuSignalsContext(MonoBehaviour view) : base(view)
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

            var startSignal = injectionBinder.GetInstance<CardsMenuStartSignal>();
            startSignal.Dispatch();

            return this;
        }


        protected override void mapBindings()
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            BindViews(assemblyTypes);
            BindSignals(assemblyTypes);

            injectionBinder.Bind<ICardService>().To<CardService>().ToSingleton();
            commandBinder.Bind<CardsMenuStartSignal>().To<CardsMenuStartCommand>().Once();
        }
    }
}

