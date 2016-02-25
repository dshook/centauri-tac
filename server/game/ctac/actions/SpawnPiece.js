import Position from '../models/Position.js';
import Direction from '../models/Direction.js';
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

    //default to spawn south for now
    this.direction = Direction.South;

    this.pieceId = null;
    this.tags = null;
  }
}
