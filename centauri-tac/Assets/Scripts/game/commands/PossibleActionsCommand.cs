using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class PossibleActionsCommand : Command
    {
        [Inject]
        public PossibleActions newPossibleActions { get; set; }

        [Inject]
        public PossibleActionsModel possibleActions { get; set; }

        [Inject]
        public PossibleActionsReceivedSignal possibleActionsReceived { get; set; }

        [Inject]
        public GameTurnModel turns { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        public override void Execute()
        {
            possibleActions.Update(newPossibleActions);

            //update evented pieces without anim for now
            foreach (var piece in pieces.Pieces)
            {
                piece.hasDeathEvent = false;
                piece.hasEvent = false;
            }

            foreach (var eventedPiece in possibleActions.eventedPieces)
            {
                var piece = pieces.Piece(eventedPiece.pieceId);
                if (eventedPiece.@event == "d") {
                    piece.hasDeathEvent = true;
                }
                else
                {
                    piece.hasEvent = true;
                }
            }

            //update met condition cards
            foreach (var card in cards.Cards)
            {
                card.metCondition = false;
            }
            if (possibleActions.metConditions.ContainsKey(turns.currentPlayerId) 
                && possibleActions.metConditions[turns.currentPlayerId] != null)
            {
                foreach (var metCondition in possibleActions.metConditions[turns.currentPlayerId])
                {
                    var card = cards.Card(metCondition.cardId);
                    if (card != null)
                    {
                        card.metCondition = true;
                        //kinda nasty, probably will chage with introduction of glow though
                        card.cardView.UpdateText(possibleActions.GetSpellDamage(turns.currentPlayerId));
                    }
                }
            }

            possibleActionsReceived.Dispatch(newPossibleActions);
        }
    }
}

