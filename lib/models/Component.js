import moment from 'moment';
import fromSQL from 'postgrizzly/fromSQLSymbol';

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
    this.registered = null;
    this.lastPing = null;
  }

  /**
   * From DB
   */
  static [fromSQL](resp): Component
  {
    const c = new Component();
    c.url = resp.url;
    c.id = resp.id;
    c.typeId = resp['component_type_id'];
    c.registered = resp.registered;
    c.lastPing = resp['last_ping'];

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
}
