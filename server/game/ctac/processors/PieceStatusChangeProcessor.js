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

    if(action.add){
      piece.statuses = piece.statuses | action.add;
    }
    if(action.remove){
      piece.statuses = piece.statuses & ~action.remove;
    }

    //remove all statuses other than silence if it was silenced
    if(action.add & Statuses.Silence){
      piece.statuses = Statuses.Silence;

      //back out any buffs on the piece
      if(piece.buffs.length > 0){

        for(let b = piece.buffs.length - 1; b >= 0; b--){
          let buff = piece.buffs[b];
          let buffChange = piece.removeBuff(buff);

          if(!buffChange){
            this.log.error('Cannot unbuff piece %j with buff %j', piece, buff);
            continue;
          }

          for(let attrib of attributes){
            let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
            action[attrib] = buffChange[attrib];
            action[newAttrib] = buffChange[newAttrib];

            this.log.info('un buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
          }
        }
      }

      //aura and events go bye bye
      piece.aura = null;
      piece.events = null;

      //remove timers for this piece
      this.cardEvaluator.cleanupTimers(piece);
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
