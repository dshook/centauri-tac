import metaDecorator from 'meta-decorator';
import {getMeta} from 'meta-decorator';

export const dispatch = {
  on: metaDecorator('on'),
};

/**
 * Class used to facilitate eventing across components via RPC and the net
 * client
 */
export default class Messenger
{
  constructor(netClient)
  {
    this.net = netClient;
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
    }
  }
}
