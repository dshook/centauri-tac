import loglevel from 'loglevel-decorator';
import manifest from './manifest.js';
import Application from 'billy';
import ComponentService from './services/ComponentService.js';
import ComponentsConfig from './config/ComponentsConfig.js';

/**
 * Application entry point
 */
@loglevel
export default class CentauriTacServer
{

  /**
   * Components is the list of components we actually want to run
   */
  constructor(components)
  {
    this.components = components;
    this.services = new Set();
    this.app = new Application();

    //process.on('SIGINT', () => this.stop());
    //doesn't work on windows :( https://github.com/remy/nodemon/issues/140
    //process.once('SIGUSR2', () => this.onNodemonRefresh());
  }

  /**
   * Actually boot the application
   */
  async start()
  {
    // meta / component configs
    this.app.registerInstance('componentsConfig', new ComponentsConfig());
    this.app.registerInstance('packageData', require('../package.json'));

    // Add all needed services for components
    this.log.info(`using components ${this.components.join(',')}`);
    this.components.forEach(x => this._processComponent(x));

    // register all needed services
    this.log.info('registering services for components');
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
   * When nodemon restarts dev server
   */
  async onNodemonRefresh()
  {
    this.log.info('nodemon refresh triggered');
    try {
      await this.app.stop();
    }
    finally {
      // wait a while to let cleanup finish
      setTimeout(() => process.kill(process.pid, 'SIGUSR2'), 1000);
    }
  }

  /**
   * Cleanup
   */
  async stop()
  {
    this.log.info('attempting to gracefully stop app');
    try {
      await this.app.stop();
    }
    finally {
      this.log.info('exiting');

      // wait a while to let cleanup finish
      setTimeout(() => process.exit(), 1000);
    }
  }

  /**
   * Add services to the set that we need to start
   */
  _processComponent(c)
  {
    const entry = manifest[c];
    if (!entry) {
      throw new Error(`no manifest entry for ${c}`);
    }

    // Add all needed services
    (entry.services || []).forEach(T => this.services.add(T));
  }
}
