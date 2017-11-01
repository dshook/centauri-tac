using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ServerMessageMediator : Mediator
    {
        [Inject] public ServerMessageView view { get; set; }
        
        [Inject] public ISoundService sounds { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        [ListensTo(typeof(MessageSignal))]
        public void onMessage(MessageModel message)
        {
            view.updateText(message.message, message.duration);
            //sounds.PlaySound("error");
        }

        [ListensTo(typeof(ActionMessageSignal))]
        public void onActionMessage(MessageModel message, SocketKey key)
        {
            view.updateText(message.message, message.duration ?? 1f);
            //sounds.PlaySound("error");
        }

    }
}

