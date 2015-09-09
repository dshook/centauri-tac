import loglevel from 'loglevel-decorator';
import manifest from './manifest.js';
import {autobind} from 'core-decorators';
import Application from 'billy';
import ComponentService from './services/ComponentService.js';

/**
 * Application entry point
 */
@loglevel
export default class CentauriTacServer
{

  /**
   * Components is the list of components we actually want to run
   */
  constructor(components: Array<String>)
  {
    this.components = components;
    this.services = new Set();
    this.app = new Application();
  }

  /**
   * Actually boot the application
   */
  async start()
  {
    // Add all needed services for components
    this.components.forEach(this._processComponent);

    // register all needed services
    this.log.info('registering services');
    this.services.forEach(T => this.app.service(T));

    // Component service at bottom of stack
    this.app.service(ComponentService);

    // add stuff from manifest in an ad-hoc service
    this.app.service((componentManager) => {

      for (const name of this.components) {
        const T = manifest[name].TComponent;
        componentManager.addComponent(name, T);
      }

    });

    this.log.info('booting application');
    await this.app.start();
    this.log.info('application started');
  }

  /**
   * Add services to the set that we need to start
   */
  @autobind _processComponent(c)
  {
    this.log.info(`using component ${c}`);

    const entry = manifest[c];
    if (!entry) {
      throw new Error(`no manifest entry for ${c}`);
    }

    // Add all needed services
    (entry.services || []).forEach(T => this.services.add(T));
  }
}
