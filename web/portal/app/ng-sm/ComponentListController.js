import ngApply from 'ng-apply-decorator';
import Component from 'models/Component';

const REFRESH_INTERVAL = 5000;

export default class ComponentListController
{
  constructor($interval, $scope, netClient)
  {
    this.$scope = $scope;
    this.net = netClient;

    // Immediately fetch
    this.refresh();

    const t = $interval(() => this.refresh(), REFRESH_INTERVAL);
    $scope.$on('$destroy', () => $interval.cancel(t));
  }

  @ngApply async refresh()
  {
    const components = await this.net.send('master', 'component');

    this.components = components.map(c => Component.fromJSON(c));
  }

}
