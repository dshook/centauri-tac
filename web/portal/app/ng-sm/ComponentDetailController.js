import ngApply from 'ng-apply-decorator';

export default class ComponentDetailController
{
  constructor($scope, netClient, $stateParams)
  {
    this.$scope = $scope;
    this.net = netClient;

    this._id = $stateParams.id;

    this.component = null;

    this.refresh();
  }

  @ngApply async refresh()
  {
    this.component = await this.net
      .send('master', `component/${this._id}`);
  }
}
