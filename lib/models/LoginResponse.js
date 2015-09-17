/**
 * Information from the server about login state
 */
export default class LoginResponse
{
  constructor()
  {
    this.status = false;
    this.message = '';
  }

  /**
   * From data
   */
  fromJSON(data)
  {
    return data ? Object.assign(new LoginResponse(), data) : null;
  }
}
