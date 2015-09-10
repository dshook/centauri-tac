import angular from 'angular';
import htmlLogin from './login.html';
import LoginController from './LoginController.js';

export default angular.module('centauri.login', [])
  .config(($stateProvider) => {
    $stateProvider
      .state('login', {
        url: '/login',
        template: htmlLogin,
        controller: LoginController,
        controllerAs: 'vm',
      });
  });


