export default class NavController
{
  constructor(netClient, $cookies, $state)
  {
    this.net = netClient;
    this.$state = $state;
    this.$cookies = $cookies;

    this._setupMenu();
  }

  _setupMenu()
  {
    this.menuLeft = [

      {
        title: 'Server Management',
        icon: 'cogs',
        items: [
          { title: 'Component list', sref: '.sm.component-list' },
          { title: 'Player list', sref: '.players.list', disabled: true},
          { divider: true},
          { title: 'Server status', sref: '.sm.component-list', disabled: true },
        ],
      },

      {
        title: 'Sandbox',
        icon: 'flag',
        items: [
          { title: 'Player auth', sref: '.sandbox.player-auth', disabled: true },
        ],
      },

      {
        title: 'Workbench',
        icon: 'object-ungroup',
        items: [
          { title: 'Card list', sref: '.workbench.cards.list', disabled: true },
        ],
      },

    ];
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
