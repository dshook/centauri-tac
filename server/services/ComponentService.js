import ComponentManager from '../components/ComponentManager.js';
import loglevel from 'loglevel-decorator';
import manifest from '../manifest.js';

/**
 * Manage and startup the components and RPC session
 */
@loglevel
export default class ComponentService
{
  constructor(container, componentList)
  {
    this.manager = container.new(ComponentManager);
    container.registerValue('componentManager', this.manager);

    for (const name of componentList) {
      const T = manifest[name].TComponent;
      this.manager.addComponent(name, T);
    }
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
