import BaseAction from './BaseAction.js';

export default class SpawnDeck extends BaseAction
{
  constructor(playerId, race)
  {
    super();
    this.playerId = playerId;
    this.race = race;
    this.cards = null;
  }
}
