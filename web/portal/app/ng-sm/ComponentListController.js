import ngApply from 'ng-apply-decorator';

export default class ComponentListController
{
  constructor($scope, netClient)
  {
    this.$scope = $scope;
    this.net = netClient;

    this.loading = false;

    // Immediately fetch
    this.refresh();
  }

  refresh()
  {
    this.loading = true;
    this._refresh();
  }

  @ngApply async _refresh()
  {
    try {
      this.components = await this.net.send('master', 'component');
    }
    finally {
      this.loading = false;
    }
  }
}
