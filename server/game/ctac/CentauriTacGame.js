import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import GameController from './controllers/GameController.js';
import PassTurn from './actions/PassTurn.js';
import SpawnPiece from './actions/SpawnPiece.js';
import DrawCard from './actions/DrawCard.js';
import SpawnDeck from './actions/SpawnDeck.js';
import Position from './models/Position.js';
import _ from 'lodash';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(queue, players, binder, host, cardDirectory, decks, hands)
  {
    this.players = players;
    this.binder = binder;
    this.host = host;
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.decks = decks;
    this.hands = hands;
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

      //give players some cards and init decks and hands
      let startingCards = 3;
      for(let p = 0; p < this.players.length; p++){
        this.decks[this.players[p].id] = [];
        this.hands[this.players[p].id] = [];
        this.queue.push(new SpawnDeck(this.players[p].id));
        for(let c = 0; c < startingCards; c++){
          this.queue.push(new DrawCard(this.players[p].id));
        }
      }

      // spawn game pieces
      for(let i = 0; i < 2; i++){
        for(let j = 0; j < 4; j++){
          this.queue.push(new SpawnPiece(this.players[i].id, i+1, new Position(j * 2, 0, i * 2)));
        }
      }

      // start first turn with random player
      const startingId = _.sample(this.players).id;
      this.queue.push(new PassTurn(startingId));

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
