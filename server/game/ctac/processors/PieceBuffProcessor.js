import GamePiece from '../models/GamePiece.js';
import PieceBuff from '../actions/PieceBuff.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceBuffProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceBuff)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to buff for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(action.attack == null && action.health == null && action.movement == null){
      this.log.warn('No attributes to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    let attribs = ['attack', 'health', 'movement'];

    for(let attrib of attribs){
      if(action[attrib] == null) continue;

      piece[attrib] += action[attrib];

      //update action with new values
      let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
      action[newAttrib] = piece[attrib];

      this.log.info('buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
    }

    piece.buffs.push(action);

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s buffed by %s to %s attack %s health %s movement',
      action.pieceId, action.name, piece.attack, piece.health, piece.movement);
    queue.complete(action);
  }
}
