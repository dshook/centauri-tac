import {default as _bcrypt} from 'bcrypt';
import {promisifyAll} from 'bluebird';
import loglevel from 'loglevel-decorator';
import hrtime from 'hrtime-log-decorator';

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
  @hrtime('created hash in %d ms')
  async hash(data)
  {
    return await bcrypt.hashAsync(data, this.rounds);
  }

  /**
   * Check a bcrypt-hashed password
   */
  @hrtime('checked hash in %d ms')
  async check(data, hash)
  {
    return await bcrypt.compareAsync(data, hash);
  }
}
