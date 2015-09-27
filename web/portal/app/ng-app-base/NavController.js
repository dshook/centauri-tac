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
          { title: 'Component List', sref: '.sm.component-list' },
          { title: 'Player List', sref: '.players.list', disabled: true},
          { divider: true},
          { title: 'Server Status', sref: '.sm.component-list', disabled: true },
        ],
      },

      {
        title: 'Sandbox',
        icon: 'flag',
        items: [
          { title: 'Player Network Flow', sref: '.sandbox.player-auth' },
        ],
      },

      {
        title: 'Workbench',
        icon: 'object-ungroup',
        items: [
          { title: 'Error Log', sref: '.workbench.client-log'},
          { title: 'Card list', sref: '.workbench.cards.list', disabled: true },
        ],
      },

    ];
  }

  get masterComponent()
  {
    return this.net.getComponentsByType('master')[0];
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
