using ctac.signals;
using strange.extensions.command.impl;

namespace ctac
{
    public class ActionCharmPieceCommand : Command
    {
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public CharmPieceModel charmedPiece { get; set; }

        [Inject] public PieceCharmedSignal pieceCharmed { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GameTurnModel gameTurns { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(charmedPiece.id)) return;

            var piece = pieces.Piece(charmedPiece.pieceId);
            piece.playerId = charmedPiece.newPlayerId;
            piece.currentPlayerHasControl = gameTurns.currentPlayerId == piece.playerId;
            piece.hasAttacked = true;
            piece.hasMoved = true;

            pieceCharmed.Dispatch(charmedPiece);

            debug.Log(string.Format("Piece {0} charmed", charmedPiece.pieceId), socketKey);
        }
    }
}

