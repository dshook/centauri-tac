import Position from '../models/Position.js';

export default class PlaySpell
{
  constructor(playerId, cardInstanceId, cardTemplateId, position, targetPieceId, pivotPosition, chooseCardTemplateId)
  {
    this.cardInstanceId = cardInstanceId;
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = position ? new Position(position.x, position.y, position.z) : null;
    this.targetPieceId = targetPieceId;
    this.pivotPosition = pivotPosition ? new Position(pivotPosition.x, pivotPosition.y, pivotPosition.z) : null;
    this.chooseCardTemplateId = chooseCardTemplateId;
  }
}
