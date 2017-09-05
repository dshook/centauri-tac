import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import GameController from './controllers/GameController.js';
import PassTurn from './actions/PassTurn.js';
import SpawnPiece from './actions/SpawnPiece.js';
import DrawCard from './actions/DrawCard.js';
import SpawnDeck from './actions/SpawnDeck.js';
//import Position from './models/Position.js';
import Direction from './models/Direction.js';

/**
 * Root controller deal for the game. Manage's state
 */
@loglevel
export default class CentauriTacGame
{
  constructor(
    queue,
    players,
    game,
    host,
    cardDirectory,
    cardState,
    playerResourceState,
    gameEventService,
    mapState
  )
  {
    this.players = players;
    this.game = game;
    this.host = host;
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
    this.playerResourceState = playerResourceState;
    this.gameEventService = gameEventService;
    this.mapState = mapState;
  }

  /**
   * Auto-start game when 2 people have joined
   */
  @on('playerJoined')
  async joined()
  {
    //maybe someday there will be more players...
    if (this.players.length !== 2) {
      this.log.info('waiting for both players to join before starting');
      return;
    }

    this.log.info('starting game!');

    // update game info
    await this.host.setGameState(3);
    await this.host.setAllowJoin(false);

    // bootup the main controller
    await this.host.addController(GameController);

    //set map state current map based on game
    this.mapState.setMap(this.game.map);

    // spawn game pieces for two players
    let allHeroes = this.cardDirectory.getByTag('Hero');
    let heroes = [
      allHeroes.find(h => h.cardTemplateId === 1902),
      allHeroes.find(h => h.cardTemplateId === 1903)
    ];

    this.queue.push(new SpawnPiece({
      playerId: this.players[0].id,
      cardTemplateId: heroes[0].cardTemplateId,
      position: this.mapState.map.startingPositions[0],
      direction: Direction.South
    }));
    this.queue.push(new SpawnPiece({
      playerId: this.players[1].id,
      cardTemplateId: heroes[1].cardTemplateId,
      position: this.mapState.map.startingPositions[1],
      direction: Direction.West
    }));

    //spawn both player decks and init hands
    let startingCards = 4;
    for(let p = 0; p < this.players.length; p++){
      let player = this.players[p];
      this.playerResourceState.init(player.id);
      this.cardState.initPlayer(player.id);
      this.queue.push(new SpawnDeck(player.id, heroes[p].race));
    }

    //draw initial cards
    for(let player of this.players){
      for(let c = 0; c < startingCards; c++){
        this.queue.push(new DrawCard(player.id));
      }
    }

    await this.queue.processUntilDone();

    this.gameEventService.gameKickoff.start();
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
