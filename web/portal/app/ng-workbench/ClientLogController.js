import ngApply from 'ng-apply-decorator';
import ClientLog from 'models/ClientLog';
import _ from 'lodash';

const REFRESH_INTERVAL = 10000;

export default class ClientLogController
{
  constructor($interval, $scope, netClient, $cookies)
  {
    this.$scope = $scope;
    this.transport = netClient;
    this.$cookies = $cookies;

    this.log = [];
    this.filter = {};

    // Immediately fetch
    var firstRefresh = this.refresh();

    firstRefresh.then(() =>
      //init filter to be all on
      this.logFilterOptions.forEach(x => this.filter[x] = true)
    );

    //const t = $interval(() => this.refresh(), REFRESH_INTERVAL);
    //$scope.$on('$destroy', () => $interval.cancel(t));

    this.filterDirty = true;
    this._filteredLog = null;

  }

  get filteredLog()
  {
    if(this.filterDirty){
      this._filteredLog = this.filterLog();
    }
    return this._filteredLog;
  }

  filterLog(){
    this.filterDirty = false;    
    return _.groupBy( 
      this.log.filter(x => this.filter[x.level] ),
      (x) => x.key ? x.key.clientId : ''
    );
  }

  get logFilterOptions()
  {
    return _.unique(
        this.log
        .filter(x => x.level)
        .map(x => x.level)
    );
  }

  toggleFilter(level)
  {
    if (!this.filter[level]) {
      this.filter[level] = true;
    }
    else {
      this.filter[level] = false;
    }
    this.filterDirty = true;
  }

  buttonClass(level){
    return 'btn' +
      (this.filter[level] ? ' active' : '' ) + 
      ' btn-' + this.levelClass(level);    
  }

  levelClass(level){
    level = level.toLowerCase();
    switch(level){
      case 'info':
      case 'warning':
        return level;
      case 'net':
        return 'primary'
      case 'error':
        return 'danger'
      default:
        return '';      
    }

  }

  @ngApply async refresh()
  {
    const log = await this.transport.get('clientlog', 'read');

    this.log = log.map(c => ClientLog.fromJSON(c));
    this.filterDirty = true;
  }
}
