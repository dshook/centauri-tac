import angular from 'angular';
import htmlServerManagement from './templates/server-management.html';
import htmlComponentList from './templates/component-list.html';
import ComponentListController from './ComponentListController.js';
import htmlComponentDetail from './templates/component-detail.html';
import ComponentDetailController from './ComponentDetailController.js';
import htmlGameList from './templates/game-list.html';
import GameListController from './GameListController.js';
import htmlGamelistComponent from './templates/gamelist-component.html';

/**
 * Server management view node
 */
export default angular.module('centauri.server-management', [])
  .config(($stateProvider) => {

    $stateProvider

      .state('app.sm', {
        abstract: true,
        url: '/server',
        template: htmlServerManagement,
      })

      .state('app.sm.component-list', {
        url: '/components',
        template: htmlComponentList,
        controller: ComponentListController,
        controllerAs: 'vm',
      })

      .state('app.sm.game-list', {
        url: '/games',
        template: htmlGameList,
        controller: GameListController,
        controllerAs: 'vm',
      })

      .state('app.sm.component-detail', {
        url: '/components/:id',
        template: htmlComponentDetail,
        controller: ComponentDetailController,
        controllerAs: 'vm',
      })

      .state('app.sm.component-detail.gamelist', {
        url: '/gamelist',
        template: htmlGamelistComponent,
      })

      .state('app.sm.status', {
        url: '/status',
      });

  });
