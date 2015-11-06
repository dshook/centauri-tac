/**
 * Spawn a piece for a player
 */
export default class SpawnPiece
{
  constructor(playerId, cardId, position)
  {
    this.cardId = cardId;
    this.playerId = playerId;
    this.position = position;

    this.pieceId = null;
  }
}
