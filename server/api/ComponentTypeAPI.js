import {route} from 'http-tools';

/**
 * Endpoint for all the component types
 */
export default class ComponentTypeAPI
{
  constructor(sql)
  {
    this.sql = sql;
  }

  /**
   * All them types
   */
  @route.get('/')
  async types()
  {
    return [...await this.sql.query(`select * from component_types`)];
  }
}
