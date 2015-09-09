import ComponentsConfig from '../config/ComponentsConfig.js';
import ComponentManager from '../components/ComponentManager.js';

/**
 * Manage and startup the components
 */
export default class ComponentService
{
  constructor(app)
  {
    this.config = new ComponentsConfig();
    this.manager = app.make(ComponentManager);

    app.registerInstance('componentsConfig', this.config);
    app.registerInstance('componentManager', this.manager);
  }

  async start()
  {
    return await this.manager.startComponents();
  }
}
