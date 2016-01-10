import Position from '../models/Position.js';

export default class PlaySpell
{
  constructor(playerId, cardId, position)
  {
    this.cardId = cardId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
  }
}
