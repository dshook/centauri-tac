//Whenever a piece takes damage or gets healed
export default class PieceHealthChange
{
  constructor(pieceId, change)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.change = change;

    this.newCurrentHealth = null;
  }
}
