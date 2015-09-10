import AuthConfig from '../config/AuthConfig.js';
import jwt from 'jsonwebtoken';
import loglevel from 'loglevel-decorator';

/**
 * Service that can create bearer JWT tokens
 */
@loglevel
export default class AuthTokenService
{
  constructor(app)
  {
    this.config = new AuthConfig();
    app.registerInstance('authConfig', this.config);
    app.registerInstance('auth', this);
  }

  /**
   * Serve up secret for whatev parts of the app need it
   */
  getSecret()
  {
    return this.config.secret;
  }

  /**
   * Create a JWT token
   */
  generateToken(
      user = null,
      roles = [],
      expires = this.config.tokenExpiresInMinutes)
  {
    const options = {
      expiresInMinutes: expires,
    };

    const payload = {
      user,
      roles,
    };

    this.log.info('generated token for roles %s', roles.join(','));
    return jwt.sign(payload, this.config.secret, options);
  }
}
