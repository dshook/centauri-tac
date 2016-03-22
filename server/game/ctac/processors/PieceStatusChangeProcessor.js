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
        let attribs = ['attack', 'health', 'movement'];

        for(let b = piece.buffs.length - 1; b >= 0; b--){
          let buff = piece.buffs[b];

          for(let attrib of attribs){
            if(buff[attrib] == null) continue;

            piece[attrib] -= buff[attrib];

            //cap at min of 0 to prevent negative attack/movement
            //TODO: check for death
            piece[attrib] = Math.max(0, piece[attrib]);

            //update action with new values
            let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
            action[newAttrib] = piece[attrib];

            this.log.info('un buffing piece %s to %s %s', piece.id, piece[attrib], attrib);
          }
        }
        piece.buffs = [];
      }
    }

    action.statuses = piece.statuses;

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
