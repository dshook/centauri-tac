using strange.extensions.mediation.impl;
using ctac.signals;
using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public class CardHoverTipMediator : Mediator
    {
        [Inject] public CardHoverTipView view { get; set; }

        [Inject] public IResourceLoaderService loader { get; set; }

        public override void OnRegister()
        {
            view.init(loader);
        }

        [ListensTo(typeof(CardHoverTipSignal))]
        public void onCardHoverTip(CardView cardView)
        {
            if(cardView != null){
                view.EnableHoverTips(cardView);
            }else{
                view.DisableHoverTips();
            }
        }

    }
}

