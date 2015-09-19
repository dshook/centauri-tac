import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';

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
  @rpc.middleware(roles(['component']))
  async register(client, {component})
  {
    const resp = await this.components.register(component);
    client.send('component', {component: resp});
  }

  /**
   * Update component registry
   */
  @rpc.command('markComponentInactive')
  @rpc.middleware(roles(['component']))
  async markInactive(client, id)
  {
    this.log.info('setting %s to inactive', id);
    await this.components.setActive(id, false);
  }

  /**
   * Keep alive
   */
  @rpc.command('pingComponent')
  @rpc.middleware(roles(['component']))
  async ping(client, id)
  {
    await this.components.ping(id);
  }
}

