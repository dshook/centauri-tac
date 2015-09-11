import loglevel from 'loglevel-decorator';
import {Router} from 'express';
import {HttpHarness} from 'http-tools';

const PING_INTERVAL = 10 * 1000;

/**
 * Manage booting up and register runtime components
 */
@loglevel
export default class ComponentManager
{
  constructor(app, httpServer, httpConfig, netClient, components)
  {
    this.server = httpServer;
    this.config = httpConfig;
    this.app = app;
    this.net = netClient;
    this.components = components;

    this.registered = new Map();
  }

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
    this.log.info('registering %s@%s', name, url);

    const component = await this.net.send('master', 'component/register', { name, url });

    if (name !== 'master') {

      this.log.info('setting up ping interval %d for %s', PING_INTERVAL, name);

      // TODO: should we manage this timer to stop it at some point? component
      // lifecycle currently is for the entire application's lifecycle....
      setInterval(async () => {
        await this.net.send('master', 'component/ping', { id: component.id });
      }, PING_INTERVAL);

    }

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

      // register this component with the master
      let entry = null;
      if (name !== 'master') {
        this.log.info('registering component %s', name);
        entry = await this.register(name, publicURL);
      }

      // base http for the component
      const router = new Router();

      // show component id in all requests
      router.use((req, res, next) => {
        res.set('component-id', entry ? entry.id : this.components.masterID);
        next();
      });

      // rest endpoint
      const rRouter = new Router();
      router.use('/rest', rRouter);
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
