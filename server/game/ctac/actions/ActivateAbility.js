export default class ActivateAbility
{
  constructor(pieceId, targetPieceId)
  {
    this.pieceId = pieceId;

    //what, if any, piece is targeted for this activation
    this.targetPieceId = targetPieceId;
  }
}
