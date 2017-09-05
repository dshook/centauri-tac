using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionKickoffCommand : Command
    {
        [Inject] public KickoffModel kickoff { get; set; }
        [Inject] public SocketKey socketKey { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public MessageSignal messageSignal { get; set; }
        
        public override void Execute()
        {
            if (!processedActions.Verify(kickoff.id)) return;

            messageSignal.Dispatch( new MessageModel() { message = kickoff.message });

        }
    }
}

