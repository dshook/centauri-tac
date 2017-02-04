import loglevel from 'loglevel-decorator';
import express from 'express';
import hoganExpress from 'hogan-express';
import path from 'path';
import ClientLog from 'models/ClientLog';
import _ from 'lodash';
import fsp from 'fs-promise';

/**
 * Game server component that hosts the portal web applicaiton static assets
 */
@loglevel
export default class PortalComponent
{
  async start(component)
  {
    const server = component.server;
    const routed = component.httpServer;

    server.engine('html', hoganExpress);
    server.set('view engine', 'html');
    server.set('views', path.resolve(__dirname, '../views'));
    server.set('layout', 'layout');
    server.use('/assets', express.static(path.resolve(__dirname, '../static-assets')));

    // redirect everything else to index
    routed.get('/', async (req, res) => {
      try{
        let clientLog = await fsp.readJson(process.env.CLIENT_LOG);
        let logs = clientLog.map(c => ClientLog.fromJSON(c));
        let groupedLogs = this.groupLog(logs);
        //this.log.info('groupedLogs', groupedLogs);

        res.render('app', {groupedLogs});
      }catch(e){
        this.log.error('Error in logs', e);
        res.write(e.message);
        res.end();
      }
    });

    this.log.info('mounted log');
  }

  groupLog(logs){
    var catchAll = 'General';
    let output = {};

    var groupedLogs = _.groupBy(logs, (x) => x.key ? x.key.clientId : catchAll);

    //ensure the default catch all group that may have been filtered out
    if(!groupedLogs[catchAll]){
      groupedLogs[catchAll] = [];
    }

    //nasty ass add to new object so object properties will be sorted
    var tmp = {};
    var groupedLogKeys = Object.keys(groupedLogs).sort();

    tmp[catchAll] = groupedLogs[catchAll];
    for(let logKey of groupedLogKeys){
      if(logKey == catchAll) continue;

      //also make sure the logs are sorted by timestamp for the next step
      tmp[logKey] = _.sortBy(groupedLogs[logKey], t => t.timestamp);
    }
    groupedLogs = tmp;
    output.clients = Object.keys(groupedLogs);

    //loop through all the clients looking for the next log timestamp until there aren't any
    //at each of those timestamps, see if there's another log at the same time from any of the other clients
    let logsRemaining;
    let orderedLogs = [];
    do{
      logsRemaining = 0;
      let nextLogs = [];
      for(let logKey in groupedLogs){
        nextLogs.push(groupedLogs[logKey][0] || null);
        logsRemaining += groupedLogs[logKey].length;
      }
      if(logsRemaining === 0) break;

      let nextTimestamp = _.minBy(nextLogs.filter(l => l), l => l.timestamp).timestamp;
      let nextLog = {parentTimestamp: nextTimestamp.format("HH:mm:ss.SSS"), clients: []};
      for(let logKey in groupedLogs){
        if(groupedLogs[logKey][0] && nextTimestamp.isSame(groupedLogs[logKey][0].timestamp)){
          nextLog.clients.push(groupedLogs[logKey].shift());
        }else{
          nextLog.clients.push(null);
        }
      }
      orderedLogs.push(nextLog);

    }while(logsRemaining > 0);

    output.orderedLogs = orderedLogs;

    // //"fill out" all the groups of logs based on timestamps
    // //so they can be displayed as one table
    // for(let logKey in groupedLogs){
    //   for(let l = 0; l < groupedLogs[logKey].length; l++){
    //     let log = groupedLogs[logKey][l];
    //     let timestamp = log.timestamp;
    //     let numAtTimestamp = _.filter(
    //       groupedLogs[logKey], x => x.timestamp.isSame(timestamp)
    //     ).length;

    //     //iterate over the other groups to fill in
    //     for(let otherLogsKey in groupedLogs){
    //       if(otherLogsKey == logKey) continue;

    //       let othersAtTimestamp = _.filter(
    //         groupedLogs[otherLogsKey], x => x.timestamp.isSame(timestamp)
    //       ).length;

    //       for(let a = 0; a < (numAtTimestamp - othersAtTimestamp); a++){
    //         let newCL = new ClientLog();
    //         newCL.timestamp = timestamp;
    //         groupedLogs[otherLogsKey].push(newCL);
    //       }
    //     }
    //   }
    // }

    // //order the groups
    // for(let logKey in groupedLogs){
    //   groupedLogs[logKey] = _.sortBy(groupedLogs[logKey], x => x.timestamp);
    // }

    //output.groupedLogs = groupedLogs;
    return output;
  }

}
