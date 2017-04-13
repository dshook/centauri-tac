import BaseAction from './BaseAction.js';

//Whenever a card is buffed, or had a buff removed
export default class CardBuff extends BaseAction
{
  constructor({cardId, name, removed, auraPieceId, cost})
  {
    super();
    this.id = null;
    this.cardId = cardId;
    this.name = name;
    this.removed = removed || false;
    this.auraPieceId = auraPieceId || null;

    //changes in stats, not abs amount
    this.cost = cost || null;

    //new values updated by proccessor
    this.newCost = null;
  }
}
