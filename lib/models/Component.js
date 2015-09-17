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
    this.version = null;
    this.realm = null;
    this.isActive = null;

    // express server on server
    this.httpServer = null;

    // http harness for rest controllers on server
    this.restServer = null;

    // socket server for socket shit on server
    this.sockServer = null;

    // socket client when on client
    this.sockClient = null;
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
    c.version = resp.version;
    c.realm = resp.realm;
    c.isActive = resp['is_active'];

    return c;
  }

  /**
   * From endpoint
   */
  static fromJSON(data)
  {
    if (data === null) {
      return null;
    }

    const c = new Component();
    Object.assign(c, data);
    c.registered = moment.parseZone(data.registered);
    c.type = ComponentType.fromJSON(data.type);

    return c;
  }

  /**
   * Output to endpoint
   */
  toJSON()
  {
    const json = {
      ...this,
      registered: this.registered ? this.registered.format() : null,
    };

    // unset these things
    delete json.sockServer;
    delete json.httpServer;
    delete json.restServer;
    delete json.sockClient;

    return json;
  }
}
