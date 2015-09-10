/**
 * RPC Client for the browser
 */
export default class WebRPCClient
{
  constructor(masterURL)
  {

    /**
     * Auth token used for secure requests
     */
    this.token = null;
  }

  /**
   * Connect to master server to get list of components to use (doesn't require
   * auth)
   */
  async connect()
  {

  }

  /**
   * Use credentials to get (and store) auth token
   */
  async auth(email, password)
  {

  }

  /**
   * Send and RPC to a component (might require auth)
   */
  async send(componentName, methodName, data)
  {

  }
}
