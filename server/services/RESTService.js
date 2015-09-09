import {HttpHarness} from 'http-tools';

/**
 * Easy way to wire up API endpoints to the server
 */
export default class RESTService
{
  constructor(app, httpServer)
  {
    const harness = new HttpHarness(httpServer, T => app.make(T));
    app.registerInstance('rest', harness);
  }
}
