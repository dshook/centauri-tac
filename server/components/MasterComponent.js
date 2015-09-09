import ComponentAPI from '../api/ComponentAPI.js';
import ComponentTypeAPI from '../api/ComponentTypeAPI.js';

/**
 * Game server component that maintains the list of all other components
 */
export default class MasterComponent
{
  async start(server, rest)
  {
    rest.mountController('/component/type', ComponentTypeAPI);
    rest.mountController('/component', ComponentAPI);
  }
}
