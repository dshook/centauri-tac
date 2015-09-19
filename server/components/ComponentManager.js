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
      messenger,
      app,
      httpServer,
      httpConfig,
      netClient,
      componentsConfig,
      packageData,
    )
  {
    this.version = packageData.version;
    this.server = httpServer;
    this.config = httpConfig;
    this.cConfig = componentsConfig;
    this.app = app;
    this.net = netClient;
    this.messenger = messenger;

    this._components = [];

    this.net.bindInstance(this);

    this.pingMaps = new Map();

    this.registered = new Map();

    this.net.on('open', (...a) => this._onConnect(...a));
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
    this.log.info('registering %s component on %s',
        component.type.name, component.realm);

    await this.net.sendCommand('master', 'registerComponent', {component});

    // make sure it worked
    await this.net.recvCommand('component');
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
    // TODO: have this be configurable
    setInterval(() => this.net.sendCommand(
          'master', 'pingComponent', component.id), 5000);
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
      wss.pingInterval = this.cConfig.serverPingInterval;
      this.log.info('ping interval set to %s', wss.pingInterval);
      const sockServer = new SockHarness(wss, U => this.app.make(U));
      sockServer.addPlugin(c => this._onHandler(c));
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

      entry.httpURL = publicURL;
      entry.restURL = publicURL + '/rest';
      entry.wsURL = publicURL
        .replace(/^http/, 'ws')
        .replace(/\/rest$/, '');

      entry.type = {name};
      entry.realm = this.cConfig.realm;
      entry.version = this.version;
      entry.typeName = name;
      entry.isActive = true;

      // Let the component boot up
      this.log.info('starting component %s at %s', name, publicURL);
      await component.start(entry);
      this.log.info('started component %s', name);

      // master registers itself
      if (name !== 'master') {
        await this.register(entry);
      }
    }
  }

  /**
   * Whenever we bind an RPC handler that wants to recv events
   */
  async _onHandler(instance)
  {
    if (!this.messenger.bindInstance(instance)) {
      // nothing bound
      return;
    }

    await this.messenger.subscribe();
  }

  /**
   * Whenever the net client connects to a component
   */
  async _onConnect({name})
  {
    // re-subscribe if needed
    if (name === 'dispatch') {
      await this.messenger.subscribe();
    }
  }
}
