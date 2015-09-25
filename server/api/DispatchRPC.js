import {rpc} from 'sock-harness';
import {EventEmitter} from 'events';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';

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
  @rpc.middleware(roles(['component']))
  broadcast(client, {event, data})
  {
    this.log.info('client %s emitting %s', client.id, event);
    this.emitter.emit(event, data);
  }

  /**
   * RPC another component uses to be a listener for events
   */
  @rpc.command('subscribe')
  @rpc.middleware(roles(['component']))
  subscribe(client, event)
  {
    // see if this client is already signed up for this shit
    // TODO: seems a bit brittle if the identity of the sock client changes and
    // may lead to dupe sent messages? Could check the clients auth object
    // potentially
    const existing = this._callbacks
      .some(x => x.client === client && x.event === event);

    if (existing) {
      return;
    }

    const delegate = data => client.send('broadcast', {event, data});
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
