import angular from 'angular';
import ngAppBase from './ng-app-base';
import config from './config.js';
import uiRouter from 'angular-ui-router';
import angularMoment from 'angular-moment';
import 'ng-table';
import ngPageHeader from './ng-page-header';
import uibs from 'angular-ui-bootstrap';
import ngPortalService from './ng-portal-service';

export default angular.module('centauri', [

  // UI stuff
  uiRouter,
  'ngTable',
  angularMoment,
  uibs,
  ngPageHeader.name,

  // services
  ngPortalService.name,

  // view node roots
  ngAppBase.name,

])
.config(config);

