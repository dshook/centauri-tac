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
  }

  /**
   * All (recently active) components
   */
  async all()
  {
    const sql = `
      select c.*, t.*
      from components as c
      join component_types as t
        on c.component_type_id = t.id
      where c.last_ping >= now() - '1 hour'::interval
    `;

    const types = [Component, ComponentType];

    const mapping = (c, t) => { c.type = t; return c; };

    const resp = await this.sql.tquery(...types)(sql, null, mapping);

    return resp.toArray();
  }

  /**
   * Get all active components in a specific realm (active in last 30 seconds)
   */
  async activeInRealm(realm)
  {
    const resp = await this.sql.tquery(Component)(`

      select *
      from components as c
      join component_types as t on c.component_type_id = t.id
      where
        c.realm = @realm
        and c.last_ping >= now() - '10 seconds'::interval

    `, {realm}, (c, t) => { c.type = t; return c; });

    return resp.toArray();
  }

  /**
   * Unique, active realms with at least an auth component
   */
  async availableRealms()
  {
    const resp = await this.sql.query(`
      select distinct realm
      from components c
      join component_types t on c.component_type_id = t.id
      where c.realm is not null
      and c.last_ping >= now() - '10 seconds'::interval
      and t.name = 'auth'`);

    const realms = resp.toArray().map(x => { return { name: x.realm }; });

    return realms;
  }

  /**
   * Update ping time by id
   */
  async ping(id)
  {
    const resp = await this.sql.query(`
      update components
        set last_ping = now()
      where id = @id returning id`, {id});

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
   * Add a component
   */
  async register(url, typeName, realm = null)
  {
    if (!realm && typeName !== 'master') {
      throw new Error(
        'realm must be provided when registering a non-master component');
    }

    const typeId = await this.getTypeIdByName(typeName);

    if (!typeId) {
      throw new Error('invalid type name ' + typeName);
    }

    const version = this.version;

    const resp = await this.sql.tquery(Component)(`

      insert into components
        (url, component_type_id, version, realm)
      values
        (@url, @typeId, @version, @realm)
      returning *

      `, {url, typeId, version, realm});

    const component = resp.firstOrNull();

    this.log.info(`registered new component id=${component.id}`);
    return component;
  }
}
