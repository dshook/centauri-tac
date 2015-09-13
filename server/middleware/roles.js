/**
 * Check the incoming auth token to see if it has the required roles
 */
export default function rolesMiddleware(roles = [])
{
  return function middleware(req, res, next) {
    if (!req.auth) {
      return res.status(401).send('not authorized');
    }

    if (!Array.isArray(req.auth.roles)) {
      return res.status(403).send('forbidden: user has no roles');
    }

    // if we have ANY of the roles in the array, we're good
    for (const role of roles) {
      if (~req.auth.roles.indexOf(role)) {
        return next();
      }
    }

    res.status(403).send('forbidden: missing role ' + roles.join(' or '));
  };
}
