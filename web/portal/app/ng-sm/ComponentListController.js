import ngApply from 'ng-apply-decorator';

export default class ComponentListController
{
  constructor($scope, netClient)
  {
    this.$scope = $scope;
    this.net = netClient;

    // Immediately fetch
    this.refresh();
  }

  @ngApply async refresh()
  {
    this.components = await this.net.send('master', 'component');
  }
}
