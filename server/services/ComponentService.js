import ComponentsConfig from '../config/ComponentsConfig.js';
import ComponentManager from '../components/ComponentManager.js';
import RPCSession from '../components/RPCSession.js';

/**
 * Manage and startup the components
 */
export default class ComponentService
{
  constructor(app)
  {
    this.config = new ComponentsConfig();
    app.registerInstance('componentsConfig', this.config);

    this.rpc = app.make(RPCSession);
    app.registerInstance('rpc', this.rpc);

    this.manager = app.make(ComponentManager);
    app.registerInstance('componentManager', this.manager);
  }

  async start()
  {
    return await this.manager.startComponents();
  }
}
