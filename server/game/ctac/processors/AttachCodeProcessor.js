import AttachCode from '../actions/AttachCode.js';
import loglevel from 'loglevel-decorator';

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

    //Add the code to the piece events. It should be fine for the piece to have duplicated events, each will get evaluated and
    //run when the event happens
    for(let event of action.eventList){
      piece.events.push(event);
    }

    this.log.info('piece %s got new code for event(s) [%s]',
      action.pieceId, action.eventList.map(e => e.event).join(','));
    queue.complete(action);
  }
}
