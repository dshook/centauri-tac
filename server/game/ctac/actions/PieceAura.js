import BaseAction from './BaseAction.js';

//Attaching an aura to a piece
export default class PieceAura extends BaseAction
{
  constructor(pieceId, pieceSelector, name)
  {
    super();
    this.id = null;
    this.pieceId = pieceId;
    this.pieceSelector = pieceSelector;
    this.name = name;

    //changes in stats, not abs amount
    this.attack = null;
    this.health = null;
    this.movement = null;
    this.range = null;
  }
}
