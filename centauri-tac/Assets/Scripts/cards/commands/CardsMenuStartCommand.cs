using strange.extensions.command.impl;

namespace ctac
{
    public class CardsMenuStartCommand : Command
    {
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            debug.Log("Starting Cards Menu");

        }
    }
}

