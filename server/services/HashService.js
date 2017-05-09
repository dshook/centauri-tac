import CryptoHasher from 'crypto-hasher';
import CryptoConfig from '../config/CryptoConfig.js';

/**
 * Crypto-hash things (like passwords)
 */
export default class HashService
{
  constructor(container)
  {
    const config = new CryptoConfig();
    container.registerValue('cryptoConfig', config);

    container.registerValue('chash', new CryptoHasher(config));
  }
}
