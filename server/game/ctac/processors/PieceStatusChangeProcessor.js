import GamePiece from '../models/GamePiece.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';
import {fromInt} from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle pieces losing or gaining their current health
 */
@loglevel
export default class PieceStatusChangeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof PieceStatusChange)) {
      return;
    }
    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to change statuses on for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(!action.add && !action.remove){
      this.log.warn('No statuses to change for piece %s', action.pieceId);
      return queue.cancel(action);
    }

    if(action.add){
      let adds = [].concat(action.add);
      piece.statuses = Array.from(new Set([...piece.statuses, ...adds]));
    }
    if(action.remove){
      let removes = [].concat(action.remove);
      piece.statuses = _.without(piece.statuses, ...removes);
    }

    action.statuses = piece.statuses;

    this.log.info('changed piece %s statuses added %j removed %j, result %j',
      action.pieceId,
      this.printStatuses(action.add),
      this.printStatuses(action.remove),
      this.printStatuses(piece.statuses)
    );
    queue.complete(action);
  }

  printStatuses(statuses){
    if(!statuses) return null;
    return [].concat(statuses).map(s => fromInt(s));
  }
}
