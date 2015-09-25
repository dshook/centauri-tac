import ngApply from 'ng-apply-decorator';
import {rpc} from 'sock-harness';

/**
 * Game ref implementation used by AuthFlow / sandbox
 */
export default class GameFlow
{
  constructor($scope, net)
  {
    this.$scope = $scope;
    this.net = net;

    this.net.bindInstance(this);

    // game state
    this.reset();
  }

  /**
   * Clear state
   */
  reset()
  {
    this.players = [];
    this.actions = [];
    this.currentTurn = 0;
    this.currentPlayerId = null;
  }

  /**
   * Reqest all actions when we join the room
   */
  @rpc.command('game', 'join')
  @ngApply async onJoin()
  {
    const lastId = !this.actions.length ?
      null :
      this.actions[this.actions.length - 1];

    this.net.sendCommand('game', 'getActionsSince', lastId);
  }

  /**
   * Turn action from server
   */
  @rpc.command('game', 'action:PassTurn')
  @ngApply async passTurn(client, turn)
  {
    this.currentTurn++;
    this.currentPlayerId = turn.to;
    this.actions.push(turn);
  }

  /**
   * Update player list
   */
  @rpc.command('game', 'player:connect')
  @rpc.command('game', 'player:join')
  @ngApply async playerConnect(client, player)
  {
    player.connected = true;

    const index = this.players.findIndex(x => x.id === player.id);

    if (~index) {
      this.players[index] = player;
      return;
    }

    this.players.push(player);
  }

  /**
   * Update player list
   */
  @rpc.command('game', 'player:disconnect')
  @ngApply async playerDisconnect(client, player)
  {
    const index = this.players.findIndex(x => x.id === player.id);

    if (~index) {
      this.players[index].connected = false;
    }
  }

  /**
   * Update player list
   */
  @rpc.command('game', 'player:part')
  @ngApply async playerPart(client, player)
  {
    const index = this.players.findIndex(x => x.id === player.id);

    if (~index) {
      this.players.splice(index, 1);
    }
  }

  /**
   * Send action to server
   */
  @ngApply async endTurn()
  {
    await this.net.sendCommand('game', 'endTurn');
  }

}
