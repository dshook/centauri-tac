import loglevel from 'loglevel-decorator';
import manifest from './manifest.js';
import {autobind} from 'core-decorators';
import Application from 'billy';
import ComponentService from './services/ComponentService.js';

/**
 * Application startup for all server components
 */
@loglevel
export default class CentauriTacServer
{
  constructor(components: Array<String>)
  {
    this.components = components;
    this.services = new Set();
    this.configs = [];
    this.app = new Application();
  }

  /**
   * Actually boot the application
   */
  async start()
  {
    // Add all needed services for components
    this.components.forEach(this._processComponent);

    this.services.forEach(T => this.app.service(T));

    // Component service at bottom of stack
    this.app.service(ComponentService);

    // add stuff from manifest
    this.app.service((componentManager) => {

      for (const name of this.components) {
        const T = manifest[name].TComponent;
        componentManager.addComponent(name, T);
      }

    });

    // Actually setup the services
    this.log.info('registering services');
    this.log.info('booting application');
    await this.app.start();
    this.log.info('application started');
  }

  /**
   * Process services and configs for each component we're using
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
