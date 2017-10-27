using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ServerMessageMediator : Mediator
    {
        [Inject] public ServerMessageView view { get; set; }
        [Inject] public ActionMessageSignal actionMessage { get; set; }
        [Inject] public MessageSignal message { get; set; }
        
        [Inject] public ISoundService sounds { get; set; }

        public override void OnRegister()
        {
            actionMessage.AddListener(onActionMessage);
            message.AddListener(onMessage);
            view.init();
        }

        public override void onRemove()
        {
            actionMessage.RemoveListener(onActionMessage);
            message.RemoveListener(onMessage);
        }

        private void onMessage(MessageModel message)
        {
            view.updateText(message.message, message.duration);
            sounds.PlaySound("error");
        }

        private void onActionMessage(MessageModel message, SocketKey key)
        {
            view.updateText(message.message, message.duration ?? 1f);
            sounds.PlaySound("error");
        }

    }
}

