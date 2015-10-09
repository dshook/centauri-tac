/**
 * Spawn a piece for a player
 */
export default class SpawnPiece
{
  constructor(playerId, pieceResourceId, position)
  {
    this.pieceResourceId = pieceResourceId;
    this.playerId = playerId;
    this.position = position;

    this.attack = null;
    this.health = null;
    this.pieceId = null;
  }
}
