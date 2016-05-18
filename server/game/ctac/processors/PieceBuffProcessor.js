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
    if(!action.removed && action.attack == null && action.health == null && action.movement == null){
      this.log.warn('No attributes to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    let attribs = ['attack', 'health', 'movement', 'range'];
    //if we're removing a buff, find it by name, pop it off, and then reverse its stat changes
    if(action.removed){
      let buff = piece.buffs.find(b => b.name === action.name);
      if(!buff){
        this.log.warn('Cannot find buff %s to remove on piece %j', action.name, piece);
        return queue.cancel(action);
      }

      let buffChange = piece.removeBuff(buff);

      if(!buffChange){
        this.log.error('Cannot unbuff piece %j with buff %j', piece, buff);
        return queue.cancel(action);
      }

      for(let attrib of attribs){
        let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
        action[attrib] = buffChange[attrib];
        action[newAttrib] = buffChange[newAttrib];

        this.log.info('un buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
      }

    }else{

      for(let attrib of attribs){
        if(action[attrib] == null) continue;

        piece[attrib] += action[attrib];

        //update action with new values
        let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
        action[newAttrib] = piece[attrib];

        this.log.info('buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
      }

      piece.buffs.push(action);
    }

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s buffed by %s to %s attack %s health %s movement',
      action.pieceId, action.name, piece.attack, piece.health, piece.movement);
    queue.complete(action);
  }
}
