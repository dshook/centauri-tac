import metaDecorator from 'meta-decorator';
import {getMeta} from 'meta-decorator';
import loglevel from 'loglevel-decorator';

export const dispatch = {
  on: metaDecorator('on'),
};

/**
 * Class used to facilitate eventing across components via RPC and the net
 * client
 */
@loglevel
export default class RPCMessenger
{
  constructor(netClient)
  {
    this.net = netClient;
    this.instances = new Set();
  }

  /**
   * Send something to the dispatcher to broadcast to all listening clients
   */
  async emit(event, data)
  {
    await this.net.sendCommand('dispatch', 'broadcast', {event, data});
  }

  /**
   * Bind instance with meta annotations to to emitted events. No unbind since
   * this will be happening on the server
   */
  bindInstance(instance)
  {
    const meta = getMeta(instance);

    const onBindings = meta.filter(x => x.property === 'on');

    if (!onBindings.length) {
      return false;
    }

    // wire up for when we recv events (only needs to happen once)
    for (const b of onBindings) {
      const fName = b.name;
      const [eventFilter] = b.args;

      this.net.on('command', ({command, params }) => {
        if (command !== 'broadcast') {
          return;
        }

        const {event, data} = params;

        if (event !== eventFilter) {
          return;
        }

        instance[fName](data);
      });

      this.log.info('on %s -> %s::%s',
          eventFilter, instance.constructor.name, fName);
    }

    // push this instance into our set so we can subscribe when the dispatch is
    // ready
    this.instances.add(instance);

    // signal we have bound things
    return true;
  }

  /**
   * Actually push our subsriptions to dispatch
   */
  async subscribe()
  {
    for (const instance of this.instances) {
      const meta = getMeta(instance);
      const onBindings = meta.filter(x => x.property === 'on');

      for (const b of onBindings) {
        const [event] = b.args;
        await this.net.sendCommand('dispatch', 'subscribe', event);
      }
    }
  }
}
