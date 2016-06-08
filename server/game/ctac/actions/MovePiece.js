import Position from '../models/Position.js';
/**
 * Action that attemps to move a game piece one step
 */
export default class MovePiece
{
  constructor(pieceId, to, isTeleport)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.to = new Position(to.x, to.y, to.z);
    this.isTeleport = isTeleport || false;

    this.direction = null;
  }
}
