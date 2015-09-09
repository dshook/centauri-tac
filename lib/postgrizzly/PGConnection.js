import Promise from 'bluebird';
import {promisifyAll} from 'bluebird';
import {default as _pg} from 'pg';
import sqlParams from 'sql-params';
import urlutil from 'url';
import PGResult from './PGResult.js';

const pg = promisifyAll(_pg);

/**
 * Context object for connection to a postgres db
 */
export default class PGConnection
{
  constructor(url: String, useSSL = true)
  {
    if (!url) {
      throw new TypeError('url');
    }

    this._url = url;
    this._useSSL = useSSL;
  }

  /**
   * Curry function based on types
   */
  tquery(...types)
  {
    return (sql, params, mapper) => this.query(sql, params, types, mapper);
  }

  /**
   * Query the database
   */
  async query(sql: String, params = [], types = null, mapper = null)
  {
    const opts = sqlParams(sql, params);
    opts.rowMode = 'array';

    // Request pooled client
    const [_client, done] = await pg.connectAsync(this._getConnectionInfo());
    const client = promisifyAll(_client);

    // Fire off actual query
    let result;
    try {
      result = await client.queryAsync(opts);
    }
    finally {
      // always release back to the pool
      done();
    }

    return new PGResult(result, types, mapper);
  }

  /**
   * Determine if this connection is valid (will throw if not)
   */
  async check(): Promise<void>
  {
    const [, done] = await pg.connectAsync(this._getConnectionInfo());
    done();
  }

  /**
   * @private
   */
  _getConnectionInfo(): Object
  {
    const info = urlutil.parse(this._url);
    const [username, password] = info.auth.split(':');

    return {
      user: username,
      database: info.pathname.substr(1),
      password,
      port: info.port,
      host: info.hostname,
      ssl: this._useSSL,
    };
  }
}
