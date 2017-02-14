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
        public GamePlayersModel players { get; set; }

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
            if (possibleActions.metConditions.ContainsKey(players.Me.id) 
                && possibleActions.metConditions[players.Me.id] != null)
            {
                foreach (var metCondition in possibleActions.metConditions[players.Me.id])
                {
                    var card = cards.Card(metCondition.cardId);
                    if (card != null)
                    {
                        card.metCondition = true;
                        //kinda nasty, probably will chage with introduction of glow though
                        card.cardView.UpdateText(possibleActions.GetSpellDamage(players.Me.id));
                    }
                }
            }

            possibleActionsReceived.Dispatch(newPossibleActions);
        }
    }
}

