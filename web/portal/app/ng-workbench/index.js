import angular from 'angular';
import htmlWorkbench from './templates/workbench.html';
import logList from './templates/log-list.html';
import ClientLogController from './ClientLogController.js';

/**
 * Server management view node
 */
export default angular.module('centauri.workbench', [])
  .config(($stateProvider) => {

    $stateProvider
      .state('app.workbench', {
        abstract: true,
        url: '/workbench',
        template: htmlWorkbench,
      })

      .state('app.workbench.client-log', {
        url: '/client-log',
        template: logList,
        controller: ClientLogController,
        controllerAs: 'vm',
      });

  });
