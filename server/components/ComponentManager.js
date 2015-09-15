import loglevel from 'loglevel-decorator';
import {Router} from 'express';
import {HttpHarness} from 'http-tools';
import SockHarness from 'sock-harness';
import cors from 'cors';
import SocketServer from 'socket-server';
import Component from 'models/Component';
import {rpc} from 'sock-harness';
import TokenRPC from '../api/TokenRPC.js';

/**
 * Manage booting up and register runtime components
 */
@loglevel
export default class ComponentManager
{
  constructor(
      app, httpServer, httpConfig, netClient, componentsConfig, packageData)
  {
    this.version = packageData.version;
    this.server = httpServer;
    this.config = httpConfig;
    this.cConfig = componentsConfig;
    this.app = app;
    this.net = netClient;

    this._components = [];

    this.net.bindInstance(this);

    this.pingMaps = new Map();

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
  async register(component)
  {
    this.log.info('registering %s component with %s@%s',
        component.type.name, component.realm, component.url);

    await this.net.sendCommand('master', 'registerComponent', {component});
  }

  /**
   * Cleanup all components in the registry
   */
  async shutdown()
  {
    this.log.info('going down!');

    for (const c of this._components) {
      await this.net.sendCommand('master', 'markComponentInactive', c.id);
    }

    this.log.info('deregistered all managed components');
  }

  /**
   * Whenever master sends us a new component we're in charge of
   */
  @rpc.command('master', 'component')
  recvComponent(client, {component})
  {
    this._components.push(component);
    this.log.info('now managing %s, %s components total recv from master',
        component.type.name, this._components.length);

    // setup pings
    setInterval(() => this.net.sendCommand('master', 'pingComponent', component.id), 5000);
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

      // base http for the component
      const router = new Router();

      // socket server
      const raw = this.server.raw;
      const wss = new SocketServer(raw, prefix);
      const sockServer = new SockHarness(wss, U => this.app.make(U));
      sockServer.addHandler(TokenRPC);

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

      // build component entry
      const entry = new Component();
      entry.httpServer = router;
      entry.restServer = rest;
      entry.sockServer = sockServer;

      entry.url = publicURL;
      entry.type = {name};
      entry.realm = this.cConfig.realm;
      entry.version = this.version;
      entry.typeName = name;
      entry.isActive = true;

      await this.register(entry);

      // Let the component boot up
      this.log.info('starting component %s at %s', name, publicURL);
      await component.start(entry);
      this.log.info('started component %s', name);
    }
  }
}
