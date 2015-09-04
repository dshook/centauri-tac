import angular from 'angular';
import uiRouter from 'angular-ui-router';
import htmlHome from './home.html';

export default angular.module('centauri.home', [
  uiRouter,
])
.config(($stateProvider) => {
  $stateProvider.state('app.home', {
    url: '/',
    template: htmlHome,
  });
});

