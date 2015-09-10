import request from 'request';
import {promisifyAll} from 'bluebird';
import loglevel from 'loglevel-decorator';

const rp = promisifyAll(request);

/**
 * Transport API that can be shared on client and server
 */
@loglevel
export default class HttpTransport
{
  async get(...args)
  {
    const [resp, body] = await rp.getAsync(...args);
    this._log(resp);
    return [resp, body];
  }

  async post(...args)
  {
    const [resp, body] = await rp.postAsync(...args);
    this._log(resp);
    return [resp, body];
  }

  _log(resp)
  {
    this.log.info('%s %s - %s %s',
        resp.statusCode,
        resp.statusMessage,
        resp.request.method,
        resp.request.href);
  }
}
