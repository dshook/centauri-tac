import angular from 'angular';
import ngAppBase from './ng-app-base';

/**
 * Setup all the applications modules needed
 */
export default function setup()
{
  angular.module('centauri', [
    ngAppBase.name,
  ])
  .config(($locationProvider) => {
    $locationProvider.html5Mode(true);
  })
  .run(() => {

  });
}

