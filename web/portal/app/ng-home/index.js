import angular from 'angular';
import htmlHome from './home.html';

/**
 * Home screen
 */
export default angular.module('centauri.home', [])
  .config(($stateProvider) => {
    $stateProvider
      .state('app.home', {
        url: '/',
        template: htmlHome,
      });
  });

