import debug from 'debug';
import chalk from 'chalk';

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

  warn(str, ...args){
    this._d(chalk.bgYellow(str), ...args);
  }

  error(str, ...args){
    this._d(chalk.bgRed(str), ...args);
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

