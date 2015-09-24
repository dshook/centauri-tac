import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';
import {dispatch} from 'rpc-messenger';

/**
 * RPC handler for the matchmaker component
 */
@loglevel
export default class MatchmakerRPC
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
    await this.matchmaker.queuePlayer(playerId);
  }

  /**
   * When a client connects
   */
  @rpc.command('_token')
  async hello(client, params, auth)
  {
    // connection from in the mesh
    if (!auth.sub) {
      return;
    }

    this.clients.add(client);

    // drop player when they DC
    const playerId = auth.sub.id;
    client.once('close', () => this.matchmaker.dequeuePlayer(playerId));
    client.once('close', () => this.clients.delete(client));
  }

  /**
   * If a player is conencted, inform them of their current game
   */
  @dispatch.on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    if (game) {
      this.matchmaker.dequeuePlayer(playerId);
    }

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
  @dispatch.on('matchmaker:status')
  _status(status)
  {
    for (const c of this.clients) {
      c.send('status', status);
    }
  }
}
