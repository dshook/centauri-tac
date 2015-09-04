import CryptoHasher from 'crypto-hasher';
import CryptoConfig from '../config/CryptoConfig.js';

/**
 * Crypto-hash things (like passwords)
 */
export default class HashService
{
  constructor(app)
  {
    const config = new CryptoConfig();
    app.registerInstance('cryptoConfig', config);

    app.registerInstance('chash', new CryptoHasher(config));
  }
}
