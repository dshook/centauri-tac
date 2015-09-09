import angular from 'angular';
import ngAppBase from './ng-app-base';
import config from './config.js';

export default angular.module('centauri', [
  ngAppBase.name,
])
.config(config);

