import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import GameController from './controllers/GameController.js';
import PassTurn from './actions/PassTurn.js';
import SpawnPiece from './actions/SpawnPiece.js';
import Position from './models/Position.js';
import _ from 'lodash';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(queue, players, binder, host)
  {
    this.players = players;
    this.binder = binder;
    this.host = host;
    this.queue = queue;
  }

  /**
   * Auto-start game when 2 people have joined
   */
  @on('playerJoined')
  async joined()
  {
    if (this.players.length === 2) {
      this.log.info('starting game!');

      // update game info
      await this.host.setGameState(3);
      await this.host.setAllowJoin(false);
      
      // bootup the main controller
      await this.host.addController(GameController);

      // start first turn with random player
      const startingId = _.sample(this.players).id;
      this.queue.push(new PassTurn(startingId));

      // spawn game pieces
      for(let i = 0; i < 2; i++){
        for(let j = 0; j < 4; j++){
          this.queue.push(new SpawnPiece(this.players[i].id, i+1, new Position(j * 2, 0, i * 2)));
        }
      }

      await this.queue.processUntilDone();

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

  /**
   * General logging
   */
  @on('playerCommand')
  logger(command, data, player)
  {
    this.log.info('%s -> %s: %j', player.email, command, data);
  }
}
