import {route} from 'http-tools';

/**
 * Get information about a realm
 */
export default class RealmAPI
{
  constructor(auth, components)
  {
    this.auth = auth;
    this.components = components;
  }

  /**
   * Public endpoint. Get all active compoents for a realm. Primary use of the
   * master controller is to provide this shit to the clients
   */
  @route.get('/:realm/components')
  async activeComponents(req)
  {
    const {realm} = req.params;
    return await this.components.getActive(realm);
  }
}
