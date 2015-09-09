import {default as _bcrypt} from 'bcrypt';
import {promisifyAll} from 'bluebird';
import loglevel from 'loglevel-decorator';

const bcrypt = promisifyAll(_bcrypt);

/**
 * Use bcrypt to safely hash passwords
 */
@loglevel
export default class CryptoHasher
{
  constructor({ rounds = 10 })
  {
    this.rounds = rounds;
    this.log.info('init using %d rounds of bcrypt per hash', this.rounds);
  }

  /**
   * Generate a bcrypt hash for a password
   */
  async hash(data: String): Promise<String>
  {
    const start = process.hrtime();

    const hash = await bcrypt.hashAsync(data, this.rounds);

    const [s, ns] = process.hrtime(start);
    const duration = (s + ns / 1e9) * 1000;
    this.log.info('created hash in %d ms', duration);

    return hash;
  }

  /**
   * Check a bcrypt-hashed password
   */
  async check(data: String, hash: String): Promise<Boolean>
  {
    const start = process.hrtime();

    const result = await bcrypt.compareAsync(data, hash);

    const [s, ns] = process.hrtime(start);
    const duration = (s + ns / 1e9) * 1000;

    this.log.info('checked hash in %d ms', duration);

    return result;
  }
}
