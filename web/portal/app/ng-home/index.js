import angular from 'angular';
import uiRouter from 'angular-ui-router';
import angularMoment from 'angular-moment';
import 'ng-table';

import htmlHome from './home.html';
import HomeController from './HomeController.js';
import ngPortalService from 'ng-portal-service';

export default angular.module('centauri.home', [
  uiRouter,
  angularMoment,
  'ngTable',

  ngPortalService.name,
])
.config(($stateProvider) => {
  $stateProvider.state('app.home', {
    url: '/',
    template: htmlHome,
    controller: HomeController,
    controllerAs: 'vm',
  });
});

