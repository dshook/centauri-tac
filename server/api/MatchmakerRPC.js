import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';
import {autobind} from 'core-decorators';

/**
 * RPC handler for the matchmaker component
 */
@loglevel
export default class MatchmakerRPC
{
  constructor(matchmaker)
  {
    this.matchmaker = matchmaker;

    this.matchmaker.on('game:current', this._currentGame);

    this.clients = new Set();
  }

  @rpc.command('queue')
  @rpc.middleware(roles(['player']))
  async queuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.matchmaker.queue(playerId);
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
  }

  /**
   * Match maker gives us a match!
   */
  @autobind _currentGame({playerId, game})
  {
    this.log.info('player %s goes to game %s', playerId, game.id);
  }
}
