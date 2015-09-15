import loglevel from 'loglevel-decorator';
import Component from 'models/Component';
import NetClientError from './NetClientError.js';
import SocketClient from 'socket-client';
import WebSocket from 'ws';
import {EventEmitter} from 'events';
import ClientBinder from './ClientBinder.js';
import {rpc} from 'sock-harness';

/**
 * Network client... for game client -> component communication for games
 * clients or component -> component communication on the server. Uses tokens
 * for auth
 *
 * Inject a httpTransport for fetching the initial component list
 */
@loglevel
export default class NetClient extends EventEmitter
{
  constructor(masterURL: String, realm: String, httpTransport)
  {
    super();
    this._masterURL = masterURL;
    this._realm = realm;
    this._transport = httpTransport;

    // used to wire up runtime instances to socket events
    this._binder = new ClientBinder(this);

    // list of components from master
    this._components = [];

    // list of socket clients for each component type (only ONE per component
    // type)
    this._socks = new Map();

    // have downloaded component list
    this._connected = false;

    // status from server
    this._loggedIn = false;
    this._loginMessage = '';

    // token used for secure requests
    this.token = null;

    // Use methods on here to handle some basic RPCs
    this.bindInstance(this);

    this.log.info('created new NetClient for %s@%s', realm, masterURL);
  }

  /**
   * Create a new net client with the same settings
   */
  clone(): NetClient
  {
    return new NetClient(
        this.masterURL, this.realm, this._transport);
  }

  /**
   * Settings master URL will clear the list of components we have
   */
  set masterURL(value)
  {
    this.disconnect();
    this._masterURL = value;
  }

  /**
   * Changing realm will clear component list
   */
  set realm(value)
  {
    this.disconnect();
    this._realm = value;
  }

  get realm() { return this._realm; }

  get masterURL() { return this._masterURL; }

  get connected() { return this._connected; }

  get loggedIn() { return this._loggedIn; }

  get loginMessage() { return this._loginMessage; }

  /**
   * If we're connected and we've found an auth server, we're ready to rock
   */
  get ready()
  {
    const haveAuth = this._components
      .find(x => x.type.name === 'auth');

    return this._connected && haveAuth;
  }

  /**
   * Wipe our list of componnets
   */
  disconnect()
  {
    if (!this.connected) {
      return;
    }

    // drop connection from all socks
    for (const client of this._socks.values()) {
      client.disconnect();
    }

    this.log.info('disconnected (cleared component list)');
    this.token = null;
    this._connected = false;
    this._components = [];
    this._loggedIn = false;
    this._loginMessage = null;
  }

  /**
   * Connec to master and get a list of all of our components
   */
  async connect()
  {
    this.disconnect();
    this.log.info('downloading component list from master');

    // connecting is just fetching the component list for our realm
    await this._refreshComponentList();
    this.log.info('found %s components', this._components.length);

    // made it (no throw)
    this._connected = true;

    return this._components;
  }

  /**
   * Post credentials to the server in hopes of getting back a token
   */
  async login(email, password)
  {
    await this.sendCommand('auth', 'login', {email, password});
    await this.waitForCommand('login');
  }

  /**
   * Got a token back from teh server
   */
  @rpc.command('auth', 'token')
  _recvToken(client, token)
  {
    this.log.info('updated token from server');
    this.token = token;
  }

  /**
   * Got an update on our login status
   */
  @rpc.command('auth', 'login')
  _recvLogin(client, {status, message})
  {
    this._loggedIn = status;
    this._loginMessage = message;

    if (status) {
      this.log.info('logged in!');
    }
    else {
      this.log.info(`login failed: ${message}`);
    }
  }

  /**
   * Send a socket command to a component
   */
  async sendCommand(name: String, command: String, params: Object)
  {
    const client = await this.getClient(name);
    await client.send(command, params);
  }

  /**
   * Wait for a specific command from the server
   */
  async recvCommand(name, timeout = 5000)
  {
    return new Promise((resolve, reject) => {

      const tId = setTimeout(() => {
        reject(new Error('timeout waiting for ' + name));
      }, timeout);

      const cb = (args) => {
        if (args.command !== name) {
          return;
        }

        clearTimeout(tId);
        this.removeListener('command', cb);
        resolve(args);
      };

      this.on('command', cb);
    });
  }

  /**
   * Get a component by type
   */
  async getComponent(name, lastChance = false)
  {
    const list = this._components
      .filter(x => x.type.name === name);

    if (!list || !list.length) {

      // Try to refresh list
      if (!lastChance) {
        this.log.info('unable to find %s, refreshing component list to make sure...', name);
        await this._refreshComponentList();
        return await this.getComponent(name, true);
      }

      throw new NetClientError(`no registered ${name} components`);
    }

    // TODO: round-robin logic ?
    return list[0];
  }

  /**
   * Every component has its own sock client
   */
  async getClient(name)
  {
    // Only 1 socket per component type
    if (this._socks.has(name)) {
      return this._socks.get(name);
    }

    this.log.info('building sock client for %s', name);

    // Create fresh client
    const url = await this._getComponentSockURL(name);
    const client = new SocketClient(new WebSocket(url));
    this._socks.set(name, client);

    // Wire up events
    client.on('command', payload => this._onCommand(name, client, payload));
    client.once('open', () => this._onOpen(name, client));
    client.once('close', () => this._onClose(name, client));

    // wait for the damn socket to come up
    await new Promise((resolve, reject) => {
      client.once('open', () => resolve(client));
      client.once('error', (e) => reject(e));
    });

    // send auth if we have it directly to the new client to authorize this pipe
    if (this.token) {
      await client.send('token', this.token);

      // wait for login
      const {params} = await this.recvCommand('login');
      const {status, message} = params;

      if (!status) {
        throw new NetClientError(
            `could not validate token with ${name}: ${message}`);
      }

      // otherwise ... made it ...
      this.log.info(`socket for ${name} is authenticated`);
    }
    else {
      this.log.info('no token so this sock is not authenticated!');
    }

    return client;
  }

  /**
   * Connect handlers
   */
  bindInstance(instance)
  {
    return this._binder.bindInstance(instance);
  }

  /**
   * Disconnect
   */
  unbindInstance(instance)
  {
    return this._binder.unbindInstance(instance);
  }

  /**
   * Refresh our list of components
   */
  async _refreshComponentList()
  {
    if (!this.realm || !this.masterURL) {
      throw new NetClientError('realm and master URL must be set');
    }

    // basic www get for list of components, will throw if a bad URL
    const masterURL = await this._getComponentURL('master');
    const url = `${masterURL}/realm/${this.realm}/components`;
    const [response, body] = await this._transport.get({url, json: true});

    // Bad server response
    if (response.statusCode !== 200) {
      throw new NetClientError(`bad http response: ${response.statusCode}`);
    }

    // Nothing there
    if (!body.length) {
      throw new NetClientError('connected, but no components on this realm');
    }

    // Dump the compoent we got back into map
    this._components = body.map(c => Component.fromJSON(c));
  }

  /**
   * Recv command on this sock and rebroadcast with info + payload
   */
  _onCommand(name, client, {command, params})
  {
    this.emit('command', {name, client, command, params});
  }

  /**
   * Cleanup whenever lose a connection
   */
  _onClose(name, client)
  {
    this.log.info('removing socket for %s, it closed', name);
    this.emit('close', {name, client});
    this._socks.delete(name);
  }

  /**
   * Hello
   */
  _onOpen(name, client)
  {
    this.log.info('socket for %s now connected', name);
    this.emit('open', {name, client});
  }

  /**
   * Just normal URL with WS protocol
   */
  async _getComponentSockURL(name)
  {
    // replace protocol and remove rest
    const url = (await this._getComponentURL(name))
      .replace(/^http/, 'ws')
      .replace(/\/rest$/, '');

    return url;
  }

  /**
   * Resolve component type into URL endpoint
   */
  async _getComponentURL(name)
  {
    if (name === 'master') {
      return this._masterURL + '/rest';
    }

    const component = await this.getComponent(name);
    return component.url;
  }
}
