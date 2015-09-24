import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(players, binder)
  {
    this.players = players;
    this.binder = binder;
  }

  /**
   * Auto-start game when 2 people have joined
   */
  @on('playerJoined')
  joined()
  {
    if (this.players.length === 2) {
      this.log.info('starting game!');
      return;
    }

    this.log.info('waiting for both players to join before starting');
  }
}
