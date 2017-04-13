import BaseAction from './BaseAction.js';

//Whenever a piece is buffed, or had a buff removed
export default class PieceBuff extends BaseAction
{
  constructor({
    pieceId,
    name,
    removed,
    auraPieceId,
    condition,
    attack,
    health,
    movement,
    range,
    spellDamage
  })
  {
    super();
    this.id = null;
    this.pieceId = pieceId;
    this.name = name;
    this.removed = removed || false;
    this.auraPieceId = auraPieceId || null;

    //compare expression that tells when the buff should be enabled or not
    this.condition = condition || null;
    this.enabled = true;

    //changes in stats, not abs amount
    this.attack      = attack      || null;
    this.health      = health      || null;
    this.movement    = movement    || null;
    this.range       = range       || null;
    this.spellDamage = spellDamage || null;

    //new values updated by proccessor
    this.newAttack      = null;
    this.newHealth      = null;
    this.newMovement    = null;
    this.newRange       = null;
    this.newSpellDamage = null;
  }
}
