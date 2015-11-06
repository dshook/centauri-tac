import Position from '../models/Position.js';
/**
 * Spawn a piece for a player
 */
export default class SpawnPiece
{
  constructor(playerId, cardId, position)
  {
    this.cardId = cardId;
    this.playerId = playerId;
    this.position = new Position(position.x, position.y, position.z);

    this.pieceId = null;
  }
}
