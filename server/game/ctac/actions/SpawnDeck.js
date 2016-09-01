import BaseAction from './BaseAction.js';

export default class SpawnDeck extends BaseAction
{
  constructor(playerId, startingPlayerId, initialDrawAmount)
  {
    super();
    this.playerId = playerId;
    this.startingPlayerId = startingPlayerId;
    this.initialDrawAmount = initialDrawAmount;
    this.cards = null;
  }
}
