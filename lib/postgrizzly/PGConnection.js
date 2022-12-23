import {Client} from 'pg';
import loglevel from 'loglevel-decorator';
import sqlParams from 'sql-params';
import urlutil from 'url';
import PGResult from './PGResult.js';

/**
 * Context object for connection to a postgres db
 */
@loglevel
export default class PGConnection
{
  constructor(url, useSSL = true)
  {
    if (!url) {
      throw new TypeError('url');
    }

    this._url = url;
    this._useSSL = useSSL;

    this._client = new Client(this._getConnectionInfo());
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
  async query(sql, params = [], types = null, mapper = null)
  {
    const opts = sqlParams(sql, params);
    opts.rowMode = 'array';

    // Fire off actual query
    let result;
    try {
      result = await client.query(opts);
    }
    finally {
      // always release back to the pool
      // done();
    }

    return new PGResult(result, types, mapper);
  }

  /**
   * Determine if this connection is valid (will throw if not)
   */
  async check()
  {
    try{
      await this._client.connect();
    }catch(err){
        this.log.error('PGConnection error', err.stack)
    }

    this.log.info('PG Connected successfully')
  }

  /**
   * @private
   */
  _getConnectionInfo()
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
      statement_timeout: 10000,
      query_timeout: 10000,
      connectionTimeoutMillis: 5000,
      idle_in_transaction_session_timeout: 10000
    };
  }
}
