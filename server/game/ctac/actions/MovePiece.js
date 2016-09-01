import BaseAction from './BaseAction.js';
import Position from '../models/Position.js';
/**
 * Action that attemps to move a game piece one step
 * If isJump is true though, means it was triggered from an action
 * And can then jump multiple spaces & bypass some checks
 * Is Teleport is a purely client side animation flag
 */
export default class MovePiece extends BaseAction
{
  constructor(pieceId, to, isJump = false, isTeleport = false)
  {
    super();
    this.pieceId = pieceId;
    this.to = new Position(to.x, to.y, to.z);
    this.isJump = isJump;
    this.isTeleport = isTeleport;

    this.direction = null;
  }
}
