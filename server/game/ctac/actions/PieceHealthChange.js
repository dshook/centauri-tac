import BaseAction from './BaseAction.js';

//Whenever a piece takes damage or gets healed
export default class PieceHealthChange extends BaseAction
{
  constructor({pieceId, change, bonus, bonusMsg, changeENumber, pieceSelectorParams, isHit, spellDamageBonus})
  {
    super();
    this.change = change;
    this.pieceId = pieceId || null;
    this.bonus = bonus || null;
    this.bonusMsg = bonusMsg || null;

    this.newCurrentHealth = null;
    this.newCurrentArmor = null;

    //props that are used to calculate change that should be deleted before sent to client
    this.changeENumber = changeENumber;
    this.pieceSelectorParams = pieceSelectorParams;
    this.isHit = isHit; //hit or heal
    this.spellDamageBonus = spellDamageBonus;
  }
}
