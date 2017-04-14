import ActionTimer from '../actions/ActionTimer.js';
import loglevel from 'loglevel-decorator';

//Add turn timers to card eval
@loglevel
export default class ActionTimerProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof ActionTimer)) {
      return;
    }

    //copy just to make sure nuttin funky happens modifying it after the fact
    let saved = [].concat(this.pieceState.lastSelectedPieces);
    let timerArray = action.isStartTimer ? this.cardEvaluator.startTurnTimers : this.cardEvaluator.endTurnTimers;

    //if the start turn timer action includes a target piece Id and nothing else is saving pieces,
    //use the target for the saved pieces
    if(!saved.length && action.pieceSelectorParams.targetPieceId){
      saved = [action.pieceSelectorParams.targetPieceId];
    }
    let attachedPiece = action.piece;
    if(!attachedPiece && action.pieceSelectorParams.targetPieceId){
      attachedPiece = {id: action.pieceSelectorParams.targetPieceId};
    }

    if(action.timerActions && action.timerActions.length > 0){
      for(let timerAction of action.timerActions){
        timerArray.push({
          saved,
          piece: attachedPiece,
          card: action.card,
          playerId: action.playerId,
          interval: action.interval,
          timer: action.timer,
          timerAction: timerAction
        });
      }
    }
    queue.complete(action);
    this.log.info('Added %s %s turn timer for player %s',
      action.timerActions.length, action.isStartTimer ? 'start' : 'end', action.playerId);
  }
}
