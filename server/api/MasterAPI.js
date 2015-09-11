import {route} from 'http-tools';

/**
 * REST endpoint for the master compponent
 */
export default class MasterAPI
{
  constructor(auth, components)
  {
    this.auth = auth;
    this.components = components;
  }

  /**
   * Get all components registered by this master
   */
  @route.get('/components')
  async ourComponents()
  {
    return { a: 123 };
  }
}
