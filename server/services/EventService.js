import {EventEmitter} from 'events';
import EmitterBinder from 'emitter-binder';

/**
 * Provide a transport instance to get things over HTTP
 */
export default class EventService
{
  constructor(app)
  {
    let emitter = new EventEmitter();
    app.registerInstance('emitter', emitter);
    let binder = new EmitterBinder(emitter);
    app.registerInstance('eventBinder', binder);
  }
}
