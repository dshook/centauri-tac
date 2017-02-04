import loglevel from 'loglevel-decorator';
import express from 'express';
import hoganExpress from 'hogan-express';
import path from 'path';

/**
 * Game server component that hosts the portal web applicaiton static assets
 */
@loglevel
export default class PortalComponent
{
  async start(component)
  {
    const server = component.server;
    const routed = component.httpServer;

    server.engine('html', hoganExpress);
    server.set('view engine', 'html');
    server.set('views', path.resolve(__dirname, '../views'));
    server.set('layout', 'layout');
    server.use('/assets', express.static(path.join(__dirname, 'static-assets')));

    // redirect everything else to index
    routed.get('/', (req, res) => {
      res.render('app', {
        key: 'value'
      });
    });

    this.log.info('mounted log');
  }
}
