import ngApply from 'ng-apply-decorator';

export default class HomeController
{
  constructor($scope, components)
  {
    this.$scope = $scope;
    this._components = components;
    this.components = [];

    this.refresh();
  }

  @ngApply async refresh()
  {
    this.components = await this._components.getComponents();
  }
}
