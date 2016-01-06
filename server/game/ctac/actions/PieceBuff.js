//Whenever a piece is buffed, which can be silenced
export default class PieceBuff
{
  constructor(pieceId, name)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.name = name;

    //changes in stats, not abs amount
    this.attack = null;
    this.health = null;
    this.movement = null;

    //new values updated by proccessor
    this.newAttack = null;
    this.newHealth = null;
    this.newMovement = null;
  }
}
