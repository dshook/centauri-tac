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
    this.purge();
  }

  /**
   * All active components across all realms
   */
  async all()
  {
    const sql = `
      select c.*, t.*
      from components as c
      join component_types as t
        on c.component_type_id = t.id
      where c.is_active
    `;

    const types = [Component, ComponentType];

    const mapping = (c, t) => { c.type = t; return c; };

    const resp = await this.sql.tquery(...types)(sql, null, mapping);

    return resp.toArray();
  }

  /**
   * Get a component by its id
   */
  async get(id)
  {
    const resp = await this.sql.tquery(Component, ComponentType)(`

      select *
      from components as c
      join component_types t on c.component_type_id = t.id
      where c.id = @id

    `, {id}, (c, t) => { c.type = t; return c; });

    return resp.firstOrNull();
  }

  /**
   * Get active components in a specific realm
   */
  async getActive(realm)
  {
    const resp = await this.sql.tquery(Component, ComponentType)(`

      select *
      from components as c
      join component_types as t on c.component_type_id = t.id
      where
        c.realm = @realm
        and c.is_active

    `, {realm}, (c, t) => { c.type = t; return c; });

    return resp.toArray();
  }

  /**
   * Resolve the id of a component type from its name
   */
  @memoize async getTypeIdByName(name)
  {
    const resp = await this.sql.query(`
      select id from component_types where name = @name`, {name});

    const result = resp.firstOrNull();
    return result ? result.id : null;
  }

  /**
   * Set the active flag on a component
   */
  async setActive(id, state = true)
  {
    await this.sql.query(`
      update components set is_active = @state
      where id = @id`, {id, state});

    this.log.info('set component %s to active=%s', id, state);
  }

  async delete(id)
  {
    await this.sql.query(`
      delete components where id = @id`, {id});

    this.log.info('deleted component %s', id);
  }

  async purge()
  {
    //TODO: needs to be cleaned up and a find a better solution for restarting the server
    await this.sql.query(`delete from game_players; delete from games;delete from components;`);

    this.log.info('purged db');
  }

  /**
   * Update last ping
   */
  async ping(id)
  {
    await this.sql.query(`
        update components set last_ping = now()
        where id = @id`, {id});
  }

  /**
   * Cleanup zombie components
   */
  async markStaleInactive()
  {
    await this.sql.query(`
        update components set is_active = false
        where last_ping < now() - '10 seconds'::interval`);
  }

  /**
   * Add a component to the registry (side effect of updating local model)
   */
  async register(component)
  {
    if (!component.realm && component.typeName !== 'master') {
      throw new Error(
        'realm must be provided when registering a non-master component');
    }

    if (!component.version) {
      throw new Error('version must be provided when registering component');
    }

    const typeName = component.type.name;
    const typeId = await this.getTypeIdByName(typeName);

    if (!typeId) {
      throw new Error('invalid type name ' + typeName);
    }

    // update local model typeID
    component.typeId = typeId;

    const resp = await this.sql.query(`

      insert into components
        (component_type_id, version, realm, is_active,
         http_url, rest_url, ws_url)
      values
        (@typeId, @version, @realm, @isActive,
         @httpURL, @restURL, @wsURL)
      returning id, registered

    `, component);

    const data = resp.firstOrNull();

    // local model updates
    component.id = data.id;
    component.registered = data.registered;

    this.log.info(`registered new component id=${component.id}`);
    return component;
  }
}
