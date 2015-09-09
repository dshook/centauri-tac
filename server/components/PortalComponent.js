import path from 'path';
import express from 'express';
import loglevel from 'loglevel-decorator';

/**
 * Static file server for portal app
 */
@loglevel
export default class PortalComponent
{
  async start(server, publicURL)
  {
    const webroot = path.join(__dirname, '../../dist/portal');

    server.use(express.static(webroot));

    this.log.info('public URL for portal: %s', publicURL);
    this.log.info('mounted portal webroot from %s', webroot);
  }
}
