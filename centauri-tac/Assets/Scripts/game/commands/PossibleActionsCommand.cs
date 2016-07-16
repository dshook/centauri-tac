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
        public PiecesModel pieces { get; set; }

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

            possibleActionsReceived.Dispatch(newPossibleActions);
        }
    }
}

