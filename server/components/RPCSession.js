import rp from 'request-promise';
import loglevel from 'loglevel-decorator';

/**
 * For talking to all the different components
 */
@loglevel
export default class RPCSession
{
  constructor(auth, componentsConfig)
  {
    this.auth = auth;
    this._endpoints = new Map();
    this._endpoints.set('master', componentsConfig.masterURL);
  }

  /**
   * Connec to master and get a list of all of our components
   */
  async connect()
  {
    const components = await this.get('master', 'component');
    return components;
  }

  /**
   * Get data from the RPC endpoint
   */
  async get(name, methodName)
  {
    this.log.info('getting %s from %s', methodName, name);

    const opt = {
      uri: this._endpoints.get(name) + '/' + methodName,
      method: 'GET',
    };

    return await rp(Object.assign(opt, this._getOptions()));
  }

  /**
   * Send an RPC and expect a response
   */
  async send(name, methodName, data)
  {
    this.log.info('sending %s to %s', methodName, name);

    const opt = {
      uri: this._endpoints.get(name) + '/' + methodName,
      method: 'POST',
      json: true,
      body: data,
    };

    return await rp(Object.assign(opt, this._getOptions()));
  }

  _getOptions()
  {
    const token = this.auth.generateToken(null, ['component']);

    return {
      headers: {
        Authorization: 'Bearer ' + token,
      },
    };
  }
}
