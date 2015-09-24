import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(players, binder, host)
  {
    this.players = players;
    this.binder = binder;
    this.host = host;
  }

  /**
   * Auto-start game when 2 people have joined
   */
  @on('playerJoined')
  joined()
  {
    if (this.players.length === 2) {
      this.log.info('starting game!');
      this.host.setGameState(3);
      return;
    }

    this.log.info('waiting for both players to join before starting');
  }

  /**
   * Host is shutting us down
   */
  @on('shutdown')
  shutdown()
  {
    this.log.info('Goodbye, world :(');
  }
}
