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

      //spawn both player decks and init hands
      for(let player of this.players){
        this.playerResourceState.init(player.id);
        this.cardState.initPlayer(player.id);
        this.queue.push(new SpawnDeck(player.id));
      }

      // start first turn with random player
      const startingId = _.sample(this.players).id;
      this.queue.push(new PassTurn(startingId));

      //draw initial cards
      let startingCards = 3;
      for(let player of this.players){
        for(let c = 0; c < startingCards; c++){
          this.queue.push(new DrawCard(player.id));
        }
      }

      // spawn game pieces
      var heroUnit = this.cardDirectory.getByTag('Hero')[0];
      for(let i = 0; i < this.players.length; i++){
        this.queue.push(new SpawnPiece(this.players[i].id, heroUnit.cardTemplateId, new Position(i * 2 + 2, 0, i * 2 + 2)));
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
