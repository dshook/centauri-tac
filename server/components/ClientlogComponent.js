import ClientlogAPI from '../api/ClientlogAPI.js';

/**
 * Serve up some lumber
 */
export default class ClientlogComponent
{
  async start(component)
  {
    //const {sockServer} = component;
    const rest = component.restServer;

    rest.mountController('/read', ClientlogAPI);
  }
}
