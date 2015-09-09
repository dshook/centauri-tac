import loglevel from 'loglevel-decorator';
import PGConnection from 'postgrizzly';

/**
 * An application service that exposes a postgres facade for querying
 */
@loglevel
export default class PostgresService
{
  constructor(app)
  {
    const url = process.env.DATABASE_URL;

    if (!url) {
      throw new Error('DATABASE_URL must be set');
    }

    this.psql = new PGConnection(url);
    app.registerInstance('sql', this.psql);
  }

  async start()
  {
    await this.psql.check();
    this.log.info('connection ok');
  }
}


