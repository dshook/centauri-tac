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
   * Post information to a REST endpoint (server-side use only)
   */
  async post(component, url, data)
  {
    // try again
    if (typeof component === 'string') {
      const c = await this.getComponent(component);
      return this.post(c, url, data);
    }

    const opts = {
      url: component.restURL + '/' + url,
      auth: {
        bearer: this.token,
      },
      json: true,
      method: 'POST',
      body: data,
    };

    this.log.info('POST %s', opts.url);

    const [response, body] = await this._transport.post(opts);

    // Bad server response
    if (response.statusCode !== 200) {
      throw new NetClientError(`bad http response: ${response.statusCode}`);
    }

    return body;
  }

  /**
   * This will add a component manually to our list fetched from master, it
   * will rpelace any existing components of the same name.
   *
   * It will get wiped next disconnect/reconnnect
   *
   * Mostly used for when we add an ad-hoc game component to start talking to
   * our game server.
   */
  addComponent(component)
  {
    const index = this._components
      .findIndex(x => x.type.id === component.type.id);

    // remove and disconnect the sock
    if (~index) {
      const oldSock = this._socks.get(component.type.name);
      if (oldSock) {
        oldSock.disconnect();
      }
      this.log.info('popping out existing component %s', component.type.name);
      this._components.splice(index, 1);
    }

    this._components.push(component);
    this.log.info('manual add of component %s: %s@%s',
        component.type.name, component.realm, component.wsURL);
  }

  /**
   * Remove a component from our list by name. Also drops any connection if one
   * is present
   */
  removeComponent(name)
  {
    const index = this._components.findIndex(x => x.type.name === name);

    if (~index) {
      this._components.splice(index, 1);
      this.log.info('removed component %s', name);
    }

    const oldSock = this._socks.get(name);

    if (oldSock) {
      oldSock.disconnect();
    }
  }

  /**
   * Wait for a specific command from the server
   */
  async recvCommand(name, predicate = () => true, timeout = 5000)
  {
    return new Promise((resolve, reject) => {

      let tId = null;

      const cb = (args) => {
        if (args.command !== name) {
          return;
        }

        if (!predicate(args)) {
          return;
        }

        clearTimeout(tId);
        this.removeListener('command', cb);
        resolve(args);
      };

      tId = setTimeout(() => {
        this.removeListener('command', cb);
        reject(new Error('timeout waiting for ' + name));
      }, timeout);

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
        this.log.info(
            'unable to find %s, refreshing component list to make sure...', name);
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
    const component = await this.getComponent(name);
    if (!component.wsURL) {
      throw new NetClientError(`component ${name} does not have a socket URL`);
    }

    // since we've suspended executing (via await) earlier, there's a chance
    // another part of the application has succesfully grabbed the URL already
    // and built a socket! lets check one more time just in case... dat async
    // life brah

    if (this._socks.has(name)) {
      return this._socks.get(name);
    }

    const client = new SocketClient(new WebSocket(component.wsURL));
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

    const opts = {};

    // authenticate (for server-side stuff)
    if (this.token) {
      opts.auth = { bearer: this.token };
    }

    // basic www get for list of components, will throw if a bad URL
    const url = `${this.masterURL}/rest/realm/${this.realm}/components`;
    const [response, body] =
      await this._transport.get({...opts, url, json: true});

    this._handleResponse(response, body);

    // Dump the compoent we got back into map
    this._components = body.map(c => Component.fromJSON(c));
  }

  /**
   * Process WWW response for master list
   */
  _handleResponse(response, body)
  {
    // Bad server response
    if (response.statusCode !== 200) {
      throw new NetClientError(`bad http response: ${response.statusCode}`);
    }

    // Nothing there
    if (!body.length) {
      throw new NetClientError('connected, but no components on this realm');
    }
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
}
