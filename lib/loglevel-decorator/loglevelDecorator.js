import debug from 'debug';

class Logger
{
  constructor(name)
  {
    this._d = debug(name);
  }

  info(...args)
  {
    this._d(...args);
  }
}

export default function inject(T)
{
  const logger = new Logger(T.name);

  Object.defineProperty(T.prototype, 'log', {
    enumerable: false,
    configurable: true,
    get: () => logger,
  });

  return T;
}

