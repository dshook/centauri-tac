import ActionQueue from 'action-queue';
import NoOpProcessor from '../processors/NoOpProcessor.js';

/**
 * Expose teh action queue
 */
export default class ActionQueueService
{
  constructor(binder, app)
  {
    const queue = new ActionQueue(T => app.make(T));
    app.registerInstance('queue', queue);

    // wire up to event web
    binder.addEmitter(queue);
    //add a catch all processor
    queue.addProcessor(NoOpProcessor);
  }
}
