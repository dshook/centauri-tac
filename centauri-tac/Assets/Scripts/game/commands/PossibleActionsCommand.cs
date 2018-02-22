using ctac.signals;
using strange.extensions.command.impl;
using System.Linq;

namespace ctac
{
    public class PossibleActionsCommand : Command
    {
        [Inject] public PossibleActions newPossibleActions { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }
        [Inject] public PossibleActionsReceivedSignal possibleActionsReceived { get; set; }

        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public CardsModel cards { get; set; }
        [Inject] public IResourceLoaderService loader { get; set; }

        [Inject] public AnimationQueueModel animationQueue { get; set; }

        public override void Execute()
        {
            possibleActions.Update(newPossibleActions);

            foreach (var piece in pieces.Pieces)
            {
                var eventedPiece = possibleActions.eventedPieces.FirstOrDefault(ep => ep.pieceId == piece.id);

                Statuses adding = Statuses.None;
                Statuses removing = Statuses.None;
                if (eventedPiece != null)
                {
                    if (eventedPiece.@event == "d" && !piece.hasDeathEvent)
                    {
                        piece.hasDeathEvent = true;
                        adding = Statuses.hasDeathEvent;
                    }
                    else if (eventedPiece.@event != "d" && !piece.hasEvent)
                    {
                        piece.hasEvent = true;
                        adding = Statuses.hasEvent;
                    }
                }
                else
                {
                    if (piece.hasDeathEvent)
                    {
                        removing = Statuses.hasDeathEvent;
                        piece.hasDeathEvent = false;
                    }
                    if (piece.hasEvent)
                    {
                        removing = Statuses.hasEvent;
                        piece.hasEvent = false;
                    }
                }

                var newStatuses = piece.statuses;
                FlagsHelper.Set(ref newStatuses, adding);
                FlagsHelper.Unset(ref newStatuses, removing);
                piece.statuses = newStatuses;

                if (adding != Statuses.None || removing != Statuses.None)
                {
                    animationQueue.Add(
                        new PieceView.ChangeStatusAnim()
                        {
                            piece = piece.pieceView,
                            loader = loader,
                            pieceStatusChange = new PieceStatusChangeModel() { add = adding, remove = removing, statuses = piece.statuses }
                        }
                    );
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

            //only send out actions for the current player right now
            if (newPossibleActions.playerId == players.Me.id)
            {
                possibleActionsReceived.Dispatch(newPossibleActions);
            }
        }
    }
}

