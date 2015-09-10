import angular from 'angular';
import htmlAppBase from './app-base.html';
import htmlNav from './nav.html';
import ngHome from '../ng-home';
import ngSm from '../ng-sm';
import ngSandbox from '../ng-sandbox';

/**
 * Root node of the view state graph
 */
export default angular.module('centauri.app-base', [

  // application parts
  ngHome.name,
  ngSm.name,
  ngSandbox.name,

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
        'nav@app': {
          template: htmlNav,
        },
      },
    });

});


