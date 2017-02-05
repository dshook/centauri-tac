import loglevel from 'loglevel-decorator';
import {Router} from 'express';
import {HttpHarness} from 'http-tools';
import SockHarness from 'sock-harness';
import cors from 'cors';
import SocketServer from 'socket-server';
import Component from 'models/Component';
import TokenRPC from '../api/TokenRPC.js';

/**
 * Manage booting up and register runtime components
 */
@loglevel
export default class ComponentManager
{
  constructor(
      app,
      eventEmitter,
      eventBinder,
      httpServer,
      httpConfig,
      componentsConfig
    )
  {
    this.server = httpServer;
    this.config = httpConfig;
    this.cConfig = componentsConfig;
    this.app = app;
    this.eventEmitter = eventEmitter;
    this.eventBinder = eventBinder;

    //this.eventBinder.bindInstance(this);

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
   * Get Instance of component
   */
  getComponent(name, T)
  {
    return this.registered.get(name);
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
      //sockServer.addPlugin(c => this._onHandler(c));
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
      entry.server = this.server;
      entry.restServer = rest;
      entry.sockServer = sockServer;

      entry.type = {name};
      entry.typeName = name;
      entry.isActive = true;

      // Let the component boot up
      this.log.info('starting component %s at %s', name, publicURL);
      await component.start(entry);
      this.log.info('started component %s', name);

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
