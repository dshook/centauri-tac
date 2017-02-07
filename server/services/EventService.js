import {EventEmitter} from 'events';
import EmitterBinder from 'emitter-binder';
import loglevel from 'loglevel-decorator';

/**
 * Provide a transport instance to get things over HTTP
 */
export default class EventService
{
  constructor(app)
  {
    let emitter = new WrappedEmitter();
    app.registerInstance('emitter', emitter);
    let binder = new EmitterBinder(emitter);
    app.registerInstance('eventBinder', binder);
  }
}


@loglevel
class WrappedEmitter extends EventEmitter{
  emit(eventName, ...args){
    this.log.info('Emitted ' + eventName, ...args);
    super.emit(eventName, ...args);
  }
}