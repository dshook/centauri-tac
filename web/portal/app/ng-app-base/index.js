import angular from 'angular';
import uiRouter from 'angular-ui-router';
import htmlAppBase from './app-base.html';

/**
 * Root node of the view state graph
 */
export default angular.module('centauri.app-base', [
  uiRouter,
])
.config(($stateProvider) => {
  $stateProvider
    .state('app', {
      url: '/',
      views: {
        '@': {
          template: htmlAppBase,
        },
      },
    });
});


