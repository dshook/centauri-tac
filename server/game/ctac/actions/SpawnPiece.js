import BaseAction from './BaseAction.js';
import Position from '../models/Position.js';
import Direction from '../models/Direction.js';
/**
 * Spawn a piece for a player
 */
export default class SpawnPiece extends BaseAction
{
  constructor({
    playerId,
    cardTemplateId,
    position,
    cardInstanceId,
    targetPieceId,
    direction,
    pivotPosition,
    chooseCardTemplateId,
    spawnKingRadius
  })
  {
    super();
    this.playerId = playerId;
    this.cardTemplateId = cardTemplateId;
    this.position = new Position(position.x, position.y, position.z);

    this.cardInstanceId = cardInstanceId || null;
    this.targetPieceId = targetPieceId;
    this.direction = direction || Direction.South;

    this.pivotPosition = pivotPosition ? new Position(pivotPosition.x, pivotPosition.y, pivotPosition.z) : null;
    this.chooseCardTemplateId = chooseCardTemplateId;
    this.spawnKingRadius = spawnKingRadius;

    this.pieceId = null;
    this.tags = null;
  }
}
