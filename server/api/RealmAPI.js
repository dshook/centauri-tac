import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';

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
  @middleware(authToken())
  async activeComponents(req)
  {
    const {realm} = req.params;

    const isComponent = req.auth &&
      req.auth.roles.find(x => x === 'component');

    const all = await this.components.getActive(realm);

    if (isComponent) {
      return all;
    }

    return all.filter(x => x.type.showInClient);
  }
}
