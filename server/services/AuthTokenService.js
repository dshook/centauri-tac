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
   * Validate a token and get the decoded response, will throw if its bad
   */
  validateToken(token)
  {
    const decoded = jwt.verify(token, this.config.secret);
    return decoded;
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
      expiresIn: expires + 'm',
    };

    const payload = {
      sub: user ? {
        id: user.id,
        email: user.email,
      } : null,
      roles,
    };

    this.log.info('generated token for roles: %s', roles.join(','));
    return jwt.sign(payload, this.config.secret, options);
  }
}
