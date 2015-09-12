import angular from 'angular';
import htmlComponentLink from './component-link.html';
import htmlRealmLink from './realm-link.html';

export default angular.module('ct.link-directives', [])

  .directive('component', () => {
    return {
      template: htmlComponentLink,
      scope: {
        component: '=',
        showRealm: '=',
      },
    };
  })
  .directive('realm', () => {
    return {
      template: htmlRealmLink,
      scope: {
        realm: '=',
      },
    };
  });

