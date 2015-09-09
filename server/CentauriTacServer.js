import loglevel from 'loglevel-decorator';
import manifest from './manifest.js';
import {autobind} from 'core-decorators';
import Application from 'billy';
import objectPath from 'object-path';

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
    this.components.forEach(this._processComponent);

    // Actually setup the services
    this.log.info('registering services');
    this.services.forEach(T => this.app.service(T));

    // Set them configs in the last setup step
    this.app.service(() => {
      for (const [k, v] of this.configs) {
        this.log.info(`setting ${k} to ${v}`);

        const [sDep, ...rest] = k.split('.');

        const dep = this.app.make(sDep);
        objectPath.set(dep, rest, v);
      }
    });

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

    // Add all configs
    const eConfigs = entry.configs || {};
    for (const key in eConfigs) {
      this.configs.push([key, eConfigs[key]]);
    }
  }
}
