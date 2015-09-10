import angular from 'angular';
import pageHeaderHtml from './page-header.html';

/**
 * Page header directive
 */
export default angular.module('page-header', [])
  .directive('pageHeader', () => {
    return {
      restrict: 'E',
      template: pageHeaderHtml,
      scope: {
        title: '@',
        subTitle: '@',
      },
      transclude: true,
    };
  });


