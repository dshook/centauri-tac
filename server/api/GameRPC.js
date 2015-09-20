import {rpc} from 'sock-harness';
import roles from '../middleware/rpc/roles.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class GameRPC
{
  constructor(gameManager)
  {
    this.manager = gameManager;
  }

  /**
   * A player is requesting to join
   */
  @rpc.command('join')
  @rpc.middleware(roles(['player']))
  playerJoin(client, gameId, auth)
  {
    const playerId = auth.sub.id;
    this.log.info('player %s is joining game %s', playerId, gameId);
    this.log.info('TODO: add player to GameHost and update gamelist');
  }
}
