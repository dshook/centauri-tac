import GamePiece from '../models/GamePiece.js';
import PieceAttributeChange from '../actions/PieceAttributeChange.js';
import loglevel from 'loglevel-decorator';
import attributes from '../util/Attributes.js';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceAttributeChangeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceAttributeChange)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change attributes on for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(action.attack == null && action.health == null && action.movement == null){
      this.log.warn('No attributes to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    for(let attrib of attributes){
      if(action[attrib] == null || piece[attrib] === action[attrib]) continue;

      piece[attrib] = action[attrib];

      //set base as well
      let baseattrib = 'base' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
      piece[baseattrib] = action[attrib];
      action[baseattrib] = action[attrib];
      this.log.info('setting piece %s %s to %s %s', piece.id, attrib, action[attrib]);
    }

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s changed attribs to %s attack %s health %s movement',
      action.pieceId, piece.attack, piece.health, piece.movement);
    queue.complete(action);
  }
}
