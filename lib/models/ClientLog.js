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
    c.levelClass = ClientLog.levelClass(data.level);
    return c;
  }

  static levelClass(level){
    if(!level) return null;
    level = level.toLowerCase();
    switch(level){
      case 'info':
      case 'warning':
        return level;
      case 'netsend':
      case 'netrecv':
        return 'primary';
      case 'error':
        return 'danger';
      default:
        return '';
    }
  }
}
