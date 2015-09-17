import loglevel from 'loglevel-decorator';
import {Server} from 'ws';
import SocketClient from 'socket-client';
import {EventEmitter} from 'events';

/**
 * How long after we connect to send first ping (to let the connection
 * stabalize)
 */
const SETTLE_TIME = 500;

/**
 * Inject with a raw httpServer instance
 *
 * events:
 * connect(client): a client has connected
 * close(client): a client disconnected
 * command({command, params, client}): client has sent a command
 */
@loglevel
export default class SocketServer extends EventEmitter
{
  constructor(server, path = null)
  {
    super();

    if (!server) {
      throw new TypeError('server');
    }

    this.clients = [];

    // how frequently to ping / update latency with remotes
    this.pingInterval = 2000;

    // arbitrary data on the client
    this._clientData = new WeakMap();

    this._path = path;
    this._started = false;
    this._wss = null;
    this._httpServer = server;

    this._init();
  }

  /**
   * Setup server
   */
  _init()
  {
    if (this._started) {
      return;
    }

    const opts = { server: this._httpServer };

    if (this._path) {
      opts.path = this._path;
    }

    const wss = this._wss = new Server(opts);

    this.log.info('binding websocket listener to server at %s', this._path);
    wss.on('connection', ws => this._onConnection(ws));
  }

  /**
   * When a client sock connects
   */
  _onConnection(ws)
  {
    const client = new SocketClient(ws);

    this._bindClient(client);

    this.clients.push(client);

    // Constnatly ping the client
    setTimeout(() => {
      const tId = setInterval(() => client.ping(), this.pingInterval);
      this._clientData.set(client, {pingId: tId});
      client.ping();
    }, SETTLE_TIME);

    this.log.info('connected client %s (%s), %d total clients',
      client.id, client.remoteAddress, this.clients.length);

    this.emit('connect', client);
  }

  /**
   * Wire up a new client to the server
   */
  _bindClient(client)
  {
    // we never actually unbind these events, but that should be okay because
    // we wont hang on to a ref of client and it should get GCed after we're
    // done
    client.once('close', () => this._onClientClose(client));
    client.once('error', () => this._onClientError(client));
    client.on('command', (...args) => this._onClientCommand(client, ...args));
  }

  /**
   * Cleanup in the server
   */
  _onClientClose(client)
  {
    const index = this.clients.indexOf(client);
    this.clients.splice(index, 1);

    const data = this._clientData.get(client);

    clearInterval(data.pingId);

    this.log.info('closed client %s, %d remaining',
        client.id, this.clients.length);

    this.emit('close', client);
    this._clientData.delete(client);
  }

  /**
   * Handle a recieved command by firing another event
   */
  _onClientCommand(client, payload)
  {
    this.emit('command', {...payload, client});
  }

  /**
   * Dump on error
   */
  _onClientError(client)
  {
    this.log.info('problem with client %s', client.id);
    this._onClientClose(client);
  }
}
