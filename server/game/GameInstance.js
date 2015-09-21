import loglevel from 'loglevel-decorator';
import {command} from './gameDecorators.js';

/**
 * The damn game
 */
@loglevel
export default class GameInstance
{
  @command('chat')
  recvChat(player, message)
  {
    this.log.info('%s: %s', player.email, message);

    for (const p in this.players) {
      const name = p.email;
      player.client.sendCommand('chat', {name, message});
    }
  }
}
