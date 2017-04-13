import BaseAction from './BaseAction.js';

export default class CharmPiece extends BaseAction
{
  constructor({pieceId})
  {
    super();
    this.pieceId = pieceId;

    this.newPlayerId = null;
  }
}
