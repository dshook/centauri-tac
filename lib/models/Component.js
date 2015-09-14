import moment from 'moment';
import fromSQL from 'postgrizzly/fromSQLSymbol';
import ComponentType from './ComponentType.js';

/**
 * Entry in the component registry
 */
export default class Component
{
  constructor()
  {
    this.url = null;
    this.id = null;
    this.typeId = null;
    this.type = null;
    this.registered = null;
    this.lastPing = null;
    this.master = null;
    this.masterId = null;
    this.version = null;
    this.realm = null;
  }

  /**
   * From DB
   */
  static [fromSQL](resp): Component
  {
    if (resp.id === null) {
      return null;
    }

    const c = new Component();
    c.url = resp.url;
    c.id = resp.id;
    c.typeId = resp['component_type_id'];
    c.registered = resp.registered;
    c.lastPing = resp['last_ping'];
    c.masterId = resp['master_component_id'];
    c.version = resp.version;
    c.realm = resp.realm;

    return c;
  }

  /**
   * From endpoint
   */
  static fromJSON(data)
  {
    const c = new Component();
    Object.assign(c, data);
    c.registered = moment.parseZone(data.registered);
    c.lastPing = data.lastPing ? moment.parseZone(data.lastPing) : null;
    c.type = ComponentType.fromJSON(data.type);

    return c;
  }

  /**
   * Output to endpoint
   */
  toJSON()
  {
    return {
      ...this,
      registered: this.registered.format(),
      lastPing: this.lastPing ? this.lastPing.format() : null,
    };
  }

  /**
   * Start makring it inactive after about 30 seconds
   */
  get isActive()
  {
    return moment().diff(this.lastPing, 'seconds') < 10;
  }
}
