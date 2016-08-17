using strange.extensions.mediation.impl;
using ctac.signals;

namespace ctac
{
    public class DeckHoverMediator : Mediator
    {
        [Inject]
        public DeckHoverView view { get; set; }
        
        [Inject] public RaycastModel raycastModel { get; set; }

        public override void OnRegister()
        {
        }

        public override void onRemove()
        {
        }

        void Update()
        {
            var hoverHit = raycastModel.cardCanvasHit;

            //check to see if a card in a deck has been hovered
            //also check to see if the hit object has been destroyed in the meantime
            if (hoverHit.HasValue 
                && hoverHit.Value.collider != null
                && hoverHit.Value.collider.gameObject.transform.parent.name == "OpponentDeck"
            )
            {
            }
            else
            {
            }

            if (hoverHit.HasValue 
                && hoverHit.Value.collider != null
                && hoverHit.Value.collider.gameObject.transform.parent.name == "Deck"
            )
            {
            }
            else
            {
            }
        }
    }
}

