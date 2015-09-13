import angular from 'angular';
import html from './panel.html';

export default angular.module('twbs-panels', [])
  .directive('panel', () => {
    return {
      template: html,
      transclude: true,
      scope: {
        title: '@',
        subTitle: '@',
        icon: '@',
        type: '@',
      },
    };
  });
