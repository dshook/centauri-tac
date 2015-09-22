import loglevel from 'loglevel-decorator';
import game from './gameDecorators.js';
import {on} from 'emitter-binder';

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

  @on('playerJoined')
  joined(player)
  {
    this.log.info('player %s joined', player.id);
  }

  @on('playerParting')
  parting(player)
  {
    this.log.info('player %s parting', player.id);
  }

  @on('playerConnected')
  connected(player)
  {
    this.log.info('player %s connected', player.id);
  }

  @on('playerDisconnected')
  disconnected(player)
  {
    this.log.info('player %s disconnected', player.id);
  }
}
