import {getMeta} from 'sock-harness';
import loglevel from 'loglevel-decorator';

/**
 * Bind and unbind other class instances to events from the net client
 */
@loglevel
export default class ClientBinder
{
  constructor(net)
  {
    this._net = net;
    this._commandBindings = new WeakMap();
    this._connectBindings = new WeakMap();
    this._disconnectBindings = new WeakMap();
  }

  /**
   * Connect instance methods to events
   */
  bindInstance(instance)
  {
    this._bindCommands(instance);

    const cBindings = this._bindEvents(instance, 'connected', 'open');
    this._connectBindings.set(instance, cBindings);

    const dBindings = this._bindEvents(instance, 'disconnected', 'close');
    this._disconnectBindings.set(instance, dBindings);
  }

  /**
   * Disconenct instance methods from events
   */
  unbindInstance(instance)
  {
    const comBindings = this._commandBindings.get(instance);
    comBindings.forEach(x => this._net.removeListener('command', x));
    this.log.info('unbound %s command methods', comBindings.length);

    const cBindings = this._connectBindings.get(instance);
    cBindings.forEach(x => this._net.removeListener('open', x));
    this.log.info('unbound %s connect methods', cBindings.length);

    const dBindings = this._disconnectBindings.get(instance);
    dBindings.forEach(x => this._net.removeListener('close', x));
    this.log.info('unbound %s disconnect methods', dBindings.length);
  }

  /**
   * Wire @rpc.command calls
   */
  _bindCommands(instance)
  {
    const meta = getMeta(instance);

    const commands = meta.filter(x => x.property === 'command');

    const cbs = [];

    for (const b of commands) {

      const fName = b.name;
      const [componentFilter, commandFilter] = b.args;

      // Handle callback from net client that will check to see if our
      // conditions are met and foward it on to the instance
      const cb = ({name, client, command, params}) => {
        if (name !== componentFilter || command !== commandFilter) {
          return;
        }
        instance[fName](client, params);
      };

      this._net.on('command', cb);
      cbs.push(cb);

      this.log.info('%s@%s -> %s::%s',
          commandFilter, componentFilter, instance.constructor.name, fName);
    }

    // Create a weak mapping from this instance to all the CBs we created to
    // unmap them later (kinda gross)
    this._commandBindings.set(instance, cbs);
  }

  /**
   * @wire @rpc.* cll
   */
  _bindEvents(instance, property, eventName)
  {
    const meta = getMeta(instance);

    const bindings = meta.filter(x => x.property === property);

    const cbs = [];

    for (const b of bindings) {
      const fName = b.name;
      const [componentFilter] = b.args;

      const cb = ({name, client}) => {
        if (name === componentFilter) {
          instance[fName](client);
        }
      };

      this._net.on(eventName, cb);
      cbs.push(cb);

      this.log.info('[%s]@%s -> %s::%s',
          eventName, componentFilter, instance.constructor.name, fName);
    }

    return cbs;
  }
}
