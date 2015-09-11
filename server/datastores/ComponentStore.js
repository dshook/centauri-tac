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
      where c.last_ping >= now() - '1 hour'::interval
    `;

    const params = {};

    // just ones for a specific master
    if (owned) {
      sql += ' and c.master_component_id = @masterID ';
      params.masterID = this.masterID;
    }

    // if we just want a single one
    if (id !== null) {
      sql += ' where c.id = @id ';
      params.id = id;
    }

    const types = [
      Component, ComponentType, Component, ComponentType,
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
   * Update ping time by id
   */
  async ping(id)
  {
    const resp = await this.sql.query(`update components
        set last_ping = now()
        where id = @id returning id`, {id});

    return resp.firstOrNull();
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

    const masterID = this.masterID;
    const version = this.version;

    const resp = await this.sql.tquery(Component)(`
      insert into components
        (url, component_type_id, master_component_id, version)
      values
        (@url, @typeId, @masterID, @version)
      returning *`, {url, typeId, masterID, version});

    const component = resp.firstOrNull();

    this.log.info(`registered new component id=${component.id}`);

    return component;
  }
}
