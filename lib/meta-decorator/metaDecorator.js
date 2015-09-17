const registry = new WeakMap();

/**
 * Get meta info for an instance of class
 */
export function getMeta(instance)
{
  return registry.get(Object.getPrototypeOf(instance)) || [];
}

/**
 * Create decorator with a property string we can use for later
 */
export default function metaDecorator(property)
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

