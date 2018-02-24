import rp from 'request-promise';
import loglevel from 'loglevel-decorator';

/**
 * Request stuff with http. Duh
 */
@loglevel
export default class HttpTransport
{
  async request(...args){
    return rp(...args);
  }
  // async get(...args)
  // {
  //   const [resp, body] = await rp.getAsync(...args);
  //   this._log(resp);
  //   return [resp, body];
  // }

  // async post(...args)
  // {
  //   const [resp, body] = await rp.postAsync(...args);
  //   this._log(resp);
  //   return [resp, body];
  // }

  // _log(resp)
  // {
  //   this.log.info('%s %s - %s %s',
  //       resp.statusCode,
  //       resp.statusMessage,
  //       resp.request.method,
  //       resp.request.href);
  // }
}
