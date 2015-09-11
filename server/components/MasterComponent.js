import ComponentAPI from '../api/ComponentAPI.js';
import ComponentTypeAPI from '../api/ComponentTypeAPI.js';
import loglevel from 'loglevel-decorator';
import MasterAPI from '../api/MasterAPI.js';

/**
 * Game server component that maintains the list of all other components
 */
@loglevel
export default class MasterComponent
{
  constructor(components)
  {
    this.components = components;
  }

  async start(server, rest, publicURL)
  {
    rest.mountController('/component/type', ComponentTypeAPI);
    rest.mountController('/component', ComponentAPI);
    rest.mountController('/', MasterAPI);

    // Register this master directly into the database
    const component = await this.components.register(publicURL, 'master');

    this.log.info('master running with component id = %s', component.id);
    this.components.masterID = component.id;
  }
}
