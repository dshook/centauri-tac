import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';

/**
 * REST API dealing with the Component model
 */
@middleware(authToken({ require: false }))
export default class ComponentAPI
{
  constructor(auth, components)
  {
    this.auth = auth;
    this.components = components;
  }

  /**
   * Master list of all components, only for portal
   */
  @route.get('/')
  @middleware(roles(['admin']))
  async getAll()
  {
    return await this.components.all();
  }

  /**
   * Single component
   */
  @route.get('/:id')
  @middleware(roles(['admin']))
  async getComponent(req)
  {
    const id = 0 | req.params.id;
    return this.components.get(id);
  }
}


