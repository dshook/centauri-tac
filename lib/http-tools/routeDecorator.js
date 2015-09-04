export const getRoutes = Symbol('get routes');

const routes = new WeakMap();

export default function routeDecorator(method, urlPattern)
{
  return (target, name) => {
    let info;

    // get list of routes
    if (!routes.has(target)) {
      info = [];
      routes.set(target, info);
      target[getRoutes] = () => info;
    }
    else {
      info = routes.get(target);
    }

    info.push([method.toLowerCase(), urlPattern, name]);
  };
}

// shortcuts
for (const m of ['get', 'put', 'post', 'delete']) {
  routeDecorator[m] = (...args) => routeDecorator(m, ...args);
}

