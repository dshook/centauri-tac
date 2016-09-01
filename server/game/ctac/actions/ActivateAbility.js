import BaseAction from './BaseAction.js';

export default class ActivateAbility extends BaseAction
{
  constructor(pieceId, targetPieceId)
  {
    super();
    this.pieceId = pieceId;

    //what, if any, piece is targeted for this activation
    this.targetPieceId = targetPieceId;
  }
}
