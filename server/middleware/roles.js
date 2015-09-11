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

    for (const role of roles) {
      if (!~req.auth.roles.indexOf(role)) {
        return res.status(405).send('forbidden: user requires role ' + role);
      }
    }

    next();
  };
}
