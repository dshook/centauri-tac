import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

/**
 * Demo / Chat
 */
@loglevel
export default class ChatDemo
{
  constructor(players)
  {
    this.players = players;
  }

  @on('playerCommand', x => x === 'chat')
  recvChat(command, message, player)
  {
    for (const {client} of this.players) {
      if (client) {
        client.send('chat', {player, message});
      }
    }
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

