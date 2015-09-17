import loglevel from 'loglevel-decorator';
import {EventEmitter} from 'events';
import present from 'present';
import jwt from 'jsonwebtoken';

let _nextId = 0;

/**
 * WebSockets client, needs to be injected with a raw socketo
 *
 * Notice nothing is async in here -- WebSockets are non-blocking but also no
 * mechanism for knowing if a send worked or failed
 *
 * Fires events:
 * * open: connection established
 * * command({command, params}): command recieved from remote
 * * latency(value): latency value was set
 * * error: problem with socket
 * * close: disconnected
 */
@loglevel
export default class SocketClient extends EventEmitter
{
  constructor(ws)
  {
    super();

    if (!ws) {
      throw new TypeError('ws');
    }

    this._socket = ws;

    this._nextPingId = 0;
    this._pings = new Map();

    // for auth purposes
    this._token = null;
    this._tokenPayload = null;

    // works on client and server sock implementations
    this.remoteAddress = ws.url || ws._socket.remoteAddress;

    // unique client id
    this.id = _nextId++;

    // how long it takes to abondon a connection attemp
    this.connectionTimeout = 5000;

    // how long it takes to give up on a ping
    this.pingTimeout = 5000;

    // our
    this.latency = 0;

    this._initSocket();
  }

  get token() { return this._token; }

  /**
   * Stash token payload
   */
  set token(val)
  {
    try {
      this._tokenPayload = jwt.decode(val);
    }
    catch (err) {
      this.log.info('problem decoding JWT token');
      this._tokenPayload = null;
    }

    this._token = val;
  }

  /**
   * Parsed token
   */
  get auth()
  {
    return this._tokenPayload;
  }

  /**
   * Send data along the wire via our "command" protocol
   */
  send(command, data = null)
  {
    if (!this._socket || this._socket.readyState !== 1) {
      this.log.info('skipping send! socket in bad state');
      return null;
    }

    const payload = command + ' ' + JSON.stringify(data);

    if (command.charAt(0) !== '_') {
      this.log.info('[%s] send %s', this.id, command);
    }

    // blast it
    this._socket.send(payload);
  }

  /**
   * Dump the socket
   */
  disconnect()
  {
    if (!this._socket) {
      return;
    }

    this._socket.close();
  }

  /**
   * Round trip latency
   */
  ping()
  {
    const pId = this._nextPingId;
    this.send('_ping', pId);
    this._pings.set(pId, present());

    setTimeout(() => {

      // made it
      if (!this._pings.has(pId)) {
        return;
      }

      this.log.info('[%s] ping timeout %s', this.id, pId);
      this._pings.delete(pId);
      this.latency = this.pingTimeout;

    }, this.pingTimeout);
  }

  /**
   * Notify of latency change (packaged up as a command for simplicity)
   */
  set latency(val)
  {
    this._latency = val;

    const command = '_latency';
    const params = val;
    this.emit('command', {command, params});
  }

  get latency() { return this._latency; }

  /**
   * Process response from remote
   */
  _pong(id)
  {
    const start = this._pings.get(id);
    if (!start) {
      return;
    }

    const delta = present() - start;

    // latency is half total trip (1 way)
    this.latency = delta / 2;
    this._pings.delete(id);

    // tell remote what is latency is
    this.send('_latency', this.latency);
  }

  /**
   * Setup listeners and watch for connection
   */
  _initSocket()
  {
    const sock = this._socket;

    this.log.info('[%s] connecting socket to %s', this.id, this.remoteAddress);

    // Timeout after a bit if this sock isn't already open
    if (sock.readyState !== 1) {
      this.log.info('[%s] waiting for sock to come up', this.id);
      this._timeoutId = setTimeout(() => {
        this.log.info('[%s] connection timeout', this.id);
        sock.close();
      }, this.connectionTimeout);
    }

    // Bind events
    sock.onopen = this._onOpen.bind(this);
    sock.onmessage = this._onMessage.bind(this);
    sock.onerror = this._onError.bind(this);
    sock.onclose = this._onClose.bind(this);
  }

  /**
   * Low level socket commands, handle them here and don't bubble em up
   */
  _handleCommand(command, params)
  {
    switch (command) {
      case '_ping':
        this.send('_pong', params);
        break;
      case '_pong':
        this._pong(params);
        break;
      case '_latency':
        this.latency = params;
        break;
      default:
        return false;
    }

    return true;
  }

  /**
   * Socket connected
   */
  _onOpen()
  {
    clearTimeout(this._timeoutId);
    this.log.info('[%s] connected to %s', this.id, this.remoteAddress);
    this.emit('open');
  }

  /**
   * Parse out message according to the protocol
   */
  _onMessage({data})
  {
    const split = data.indexOf(' ');
    const command = data.substr(0, split);
    const params = JSON.parse(data.substr(split + 1));

    // system commands, swallow the command
    if (this._handleCommand(command, params)) {
      return;
    }

    this.log.info('[%s] recv %s', this.id, command);

    this.emit('command', {command, params});
  }

  /**
   * Emit error
   */
  _onError()
  {
    this.log.info('[%s] error', this.id);
    this.emit('error');
    this._closeSocket();
  }

  /**
   * DC
   */
  _onClose()
  {
    this._closeSocket();
  }

  /**
   * Cleanup sock
   */
  _closeSocket()
  {
    if (!this._socket) {
      return;
    }

    clearTimeout(this._timeoutId);

    // wipe
    this._socket.onmessage =
      this._socket.onerror =
      this._socket.onclose =
      this._socket.onopen =
      null;
    this._socket = null;

    this.log.info('[%s] disconnected', this.id);
    this.emit('close');
  }
}
