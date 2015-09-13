import NetClient from 'net-client';
import ngApply from 'ng-apply-decorator';

class FlowSim
{
  constructor(player, net, $scope)
  {
    this.$scope = $scope;
    this.player = player;

    this.email = `player${player}@gmail.com`;
    this.password = 'pw';

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
  }
}

/**
 * Demo player auth flow and game stuff
 */
export default class PlayerAuthDemoController
{
  constructor($scope, netClient)
  {
    this.net = netClient;

    // Flow for 2 players
    this.flows = [1, 2].map(x => new FlowSim(x, this.net, $scope));

    // autoconnect
    this.flows.forEach(x => x.connect());
  }
}
