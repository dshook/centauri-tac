import loglevel from 'loglevel-decorator';
import angular from 'angular';
import app from './app';

@loglevel
export default class WebPortal
{
  constructor()
  {
    this.log.info('web portal started');
  }

  async start()
  {
    angular.element(document).ready(() => this._bootAngular());
  }

  _bootAngular()
  {
    this.log.info('document ready, setting up angular app');

    // setup application angular module
    app();

    this.log.info('booting angular');

    angular.bootstrap(document, ['centauri-tac']);

    this.log.info('booted angular');
  }
}
