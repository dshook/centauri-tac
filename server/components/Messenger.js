/**
 * Class used to facilitate eventing across components
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
  bindInstance()
  {
    // const meta = [];

    // for (const b in meta) {
    //   this.net.on('command', ({command, params}) => {

    //     if (command !== 'broadcast') {
    //       return;
    //     }

    //     const {event, data} = params;

    //     if (event === b.on) {
    //       f
    //     }

    //   });
    // }
  }
}
