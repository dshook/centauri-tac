import Position from '../models/Position.js';
/**
 * Plays a card
 */
export default class ActivateCard
{
  constructor(playerId, cardId, position)
  {
    this.cardId = cardId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
  }
}
