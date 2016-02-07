import Position from '../models/Position.js';

export default class PlaySpell
{
  constructor(playerId, cardInstanceId, cardTemplateId, position, targetPieceId)
  {
    this.cardInstanceId = cardInstanceId;
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
    this.targetPieceId = targetPieceId;
  }
}
