import BaseAction from './BaseAction.js';

//Whenever a piece's stats change, which is an unsilencable effect
//Note that this will change the base stats as well
export default class PieceAttributeChange extends BaseAction
{
  constructor({pieceId, attack, health, movement, range, spellDamage})
  {
    super();
    this.pieceId = pieceId;

    this.attack      = attack      || null;
    this.health      = health      || null;
    this.movement    = movement    || null;
    this.range       = range       || null;
    this.spellDamage = spellDamage || null;

    //set by processor
    this.baseAttack      = null;
    this.baseHealth      = null;
    this.baseMovement    = null;
    this.baseRange       = null;
    this.baseSpellDamage = null;
  }
}
