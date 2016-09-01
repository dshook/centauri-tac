import BaseAction from './BaseAction.js';

export default class CardAura extends BaseAction
{
  constructor(pieceId, cardSelector, name)
  {
    super();
    this.serverOnly = true;

    this.pieceId = pieceId;
    this.cardSelector = cardSelector;
    this.name = name;

    //changes in stats, not abs amount
    this.cost = null;
  }
}
