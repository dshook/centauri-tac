import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import GameController from './controllers/GameController.js';
import PassTurn from './actions/PassTurn.js';
import SpawnPiece from './actions/SpawnPiece.js';
import DrawCard from './actions/DrawCard.js';
import SpawnDeck from './actions/SpawnDeck.js';
import Position from './models/Position.js';
import Direction from './models/Direction.js';
import _ from 'lodash';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(queue, players, binder, host, cardDirectory, cardState, playerResourceState)
  {
    this.players = players;
    this.binder = binder;
    this.host = host;
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
    this.playerResourceState = playerResourceState;
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

      // spawn game pieces for two players
      var heroes = this.cardDirectory.getByTag('Hero');
      if(this.players.length === 2){
        this.queue.push(new SpawnPiece(this.players[0].id, null, heroes[0].cardTemplateId, new Position(2, 0, 4), null, Direction.South));
        this.queue.push(new SpawnPiece(this.players[1].id, null, heroes[1].cardTemplateId, new Position(5, 0, 2), null, Direction.West));
      }

      //spawn both player decks and init hands
      for(let player of this.players){
        let startingCards = player.id === startingId ? 3 : 4;
        this.playerResourceState.init(player.id);
        this.cardState.initPlayer(player.id);
        this.queue.push(new SpawnDeck(player.id, startingId, startingCards));
      }

      //draw initial cards
      for(let player of this.players){
        let startingCards = player.id === startingId ? 3 : 4;
        for(let c = 0; c < startingCards; c++){
          this.queue.push(new DrawCard(player.id));
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
