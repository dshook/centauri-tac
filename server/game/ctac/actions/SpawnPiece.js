import Position from '../models/Position.js';
/**
 * Spawn a piece for a player
 */
export default class SpawnPiece
{
  constructor(playerId, cardInstanceId, cardTemplateId, position, targetPieceId)
  {
    this.cardInstanceId = cardInstanceId;
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
    this.targetPieceId = targetPieceId;

    this.pieceId = null;
    this.tags = null;
  }
}
