import angular from 'angular';
import uiRouter from 'angular-ui-router';
import htmlAppBase from './app-base.html';
import htmlNav from './nav.html';

import ngHome from '../ng-home';

/**
 * Root node of the view state graph
 */
export default angular.module('centauri.app-base', [
  uiRouter,

  // application parts
  ngHome.name,

])
.config(($stateProvider) => {
  $stateProvider
    .state('app', {
      abstract: true,
      views: {

        // root view for all app- sub views
        '@': {
          template: htmlAppBase,
        },

        // mount header in this view
        'header@app': {
          template: htmlNav,
        },
      },
    });
});


