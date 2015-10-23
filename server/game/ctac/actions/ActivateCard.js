/**
 * Plays a card
 */
export default class ActivateCard
{
  constructor(playerId, cardId, position)
  {
    this.cardId = cardId;
    this.playerId = playerId;
    this.position = position;
  }
}
