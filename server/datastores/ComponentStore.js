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
  @memoize async getComponentIdFromName(name: String): ?Number
  {
    const resp = await this.sql.tquery(Component)(`select id
        from component_types
        where name = @name`, {name});

    return resp.firstOrNull();
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
  async register(component)
  {
    const existing = await this.getComponent(component.url, component.typeId);

    if (!existing) {
      const resp = await this.sql.tquery(Component)(`
          insert into components (url, component_type_id)
          values (@url, @typeId)
          returning id`, component);

      const result = resp.firstOrNull();
      component.id = result.id;
      this.log.info(`registered new component id=${component.id}`);
    }
    else {
      component.id = existing.id;

      await this.sql.tquery(Component)(`update components
          set registered = now() where id = @id`, existing);

      this.log.info(`updated component id=${existing.id}`);
    }

    return component;
  }
}
