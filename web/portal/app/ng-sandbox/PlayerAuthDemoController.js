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

    this.games = [];

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
    await this.fetchGames();
  }

  @ngApply async fetchGames()
  {
    if (!this.net.token) {
      return;
    }

    const resp = await this.net.send('gamelist', 'game');
    const games = resp.map(x => Game.fromJSON(x));

    this.games = games;
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
