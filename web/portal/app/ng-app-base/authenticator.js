import debug from 'debug';

const _d = debug('authenticator');

/**
 * Function that is run before hitting the app base view ndoe state, if it
 * throws, then it won't load the state
 */
export default async function authenticator($cookies, $state, netClient)
{
  _d('attempting to authenticate locally');

  // check login to see if we can update net client
  const login = $cookies.getObject('login');

  if (login) {
    netClient.masterURL = login.url;
    netClient.realm = login.realm;
    _d('restored master URL and realm from previous session');
  }

  try {

    // attempt to connect if we can
    if (!netClient.connected) {
      await netClient.connect();
    }

    if (!netClient.token) {

      // see if we have a token from a previous situation
      const token = $cookies.get('auth');

      if (!token) {
        _d('no local token');
        throw new Error();
      }

      // attempt to connect auth and verify token
      await netClient.sendCommand('auth', 'token', token);
      const {params} = await netClient.recvCommand('login');
      const {status, message} = params;

      if (status) {
        _d('login successful!');
        netClient.token = token;
      }
      else {
        _d('login failed!');
        throw new Error(message);
      }

    }

    // attempt auth call
    // await netClient.send('auth', 'player/me');
    _d('verified token... continuing on route');
  }
  catch (err) {
    _d('couldnt authenticate(' + err.message + '), throwing back to login');

    // if anything fails, go to the login state AFTER the stack frame clears
    setTimeout(() => $state.go('login'));
    throw err;
  }
}

