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
  }
}
