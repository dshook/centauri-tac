/**
 * Function that is run before hitting the app base view ndoe state, if it
 * throws, then it won't load the state
 */
export default async function authenticator($cookies, $state, netClient)
{
  try {

    // attempt to connect if we can
    if (!netClient.connected) {
      await netClient.connect();
    }

    if (!netClient.token) {
      // see if we have a token from a previous situation
      const token = $cookies.get('auth');

      if (!token) {
        throw new Error();
      }

      netClient.token = token;
    }

    // attempt auth call
    await netClient.send('auth', 'player/me');

  }
  catch (err) {

    // if anything fails, go to the login state AFTER the stack frame clears
    setTimeout(() => $state.go('login'));
    throw err;
  }
}

