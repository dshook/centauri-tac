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

    /**
     * Needs to be set so that store operations can link additional components
     * to this master
     */
    this.masterID = null;
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
   * Get all components that were registered by this master. Will throw if we
   * dont know the masterID yet
   */
  async allOwned()
  {
    const sql = `
        select * from components as c
        join component_types as t
          on c.component_type_id = t.id
        where c.master_component_id = @masterID
          `;

    const masterID = this.masterID;

    const resp = await this.sql.tquery(Component, ComponentType)(
        sql, {masterID}, (c, t) => { c.type = t; return c; });

    return resp.toArray();
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
    const resp = await this.sql.tquery(Component)(`
        select * from components
        where url = @url and
        component_type_id = @typeId`, {url, typeId});

    return resp.firstOrNull();
  }

  /**
   * Add a component (or update if its already in the registry)
   */
  async register(url, typeName)
  {
    if (typeName !== 'master' && !this.masterID) {
      throw new Error('Cannot register components in store without masterID');
    }

    const typeId = await this.getTypeIdByName(typeName);

    if (!typeId) {
      throw new Error('invalid type name ' + typeName);
    }

    const existing = await this.getComponent(url, typeId);

    const masterID = this.masterID;

    // create brand new component
    if (!existing) {
      const resp = await this.sql.tquery(Component)(`
          insert into components
            (url, component_type_id, master_component_id)
          values
            (@url, @typeId, @masterID)
          returning *`, {url, typeId, masterID});

      const component = resp.firstOrNull();
      this.log.info(`registered new component id=${component.id}`);

      return component;
    }

    // Update what we have
    await this.sql.tquery(Component)(`
        update components
        set registered = now(),
        master_component_id = @masterID
        where id = @id`, {...existing, masterID});

    this.log.info(`updated component id=${existing.id}`);

    return existing;
  }
}
