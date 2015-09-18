import ngApply from 'ng-apply-decorator';
import {rpc} from 'sock-harness';
import Game from 'models/Game';

/**
 * Example controller that will use the network to connect, authorize, get a
 * list of games and either create a new one or join an existing one.
 *
 * The demo page will instaniate n number of them in columns to walk through
 * the process.
 *
 * This is effectively a really fat view model that represents a comprehensive
 * use case of handling the network stack in-game.
 */
export default class AuthFlow
{
  constructor($scope, player, net)
  {
    this.$scope = $scope;
    this.player = player;

    // Form state for auth
    this.email = `player${player}@gmail.com`;
    this.password = 'pw';

    // Data from server
    this.me = null;
    this.games = [];
    this.currentGame = null;

    // give us our own net client
    this.net = net.clone();

    this.net.bindInstance(this);

    // refresh UI whenever a command comes back on the sock (lol angalang)
    this.net.on('command', () => $scope.$apply());
  }

  /**
   * Connect to master server and DL components
   */
  @ngApply async connect()
  {
    await this.net.connect();

    // automatically try to login
    await this.auth();
  }

  /**
   * Drop connection
   */
  @ngApply async disconnect()
  {
    await this.net.disconnect();
  }

  /**
   * Request token from auth via our email and password
   */
  @ngApply async auth()
  {
    const email = this.email;
    const password = this.password;

    // post our crendentials to auth and wait for a login response
    await this.net.sendCommand('auth', 'login', {email, password});
    const {params} = await this.net.recvCommand('login');
    const {status} = params;

    // if we logged in, get the game list and our profile
    if (status) {
      await this.net.sendCommand('auth', 'me');
      await this.net.sendCommand('gamelist', 'gamelist');
    }
  }

  /**
   * Got back profile from server
   */
  @rpc.command('auth', 'me')
  @ngApply async _recvMe(client, profile)
  {
    this.me = profile;
  }

  /**
   * Getting games in from the server
   */
  @rpc.command('gamelist', 'game')
  @ngApply _recvGame(client, game)
  {
    const g = Game.fromJSON(game);
    const index = this.games.findIndex(x => x.id === game.id);

    // Replace if it's just an update
    if (~index) {
      this.games[index] = g;
      return;
    }

    this.games.push(g);
  }

  /**
   * Sever tells us our current game
   */
  @rpc.command('gamelist', 'game:current')
  @ngApply _recvCurrentGame(client, game)
  {
    this.currentGame = Game.fromJSON(game);
  }

  /**
   * Join any game with open player spots
   */
  @ngApply async joinFirstEmptyGame()
  {

  }

  /**
   * Bounce out of current game
   */
  @ngApply async leaveCurrentGame()
  {

  }

  /**
   * Create a new game hosted by our player
   */
  @ngApply async createGame()
  {
    const name = `Player ${this.player} game`;
    this.net.sendCommand('gamelist', 'create', {name});
  }
}
