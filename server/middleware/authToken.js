import jwt from 'express-jwt';

/**
 * Use the injected authToken service to verify
 */
export default function authToken({ required = false })
{
  return function middleware() {
    if (!this.auth) {
      throw new Error('authToken requires auth instance on controller.');
    }

    const secret = this.auth.getSecret();

    // build actual middleware for this instance
    const m = jwt({
      secret,
      requestProperty: 'auth',
      credentialsRequired: required,
    });

    return m(...arguments);
  };
}
