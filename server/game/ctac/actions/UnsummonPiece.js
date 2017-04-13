import BaseAction from './BaseAction.js';

export default class UnsummonPiece extends BaseAction
{
  constructor({pieceId})
  {
    super();
    this.pieceId = pieceId;

    //new card id for returned to hand
    this.cardId = null;
  }
}
