import loglevel from 'loglevel-decorator';
import Component from 'models/Component';
import NetClientError from './NetClientError.js';

/**
 * Network client... for game client -> component or component -> component
 * communication. Uses tokens for auth
 */
@loglevel
export default class NetClient
{
  constructor(masterURL, realm, httpTransport)
  {
    this._masterURL = masterURL;
    this._realm = realm;
    this._transport = httpTransport;

    this.log.info('created new NetClient for %s@%s', realm, masterURL);

    this._components = [];

    this._connected = false;

    /**
     * Auth token used for secure requests
     */
    this.token = null;
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

  /**
   * @readonly
   */
  get connected() { return this._connected; }

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

    this.log.info('disconnected (cleared component list)');
    this.token = null;
    this._connected = false;
    this._components = [];
  }

  /**
   * Connec to master and get a list of all of our components
   */
  async connect()
  {
    this.disconnect();
    this.log.info('downloading components from master');

    // barf
    if (!this.realm || !this.masterURL) {
      throw new NetClientError('realm and master URL must be set');
    }

    let resp;

    try {
      resp = await this.send(
        'master', `realm/${this.realm}/components`);
    }
    catch (err) {

      // bad problem
      throw new Error('problem with transport');

    }

    // Dump the compoent we got back into map
    this._components = resp.map(c => Component.fromJSON(c));
    this._connected = true;

    if (!resp.length) {
      this.log.info('connected, but not components were found for this realm');
    }

    return this._components;
  }

  /**
   * Login a player and set the token on this instance for future requests
   */
  async login(email, password)
  {
    const resp = await this.send(
        'auth', 'player/login', {email, password});

    this.token = resp.token;
  }

  /**
   * Call REST method on a component.
   */
  async send(componentName, methodName, data = null)
  {
    const secure = this.token;

    this.log.info('%s%s %s to %s',
        secure ? '[secure] ' : '',
        data ? 'POST' : 'GET',
        methodName,
        componentName);

    const options = {
      url: this._getComponentURL(componentName) + '/' + methodName,
      json: true,
    };

    if (secure) {
      options.auth = { bearer: this.token };
    }

    let response, body;

    // POST or GET depends on if we have data
    if (data) {
      options.body = data;
      [response, body] = await this._transport.post(options);
    }
    else {
      [response, body] = await this._transport.get(options);
    }

    this._handleErrors(response, body);

    return body;
  }

  _handleErrors(response, body)
  {
    // REST errors from API
    switch (response.statusCode) {
      case 200:
        // OK!
        break;
      case 401:
        throw new NetClientError('Invalid credentials: ' + body);
      default:
        throw new Error('Problem with transport: ' + body);
    }
  }

  /**
   * Resolve component type into URL endpoint
   */
  _getComponentURL(name)
  {
    if (name === 'master') {
      return this._masterURL + '/rest';
    }

    const list = this._components
      .filter(x => x.type.name === name);

    if (!list || !list.length) {
      throw new Error(`no registered ${name} components`);
    }

    // TODO: round-robin logic ?
    return list[0].url + '/rest';
  }
}
