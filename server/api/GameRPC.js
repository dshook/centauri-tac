import {rpc} from 'sock-harness';
import roles from '../middleware/rpc/roles.js';
import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

@loglevel
export default class GameRPC
{
  constructor(gameManager)
  {
    this.manager = gameManager;
  }

  @on('game:created')
  gameCreated(game)
  {
    this.manager.create(game);
  }

  /**
   * A player is requesting to join
   */
  @rpc.command('join')
  @rpc.middleware(roles(['player']))
  playerJoin(client, gameId, auth)
  {
    const playerId = auth.sub.id;
    this.manager.playerJoin(client, playerId, gameId);
  }

  /**
   * A player is trying to BOUNCE
   */
  @rpc.command('part')
  @rpc.middleware(roles(['player']))
  playerPart(client, params, auth)
  {
    const playerId = auth.sub.id;
    this.manager.playerPart(client, playerId);
  }

}
