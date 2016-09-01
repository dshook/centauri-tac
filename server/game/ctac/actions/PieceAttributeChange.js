import BaseAction from './BaseAction.js';

//Whenever a piece's stats change, which is an unsilencable effect
//Note that this will change the base stats as well
export default class PieceAttributeChange extends BaseAction
{
  constructor(pieceId)
  {
    super();
    this.pieceId = pieceId;

    this.attack = null;
    this.health = null;
    this.movement = null;
    this.range = null;

    //set by processor
    this.baseAttack = null;
    this.baseHealth = null;
    this.baseMovement = null;
    this.baseRange = null;
  }
}
