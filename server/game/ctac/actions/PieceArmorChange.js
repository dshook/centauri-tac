import BaseAction from './BaseAction.js';

export default class PieceArmorChange extends BaseAction
{
  constructor(pieceId, change)
  {
    super();
    this.pieceId = pieceId;
    this.change = change;

    this.newArmor = null;
  }
}
