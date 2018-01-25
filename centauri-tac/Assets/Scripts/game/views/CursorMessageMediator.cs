using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class CursorMessageMediator : Mediator
    {
        [Inject] public CursorMessageView view { get; set; }

        public override void OnRegister()
        {
            view.init();
        }

        [ListensTo(typeof(CursorMessageSignal))]
        public void onMessage(MessageModel message)
        {
            if(message == null){
                view.updateText(null, null);
            }else{
                view.updateText(message.message, message.duration);
            }
        }

    }
}

