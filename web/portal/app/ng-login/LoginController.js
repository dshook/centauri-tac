import ngApply from 'ng-apply-decorator';
import {debounce} from 'core-decorators';
import moment from 'moment';

export default class LoginController
{
  constructor($scope, $state, packageData, netClient, $cookies)
  {
    this.$scope = $scope;
    this.$state = $state;
    this.$cookies = $cookies;
    this.net = netClient;

    // display data
    this.version = packageData.version;

    // state data
    this.connecting = false;
    this.loggingIn = false;

    // form data
    this.email = '';
    this.password = '';
    this.remember = true;
    this.masterURL = location.origin + '/components/master';
    this.realm = 'local';

    this.realms = [];

    // restore form
    const form = $cookies.getObject('login');
    if (form) {
      this.email = form.email;
      this.masterURL = form.url;
      this.realm = form.realm;
    }

    // connect right away to master
    this.connect();
  }

  /**
   * Get realm list
   */
  @ngApply async refreshRealms()
  {
    if (!this.net.connected) {
      this.realms = [];
      return;
    }

    this.realms = await this.net.send('master', 'realm');
  }

  /**
   * Attempt to auth
   */
  @ngApply async login()
  {
    if (!this.allowLogin) {
      return;
    }

    this.error = null;
    this.loggingIn = true;

    try {
      await this.net.login(this.email, this.password);
    }
    catch (err) {
      this.error = err.message;
      return;
    }
    finally {
      this.loggingIn = false;
    }

    if (this.remember) {
      // for auth
      this.$cookies.put('auth', this.net.token, {
        expires: moment().add(1, 'year').toDate(),
      });

      // saved form if it works
      this.$cookies.putObject('login', {
        email: this.email,
        url: this.masterURL,
        realm: this.realm,
      });
    }

    // made it
    this.$state.go('app.home');
  }

  /**
   * used to disable login form when busy with something or not yet connected
   * to master
   */
  get allowLogin()
  {
    return !this.loggingIn &&
      this.net.connected &&
      !this.net.token &&
      this.net.ready;
  }

  /**
   * Connect to master server
   */
  @debounce(250) @ngApply async connect()
  {
    if (this.connecting) {
      return;
    }

    this.net.masterURL = this.masterURL;
    this.net.realm = this.realm;
    await this.net.connect();

    await this.refreshRealms();
  }
}
