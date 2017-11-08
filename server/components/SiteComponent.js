import loglevel from 'loglevel-decorator';
import express from 'express';
// import hoganExpress from 'hogan-express';
import path from 'path';
// import fsp from 'fs-promise';

/**
 * Game server component that hosts the portal web applicaiton static assets
 */
@loglevel
export default class PortalComponent
{
  async start(component)
  {
    const server = component.server;
    // const routed = component.httpServer;

    // server.engine('html', hoganExpress);
    // server.set('view engine', 'html');
    // server.set('views', path.resolve(__dirname, '../views'));
    // server.set('layout', 'layout');
    server.use('/', express.static(path.resolve(__dirname, '../static-assets')));
    server.use('/assets', express.static(path.resolve(__dirname, '../static-assets')));

    // routed.get('/', async (req, res) => {
    //   try{
    //     res.render('app', {groupedLogs});
    //   }catch(e){
    //     this.log.error('Error in logs', e);
    //     res.write(e.message);
    //     res.end();
    //   }
    // });

    this.log.info('mounted site');
  }

}
