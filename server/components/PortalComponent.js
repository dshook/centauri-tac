import path from 'path';
import express from 'express';
import loglevel from 'loglevel-decorator';
import compression from 'compression';

/**
 * Game server component that hosts the portal web applicaiton static assets
 */
@loglevel
export default class PortalComponent
{
  async start(server)
  {
    // static files
    const webroot = path.join(__dirname, '../../dist/portal');
    server.use(compression());
    server.use(express.static(webroot));

    // redirect everything else to index
    server.get('*', (req, res) => {
      res.sendFile(path.join(webroot, '/index.html'));
    });

    this.log.info('mounted portal webroot from %s', webroot);
  }
}
