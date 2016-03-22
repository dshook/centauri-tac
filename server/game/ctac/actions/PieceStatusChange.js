/**
 * Whenever a piece loses or gains a status
 */
export default class PieceStatusChange
{
  constructor(pieceId, add, remove)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.add = add;
    this.remove = remove;

    this.statuses = null;

    //new values updated by proccessor, null meaning no change
    this.newAttack = null;
    this.newHealth = null;
    this.newMovement = null;
  }
}
