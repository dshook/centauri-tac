import NetClient from 'net-client';
import ngApply from 'ng-apply-decorator';
import Game from 'models/Game';

class FlowSim
{
  constructor(player, net, $scope, $interval)
  {
    this.$scope = $scope;
    this.player = player;

    this.email = `player${player}@gmail.com`;
    this.password = 'pw';

    this.me = null;

    this.games = [];

    this.myGame = null;

    const t = $interval(() => this.fetchGames(), 5000);
    $scope.$on('$destroy', () => $interval.cancel(t));

    // clone app's net
    this.net = new NetClient(net.masterURL, net.realm, net._transport);
  }

  @ngApply async connect()
  {
    await this.net.connect();

    // auto auth on connect
    await this.auth();
  }

  @ngApply async auth()
  {
    await this.net.login(this.email, this.password);

    this.me = await this.net.send('auth', 'player/me');

    await this.fetchGames();
  }

  @ngApply async fetchGames()
  {
    if (!this.net.token) {
      return;
    }

    await this.getCurrentGame();
    const resp = await this.net.send('gamelist', 'game');
    this.games = resp.map(x => Game.fromJSON(x));
  }

  @ngApply async getCurrentGame()
  {
    this.myGame = await this.net.send('gamelist', 'game/current');
  }

  @ngApply async joinFirstEmptyGame()
  {
    const game = this.games[0];
    await this.net.send('gamelist', `game/${game.id}/join`, {});
    await this.fetchGames();
  }

  @ngApply async leaveCurrentGame()
  {
    const game = this.myGame;
    await this.net.send('gamelist', `game/${game.id}/part`, {});
    await this.fetchGames();
  }
}

/**
 * Demo player auth flow and game stuff
 */
export default class PlayerAuthDemoController
{
  constructor($scope, $interval, netClient)
  {
    this.net = netClient;

    // Flow for 2 players
    this.flows = [1, 2].map(x => new FlowSim(x, this.net, $scope, $interval));

    // autoconnect
    this.flows.forEach(x => x.connect());
  }
}
