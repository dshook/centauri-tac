import Promise from 'bluebird';
import {promisifyAll} from 'bluebird';
import {default as _pg} from 'pg';
import sqlParams from 'sql-params';
import urlutil from 'url';
import loglevel from 'loglevel-decorator';
import moment from 'moment';
import _ from 'lodash';

const pg = promisifyAll(_pg);

/**
 * Wrapper class used to handle results returned from queries
 */
class PostgresQueryResults
{
  constructor(iterable)
  {
    this._iter = iterable;
  }

  firstOrNull(): ?Object
  {
    for (const m of this._iter) {
      return m;
    }

    return null;
  }

  toArray(): Array<Object>
  {
    return [...this._iter];
  }

  * [Symbol.iterator]()
  {
    yield* this._iter;
  }
}

/**
 * Simple SQL interface facade for postgres. Reduces interaction with the
 * database simply to providing a connection url and calling a Promise-driven
 * API.
 */
@loglevel
export default class PostgresFacade
{
  constructor(url: String, useSsl = true)
  {
    if (!url) {
      throw new TypeError('url');
    }

    this._url = url;
    this._useSsl = useSsl;
  }

  /**
   * Curried function for querying with model mapping
   */
  tquery(TTypes)
  {
    return (sql, params) => this.query(sql, params, TTypes);
  }

  /**
   * Execute a query
   */
  async query(sql: String, params = [], TTypes = null)
  {
    const opts = sqlParams(sql, params);
    opts.rowMode = 'array';

    this.log.info('query: %s', opts.text);

    const [_client, done] = await pg.connectAsync(this._getConnectionInfo());
    const client = promisifyAll(_client);

    let result;
    try {
      result = await client.queryAsync(opts);
    }
    finally {
      done();
      this._logPool();
    }

    return new PostgresQueryResults(this._mapResult(result, TTypes));
  }

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
      ssl: this._useSsl,
    };
  }

  /**
   * Determine if this facade can connect
   */
  async check(): Promise<void>
  {
    const [, done] = await pg.connectAsync(this._getConnectionInfo());
    done();
  }

  /**
   * Given the result of the query from the driver, return an iterable of
   * either the rows or an array of the rows if a join.
   * @private
   */
  * _mapResult(result, TTypes = null)
  {
    if (TTypes && !Array.isArray(TTypes)) {
      TTypes = [TTypes];
    }

    for (const row of result.rows) {
      const models = new Map();

      for (const [index, f] of result.fields.entries()) {
        const tId = f.tableID;
        let model = models.get(tId);
        if (!model) {
          model = {};
          models.set(tId, model);
        }

        let val = row[index];

        // timestamp with time zone
        if (f.dataTypeID === 1184) {
          val = val !== null ? moment.parseZone(val) : null;
        }

        model[f.name] = val;
      }

      // Either array when multiple models or single if one
      const r = [...models.values()];

      // map em out if we can
      if (TTypes) {
        for (const [index, model] of r.entries()) {
          if (index >= TTypes.length) {
            continue;
          }

          const T = TTypes[index];

          if (typeof T.fromSql === 'function') {
            r[index] = T.fromSql(model);
          }
          else {
            const m = new T();
            Object.assign(m, _.cloneDeep(model));
            r[index] = m;
          }

        }
      }

      // attempt to map into model classes
      yield r.length === 1 ? r[0] : r;
    }
  }

  /**
   * output info about the pool
   * @private
   */
  _logPool()
  {
    const pool = pg.pools.getOrCreate(this._getConnectionInfo());
    const size = pool.getPoolSize();
    const available = pool.availableObjectsCount();
    this.log.info('pool: %d / %d', size - available, size);
  }

}
