import {route} from 'http-tools';
import {HttpError} from 'http-tools';
import file from 'fs';
var fs = Promise.promisifyAll(file);

/**
 * REST API for player stuff
 */
export default class ClientlogAPI
{
  constructor()
  {
  }

  /**
   * Register an email / username (public)
   */
  @route.get('/')
  async read(req)
  {

    try {
      var raw = await fs.readFileAsync(process.env.client_log);
      return await JSON.parse(raw);
    }
    catch (err) {
      // invalid registration
      throw new HttpError(400, err.message);
    }
  }
}

