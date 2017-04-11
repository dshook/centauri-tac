import BaseAction from './BaseAction.js';

//Whenever a piece takes damage or gets healed
export default class PieceHealthChange extends BaseAction
{
  constructor({pieceId, change, bonus, bonusMsg})
  {
    super();
    this.change = change;
    this.pieceId = pieceId || null;
    this.bonus = bonus || null;
    this.bonusMsg = bonusMsg || null;

    this.newCurrentHealth = null;
    this.newCurrentArmor = null;
  }
}
