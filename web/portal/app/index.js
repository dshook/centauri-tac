import angular from 'angular';
import ngAppBase from './ng-app-base';
import config from './config.js';
import uiRouter from 'angular-ui-router';
import angularMoment from 'angular-moment';
import 'ng-table';
import ngPageHeader from './ng-page-header';
import uibs from 'angular-ui-bootstrap';
import HttpTransportProvider from './services/HttpTrasnportService.js';
import NetClientProvider from './services/NetClientProvider.js';
import ngLogin from './ng-login';
import packageData from '../../../package.json';
import ngCookies from 'angular-cookies';
import ngLinks from './ng-links';

export default angular.module('centauri', [

  // low level stuff
  ngCookies,

  // UI stuff
  uiRouter,
  'ngTable',
  angularMoment,
  uibs,
  ngPageHeader.name,
  ngLinks.name,

  // view node roots
  ngAppBase.name,
  ngLogin.name,

])
.config(config)
.value('packageData', packageData)
.provider('netClient', NetClientProvider)
.provider('httpTransport', HttpTransportProvider);

