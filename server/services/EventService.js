import {EventEmitter} from 'events';
import EmitterBinder from 'emitter-binder';
import loglevel from 'loglevel-decorator';

/**
 * Provide a transport instance to get things over HTTP
 */
export default class EventService
{
  constructor(container)
  {
    let emitter = new WrappedEmitter();
    container.registerValue('emitter', emitter);
    let binder = new EmitterBinder(emitter);
    container.registerValue('eventBinder', binder);
  }
}


@loglevel
class WrappedEmitter extends EventEmitter{
  emit(eventName, ...args){
    this.log.info('Emitted ' + eventName, ...args);
    super.emit(eventName, ...args);
  }
}