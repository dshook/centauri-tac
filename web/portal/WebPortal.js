import loglevel from 'loglevel-decorator';
import angular from 'angular';
import app from './app';

@loglevel
export default class WebPortal
{
  constructor()
  {
    this.log.info('portal web app starting');
  }

  async start()
  {
    angular.element(document).ready(() => this._bootAngular());
  }

  _bootAngular()
  {
    // setup application angular module
    app();
    angular.bootstrap(document, ['centauri']);
    this.log.info('booted angular');
  }
}
