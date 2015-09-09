import angular from 'angular';
import angularResource from 'angular-resource';
import ComponentService from './ComponentService.js';

export default angular.module('ct.portal-service', [angularResource])
  .service('components', ComponentService);


