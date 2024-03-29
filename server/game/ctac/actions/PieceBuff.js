import BaseAction from './BaseAction.js';

//Whenever a piece is buffed, or had a buff removed
export default class PieceBuff extends BaseAction
{
  constructor({
    buffId,
    pieceId,
    name,
    removed,
    auraPieceId,
    condition,
    attack,
    health,
    movement,
    range,
    spellDamage,
    addStatus,
    removeStatus,
    buffAttributes
  })
  {
    super();
    this.buffId = buffId;
    this.pieceId = pieceId;
    this.name = name;
    this.removed = removed || false;
    this.auraPieceId = auraPieceId || null;

    //compare expression that tells when the buff should be enabled or not
    this.condition = condition || null;
    this.enabled = true;

    //statuses to add or remove
    this.addStatus = addStatus;
    this.removeStatus = removeStatus;

    //final statuses updated by processor
    this.statuses = null;

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

    //The raw array of buff attributes that can be re-evaluated if they're eNumbers
    this.buffAttributes = buffAttributes || [];
  }
}
