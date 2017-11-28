import BaseAction from './BaseAction.js';

export default class SpawnDeck extends BaseAction
{
  constructor(playerId)
  {
    super();
    this.playerId = playerId;
    this.cards = null;
  }
}
