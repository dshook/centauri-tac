import ComponentManager from '../components/ComponentManager.js';
import loglevel from 'loglevel-decorator';

/**
 * Manage and startup the components and RPC session
 */
@loglevel
export default class ComponentService
{
  constructor(container)
  {
    this.manager = container.new(ComponentManager);
    container.registerValue('componentManager', this.manager);
  }

  async start()
  {
    this.log.info('starting all components');
    await this.manager.startComponents();
  }

  async stop()
  {
    await this.manager.shutdown();
    this.log.info('shut down manager');
  }
}
