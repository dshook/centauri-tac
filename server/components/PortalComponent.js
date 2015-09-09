import path from 'path';
import express from 'express';
import loglevel from 'loglevel-decorator';

/**
 * Static file server for portal app
 */
@loglevel
export default class PortalComponent
{
  constructor(httpServer, httpConfig)
  {
    this.httpServer = httpServer;
    this.httpConfig = httpConfig;
  }

  async start()
  {
    const webroot = path.join(__dirname, '../../dist/portal');

    this.httpServer.use('/portal', express.static(webroot));

    this.log.info('public URL for portal: %s/portal', this.httpConfig.publicURL);
    this.log.info('mounted portal webroot from %s', webroot);
  }
}
