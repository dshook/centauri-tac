import loglevel from 'loglevel-decorator';
import {Router} from 'express';
import {HttpHarness} from 'http-tools';
import cors from 'cors';

const PING_INTERVAL = 5 * 1000;

/**
 * Manage booting up and register runtime components
 */
@loglevel
export default class ComponentManager
{
  constructor(app, httpServer, httpConfig, netClient, componentsConfig, packageData)
  {
    this.version = packageData.version;
    this.server = httpServer;
    this.config = httpConfig;
    this.cConfig = componentsConfig;
    this.app = app;
    this.net = netClient;

    this.registered = new Map();
  }

  /**
   * Add another component to be registered on start
   */
  addComponent(name, T)
  {
    if (this.registered.has(name)) {
      throw new Error(`duplicate component name ${name}`);
    }

    this.registered.set(name, T);
  }

  /**
   * POST to master server and ping occasionally
   */
  async register(name, url)
  {
    const realm = this.cConfig.realm;

    this.log.info('registering %s component with %s@%s', name, realm, url);

    const version = this.version;

    // RPC to register
    const component = await this.net.send(
        'master', 'component/register', { name, url, realm, version });

    // Ping master occasionally
    this.log.info('setting up ping interval %d for %s', PING_INTERVAL, name);
    setInterval(async () => {
      await this.net.send('master', 'component/ping', { id: component.id });
    }, PING_INTERVAL);

    return component;
  }

  /**
   * Start all added components
   */
  async startComponents()
  {
    for (const [name, T] of this.registered.entries()) {
      this.log.info('creating component %s via %s', name, T.name);
      const component = this.app.make(T);

      // Determine the URL this component will be visible at
      const prefix = `/components/${name}`;
      const publicURL = this.config.publicURL + prefix;

      // register this component with the master (if its not the master)
      if (name !== 'master') {
        this.log.info('registering component %s', name);
        await this.register(name, publicURL);
      }

      // base http for the component
      const router = new Router();

      // rest endpoint
      const rRouter = new Router();
      router.use('/rest', rRouter);
      rRouter.use(cors({
        allowedHeaders: [
          'content-type',
          'authorization',
        ],
      }));
      const rest = new HttpHarness(rRouter, U => this.app.make(U));

      // Mount to root of process HTTP server
      this.server.use(prefix, router);

      // Let the component boot up
      this.log.info('starting component %s at %s', name, publicURL);
      await component.start(router, rest, publicURL);
      this.log.info('started component %s', name);
    }
  }
}
