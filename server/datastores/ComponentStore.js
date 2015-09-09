import {memoize} from 'core-decorators';
import Component from 'models/Component';
import loglevel from 'loglevel-decorator';

/**
 * Data layer around components
 */
@loglevel
export default class ComponentStore
{
  constructor(sql)
  {
    this.sql = sql;
  }

  /**
   * All components
   */
  async all()
  {
    const resp = await this.sql.tquery(Component)('select * from components');
    return resp.toArray();
  }

  async getById(id)
  {
    const resp = await this.sql.tquery(Component)(`select * from components
        where id = @id`, {id});

    return resp.firstOrNull();
  }

  /**
   * Resolve the id of a component type
   */
  @memoize async getTypeIdByName(name: String): ?Number
  {
    const resp = await this.sql.query(`select id
        from component_types
        where name = @name`, {name});

    const result = resp.firstOrNull();

    if (result) {
      return result.id;
    }

    return null;
  }

  /**
   * Get a component if there already is one
   */
  async getComponent(url, typeId): ?Component
  {
    const resp = await this.sql.tquery(Component)(`select * from components
        where url = @url and component_type_id = @typeId`, {url, typeId});

    return resp.firstOrNull();
  }

  /**
   * Update latest ping time
   */
  async pingById(id)
  {
    const resp = await this.sql.tquery(Component)(`update components
        set last_ping = now()
        where id = @id
        returning *`, {id});

    return resp.firstOrNull();
  }

  /**
   * Drop in the component
   */
  async register(url, typeName)
  {
    const typeId = await this.getTypeIdByName(typeName);

    if (!typeId) {
      throw new Error('invalid type name ' + typeName);
    }

    const existing = await this.getComponent(url, typeId);

    if (!existing) {
      const resp = await this.sql.tquery(Component)(`
          insert into components (url, component_type_id)
          values (@url, @typeId)
          returning *`, {url, typeId});

      const component = resp.firstOrNull();
      this.log.info(`registered new component id=${component.id}`);

      return component;
    }

    // Update what we have
    await this.sql.tquery(Component)(`update components
        set registered = now() where id = @id`, existing);

    this.log.info(`updated component id=${existing.id}`);

    return existing;
  }
}
