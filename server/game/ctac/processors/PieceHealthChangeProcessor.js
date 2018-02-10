import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceHealthChangeProcessor
{
  constructor(pieceState, cardEvaluator, selector)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;
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
    if(!action.change && action.changeENumber){
      //calculate heal or hit based on evaluating the eNum
      action.change = this.selector.eventualNumber(action.changeENumber, action.pieceSelectorParams) + action.spellDamageBonus;
      if(action.isHit){ action.change *= -1; }

      //cleanup server action stuff
      delete action.changeENumber;
      delete action.pieceSelectorParams;
      delete action.isHit;
      delete action.spellDamageBonus;
    }
    if(action.change == 0){
      this.log.warn('No health to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    let hpBeforeChange = piece.health;
    let removeShield = false;

    //check for shield and nullify damage
    if(action.change < 0 && (piece.statuses & Statuses.Shield) == Statuses.Shield){
      action.change = 0;
      action.bonus = 0;
      removeShield = true;
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
    if(removeShield){
      queue.pushFront(new PieceStatusChange({pieceId: piece.id, remove: Statuses.Shield}));
    }
    //Remove cloak if they took damage
    if(piece.health > 0 && piece.statuses & Statuses.Cloak){
      queue.pushFront(new PieceStatusChange({pieceId: piece.id, remove: Statuses.Cloak}));
    }
  }
}
