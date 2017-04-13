import BaseAction from './BaseAction.js';

//Whenever a piece gets flat up destroyed, and didn't die from taking damage
export default class PieceDestroyed extends BaseAction
{
  constructor({pieceId})
  {
    super();
    this.pieceId = pieceId;
  }
}
