import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import MovePiece from '../actions/MovePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import ActivateAbility from '../actions/ActivateAbility.js';
import ActivateCard from '../actions/ActivateCard.js';
import RotatePiece from '../actions/RotatePiece.js';
import DrawCard from '../actions/DrawCard.js';

/**
 * Deals with handling turn stuff and processing the action queue. "low level"
 * game actions
 */
@loglevel
export default class GameController
{
  constructor(hostManager, players, queue, pieceState, turnState, cardState, possibleActions, gameConfig, gameEventService, selector)
  {
    this.hostManager = hostManager;
    this.players = players;
    this.queue = queue;
    this.pieceState = pieceState;
    this.possibleActions = possibleActions;
    this.gameEventService = gameEventService;
    this.turnState = turnState;
    this.cardState = cardState;
    this.config = gameConfig;
    this.selector = selector;
  }

  /**
   * Catch a client up if they need actions
   */
  @on('playerCommand', x => x === 'getActionsSince')
  async getActionsSince(command, actionId, player)
  {
    this.log.info('player %s catching up on actions since %s', player.id, actionId);

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
    this.log.info('Dev End Turn');

    //stop the turn event timers
    this.gameEventService.stopAll();

    //and then reset, the energy timer gets restarted by the pass turn
    this.gameEventService.passTurn();
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
    this.log.info('Dev Pause');

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
    this.log.info('Dev Resume');

    this.gameEventService.resumeAll();
  }

  /**
   * Move a piece
   */
  @on('playerCommand', x => x === 'move')
  movePiece(command, data, player)
  {
    this.log.info('Player %j moving piece', player);
    let {pieceId, route} = data;

    var piece = this.pieceState.piece(pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to move %j', pieceId, this.pieceState);
      return;
    }
    if(!this.checkPlayerAuthCommand(player, piece))
    {
      this.log.warn('Player %j not authorized to move piece %j', player, piece);
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
  moveAttackPiece(command, data, player)
  {
    let {attackingPieceId, targetPieceId, route} = data;

    var piece = this.pieceState.piece(attackingPieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to attack with %j', attackingPieceId, this.pieceState);
      return;
    }
    if(!this.checkPlayerAuthCommand(player, piece))
    {
      this.log.warn('Player %j not authorized to move piece %j', player, piece);
      return;
    }

    if(route){
      for (let step of route) {
        this.queue.push(new MovePiece({pieceId: attackingPieceId, to: step}));
      }
    }
    this.queue.push(new AttackPiece(attackingPieceId, targetPieceId));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'activatecard')
  activateCard(command, data, player)
  {
    let {playerId, cardInstanceId, position, targetPieceId, pivotPosition, chooseCardTemplateId} = data;

    let card = this.cardState.hands[playerId].find(c => c.id === cardInstanceId);
    if(!this.checkPlayerAuthCommand(player, card))
    {
      this.log.warn('Player %j not authorized to activeate card %j', player, card);
      return;
    }

    this.queue.push(new ActivateCard(playerId, cardInstanceId, position, targetPieceId, pivotPosition, chooseCardTemplateId));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'rotate')
  rotatePiece(command, data, player)
  {
    let {pieceId, direction} = data;

    var piece = this.pieceState.piece(pieceId);

    if(!this.checkPlayerAuthCommand(player, piece))
    {
      this.log.warn('Player %j not authorized to move piece %j', player, piece);
      return;
    }

    this.queue.push(new RotatePiece(pieceId, direction));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'activateability')
  activateAbility(command, data, player)
  {
    let {pieceId, targetPieceId} = data;

    var piece = this.pieceState.piece(pieceId);

    let ability = piece.events.find(e => e.event === 'ability');

    if(!ability){
      this.log.warn('Piece %j has no ability to activate', piece);
      return;
    }

    this.log.info('Ability found');
    let allowedToUseEnemyAbility = false;

    //Check to see if the piece allows enemies to use its ability
    if(ability.args.length && ability.args[3]){
      let playerSelected = this.selector.selectPlayer(piece.playerId, ability.args[3]);
      if(playerSelected === player.id){
        allowedToUseEnemyAbility = true;
        this.log.info('Enemy allowed');
      }
    }

    if(!allowedToUseEnemyAbility && !this.checkPlayerAuthCommand(player, piece))
    {
      this.log.warn('Player %j not authorized to activate piece ability %j', player, piece);
      return;
    }

    this.log.info('Adding activate ability');
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
    //TODO: think about ties when both players die at the same time
    let loser = null;
    for (const p of this.players) {
      const hero = this.pieceState.hero(p.id);
      if(!hero){
        loser = p.id;
        break;
      }
    }

    if(loser !== null){
      let winner = this.players.find(w => w.id != loser);
      this.log.info('player %s LOSES, player %s WINS!', loser, winner.id);
      this.gameEventService.shutdown();
      this.hostManager.completeGame(winner.id, null);
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

    let finalAction = this.censorInformation(player, action, cancelled);
    const verb = cancelled ? 'actionCancelled:' : 'action:';
    player.client.send(verb + action.constructor.name, finalAction);
  }

  //Remove info from actions before sending it to players
  censorInformation(player, action, cancelled){
    if(action instanceof DrawCard && player.id != action.playerId && !action.overdrew){
      let censoredAction = Object.assign({}, action);
      censoredAction.cardTemplateId = null;
      return censoredAction;
    }

    return action;
  }

  //Checks to see if the player is authorized to make the command
  //This just checks the data object (a piece or card) to see if the player id matches
  checkPlayerAuthCommand(player, dataObj){
    if(!dataObj || !dataObj.playerId) return false;

    return player.id === dataObj.playerId;
  }
}
