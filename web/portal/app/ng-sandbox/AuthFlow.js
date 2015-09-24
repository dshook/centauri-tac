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

    this.automatch = true;

    // Form state for auth
    this.email = `player${player}@gmail.com`;
    this.password = 'pw';

    // Data from server
    this.me = null;
    this.games = null;
    this.currentGame = null;
    this.mmStatus = null;

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
    this.me = null;
    this.games = null;
    this.currentGame = null;
    this.mmStatus = null;
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

      if (!this.automatch) {
        this.games = [];
        await this.net.sendCommand('gamelist', 'gamelist');
      }
      else {
        await this.joinAutomatch();
      }
    }
  }

  /**
   * Got back profile from server
   */
  @rpc.command('auth', 'me')
  @ngApply async _recvMe(client, profile)
  {
    this.me = profile;

    // done talking to auth for now.
    this.net.removeComponent('auth');
  }

  /**
   * Server sends in MM status
   */
  @rpc.command('matchmaker', 'status')
  @ngApply _recvMatchmakerStatus(client, status)
  {
    this.mmStatus = status;
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

  @rpc.command('gamelist', 'game:remove')
  @ngApply _recvGameRemove(client, gameId)
  {
    const index = this.games.findIndex(x => x.id === gameId);

    if (!~index) {
      return;
    }

    this.games.splice(index, 1);
  }

  /**
   * Sever tells us our current game
   */
  @rpc.command('gamelist', 'game:current')
  @rpc.command('matchmaker', 'game:current')
  @ngApply async _recvCurrentGame(client, game)
  {
    // update VM for our current game
    this.currentGame = Game.fromJSON(game);

    // If we're in a game, manually add it to our net client so we can talk to
    // the server. If not, drop it out of the client
    if (game) {
      await this.joinGame(game);
    }
    else if (!game) {
      await this.net.removeComponent('game');
      this.currentGame = null;
      this.games = [];
      await this.net.sendCommand('gamelist', 'gamelist');
    }
  }

  /**
   * Join a specific game
   */
  @ngApply async joinGame(game)
  {
    // dont need to talk to the gamelist or matchmaker anymore
    await this.net.removeComponent('gamelist');
    await this.net.removeComponent('matchmaker');
    this.games = null;
    this.mmStatus = null;

    await this.net.addComponent(game.component);
    await this.net.sendCommand('game', 'join', game.id);

    // TODO: actually talk to the game. At this point though we are connected
    // to the instance that has been spun up and is running a
    // GameHost/GameInstance pair
  }

  /**
   * Bounce out of current game
   */
  @ngApply async leaveCurrentGame()
  {
    this.net.sendCommand('game', 'part');
    this.currentGame = null;

    // reconnect to gamelist/mm and get current games
    if (this.automatch) {
      await this.joinAutomatch();
    }
    else {
      this.games = [];
      await this.net.sendCommand('gamelist', 'gamelist');
    }
  }

  /**
   * Create a new game hosted by our player
   */
  @ngApply async createGame()
  {
    const name = `Player ${this.player} game`;
    this.net.sendCommand('gamelist', 'create', {name});
  }

  @ngApply async joinAutomatch()
  {
    this.games = null;
    this.mmStatus = null;
    await this.net.sendCommand('matchmaker', 'queue');
  }

  @ngApply async cancelAutomatch()
  {
    await this.net.removeComponent('matchmaker');
    this.mmStatus = null;
    this.games = [];
    await this.net.sendCommand('gamelist', 'gamelist');
  }

  @ngApply async endTurn()
  {
    await this.net.sendCommand('game', 'endTurn');
  }
}
