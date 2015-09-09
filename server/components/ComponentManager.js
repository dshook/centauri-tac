import loglevel from 'loglevel-decorator';
import {Router} from 'express';
import {HttpHarness} from 'http-tools';

/**
 * Handles components
 */
@loglevel
export default class ComponentManager
{
  constructor(app, httpServer, httpConfig)
  {
    this.server = httpServer;
    this.config = httpConfig;
    this.app = app;

    this.components = new Map();
  }

  addComponent(name, T)
  {
    if (this.components.has(name)) {
      throw new Error(`duplicate component name ${name}`);
    }

    this.components.set(name, T);
  }

  /**
   * Start all added components
   */
  async startComponents()
  {
    for (const [name, T] of this.components.entries()) {
      this.log.info('creating component "%s" via %s', name, T.name);
      const component = this.app.make(T);

      // every component gets a scoped express server instance, REST endpoint,
      // as well as the public URL its running under
      const prefix = `/components/${name}`;
      const router = new Router();
      const publicURL = this.config.publicURL + prefix;

      const rRouter = new Router();
      router.use('/rest', rRouter);
      const rest = new HttpHarness(rRouter, U => this.app.make(U));

      this.server.use(prefix, router);

      this.log.info('starting component "%s" at %s', name, publicURL);
      await component.start(router, rest, publicURL);
      this.log.info('started component "%s"', name);
    }
  }
}
