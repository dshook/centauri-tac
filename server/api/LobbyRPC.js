import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';
import {on} from 'emitter-binder';

/**
 * RPC handler for the matchmaker component
 */
@loglevel
export default class LobbyRPC
{
  constructor(matchmaker)
  {
    this.matchmaker = matchmaker;
    this.clients = new Set();
  }

  /**
   * Drop a player into the queue
   */
  @rpc.command('queue')
  @rpc.middleware(roles(['player']))
  async queuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.matchmaker.queuePlayer(playerId, client);
  }

  /**
   * And back out again
   */
  @rpc.command('dequeue')
  @rpc.middleware(roles(['player']))
  async dequeuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.matchmaker.dequeuePlayer(playerId, client);
  }

  /**
   * When a client connects
   */
  @rpc.command('_token')
  async hello(client, params, auth)
  {
    // connection from in the mesh
    if (!auth || !auth.sub) {
      return;
    }

    this.clients.add(client);

    // drop player when they DC
    const playerId = auth.sub.id;
    client.once('close', () => this.matchmaker.dequeuePlayer(playerId, client));
    client.once('close', () => this.clients.delete(client));
  }

  /**
   * If a player is connected, inform them of their current game
   * don't think this is needed anymore,
   */
  @on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    for (const c of this.clients) {
      const {id} = c.auth.sub;
      if (playerId === id) {
        c.send('game:current', game);
      }
    }
  }

  /**
   * broadcast status
   */
  @on('matchmaker:status')
  _status(status)
  {
    for (const c of this.clients) {
      const {id} = c.auth.sub;
      if (status.playerId === id) {
        c.send('status', status);
      }
    }
  }
}
