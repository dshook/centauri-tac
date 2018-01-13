import BaseAction from './BaseAction.js';
import Position from '../models/Position.js';

export default class PlaySpell extends BaseAction
{
  constructor(playerId, cardInstanceId, cardTemplateId, position, targetPieceId, pivotPosition, chooseCardTemplateId)
  {
    super();
    this.cardInstanceId = cardInstanceId;
    this.cardTemplateId = cardTemplateId;
    this.playerId = playerId;
    this.position = position ? new Position(position.x, position.y, position.z) : null;
    this.targetPieceId = targetPieceId;
    this.pivotPosition = pivotPosition ? new Position(pivotPosition.x, pivotPosition.y, pivotPosition.z) : null;
    this.chooseCardTemplateId = chooseCardTemplateId;

    //Gets filled out by the server telling you how much spell damage was present when the card was played
    this.spellDamage = 0;
  }
}
