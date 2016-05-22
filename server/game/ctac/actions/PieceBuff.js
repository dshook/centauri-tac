//Whenever a piece is buffed, or had a buff removed
export default class PieceBuff
{
  constructor(pieceId, name, removed = false)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.name = name;
    this.removed = removed;

    //changes in stats, not abs amount
    this.attack = null;
    this.health = null;
    this.movement = null;
    this.range = null;

    //new values updated by proccessor
    this.newAttack = null;
    this.newHealth = null;
    this.newMovement = null;
    this.newRange = null;
  }
}
