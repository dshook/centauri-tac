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
   * Master list of all components, public endpoint
   */
  @route.get('/')
  async getAll()
  {
    return await this.components.all();
  }

  /**
   * Register a component with the master
   */
  @route.post('/register')
  @middleware(roles(['component']))
  async register(req)
  {
    const {url, name} = req.body;
    return await this.components.register(url, name);
  }
}


