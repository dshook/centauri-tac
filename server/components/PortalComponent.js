import path from 'path';
import express from 'express';
import loglevel from 'loglevel-decorator';

/**
 * Static file server for portal app
 */
@loglevel
export default class PortalComponent
{
  constructor(httpServer)
  {
    this.httpServer = httpServer;
  }

  async start()
  {
    const webroot = path.join(__dirname, '../../dist/portal');

    this.httpServer.use('/portal', express.static(webroot));

    this.log.info('mounted portal webroot from %s', webroot);
  }
}
