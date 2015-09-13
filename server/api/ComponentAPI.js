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

  /**
   * Register a component with the master, only happens from a component
   */
  @route.post('/register')
  @middleware(roles(['component']))
  async register(req)
  {
    const {url, name, realm, version} = req.body;
    return await this.components.register(url, name, realm, version);
  }

  /**
   * Update ping time from component by id
   */
  @route.post('/ping')
  @middleware(roles(['component']))
  async ping(req)
  {
    const {id} = req.body;
    return await this.components.ping(id);
  }
}


