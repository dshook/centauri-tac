import Position from '../models/Position.js';
/**
 * Plays a card
 */
export default class ActivateCard
{
  constructor(playerId, cardInstanceId, position, targetPieceId)
  {
    //which specific card was activated
    this.cardInstanceId = cardInstanceId;
    this.playerId = playerId;
    this.position = position ? new Position(position.x, position.y, position.z) : null;

    //what, if any, piece is targeted for this activation
    this.targetPieceId = targetPieceId;
  }
}
