import ComponentAPI from '../api/ComponentAPI.js';
import loglevel from 'loglevel-decorator';
import RealmAPI from '../api/RealmAPI.js';
import MasterRPC from '../api/MasterRPC.js';

const CLEANUP_INTERVAL = 120 * 1000;

/**
 * Game server component that maintains the list of all other components
 */
@loglevel
export default class MasterComponent
{
  constructor(components)
  {
    this.components = components;

    setInterval(() => this.cleanup(), CLEANUP_INTERVAL);
    this.cleanup();
  }

  async start(component)
  {
    const rest = component.restServer;
    const sock = component.sockServer;

    rest.mountController('/component', ComponentAPI);
    rest.mountController('/realm', RealmAPI);

    sock.addHandler(MasterRPC);
  }

  async cleanup()
  {
    this.log.info('cleaning up stale components');
    this.components.markStaleInactive();
  }
}
