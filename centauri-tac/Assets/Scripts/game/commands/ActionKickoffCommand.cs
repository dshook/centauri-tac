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

        [Inject] public GameInputStatusModel gameInputStatus { get; set; }

        [Inject]
        public MessageSignal messageSignal { get; set; }
        
        public override void Execute()
        {
            if (!processedActions.Verify(kickoff.id)) return;

            gameInputStatus.inputEnabled = true;
            messageSignal.Dispatch( new MessageModel() { message = kickoff.message, duration = 1f });

        }
    }
}

