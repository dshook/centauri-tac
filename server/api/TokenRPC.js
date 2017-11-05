import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';

/**
 * RPC handler for the token RPC. Every component implements this to allow for
 * secure connections
 */
@loglevel
export default class AuthRPC
{
  constructor(auth)
  {
    this.auth = auth;
  }

  /**
   * When a token is posted by the user, validate it and if its good, send back
   * login status change and stash in socket
   */
  @rpc.command('token')
  async token(client, token)
  {
    try {
      this.auth.validateToken(token);
    }
    catch (err) {

      // barf
      const status = false;

      //decode the jwt errors into something usable for the client
      let message = 'Invalid Login';
      if(err.name === 'TokenExpiredError'){
        message = 'Session expired, please log in again'
      }

      client.token = null;
      client.send('login', {status, message});
      return;
    }

    // update our token
    client.token = token;

    const status = true;
    const message = 'token ok!';
    client.send('login', {status, message});
  }
}
