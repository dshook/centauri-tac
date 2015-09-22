import loglevel from 'loglevel-decorator';
import game from './gameDecorators.js';

/**
 * The damn game
 */
@loglevel
export default class GameInstance
{
  @game.playerCommand('chat')
  recvChat(player, message)
  {
    this.log.info('%s: %s', player.email, message);
  }

  @game.playerJoined()
  joined(player)
  {
    this.log.info('player %s joined', player.id);
  }

  @game.playerParting()
  parting(player)
  {
    this.log.info('player %s parting', player.id);
  }

  @game.playerConnected()
  connected(player)
  {
    this.log.info('player %s connected', player.id);
  }

  @game.playerDisconnected()
  disconnected(player)
  {
    this.log.info('player %s disconnected', player.id);
  }
}
