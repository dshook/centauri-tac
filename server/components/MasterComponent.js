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
  }

  async start(component)
  {
    const rest = component.restServer;
    const sock = component.sockServer;

    await this.cleanup();

    // Register ourselves into the DB
    const {id} = await this.components.register(component);

    this.log.info('self-registered master, id = %s', id);

    // self-ping
    // TODO: have this be configurable
    setInterval(() => this.components.ping(id), 5000);

    rest.mountController('/realm', RealmAPI);
    sock.addHandler(MasterRPC);
  }

  async cleanup()
  {
    this.log.info('cleaning up stale components');
    await this.components.markStaleInactive();
  }
}
