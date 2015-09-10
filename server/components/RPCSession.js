import rp from 'request-promise';
import loglevel from 'loglevel-decorator';

/**
 * For talking to all the different components
 */
@loglevel
export default class RPCSession
{
  constructor(componentsConfig)
  {
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

    return await rp(opt);
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

    return await rp(opt);
  }
}
