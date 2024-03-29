import BaseAction from './BaseAction.js';

//choose wisely
export default class Choose extends BaseAction
{
  //lots o stuff to save and essentially proxy back to the choice evaluation
  constructor(
    choice1,
    choice2,
    selectedChoice,
    {
      playerId,
      activatingPiece,
      selfPiece,
      targetPieceId,
      position,
      pivotPosition
    }
  )
  {
    super();
    this.serverOnly = true;

    this.choice1 = choice1;
    this.choice2 = choice2;
    this.selectedChoice = selectedChoice;

    this.playerId = playerId;
    this.activatingPiece = activatingPiece;
    this.selfPiece = selfPiece;
    this.targetPieceId = targetPieceId;
    this.position = position;
    this.pivotPosition = pivotPosition;
  }
}
