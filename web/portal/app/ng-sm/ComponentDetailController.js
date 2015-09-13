import ngApply from 'ng-apply-decorator';

export default class ComponentDetailController
{
  constructor($state, $scope, netClient, $stateParams)
  {
    this.$state = $state;
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

    // show the child template for the type of component w'eve got
    switch (this.component.type.name) {
      case 'gamelist':
        this.$state.go('.gamelist', {component: this.component});
        break;
    }

  }
}
