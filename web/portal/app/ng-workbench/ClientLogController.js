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
    var groupedLogs = _.groupBy( 
      this.log.filter(x => this.filter[x.level] ),
      (x) => x.key ? x.key.clientId : 'General'
    );

    //"fill out" all the groups of logs based on timestamps 
    //so they can be displayed as one table
    for(let logKey in groupedLogs){
      for(let l = 0; l < groupedLogs[logKey].length; l++){
        let log = groupedLogs[logKey][l];
        let timestamp = log.timestamp;
        let numAtTimestamp = _.filter(
          groupedLogs[logKey], x => x.timestamp.isSame(timestamp)
        ).length;

        //iterate over the other groups to fill in
        for(let otherLogsKey in groupedLogs){
          if(otherLogsKey == logKey) continue;

          let othersAtTimestamp = _.filter(
            groupedLogs[otherLogsKey], x => x.timestamp.isSame(timestamp)
          ).length;

          for(let a = 0; a < (numAtTimestamp - othersAtTimestamp); a++){
            let newCL = new ClientLog();
            newCL.timestamp = timestamp;
            groupedLogs[otherLogsKey].push(newCL);
          }
        }
      }
    }

    //order the groups
    for(let logKey in groupedLogs){
      groupedLogs[logKey] = _.sortBy(groupedLogs[logKey], x => x.timestamp);
    }

    return groupedLogs;
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
    if(!level) return '';
    return 'btn' +
      (this.filter[level] ? ' active' : '' ) + 
      ' btn-' + this.levelClass(level);    
  }

  levelClass(level){
    if(!level) return '';
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
