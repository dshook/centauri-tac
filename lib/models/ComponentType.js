import fromSQL from 'postgrizzly/fromSQLSymbol';

export default class ComponentType
{
  constructor()
  {
    this.id = null;
    this.name = null;
    this.showInClient = null;
    this.enableREST = null;
    this.enableHTTP = null;
    this.enableWS = null;
  }

  /**
   * From DB
   */
  static [fromSQL](resp)
  {
    const ct = new ComponentType();
    ct.id = resp.id;
    ct.name = resp.name;
    ct.showInClient = resp['show_in_client'];
    ct.enableREST = resp['enable_rest'];
    ct.enableHTTP = resp['enable_http'];
    ct.enableWS = resp['enable_ws'];

    return ct;
  }

  /**
   * From data
   */
  static fromJSON(data)
  {
    return data ? Object.assign(new ComponentType(), data) : null;
  }
}
