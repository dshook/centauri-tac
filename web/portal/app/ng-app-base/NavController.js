export default class NavController
{
  constructor(netClient, $cookies, $state)
  {
    this.net = netClient;
    this.$state = $state;
    this.$cookies = $cookies;
  }

  get displayName()
  {
    if (this.net.connected) {
      return 'User';
    }
  }

  /**
   * Wipe local token and reset net client
   */
  logout()
  {
    this.net.disconnect();
    this.$cookies.remove('auth');
    this.$state.go('login');
  }
}
