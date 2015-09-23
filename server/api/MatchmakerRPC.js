import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';

/**
 * RPC handler for the matchmaker component
 */
@loglevel
export default class MatchmakerRPC
{
  constructor()
  {

  }

  @rpc.command('queue')
  @rpc.middleware(roles(['player']))
  queuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    this.log.info('player %s queueing', playerId);
  }
}
