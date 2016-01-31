import Position from '../models/Position.js';
/**
 * Spawn a piece for a player
 */
export default class SpawnPiece
{
  constructor(playerId, cardTemplateId, position, targetPieceId)
  {
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);
    this.targetPieceId = targetPieceId;

    this.pieceId = null;
    this.tags = null;
  }
}
