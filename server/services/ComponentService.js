import ComponentManager from '../components/ComponentManager.js';
import loglevel from 'loglevel-decorator';

/**
 * Manage and startup the components and RPC session
 */
@loglevel
export default class ComponentService
{
  constructor(app)
  {
    this.manager = app.make(ComponentManager);
    app.registerInstance('componentManager', this.manager);
  }

  async start()
  {
    this.log.info('starting all components');
    await this.manager.startComponents();
  }
}
