import {memoize} from 'core-decorators';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';
import loglevel from 'loglevel-decorator';

/**
 * Data layer handling components
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
    const sql = `
        select * from components as c
        join component_types as t
          on c.component_type_id = t.id`;

    const resp = await this.sql.tquery(Component, ComponentType)(
        sql, null, (c, t) => { c.type = t; return c; });

    return resp.toArray();
  }

  /**
   * Get a single component
   */
  async getById(id)
  {
    const resp = await this.sql.tquery(Component)(`
        select * from components
        where id = @id`, {id});

    return resp.firstOrNull();
  }

  /**
   * Resolve the id of a component type from its name
   */
  @memoize async getTypeIdByName(name: String): ?Number
  {
    const resp = await this.sql.query(`
        select id from component_types
        where name = @name`, {name});

    const result = resp.firstOrNull();

    return result ? result.id : null;
  }

  /**
   * Get a component by its url and type ID if there already is one
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
   * Add a component (or update if its already in the registry)
   */
  async register(url, typeName)
  {
    const typeId = await this.getTypeIdByName(typeName);

    if (!typeId) {
      throw new Error('invalid type name ' + typeName);
    }

    const existing = await this.getComponent(url, typeId);

    // create brand new component
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
