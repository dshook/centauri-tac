import Position from '../models/Position.js';

export default class PlaySpell
{
  constructor(playerId, cardTemplateId, position)
  {
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
  }
}
