//Whenever a piece takes damage or gets healed
export default class PieceHealthChange
{
  constructor(pieceId, change, bonus, bonusMsg)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.change = change;
    this.bonus = bonus || null;
    this.bonusMsg = bonusMsg || null;

    this.newCurrentHealth = null;
  }
}
