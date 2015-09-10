const stacks = new WeakMap();

/**
 * object instance -> map of name to middleware stacks
 */
export function getMiddlewaresFor(instance)
{
  // If passed a constructor, get class-level decorators
  if (typeof instance === 'function') {
    const m = stacks.get(instance);
    return m ? m.get('') : null;
  }

  // otherwise go off the prototype targeted for method-level decorators
  const proto = Object.getPrototypeOf(instance);
  return stacks.get(proto);
}

export default function middlewareRouter(middleware)
{
  return (target, name = '') => {

    // this target's map of name -> stacks
    let middlewares;

    if (!stacks.has(target)) {
      middlewares = new Map();
      stacks.set(target, middlewares);
    }
    else {
      middlewares = stacks.get(target);
    }

    // The stacks of middleware
    let info;

    if (!middlewares.has(name)) {
      info = [];
      middlewares.set(name, info);
    }
    else {
      info = middlewares.get(name);
    }

    // add item to beginning of stack since the decorators are processed FILO
    // but we want middleware run in the order its listed
    info.unshift(middleware);
  };
}
