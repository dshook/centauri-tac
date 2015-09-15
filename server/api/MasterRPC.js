import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';

/**
 * RPC API for the master component
 */
@loglevel
export default class MasterRPC
{
  constructor(components)
  {
    this.components = components;
  }

  /**
   * Update component registry
   */
  @rpc.command('registerComponent')
  async register(client, {component})
  {
    const resp = await this.components.register(component);
    client.send('component', {component: resp});
  }

  /**
   * Update component registry
   */
  @rpc.command('markComponentInactive')
  async markInactive(client, id)
  {
    this.log.info('setting %s to inactive', id);
    await this.components.setActive(id, false);
  }

  /**
   * Keep alive
   */
  @rpc.command('pingComponent')
  async ping(client, id)
  {
    await this.components.ping(id);
  }
}

