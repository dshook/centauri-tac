import BaseAction from './BaseAction.js';

export default class ShuffleToDeck extends BaseAction
{
  constructor(playerId, cardTemplateId)
  {
    super();
    this.playerId = playerId;
    this.cardTemplateId = cardTemplateId;

    this.cardId = null;
  }
}
