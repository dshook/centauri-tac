import {Router} from 'express';
import loglevel from 'loglevel-decorator';
import {getRoutes} from './routeDecorator.js';
import HttpError from './HttpError.js';

/**
 * Helper class to build out an API on top of an express-style router via
 * declarative decorators
 */
@loglevel
export default class HttpHarness
{
  constructor(router, factory = T => new T())
  {
    this._factory = factory;
    this._router = router;
  }

  /**
   * Add a controller to a subroute on the API program
   */
  mountController(url, T)
  {
    const controller = this._factory(T);
    const routes = controller[getRoutes]();

    // mount sub-program to root router
    const api = new Router();
    this._router.use(url, api);

    // for each registered route on controller, mount new rout
    for (const [method, pattern, fName] of routes) {
      this.log.info('%s %s%s -> %s::%s',
          method.toUpperCase(), url, pattern,
          controller.constructor.name, fName);
      api[method](pattern, this._buildHandler(controller, fName));
    }
  }

  /**
   * Express-style middleware for an async controller method
   */
  _buildHandler(controller, fName)
  {
    return async (req, res) => {
      try {
        res.send(await controller[fName](req.params, req, res));
      }
      catch (err) {

        // Purposeful HTTP error, send info back
        if (err instanceof HttpError) {
          res.status(err.statusCode)
            .type('text')
            .send(err.message);
        }

        // Otherwise dump stack
        else {
          res.status(500)
            .type('text')
            .send(err.stack);
        }
      }
    };
  }

}
