import {HttpError} from 'http-tools';
import {route} from 'http-tools';
import Component from 'models/Component';

/**
 * REST API dealing with the Component model
 */
export default class ComponentAPI
{
  constructor(components)
  {
    this.components = components;
  }

  @route.get('/')
  async getAll()
  {
    return await this.components.all();
  }

  @route.post('/register')
  async register(params, req)
  {
    const {url, name} = req.body;
    return await this.components.register(url, name);
  }

  @route.post('/ping/:id')
  async ping({id})
  {
    const component = await this.components.pingById(id);

    if (!component) {
      throw new HttpError(404, 'component not found');
    }

    return component;
  }

  @route.get('/:id')
  async get({id})
  {
    const component = await this.components.getById(id);

    if (!component) {
      throw new HttpError(404, 'component not found');
    }

    return component;
  }

}


