import {Router} from 'express';
import loglevel from 'loglevel-decorator';
import {getRoutes} from './routeDecorator.js';
import HttpError from './HttpError.js';
import bodyParser from 'body-parser';
import compression from 'compression';
import {getMiddlewaresFor} from './middlewareDecorator.js';

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
   * Add a controller that has route decorators to a subroute on the API
   * root
   */
  mountController(url, T)
  {
    const controller = this._factory(T);
    const routes = controller[getRoutes]();

    // mount sub-program to root router
    const api = new Router();

    // standard stuff
    api.use(compression());
    api.use(bodyParser.json());
    api.use(bodyParser.urlencoded({ extended: true }));

    // Mount "class level" middleware at the top of this routee
    const apiMiddlewareStack = getMiddlewaresFor(T);
    if (apiMiddlewareStack) {
      api.use(...apiMiddlewareStack.map(f => f.bind(controller)));
    }

    // mount to root router
    this._router.use(url, api);

    const middlewares = getMiddlewaresFor(controller);

    // for each registered route on controller, add the handler
    for (const [method, pattern, fName] of routes) {

      let stack = [];

      // middleware?
      if (middlewares && middlewares.has(fName)) {
        stack = middlewares.get(fName).map(f => f.bind(controller));
      }

      this.log.info('Mounted %s %s%s -> %s::%s',
          method.toUpperCase(), url, pattern,
          controller.constructor.name, fName);
      api[method](pattern, ...stack, this._buildHandler(controller, fName));
    }
  }

  /**
   * Express-style middleware for an async controller method
   */
  _buildHandler(controller, fName)
  {
    return async (req, res) => {
      try {
        res.send(await controller[fName](req, res));
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

          this.log.info('unexpected error in HTTP controller %s::%s: %s',
              controller.constructor.name, fName, err);

          res.status(500)
            .type('text')
            .send(err.stack);
        }
      }
    };
  }
}
