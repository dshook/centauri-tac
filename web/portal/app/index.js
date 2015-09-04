import angular from 'angular';
import ngAppBase from './ng-app-base';
import config from './config.js';

/**
 * Setup all the applications modules needed
 */
export default function setup()
{
  angular.module('centauri', [
    ngAppBase.name,
  ])
  .config(config);
}

