import UnsummonPiece from '../actions/UnsummonPiece.js';
import loglevel from 'loglevel-decorator';

//Move a piece back to the hand of the player that controls it
@loglevel
export default class UnsummonPieceProcessor
{
  constructor(pieceState, cardState, cardDirectory)
  {
    this.pieceState = pieceState;
    this.cardState = cardState;
    this.cardDirectory = cardDirectory;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof UnsummonPiece)) {
      return;
    }

    var piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to unsummon %j', action.pieceId, this.pieceState);
      return queue.cancel(action);
    }

    //remove from piecestate and add back to hand
    this.pieceState.remove(piece.id);

    let givenCard = this.cardDirectory.newFromId(piece.cardTemplateId);
    this.cardState.addToHand(piece.playerId, givenCard);

    action.cardId = givenCard.id;

    queue.complete(action);
    this.log.info('Unsummoed piece %s to player %s', piece.id, piece.playerId);
  }
}
