import angular from 'angular';
import htmlSandbox from './templates/sandbox.html';
import htmlPlayerAuth from './templates/player-auth.html';
import PlayerAuthDemoController from './PlayerAuthDemoController.js';

/**
 * Sandbox view node
 */
export default angular.module('centauri.sandbox', [])
  .config(($stateProvider) => {

    $stateProvider

      .state('app.sandbox', {
        abstract: true,
        url: '/sandbox',
        template: htmlSandbox,
      })

      .state('app.sandbox.player-auth', {
        url: '/player-auth',
        template: htmlPlayerAuth,
        controller: PlayerAuthDemoController,
        contorllerAs: 'vm',
      });

  });

