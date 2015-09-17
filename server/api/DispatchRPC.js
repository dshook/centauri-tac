import {rpc} from 'sock-harness';
import {EventEmitter} from 'events';
import loglevel from 'loglevel-decorator';

@loglevel
export default class DispatchRPC
{
  constructor()
  {
    this.emitter = new EventEmitter();

    // keep ref to delegates to de-wire later
    this._callbacks = [];
  }

  /**
   * RPC that will trigger a sending of an event to all listeners
   */
  @rpc.command('broadcast')
  broadcast(client, {event, data})
  {
    this.log.info('client %s emitting %s', client.id, event);
    this.emitter.emit(event, data);
  }

  /**
   * Actually send the message
   */
  _broadcast(client, event, data)
  {
    client.send('broadcast', {event, data});
  }

  /**
   * RPC another component uses to be a listener for events
   */
  @rpc.command('subscribe')
  subscribe(client, event)
  {
    const delegate = data => this._broadcast(client, {event, data});
    this.emitter.on(event, delegate);

    this.log.info('client %s subscribed to %s', client.id, event);

    // keep track of CB to remove later
    this._callbacks.push({delegate, client, event});
  }

  /**
   * Remove a client's listeners
   */
  @rpc.disconnected()
  onDisconnect(client)
  {
    const cbs = this._callbacks.filter(x => x.client === client);

    this.log.info('client %s disconnected, dropping %s callbacks',
        client.id, cbs.length);

    cbs.forEach(x => this.emitter.removeListener(x.event, x.delegate));
  }
}
