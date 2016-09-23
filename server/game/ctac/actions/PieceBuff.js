import BaseAction from './BaseAction.js';

//Whenever a piece is buffed, or had a buff removed
export default class PieceBuff extends BaseAction
{
  constructor(pieceId, name, removed = false, auraPieceId = null)
  {
    super();
    this.id = null;
    this.pieceId = pieceId;
    this.name = name;
    this.removed = removed;
    this.auraPieceId = auraPieceId;

    //compare expression that tells when the buff should be enabled or not
    this.condition = null;
    this.enabled = true;

    //changes in stats, not abs amount
    this.attack      = null;
    this.health      = null;
    this.movement    = null;
    this.range       = null;
    this.spellDamage = null;

    //new values updated by proccessor
    this.newAttack      = null;
    this.newHealth      = null;
    this.newMovement    = null;
    this.newRange       = null;
    this.newSpellDamage = null;
  }
}
