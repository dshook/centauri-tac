import TransformPiece from '../actions/TransformPiece.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class TransformPieceProcessor
{
  constructor(cardDirectory, pieceState, cardEvaluator)
  {
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof TransformPiece)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to transform %j', action.pieceId, this.pieceState);
      return queue.cancel(action);
    }

    let transformCard = null;
    if(action.cardTemplateId){
      transformCard = this.cardDirectory.directory[action.cardTemplateId];
      if(transformCard && transformCard.tags.includes('Minion')){
        this.log.info('Transforming piece into card %s', transformCard.name);

        this.pieceState.copyPropertiesFromCard(piece, transformCard);
        this.pieceState.setInitialMoveAttackStatus(piece);

        //Fire off the play minion event for the newly transformed piece to ensure things like aura's are set up
        this.cardEvaluator.evaluatePieceEvent('playMinion', piece, {position: piece.position});
      }
    }

    let transformPiece = null;
    if(action.transformPieceId){
      transformPiece = this.pieceState.piece(action.transformPieceId);
      if(!transformPiece){
        this.log.warn('Could not find piece %s to transform into %j', action.transformPieceId, this.pieceState);
        return queue.cancel(action);
      }

      this.pieceState.copyPropertiesFromPiece(transformPiece, piece);
      this.pieceState.setInitialMoveAttackStatus(piece);
    }

    action.updatedPiece = piece;

    this.cardEvaluator.updateTransformedPiece(piece, transformPiece);

    //TOOD: update card eval with copied turn timers

    queue.complete(action);
    this.log.info('Transformed piece');
  }
}
