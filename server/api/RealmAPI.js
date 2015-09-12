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
   * Get all active compoents for a realm
   */
  @route.get('/:realm/components')
  async activeComponents(req)
  {
    const {realm} = req.params;
    return await this.components.activeInRealm(realm);
  }

  /**
   * Realms currently available
   */
  @route.get('/')
  async availableRealms()
  {
    return await this.components.availableRealms();
  }

}
