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
  constructor(sql, packageData)
  {
    this.sql = sql;

    this.version = packageData.version;

    /**
     * Needs to be set so that store operations can link additional components
     * to this master
     */
    this.masterID = null;
  }

  /**
   * All components
   */
  async all(owned = false, id = null)
  {
    let sql = `
      select c.*, t.*, m.*,tm.*
      from components as c
      join component_types as t
        on c.component_type_id = t.id
      left join components as m
        on c.master_component_id = m.id
      left join component_types as tm
        on m.component_type_id = tm.id
    `;

    const params = {};

    if (owned) {
      sql += ` where c.master_component_id = @masterID `;
      params.masterID = this.masterID;
    }

    if (id !== null) {
      sql += ` where c.id = @id `;
      params.id = id;
    }

    const types = [
      Component, ComponentType, Component, ComponentType
    ];

    const mapping = (c, t, m, tm) => {
      c.type = t;
      c.master = m;
      if (c.master) {
        c.master.type = tm;
      }
      return c;
    };

    const resp = await this.sql.tquery(...types)(sql, params, mapping);

    return resp.toArray();
  }

  /**
   * Get all components that were registered by this master. Will throw if we
   * dont know the masterID yet
   */
  async allOwned()
  {
    return this.all(true);
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
   * Get a component by its id
   */
  async get(id)
  {
    const resp = await this.all(false, id);
    return resp ? resp[0] : null;
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
    const version = this.version;

    // create brand new component
    if (!existing) {
      const resp = await this.sql.tquery(Component)(`
          insert into components
            (url, component_type_id, master_component_id, version)
          values
            (@url, @typeId, @masterID)
          returning *`, {url, typeId, masterID, version});

      const component = resp.firstOrNull();
      this.log.info(`registered new component id=${component.id}`);

      return component;
    }

    // Update what we have
    await this.sql.tquery(Component)(`
        update components
        set registered = now(),
        master_component_id = @masterID,
        version = @version
        where id = @id`, {...existing, masterID, version});

    this.log.info(`updated component id=${existing.id}`);

    return existing;
  }
}
