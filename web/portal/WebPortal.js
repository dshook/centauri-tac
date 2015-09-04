import loglevel from 'loglevel-decorator';
import angular from 'angular';
import app from './app';

@loglevel
export default class WebPortal
{
  async start()
  {
    angular.element(document).ready(() => this._bootAngular());
  }

  _bootAngular()
  {
    // setup application angular module
    angular.bootstrap(document, [app.name]);
    this.log.info('booted angular');
  }
}
