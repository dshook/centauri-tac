import loglevel from 'loglevel-decorator';
import PGConnection from 'postgrizzly';

/**
 * An application service that exposes a postgres facade for querying
 */
@loglevel
export default class PostgresService
{
  constructor(container)
  {
    const url = process.env.DATABASE_URL;

    if (!url) {
      throw new Error('DATABASE_URL must be set');
    }

    const useSSL = process.env.PG_USE_SSL !== undefined ?
      process.env.PG_USE_SSL === 'true'
      : true;

    this.log.info(`${useSSL ? 'using' : 'not using'} SSL for PG connection`);

    this.psql = new PGConnection(url, useSSL);
    container.registerValue('sql', this.psql);
  }

  async start()
  {
    this.log.info('connecting to postgres');
    await this.psql.check();
    this.log.info('connection ok');
  }
}


