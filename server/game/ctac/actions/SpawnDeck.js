import BaseAction from './BaseAction.js';

export default class SpawnDeck extends BaseAction
{
  constructor(playerId, initialDrawAmount)
  {
    super();
    this.playerId = playerId;
    this.initialDrawAmount = initialDrawAmount;
    this.cards = null;
  }
}
