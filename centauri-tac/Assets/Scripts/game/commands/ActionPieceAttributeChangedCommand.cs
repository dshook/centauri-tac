using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public class ActionPieceAttributeChangedCommand : Command
    {
        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public PieceAttributeChangeModel pieceChanged { get; set; }

        [Inject]
        public PieceAttributeChangedSignal attribChanged { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        [Inject]
        public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(pieceChanged.id)) return;

            var piece = pieces.Piece(pieceChanged.pieceId);

            piece.health = pieceChanged.health ?? piece.health;
            piece.baseHealth = pieceChanged.baseHealth ?? piece.baseHealth;

            piece.attack = pieceChanged.attack ?? piece.attack;
            piece.baseAttack = pieceChanged.baseAttack ?? piece.baseAttack;

            piece.movement = pieceChanged.health ?? piece.movement;
            piece.baseMovement = pieceChanged.baseMovement ?? piece.baseMovement;

            attribChanged.Dispatch(pieceChanged);

            debug.Log( string.Format("Piece {0} changed attribs", pieceChanged.pieceId), socketKey );
        }
    }
}

