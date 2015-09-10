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
    // static files
    const webroot = path.join(__dirname, '../../dist/portal');
    server.use(express.static(webroot));

    // redirect everything else to index
    server.get('*', (req, res) => {
      res.sendfile(path.join(webroot, '/index.html'));
    });

    this.log.info('mounted portal webroot from %s', webroot);
  }
}
