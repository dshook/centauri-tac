import AuthFlow from './AuthFlow.js';

/**
 * Demo player auth flow and game stuff
 */
export default class PlayerAuthDemoController
{
  constructor($scope, $interval, netClient)
  {
    this.net = netClient;
    this.flows = [];

    // Flow for 2 players
    for (const p of [1, 2, 3]) {
      const flow = new AuthFlow($scope, p, this.net);
      this.flows.push(flow);

      $scope.$on('$destroy', () => flow.disconnect());

      // auto connect
      flow.connect();
    }
  }
}
