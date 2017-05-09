import ActionQueue from 'action-queue';
import NoOpProcessor from '../processors/NoOpProcessor.js';

/**
 * Expose teh action queue
 */
export default class ActionQueueService
{
  constructor(binder, container)
  {
    const queue = new ActionQueue(T => container.new(T));
    container.registerValue('queue', queue);

    // wire up to event web
    binder.addEmitter(queue);
    //add a catch all processor
    queue.addProcessor(NoOpProcessor);
  }
}
