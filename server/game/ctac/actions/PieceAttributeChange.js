//Whenever a piece's stats change, which is an unsilencable effect
//Note that this will change the base stats as well
export default class PieceAttributeChange
{
  constructor(pieceId)
  {
    this.id = null;
    this.pieceId = pieceId;

    this.attack = null;
    this.health = null;
    this.movement = null;

    //set by processor
    this.baseAttack = null;
    this.baseHealth = null;
    this.baseMovement = null;
  }
}