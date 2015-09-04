import angular from 'angular';
import uiRouter from 'angular-ui-router';

/**
 * Setup all the applications modules needed
 */
export default function setup()
{
  angular.module('centauri-tac', [
    uiRouter,
  ]);
}

