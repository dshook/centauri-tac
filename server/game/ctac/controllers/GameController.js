import {on} from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import MovePiece from '../actions/MovePiece.js';
import AttackPiece from '../actions/AttackPiece.js';
import ActivateCard from '../actions/ActivateCard.js';

/**
 * Deals with handling turn stuff and processing the action queue. "low level"
 * game actions
 */
@loglevel
export default class GameController
{
  constructor(players, queue, pieceState, hands, turnState, selector)
  {
    this.players = players;
    this.queue = queue;
    this.pieceState = pieceState;
    this.hands = hands;
    this.turnState = turnState;
    this.selector = selector;
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
    let id = toPlayerId;

    // implicit just pass to the other player
    if (toPlayerId === null) {
      if (this.players.length !== 2) {
        throw new Error('to field required when not 2 players');
      }

      const toPlayer = this.players.find(x => x.id !== player.id);
      id = toPlayer.id;
    }

    const action = new PassTurn(id, player.id);
    this.queue.push(action);
    this.queue.processUntilDone();
  }

  /**
   * Move a piece
   */
  @on('playerCommand', x => x === 'move')
  movePiece(command, data)
  {
    let {pieceId, route} = data;

    if(route){
      for (let step of route) {
        const action = new MovePiece(pieceId, step);
        this.queue.push(action);
      }
      this.queue.processUntilDone();
    }
  }

  @on('playerCommand', x => x === 'moveattack')
  moveAttackPiece(command, data)
  {
    let {attackingPieceId, targetPieceId, route} = data;

    if(route){
      for (let step of route) {
        this.queue.push(new MovePiece(attackingPieceId, step));
      }
    }
    this.queue.push(new AttackPiece(attackingPieceId, targetPieceId));

    this.queue.processUntilDone();
  }

  @on('playerCommand', x => x === 'activatecard')
  activateCard(command, data)
  {
    let {playerId, cardInstanceId, position, targetPieceId} = data;

    this.queue.push(new ActivateCard(playerId, cardInstanceId, position, targetPieceId));

    this.queue.processUntilDone();
  }

  /**
   * An action has finished
   */
  @on('actionComplete')
  async actionComplete(action)
  {
    for (const player of this.players) {

      // disconnceted played
      if (!player.client) {
        continue;
      }

      this._sendAction(player, action);
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

    //let possibleActions = this._findPossibleActions();

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
  _sendAction(player, action)
  {
    player.client.send('action:' + action.constructor.name, action);
  }

  //look through the current players hand for any cards needing a TARGET
  //and then find what the possible targets are
  // {
  //   playerId: 1,
  //   targets: [
  //     {cardId: 2, targetPieceIds: [4,5,6]}
  //   ]
  // }
  _findPossibleActions(){
    let targets = [];
    for(let card of this.hands[this.turnState.currentPlayerId]){
      let targetPieceIds = this.selector.selectTargets(this.turnState.currentPlayerId, card.events);
      if(targetPieceIds.length > 0){
        targets.push({
          cardId: card.id,
          targetPieceIds
        });
      }
    }
    return {
      playerId: this.turnState.currentPlayerId,
      targets
    };
  }
}
