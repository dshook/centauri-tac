export default class DrawCard
{
  constructor(playerId)
  {
    this.cardId = null;
    this.cardTemplateId = null;
    this.playerId = playerId;

    this.milled = false;
  }
}
