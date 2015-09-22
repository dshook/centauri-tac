import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

/**
 * Demo / Chat
 */
@loglevel
export default class ChatDemo
{
  @on('playerCommand', x => x === 'chat')
  recvChat(command, message, player)
  {
    this.log.info('%s: %s', player.email, message);
  }

  @on('latency')
  recvLatency(val, client)
  {
    this.log.info('client %s has latency %s', client.id, val);
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

