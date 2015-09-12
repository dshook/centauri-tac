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

    this._components = new Map();

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
    return this._connected && this._components.has('auth');
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
    this._components.clear();
  }

  /**
   * Connec to master and get a list of all of our components
   */
  async connect()
  {
    this.disconnect();
    this.log.info('downloading components from master');

    let resp;

    try {
      resp = await this.send(
        'master', 'realm/' + this._realm + '/components');
    }
    catch (err) {

      // bad problem
      throw new Error('problem with transport');

    }

    // Dump the compoent we got back into map
    const components = resp.map(c => Component.fromJSON(c));
    const cMap = this._components;
    for (const c of components) {
      const tName = c.type.name;
      let list;
      if (!cMap.has(tName)) {
        list = [];
        cMap.set(tName, list);
      }
      else {
        list = cMap.get(tName);
      }

      this.log.info('found %s component %s on %s@%s',
          tName, c.id, c.realm, c.url);

      list.push(c);
    }

    this._connected = true;

    if (!resp.length) {
      this.info.log('connected, but not components were found for this realm');
    }

    return components;
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

    // REST errors from API
    if (response.statusCode === 401) {
      throw new NetClientError('Invalid credentials: ' + body);
    }
    else if (response.statusCode !== 200) {
      throw new Error('Problem with transport: ' + body);
    }

    return body;
  }

  /**
   * Resolve component type into URL endpoint
   */
  _getComponentURL(name)
  {
    if (name === 'master') {
      return this._masterURL + '/rest';
    }

    const list = this._components.get(name);

    if (!list || !list.length) {
      throw new Error(`no registered ${name} components`);
    }

    // TODO: round-robin logic ?
    return list[0].url + '/rest';
  }
}
