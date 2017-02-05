import {EventEmitter} from 'events';
import EmitterBinder from 'emitter-binder';

/**
 * Provide a transport instance to get things over HTTP
 */
export default class EventService
{
  constructor(app)
  {
    app.registerSingleton('eventEmitter', EventEmitter);
    app.registerSingleton('eventBinder', EmitterBinder);
  }
}
