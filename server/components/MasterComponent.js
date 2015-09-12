import ComponentAPI from '../api/ComponentAPI.js';
import loglevel from 'loglevel-decorator';
import RealmAPI from '../api/RealmAPI.js';

const PING_INTERVAL = 5 * 1000;

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
    rest.mountController('/component', ComponentAPI);
    rest.mountController('/realm', RealmAPI);

    // Register this master directly into the database with our hardcoded realm
    const component = await this.components.register(publicURL, 'master');

    // auto ping
    setInterval(() => this.components.ping(component.id), PING_INTERVAL);

    this.log.info('master running with component id = %s', component.id);
  }
}
