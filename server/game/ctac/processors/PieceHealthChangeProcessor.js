import GamePiece from '../models/GamePiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceHealthChangeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceHealthChange)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change health on for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(action.change == 0){
      this.log.warn('No health to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    let hpBeforeChange = piece.health;

    //check for shield and nullify damage
    if((piece.statuses & Statuses.Shield) == Statuses.Shield){
      action.change = 0;
      action.bonus = 0;
      queue.push(new PieceStatusChange(piece.id, null, Statuses.Shield));
    }

    let healthChange = action.change + (action.bonus || 0);
    let remainingArmor = 0;
    //take off armor first on damage
    if(piece.armor > 0 && healthChange < 0){
      remainingArmor = piece.armor + healthChange;
      if(remainingArmor < 0){
        piece.armor = 0;
        healthChange = remainingArmor;
      }else{
        piece.armor = remainingArmor;
        healthChange = 0;
      }
    }

    piece.health = piece.health + healthChange;

    //cap hp at base health and adjust action change amounts
    let maxHp = piece.maxBuffedHealth;
    if(piece.health > maxHp){
      action.change = maxHp - hpBeforeChange;
      piece.health = maxHp;
    }
    action.newCurrentHealth = piece.health;
    action.newCurrentArmor = piece.armor;

    if(healthChange < 0){
      this.cardEvaluator.evaluatePieceEvent('damaged', piece);
    }

    if(healthChange > 0){
      this.cardEvaluator.evaluatePieceEvent('healed', piece);
    }

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('piece %s %s %s health, now %s',
      action.pieceId, (action.change > 0 ? 'gained' : 'lost'), action.change, action.newCurrentHealth);
    queue.complete(action);
  }
}
