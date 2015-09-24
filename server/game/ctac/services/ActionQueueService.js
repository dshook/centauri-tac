import ActionQueue from 'action-queue';

/**
 * Expose teh action queue and setup all the processors
 */
export default class ActionQueueService
{
  constructor(binder, app)
  {
    const queue = new ActionQueue(T => app.make(T));
    app.registerInstance('queue', queue);

    // wire up to event web
    binder.addEmitter(queue);
  }
}
