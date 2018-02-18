import PieceBuff from '../actions/PieceBuff.js';
import loglevel from 'loglevel-decorator';

/**
 * Attach or remove buffs from a piece
 */
@loglevel
export default class PieceBuffProcessor
{
  constructor(pieceState, cardEvaluator, selector)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceBuff)) {
      return;
    }

    if(action.alreadyComplete){
      this.log.info('buff already complete');
      return queue.complete(action);
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to buff for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(!action.removed
      && action.attack == null
      && action.health == null
      && action.movement == null
      && action.range == null
      && action.spellDamage == null
      && !action.addStatus
      && !action.removeStatus
    ){
      this.log.warn('No attributes to change for piece %s, buff %j', action.pieceId, action);
      return queue.cancel(action);
    }

    //if we're removing a buff, find it by name, pop it off, and then reverse its stat changes
    //We find it by name here because that's how you specify to find it in the language.
    //All buffs with the same name should be doing the exact same thing so even if a piece has multiple buffs
    //with the same name, it shouldn't matter which of them we remove
    if(action.removed){
      let buff = piece.buffs.find(b => b.name === action.name);
      if(!buff){
        this.log.warn('Cannot find buff %s to remove on piece %j', action.name, piece);
        return queue.cancel(action);
      }

      let buffChange = piece.removeBuff(buff, this.cardEvaluator);

      if(!buffChange){
        this.log.error('Cannot unbuff piece %j with buff %j', piece, buff);
        return queue.cancel(action);
      }

      for(let buffKey in buffChange){
        action[buffKey] = buffChange[buffKey];
      }
      this.log.info('un buffing piece %s to %j', piece.id, buffChange);

    }else{

      let buffChange = piece.addBuff(action, this.cardEvaluator);

      //if the buff has a condition and the condition is not met then disable it immediately
      //and don't send it to the client
      if(action.condition){
        let buffConditionResult = this.selector.compareExpression(
          action.condition,
          this.pieceState.pieces,
          {
            selfPiece: piece,
            controllingPlayerId: piece.playerId
          }
          , this.selector.selectPieces
        );
        if(buffConditionResult.length === 0){
          this.log.info("piece %s didn't meet buff condition", piece.id, buffChange);
          piece.disableBuff(action);
          action.serverOnly = true;
        }
      }

      for(let buffKey in buffChange){
        action[buffKey] = buffChange[buffKey];
      }
      this.log.info('buffing piece %s to %j', piece.id, buffChange);

    }

    action.statuses = piece.statuses;

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s buffed by %s to %s attack %s health %s movement',
      action.pieceId, action.name, piece.attack, piece.health, piece.movement);
    queue.complete(action);
  }
}
