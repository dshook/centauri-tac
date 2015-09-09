import {promisifyAll} from 'bluebird';
import express from 'express';
import loglevel from 'loglevel-decorator';
import HttpConfig from '../config/HttpConfig.js';

/**
 * Expose an HTTP server instance
 */
@loglevel
export default class HttpService
{
  constructor(app)
  {
    this.server = promisifyAll(express());
    app.registerInstance('httpServer', this.server);

    this.config = new HttpConfig();
    app.registerInstance('httpConfig', this.config);
  }

  /**
   * Boot the service
   */
  async start()
  {
    await this.server.listenAsync(this.config.port);

    this.log.info(`http listening on port ${this.config.port}`);
  }
}
