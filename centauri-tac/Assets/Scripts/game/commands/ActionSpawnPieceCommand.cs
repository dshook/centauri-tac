using strange.extensions.command.impl;
using ctac.signals;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class ActionSpawnPieceCommand : Command
    {
        [Inject] public SpawnPieceModel spawnedPiece { get; set; }
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public CardActivatedSignal cardActivatedSignal { get; set; }
        [Inject] public PieceSpawnedSignal pieceSpawned { get; set; }

        [Inject] public ActionsProcessedModel processedActions { get; set; }

        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public CardsModel cards { get; set; }

        [Inject] public CardDirectory cardDirectory { get; set; }
        [Inject] public ICardService cardService { get; set; }
        [Inject] public IPieceService pieceService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        public override void Execute()
        {
            if (!processedActions.Verify(spawnedPiece.id)) return;

            //First see if we need to activate the card that spawned this piece
            if(spawnedPiece.cardInstanceId.HasValue){
                var cardActivated = cardService.ActivateCardInstance(cards, cardDirectory, spawnedPiece.cardInstanceId.Value, spawnedPiece.cardTemplateId, 0);

                cardActivatedSignal.Dispatch(cardActivated);
            }

            //search for phantom piece that was created when targeting started to update props from real spawn event
            var phantomPiece = pieces.Pieces.FirstOrDefault(p => p.tags.Contains(Constants.targetPieceTag));

            PieceModel pieceModel = null;

            if (phantomPiece != null)
            {
                //update props from real spawn event
                pieceModel = phantomPiece;

                //should be a NOOP but just in case...
                pieceModel.gameObject.transform.position = map.tiles[spawnedPiece.position.Vector2].fullPosition;
                pieceModel.gameObject.name = "Piece " + spawnedPiece.pieceId;
                pieceModel.id = spawnedPiece.pieceId.Value;
                pieceModel.playerId = spawnedPiece.playerId;
                pieceModel.cardTemplateId = spawnedPiece.cardTemplateId;
                pieceModel.tags = spawnedPiece.tags;

            }
            else
            {
                pieceModel = pieceService.CreatePiece(spawnedPiece);
            }

            pieceSpawned.Dispatch(new PieceSpawnedModel(){ 
                spawnPieceAction = spawnedPiece, 
                piece = pieceModel, 
                alreadyDeployed = phantomPiece != null,
            });
            debug.Log(string.Format("Spawned piece {0} for player {1}", spawnedPiece.cardTemplateId, spawnedPiece.playerId), socketKey);
        }
    }
}

