/**
 * Attack dat
 */
export default class AttackPiece
{
  constructor(attackingPieceId, targetPieceId, isTauntAttack = false)
  {
    this.id = null;
    this.attackingPieceId = attackingPieceId;
    this.targetPieceId = targetPieceId;
    this.isTauntAttack = isTauntAttack;

    //resulting direction of the attacker
    this.direction = null;
  }
}
