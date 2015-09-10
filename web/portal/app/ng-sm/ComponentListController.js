import ngApply from 'ng-apply-decorator';

export default class ComponentListController
{
  constructor($scope, components)
  {
    this.$scope = $scope;
    this._components = components;

    // Immediately fetch
    this.refresh();
  }

  @ngApply async refresh()
  {
    this.components = await this._components.getComponents();
  }
}
