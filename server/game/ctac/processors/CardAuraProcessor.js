import CardAura from '../actions/CardAura.js';
import loglevel from 'loglevel-decorator';

/**
 * Attach an aura to a piece that affects cards, mostly handled in update aura processor
 */
@loglevel
export default class CardAuraProcessor
{
  constructor(pieceState)
  {
    this.pieceState = pieceState;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof CardAura)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to add card aura to for id %s', action.pieceId);
      return queue.cancel(action);
    }

    if(piece.aura){
      this.log.warn('Replacing card aura %j on piece %s', piece.aura, action.pieceId);
    }
    piece.aura = action;

    this.log.info('piece %s got card aura %s', action.pieceId, action.name);
    queue.complete(action);
  }
}
