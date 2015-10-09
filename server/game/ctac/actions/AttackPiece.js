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

    this.attackerNewHp = null;
    this.targetNewHp = null;
  }
}
