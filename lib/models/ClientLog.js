import moment from 'moment';

/**
 * Entry in the component registry
 */
export default class ClientLog
{
  constructor()
  {
    this.timestamp = null;
    this.level = null;
    this.key = null;
    this.message = null;
  }

  /**
   * From endpoint
   */
  static fromJSON(data)
  {
    if (!data) {
      return null;
    }

    const c = Object.assign(new ClientLog(), data);
    c.timestamp = moment.parseZone(data.timestamp);
    return c;
  }
}
