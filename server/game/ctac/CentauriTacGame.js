import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import GameController from './controllers/GameController.js';
import PassTurn from './actions/PassTurn.js';
import SpawnPiece from './actions/SpawnPiece.js';
import DrawCard from './actions/DrawCard.js';
import SpawnDeck from './actions/SpawnDeck.js';
import Position from './models/Position.js';
import Direction from './models/Direction.js';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(queue, players, binder, host, cardDirectory, cardState, playerResourceState, gameEventService)
  {
    this.players = players;
    this.binder = binder;
    this.host = host;
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
    this.playerResourceState = playerResourceState;
    this.gameEventService = gameEventService;
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
      this.queue.push(new PassTurn());

      // spawn game pieces for two players
      //var heroes = this.cardDirectory.getByTag('Hero');
      if(this.players.length === 2){
        this.queue.push(new SpawnPiece({
          playerId: this.players[0].id,
          cardTemplateId: 100, //heroes[3].cardTemplateId,
          position: new Position(2, 0, 4),
          direction: Direction.South
        }));
        this.queue.push(new SpawnPiece({
          playerId: this.players[1].id,
          cardTemplateId: 101, //heroes[2].cardTemplateId,
          position: new Position(5, 0, 2),
          direction: Direction.West
        }));
      }

      //spawn both player decks and init hands
      let startingCards = 4;
      for(let player of this.players){
        this.playerResourceState.init(player.id);
        this.cardState.initPlayer(player.id);
        this.queue.push(new SpawnDeck(player.id, startingCards));
      }

      //draw initial cards
      for(let player of this.players){
        for(let c = 0; c < startingCards; c++){
          this.queue.push(new DrawCard(player.id));
        }
      }

      await this.queue.processUntilDone();

      this.gameEventService.autoTurnInterval.start();

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
    this.gameEventService.shutdown();
  }

  /**
   * General logging
   */
  @on('playerCommand')
  logger(command, data, player)
  {
    this.log.info('%s -> %s: %j', player ? player.email : 'NO PLAYER', command, data);
  }
}
