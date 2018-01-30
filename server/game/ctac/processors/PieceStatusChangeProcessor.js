import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';
import attributes from '../util/Attributes.js';

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

    let statusChanges = null;
    if(action.add){
      statusChanges = piece.addStatuses(action.add, this.cardEvaluator);
    }
    if(action.remove){
      //remove can't change attributes for now
      piece.removeStatuses(action.remove, this.cardEvaluator);
    }

    if(statusChanges){
      if(statusChanges.addStatus){ action.add = statusChanges.addStatus; }
      if(statusChanges.removeStatus){ action.remove = statusChanges.removeStatus; }

      for(let attrib of attributes){
        if(!statusChanges[attrib]) continue;

        let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
        action[attrib] = statusChanges[attrib];
        action[newAttrib] = statusChanges[newAttrib];

        this.log.info('un buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
      }
    }

    action.statuses = piece.statuses;

    if(piece.health <= 0){
      this.cardEvaluator.evaluatePieceEvent('death', piece);
      this.pieceState.remove(piece.id);
    }

    this.log.info('changed piece %s statuses added %s removed %s, result %s',
      action.pieceId,
      this.printStatuses(action.add),
      this.printStatuses(action.remove),
      this.printStatuses(piece.statuses)
    );
    queue.complete(action);
  }

  printStatuses(statuses){
    if(!statuses) return null;
    let ret = [];
    let statusKeys = Object.keys(Statuses);
    for(let key of statusKeys){
      if(statuses & Statuses[key]){
        ret.push(key);
      }
    }
    return ret.join(' ');
  }
}
