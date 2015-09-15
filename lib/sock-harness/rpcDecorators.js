const registry = new WeakMap();

/**
 * Get meta info for an instance of class
 */
export function getMeta(instance)
{
  return registry.get(Object.getPrototypeOf(instance)) || [];
}

function rpcMiddleware(property)
{
  return (...args) => {
    return (target, name) => {

      // get existing meta info
      let metas;
      if (!registry.has(target)) {
        metas = [];
        registry.set(target, metas);
      }
      else {
        metas = registry.get(target);
      }

      metas.push({name, property, args});

    };
  };
}

export default
{
  command: rpcMiddleware('command'),
  connected: rpcMiddleware('connected'),
  disconnected: rpcMiddleware('disconnected'),
  roles: rpcMiddleware('roles'),
};

