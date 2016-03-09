/**
 * Whenever a piece loses or gains a status
 */
export default class PieceStatusChange
{
  constructor(pieceId, statuses)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.statuses = statuses;
  }
}
