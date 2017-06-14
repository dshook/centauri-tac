using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class CardsMenuStartCommand : Command
    {
        [Inject] public IDebugService debug { get; set; }
        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public CardsKickoffSignal cardKickoff { get; set; }

        public override void Execute()
        {
            debug.Log("Starting Cards Menu");
            cardDirectory.LoadCards();

            cardKickoff.Dispatch();
        }
    }
}

