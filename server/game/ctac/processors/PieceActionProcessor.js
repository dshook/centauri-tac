import PieceAction from '../actions/PieceAction.js';
import loglevel from 'loglevel-decorator';

/**
 * Run the selector and create an action for each selected piece
 */
@loglevel
export default class PieceActionProcessor
{
  constructor(selector, pieceState)
  {
    this.selector = selector;
    this.pieceState = pieceState;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceAction)) {
      return;
    }

    let additionalActions = [];
    if(action.selector && action.pieceSelectorParams){

      let selected = this.selector.selectPieces(action.selector, action.pieceSelectorParams);
      this.log.info('Selected %s pieces for action %s', selected.length, action.actionClass.name);
      this.pieceState.lastSelectedPieces = selected.map(p => p.id) || [];
      if(selected && selected.length > 0){
        for(let s of selected){
          let actionParams = Object.assign({}, action.actionParams, {pieceId: s.id});
          let newAction = new action.actionClass(actionParams);
          newAction.activatingPieceId = action.activatingPieceId;

          additionalActions.push(newAction);
        }
      }
    }else{
      this.log.error('Cannot expand piece action without selector or selector params')
      return queue.cancel(action);
    }

    queue.complete(action);

    queue.pushFront(additionalActions);
  }
}
