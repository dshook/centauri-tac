import angular from 'angular';
import htmlComponentDirective from './component-directive.html';

export default angular.module('ct.component-directive', [])
  .directive('componentLink', () => {
    return {
      restrict: 'E',
      template: htmlComponentDirective,
      scope: {
        component: '=',
      },
    };
  });
