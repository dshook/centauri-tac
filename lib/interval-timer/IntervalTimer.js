import loglevel from 'loglevel-decorator';

@loglevel
export default class IntervalTimer{
  constructor(name, callback, interval){
    this.remaining = 0;
    this.state = 0; //  0 = idle, 1 = running, 2 = paused, 3= resumed

    this.name = name;
    this.interval = interval;
    this.callback = callback;
  }

  start(){
    this.log.info('Starting Timer ' + this.name);
    this.startTime = new Date();
    this.timerId = setInterval(this.callback, this.interval);
    this.state = 1;
  }

  pause() {
    if (this.state != 1) return;

    this.log.info('Pausing Timer ' + this.name);
    this.remaining = this.interval - (new Date() - this.startTime);
    clearInterval(this.timerId);
    this.state = 2;
  }

  resume() {
    if (this.state != 2) return;

    this.log.info('Resuming Timer ' + this.name);
    this.state = 3;
    setTimeout(this.timeoutCallback, this.remaining);
  }

  timeoutCallback() {
    if (this.state != 3) return;

    this.callback();

    this.start();
  }

  stop(){
    this.log.info('Stopping Timer ' + this.name);
    clearInterval(this.timerId);
    this.state = 0;
  }
}