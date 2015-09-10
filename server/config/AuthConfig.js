/**
 * Configuration for any auth needs
 */
export default class AuthConfig
{
  constructor()
  {

    /**
     * Secret used to sign auth tokens
     */
    this.secret = process.env.AUTH_SECRET || 'need a secret';

    /**
     * How quikcly auth tokens expire (default is 90 days)
     */
    this.tokenExpiresInMinutes = 60 * 24 * 90;
  }
}

