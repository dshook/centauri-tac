/**
 * Attack dat
 */
export default class AttackPiece
{
  constructor(attackingPieceId, targetPieceId)
  {
    this.id = null;
    this.attackingPieceId = attackingPieceId;
    this.targetPieceId = targetPieceId;

    //resulting direction of the attacker
    this.direction = null;
  }
}
