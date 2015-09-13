import {route} from 'http-tools';

/**
 * Get information about a realm
 */
export default class RealmAPI
{
  constructor(components)
  {
    this.components = components;
  }

  /**
   * Get all active compoents for a realm. Primary use of the master controller
   * is to provide this shit to the clients
   * TODO: decorator for C# client REST
   */
  @route.get('/:realm/components')
  @route.post('/:realm/components')
  async activeComponents(req)
  {
    const {realm} = req.params;
    return await this.components.activeInRealm(realm);
  }
}
