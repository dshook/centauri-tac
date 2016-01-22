using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class ServerMessageMediator : Mediator
    {
        [Inject]
        public ServerMessageView view { get; set; }

        [Inject]
        public ActionMessageSignal message { get; set; }

        public override void OnRegister()
        {
            message.AddListener(onMessage);
            view.init();
        }

        public override void onRemove()
        {
            message.RemoveListener(onMessage);
        }

        private void onMessage(MessageModel message, SocketKey key)
        {
            view.updateText(message.message, message.duration ?? 1f);
        }

    }
}

