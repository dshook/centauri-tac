import ngApply from 'ng-apply-decorator';
import Game from 'models/Game';

const REFRESH_INTERVAL = 5000;

export default class GameListController
{
  constructor($interval, $scope, netClient)
  {
    this.$scope = $scope;
    this.net = netClient;

    this.games = [];

    // Immediately fetch
    this.refresh();

    const t = $interval(() => this.refresh(), REFRESH_INTERVAL);
    $scope.$on('$destroy', () => $interval.cancel(t));
  }

  @ngApply async refresh()
  {
    const games = await this.net.send('gamelist', 'game');

    this.games = games.map(c => Game.fromJSON(c));
  }

}

