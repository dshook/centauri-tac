import GamePiece from '../models/GamePiece.js';
import CardBuff from '../actions/CardBuff.js';
import loglevel from 'loglevel-decorator';

/**
 * Attach or remove buffs to a card
 */
@loglevel
export default class CardBuffProcessor
{
  constructor(cardState)
  {
    this.cardState = cardState;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof CardBuff)) {
      return;
    }

    let card = this.cardState.card(action.cardId);

    if(!card){
      this.log.warn('Cannot find card to buff for id %s', action.cardId);
      return queue.cancel(action);
    }
    if(!action.removed && action.cost == null){
      this.log.warn('No attributes to change for card %s', action.cardId);
      return queue.cancel(action);
    }

    //if we're removing a buff, find it by name, pop it off, and then reverse its stat changes
    if(action.removed){
      let buff = card.buffs.find(b => b.name === action.name);
      if(!buff){
        this.log.warn('Cannot find buff %s to remove on card %j', action.name, card);
        return queue.cancel(action);
      }

      let buffChange = card.removeBuff(buff);

      if(!buffChange){
        this.log.error('Cannot unbuff card %j with buff %j', card, buff);
        return queue.cancel(action);
      }

      for(let buffKey in buffChange){
        action[buffKey] = buffChange[buffKey];
      }
      this.log.info('un buffing card %s to %j', card.id, buffChange);

    }else{

      let buffChange = card.addBuff(action);

      for(let buffKey in buffChange){
        action[buffKey] = buffChange[buffKey];
      }
      this.log.info('buffing card %s to %j', card.id, buffChange);

    }

    this.log.info('card %s buffed by %s to %s attack %s health %s movement',
      action.cardId, action.name, card.attack, card.health, card.movement);
    queue.complete(action);
  }
}
