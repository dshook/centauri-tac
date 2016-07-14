import GamePiece from '../models/GamePiece.js';
import AttachCode from '../actions/AttachCode.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle pieces losing or gaining armor
 */
@loglevel
export default class AttachCodeProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }

  async handleAction(action, queue)
  {
    if (!(action instanceof AttachCode)) {
      return;
    }

    let piece = this.pieceState.piece(action.pieceId);

    if(!piece){
      this.log.warn('Cannot find piece to attach code for id %s', action.pieceId);
      return queue.cancel(action);
    }
    if(!action.eventList || action.eventList.length === 0){
      this.log.warn('No code to attach for %s', action.pieceId);
      return queue.cancel(action);
    }

    if(piece.events === null){
      piece.events = [];
    }
    //merge all events in action onto already set up piece
    //noting that we can only append to actions when there aren't any event args since they won't be compatible
    for(let event of action.eventList){
      let existingEvent = piece.events.find(e => e.event === event.event);
      if(existingEvent && !existingEvent.args){
        existingEvent.actions = existingEvent.actions.concat(event.actions);
      }else{
        piece.events.push(event);
      }
    }

    this.log.info('piece %s got new code for event(s) [%s]',
      action.pieceId, action.eventList.map(e => e.event).join(','));
    queue.complete(action);
  }
}
