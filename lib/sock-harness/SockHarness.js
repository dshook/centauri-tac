import {getMeta} from 'meta-decorator';
import loglevel from 'loglevel-decorator';
import jwt from 'jsonwebtoken';

/**
 * Wrapper class to build functionality on top of a SocketServer via
 * meta-decorated classes
 *
 * @rpc.command('rpcName') to handle commands from client
 * @rpc.connected() to handle client connects
 * @rpc.disconnected() to handle client disconnects
 *
 * Not able to unbind since this is server-level and controllers are going to
 * run the entire time
 */
@loglevel
export default class SockHarness
{
  constructor(server, factory = T => new T(), eventBinder)
  {
    this._server = server;
    this._factory = factory;
    this._plugins = [];
    this.eventBinder = eventBinder;
  }

  /**
   * Add a controller that has been decorated with @rpc. Used for server-side
   * handler
   */
  addHandler(T)
  {
    const controller = this._factory(T);

    this._plugins.forEach(x => x(controller));

    this._mapCommands(controller);
    this._mapEvents(controller, 'connected', 'connect');
    this._mapEvents(controller, 'disconnected', 'close');
    this.eventBinder.bindInstance(controller);
  }

  /**
   * Add a function thats called every time a handler is created
   */
  addPlugin(plugin)
  {
    if (typeof plugin !== 'function') {
      throw new Error('plugin must be a function that takes a controller');
    }

    this._plugins.push(plugin);
  }

  /**
   * @rpc.command maps
   */
  _mapCommands(controller)
  {
    const meta = getMeta(controller);

    // map all @rpc.command()
    const commandBindings = meta.filter(x => x.property === 'command');

    for (const b of commandBindings) {
      const {name} = b;
      const [listenForCommand] = b.args;

      const middlewares = meta
        .filter(x => x.property === 'middleware' && x.name === name)
        .map(x => x.args[0]);

      // relay command down to the contoller
      this._server.on('command', async ({client, command, params}) => {
        if (command !== listenForCommand) {
          return;
        }

        // if socket is authed, provide info to the controller
        // TODO: handle errors
        let auth;
        if (client.token) {
          auth = jwt.decode(client.token);
        }

        try {
          // run all middleware, if anything throws we bounce
          // TODO: handle errors
          for (const middleware of middlewares) {
            await middleware.call(controller, client, params, auth);
          }

          await controller[name](client, params, auth);
        }
        catch (err) {
          // Probably Need some common way of addressing how an RPC from client doesnt
          // have a return value but can have an error associated with it
          this.log.warn('Middleware error', err.message);
          client.send('error', err.message);
        }
      });

      this.log.info('%s -> %s::%s',
          listenForCommand, controller.constructor.name, name);
    }
  }

  /**
   * @rpc.* calls
   */
  _mapEvents(controller, property, eventName)
  {
    const meta = getMeta(controller);

    const bindings = meta.filter(x => x.property === property);

    for (const b of bindings) {
      const fName = b.name;
      this._server.on(eventName, e => controller[fName](e));
      this.log.info('[%s] -> %s::%s',
          eventName, controller.constructor.name, fName);
    }
  }
}
