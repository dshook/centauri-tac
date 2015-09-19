import {rpc} from 'sock-harness';
import roles from '../middleware/rpc/roles.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class GameRPC
{
  constructor()
  {

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

  /**
   * A player is trying to leave a game
   */
  @rpc.command('part')
  @rpc.middleware(roles(['player']))
  playerPart(client, params, auth)
  {
    const playerId = auth.sub.id;

    // todo: determine game player is parting frome
    const gameId = null;

    this.log.info('player %s is parting game %s', playerId, gameId);
    this.log.info('TODO: remove player from GameHost and update gamelist');
  }

  /**
   * A gamelist is instructing us to create a new game
   */
  @rpc.command('create')
  @rpc.middleware(roles(['gamelist']))
  newGame(client, {game})
  {
    this.log.info('TODO: create a new game %s for gamelist', game.id);
  }

}
