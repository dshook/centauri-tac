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
  constructor(){
    super();
    this.reportInterval = 10;
    this.emitCount = 0;
    this.interval = setInterval(() => this.report(), this.reportInterval * 1000);
  }
  emit(eventName, ...args){
    this.log.info('Emitted ' + eventName, ...args);
    this.emitCount++;
    super.emit(eventName, ...args);
  }

  report(){
    if(this.emitCount === 0) return;
    this.log.info('Emitted %s events in %s seconds, %s eps', this.emitCount, this.reportInterval, this.emitCount / this.reportInterval );
    this.emitCount = 0;
  }
}