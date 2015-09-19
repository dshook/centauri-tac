export default function rolesMiddleware(allowedRoles)
{
  return function middleware(client, params, auth)
  {
    if (!auth) {
      throw new Error('no auth');
    }

    if (!Array.isArray(auth.roles)) {
      throw new Error('forbidden: user has no roles');
    }

    // if we have ANY of the roles in the array, we're good
    for (const role of allowedRoles) {
      if (~auth.roles.indexOf(role)) {
        return;
      }
    }

    throw new Error('forbidden: missing role ' + allowedRoles.join(' or '));
  };
}
