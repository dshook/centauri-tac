import angular from 'angular';
import htmlComponentLink from './component-link.html';
import htmlRealmLink from './realm-link.html';
import htmlPlayerLink from './player-link.html';
import htmlGameLink from './game-link.html';

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
  .directive('game', () => {
    return {
      template: htmlGameLink,
      scope: {
        game: '=',
      },
    };
  })
  .directive('player', () => {
    return {
      template: htmlPlayerLink,
      scope: {
        player: '=',
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

