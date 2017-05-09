import express from 'express';
import loglevel from 'loglevel-decorator';
import HttpConfig from '../config/HttpConfig.js';

/**
 * Expose an HTTP server instance
 */
@loglevel
export default class HttpService
{
  constructor(container)
  {
    this.server = express();
    this.server.disable('x-powered-by');
    this.server.disable('etag');

    container.registerValue('httpServer', this.server);

    this.config = new HttpConfig();
    container.registerValue('httpConfig', this.config);
  }

  /**
   * Boot the service
   */
  async start()
  {
    const s = this.server;
    const port = this.config.port;

    // Gotta do this the old fashioned way due to express's event listener
    // style
    return new Promise((res) => {

      s.raw = s.listen(port, () => {
        this.log.info(`http listening on port ${port}`);
        res();
      });

      // never rejects ...

    });
  }
}
