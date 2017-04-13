import BaseAction from './BaseAction.js';

//Attaching an aura to a piece
export default class PieceAura extends BaseAction
{
  constructor({pieceId, pieceSelector, name, attack, health, movement, range, spellDamage})
  {
    super();
    this.id = null;
    this.pieceId = pieceId;
    this.pieceSelector = pieceSelector;
    this.name = name;

    //changes in stats, not abs amount
    this.attack      = attack      || null;
    this.health      = health      || null;
    this.movement    = movement    || null;
    this.range       = range       || null;
    this.spellDamage = spellDamage || null;
  }
}
