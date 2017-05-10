import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import MovePiece from '../actions/MovePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import ActivateAbility from '../actions/ActivateAbility.js';
import ActivateCard from '../actions/ActivateCard.js';
import RotatePiece from '../actions/RotatePiece.js';

/**
 * Deals with handling turn stuff and processing the action queue. "low level"
 * game actions
 */
@loglevel
export default class GameController
{
  constructor(players, queue, pieceState, turnState, possibleActions, gameConfig, gameEventService)
  {
    this.players = players;
    this.queue = queue;
    this.pieceState = pieceState;
    this.possibleActions = possibleActions;
    this.gameEventService = gameEventService;
    this.turnState = turnState;
    this.config = gameConfig;
  }

  /**
   * Catch a client up if they need actions
   */
  @on('playerCommand', x => x === 'getActionsSince')
  async getActionsSince(command, actionId, player)
  {
    this.log.info('player %s catching up on actions since %s',
        player.id, actionId);

    const actions = this.queue.iterateCompletedSince(actionId);

    player.client.send('actionHistoryStart');

    for (const action of actions) {
      this._sendAction(player, action);
    }

    player.client.send('actionHistoryComplete');
  }

  /**
   * Pass turn to another player
   */
  @on('playerCommand', x => x === 'endTurn')
  passTurn(command, toPlayerId = null, player)
  {
    if(!this.config.dev){
      this.log.error('Cannot end turn in non dev mode');
      return;
    }

    //stop the turn event timers
    this.gameEventService.stopAll();

    const action = new PassTurn();
    this.queue.push(action);
    this.queue.processUntilDone();

    //and then reset, the energy timer gets restarted by the pass turn
    this.gameEventService.autoTurnInterval.start();
  }

  /**
   * Pause all timers, only for dev
   */
  @on('playerCommand', x => x === 'pauseGame')
  pauseGame(command)
  {
    if(!this.config.dev){
      this.log.error('Cannot end turn in non dev mode');
      return;
    }

    this.gameEventService.pauseAll();
  }

  /**
   * Pause all timers, only for dev
   */
  @on('playerCommand', x => x === 'resumeGame')
  resumeGame(command)
  {
    if(!this.config.dev){
      this.log.error('Cannot end turn in non dev mode');
      return;
    }

    this.gameEventService.resumeAll();
  }

  /**
   * Move a piece
   */
  @on('playerCommand', x => x === 'move')
  movePiece(command, data)
  {
    let {pieceId, route} = data;

    var piece = this.pieceState.piece(pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to move %j', pieceId, this.pieceState);
      return;
    }

    if(piece.hasMoved){
      this.log.warn('Piece %s has already moved', pieceId);
      return;
    }


    if(route){
      for (let step of route) {
        const action = new MovePiece({pieceId, to: step});
        this.queue.push(action);
      }
      this.queue.processUntilDone();
    }
  }

  @on('playerCommand', x => x === 'moveattack')
  moveAttackPiece(command, data)
  {
    let {attackingPieceId, targetPieceId, route} = data;

    var piece = this.pieceState.piece(attackingPieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to attack with %j', attackingPieceId, this.pieceState);
      return;
    }

    if(route){
      if(piece.hasMoved){
        this.log.warn('Piece %s has already moved', attackingPieceId);
        return;
      }

      for (let step of route) {
        this.queue.push(new MovePiece({pieceId: attackingPieceId, to: step}));
      }
    }
    this.queue.push(new AttackPiece(attackingPieceId, targetPieceId));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'activatecard')
  activateCard(command, data)
  {
    let {playerId, cardInstanceId, position, targetPieceId, pivotPosition, chooseCardTemplateId} = data;

    this.queue.push(new ActivateCard(playerId, cardInstanceId, position, targetPieceId, pivotPosition, chooseCardTemplateId));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'rotate')
  rotatePiece(command, data)
  {
    let {pieceId, direction} = data;
    this.queue.push(new RotatePiece(pieceId, direction));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'activateability')
  activateAbility(command, data)
  {
    let {pieceId, targetPieceId} = data;

    this.queue.push(new ActivateAbility(pieceId, targetPieceId));

    this.queue.processUntilDone();
  }

  /**
   * An action has finished
   */
  @on('actionComplete')
  async actionComplete(action)
  {
    //this.log.info('sending action %s', action.constructor.name);
    for (const player of this.players) {
      // disconnected player
      if (!player.client) {
        continue;
      }

      this._sendAction(player, action);
    }
  }

  /**
   * An action was cancelled
   */
  @on('actionCancelled')
  async actionCancelled(action)
  {
    for (const player of this.players) {
      // disconnceted played
      if (!player.client) {
        continue;
      }

      this._sendAction(player, action, true);
    }
  }

  /**
   * When queue processing is complete, tell all clients
   */
  @on('qpc')
  queueProcessComplete(ticks)
  {
    for (const p of this.players.filter(x => x.client)) {
      p.client.send('qpc', ticks);
    }

    for (const player of this.players) {
      // disconnected played
      if (!player.client) {
        continue;
      }

      //find actions the client can take that could be filtered on the server side
      let possibleActions = this.possibleActions.findPossibleActions(player.id);
      player.client.send('possibleActions', possibleActions);
    }

    //check for game win condition
    let loser = null;
    for (const p of this.players) {
      const hero = this.pieceState.hero(p.id);
      if(!hero){
        loser = p.id;
        break;
      }
    }

    //TODO: actually shut down the game
    if(loser !== null){
      this.log.info('player %s LOSES', loser);
      for (const p of this.players.filter(x => x.client)) {
        p.client.send('game:finished',
          {
            id: 99999,
            winnerId: this.players.find(w => w.id != loser).id,
            loserId: loser
          }
        );
      }
      this.gameEventService.shutdown();
    }
  }

  /**
   * When queue proc is started, tell all clients
   */
  @on('qps')
  queueProcessStart(ticks)
  {
    for (const p of this.players.filter(x => x.client)) {
      p.client.send('qps', ticks);
    }
  }

  /**
   * Broadcast an action
   */
  _sendAction(player, action, cancelled = false)
  {
    if(action.serverOnly) return;
    if(action.private && action.playerId && action.playerId !== player.id) return;

    const verb = cancelled ? 'actionCancelled:' : 'action:';
    player.client.send(verb + action.constructor.name, action);
  }
}
