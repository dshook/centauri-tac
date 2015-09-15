import ngApply from 'ng-apply-decorator';
import {debounce} from 'core-decorators';
import moment from 'moment';
import {rpc} from 'sock-harness';

export default class LoginController
{
  constructor($scope, $state, packageData, netClient, $cookies)
  {
    this.$scope = $scope;
    this.$state = $state;
    this.$cookies = $cookies;
    this.net = netClient;

    // wire up RPCs
    netClient.bindInstance(this);
    $scope.$on('$destroy', () => netClient.unbindInstance(this));

    // display data
    this.version = packageData.version;
    this.error = null;
    this.loggingIn = false;
    this.authState = 'Not connected';

    // form data
    this.email = '';
    this.password = '';
    this.remember = true;
    this.masterURL = location.origin + '/components/master';
    this.realm = 'local';

    this._restoreForm();

    // connect right away to master
    this.connect();
  }

  /**
   * When the user presses the login button
   */
  @ngApply async login()
  {
    this.loggingIn = true;

    const email = this.email;
    const password = this.password;

    try {

      this.net.sendCommand('auth', 'login', {email, password});

      // wait for server to respond
      const {params} = await this.net.recvCommand('login');
      const {status, message} = params;

      if (!status) {
        this.error = message;
      }
      else {
        // made it!
        this.error = null;
        this._saveToken();
        this._saveForm();
        this.$state.go('app.home');
      }

    }
    catch (err) {
      this.error = err.message;
    }
    finally {
      this.loggingIn = false;
    }
  }

  @rpc.connected('auth')
  @ngApply helloAuth()
  {
    this.authState = 'Connected';
  }

  @rpc.disconnected('auth')
  @ngApply goodbyeAuth()
  {
    this.authState = 'Not connected';
    this.authLatency = null;
  }

  @rpc.command('auth', '_latency')
  @ngApply async lat(client, latency)
  {
    this.authLatency = latency;
  }

  /**
   * Connect to master server and DL lists, called on master setting changes
   */
  @debounce(250) @ngApply async connect()
  {
    this.masterError = null;

    this.net.masterURL = this.masterURL;
    this.net.realm = this.realm;

    try {
      await this.net.connect();
    }
    catch (err) {
      this.masterError = err.message;
    }
  }

  get allowLogin()
  {
    return !this.loggingIn && this.net.ready;
  }

  /**
   * persit login token
   */
  _saveToken()
  {
    if (!this.net.token) {
      return;
    }

    this.$cookies.put('auth', this.net.token, {
      expires: moment().add(1, 'year').toDate(),
    });
  }

  /**
   * persit form data
   */
  _saveForm()
  {
    this.$cookies.putObject('login', {
      email: this.email,
      url: this.masterURL,
      realm: this.realm,
      expires: moment().add(1, 'year').toDate(),
    });
  }

  /**
   * pull in form data
   */
  _restoreForm()
  {
    // restore form
    const form = this.$cookies.getObject('login');
    if (form) {
      this.email = form.email;
      this.masterURL = form.url;
      this.realm = form.realm;
    }
  }
}
