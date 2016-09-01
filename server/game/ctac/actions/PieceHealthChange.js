import BaseAction from './BaseAction.js';

//Whenever a piece takes damage or gets healed
export default class PieceHealthChange extends BaseAction
{
  constructor(pieceId, change, bonus, bonusMsg)
  {
    super();
    this.pieceId = pieceId;
    this.change = change;
    this.bonus = bonus || null;
    this.bonusMsg = bonusMsg || null;

    this.newCurrentHealth = null;
    this.newCurrentArmor = null;
  }
}
