import bcrypt from 'bcrypt-nodejs';
import sha1 from 'sha1';
import loglevel from 'loglevel-decorator';
import hrtime from 'hrtime-log-decorator';


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
    var salt = bcrypt.genSaltSync(this.rounds);
    return await new Promise((resolve, reject) => {
      bcrypt.hash(data, salt, () => {}, (err, res) => {
        if(err){
          reject(err);
        }
        resolve(res);
      });
    });
  }

  /**
   * Check a bcrypt-hashed password
   */
  @hrtime('checked hash in %d ms')
  async check(data, hash)
  {
    return await new Promise((resolve, reject) => {
      bcrypt.compare(data, hash, (err, res) => {
        if(err){
          reject(err);
        }
        resolve(res);
      });
    });
  }

  //Not for password storage!
  sha1Hash(data){
    return sha1(data);
  }
}
