import {Router} from 'express';
import {HttpHarness} from 'http-tools';
import bodyParser from 'body-parser';

/**
 * Easy way to wire up API endpoints to the server
 */
export default class RESTService
{
  constructor(app, httpServer)
  {
    this.app = app;

    // install REST endpoint
    this.api = new Router();
    httpServer.use('/api', this.api);

    // Process data on the API
    this.api.use(bodyParser.json());
    this.api.use(bodyParser.urlencoded({ extended: true }));

    const harness = new HttpHarness(this.api, T => app.make(T));
    app.registerInstance('rest', harness);
  }
}
