import BaseAction from './BaseAction.js';

export default class DiscardCard extends BaseAction
{
  constructor(playerId)
  {
    super();
    this.playerId = playerId;
    this.cardId = null;
  }
}
