import ComponentsConfig from '../config/ComponentsConfig.js';
import ComponentManager from '../components/ComponentManager.js';
import RPCSession from '../components/RPCSession.js';
import loglevel from 'loglevel-decorator';

/**
 * Manage and startup the components and RPC session
 */
@loglevel
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
    this.log.info('starting all components');
    await this.manager.startComponents();

    this.log.info('connecting to RPC session');
    await this.rpc.connect();
  }
}
