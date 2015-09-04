import log from 'loglevel';

export default function inject(T)
{
  Object.defineProperty(T.prototype, 'log', {
    enumerable: false,
    configurable: true,
    get: () => log.getLogger(T.name),
  });

  return T;
}
