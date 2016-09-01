import BaseAction from './BaseAction.js';

export default class DrawCard extends BaseAction
{
  constructor(playerId)
  {
    super();
    this.cardId = null;
    this.cardTemplateId = null;
    this.playerId = playerId;

    this.milled = false;
    this.overdrew = false;
  }
}
