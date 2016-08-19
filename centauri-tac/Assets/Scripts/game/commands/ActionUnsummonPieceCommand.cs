using ctac.signals;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ctac
{
    public class ActionUnsummonPieceCommand : Command
    {
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public UnsummonPieceModel unsummonedPiece { get; set; }

        [Inject] public PieceUnsummonedSignal pieceUnsummoned { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GameTurnModel gameTurns { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject]
        public CardDrawnSignal cardDrawn { get; set; }

        [Inject]
        public CardDirectory cardDirectory { get; set; }


        [Inject]
        public PieceDiedSignal pieceDied { get; set; }
        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        [Inject]
        public IDebugService debug { get; set; }
        [Inject]
        public IPieceService pieceService { get; set; }
        [Inject]
        public IResourceLoaderService loader { get; set; }

        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }


        public override void Execute()
        {
            if (!processedActions.Verify(unsummonedPiece.id)) return;

            var piece = pieces.Piece(unsummonedPiece.pieceId);

            if (piece == null)
            {
                debug.LogWarning(string.Format("Could not find piece with id {0} to unsummon", unsummonedPiece.pieceId), socketKey);
                return;
            }

            pieceUnsummoned.Dispatch(unsummonedPiece);

            var pieceView = piece.gameObject.GetComponent<PieceView>();

            animationQueue.Add(
                new PieceView.UnsummonAnim()
                {
                    piece = pieceView,
                    pieceDied = pieceDied
                }
            );


            var newCardModel = cardDirectory.NewFromTemplate(unsummonedPiece.cardId, piece.cardTemplateId, piece.playerId);
            GameObject cardPrefab = loader.Load("Card");
            var DeckGO = GameObject.Find("Deck");


            var newCard = GameObject.Instantiate(
                cardPrefab,
                Constants.cardSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            newCard.transform.SetParent(DeckGO.transform, false);
            newCard.name = "Player " + piece.playerId + " Card " + unsummonedPiece.cardId;


            newCardModel.SetupGameObject(newCard);
            newCardModel.SetCardInPlay(contextView);

            cardDrawn.Dispatch(newCardModel);


            debug.Log(string.Format("Piece {0} charmed", unsummonedPiece.pieceId), socketKey);
        }
    }
}

