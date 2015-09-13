import ngApply from 'ng-apply-decorator';
import Component from 'models/Component';
import _ from 'lodash';

const REFRESH_INTERVAL = 5000;

export default class ComponentListController
{
  constructor($interval, $scope, netClient, $cookies)
  {
    this.$scope = $scope;
    this.net = netClient;
    this.$cookies = $cookies;

    this.components = [];

    // Immediately fetch
    this.refresh();

    const t = $interval(() => this.refresh(), REFRESH_INTERVAL);
    $scope.$on('$destroy', () => $interval.cancel(t));

    this.realmFilter = $cookies.get('realmFilter') || null;
  }

  get filteredComponents()
  {
    if (!this.realmFilter) {
      return this.components;
    }

    return this.components.filter(x => x.realm === this.realmFilter);
  }

  get realmFilterOptions()
  {
    return _.unique(this.components
        .filter(x => x.realm)
        .map(x => x.realm));
  }

  setRealmFilter(filter)
  {
    this.realmFilter = filter;

    if (!filter) {
      this.$cookies.remove('realmFilter');
    }
    else {
      this.$cookies.put('realmFilter', filter);
    }
  }

  @ngApply async refresh()
  {
    const components = await this.net.send('master', 'component');

    this.components = components.map(c => Component.fromJSON(c));
  }
}
