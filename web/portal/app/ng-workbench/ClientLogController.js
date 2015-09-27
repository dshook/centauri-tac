import ngApply from 'ng-apply-decorator';
import ClientLog from 'models/ClientLog';
import _ from 'lodash';

const REFRESH_INTERVAL = 5000;

export default class ClientLogController
{
  constructor($interval, $scope, netClient, $cookies)
  {
    this.$scope = $scope;
    this.transport = netClient;
    this.$cookies = $cookies;

    this.log = [];

    // Immediately fetch
    this.refresh();

    const t = $interval(() => this.refresh(), REFRESH_INTERVAL);
    $scope.$on('$destroy', () => $interval.cancel(t));

  }

  get filteredComponents()
  {
    if (!this.logFilter) {
      return this.log;
    }

    return this.log.filter(x => x.level === this.logFilter);
  }

  get logFilterOptions()
  {
    return _.unique(this.log
        .filter(x => x.level)
        .map(x => x.level));
  }

  setRealmFilter(filter)
  {
    this.logFilter = filter;

    if (!filter) {
      this.$cookies.remove('logFilter');
    }
    else {
      this.$cookies.put('logFilter', filter);
    }
  }

  @ngApply async refresh()
  {
    const log = await this.transport.get('clientlog', 'read');

    this.log = log.map(c => ClientLog.fromJSON(c));
  }
}
