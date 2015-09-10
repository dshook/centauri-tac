/**
 * Check the incoming auth token to see if it has the required roles
 */
export default function rolesMiddleware(roles = [])
{
  return function middleware(req, res, next) {
    if (!req.auth) {
      return res.status(405).send('no auth');
    }

    if (!Array.isArray(req.auth.roles)) {
      return res.status(405).send('no roles');
    }

    for (const role of roles) {
      if (!~req.auth.roles.indexOf(role)) {
        return res.status(405).send('requires role ' + role);
      }
    }

    next();
  };
}
