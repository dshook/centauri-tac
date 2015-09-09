import path from 'path';
import express from 'express';
import loglevel from 'loglevel-decorator';

/**
 * Game server component that hosts the portal web applicaiton static assets
 */
@loglevel
export default class PortalComponent
{
  async start(server)
  {
    const webroot = path.join(__dirname, '../../dist/portal');
    server.use(express.static(webroot));
    this.log.info('mounted portal webroot from %s', webroot);
  }
}
