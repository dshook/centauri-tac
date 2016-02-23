import Position from '../models/Position.js';
import Direction from '../models/Direction.js';
/**
 * Action that attemps to move a game piece one step
 */
export default class MovePiece
{
  constructor(pieceId, to)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.to = new Position(to.x, to.y, to.z);

    this.direction = null;
  }
}
