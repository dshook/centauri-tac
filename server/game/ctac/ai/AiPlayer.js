import Player from 'models/player';
import {MockClient} from 'socket-client';
import EmitterBinder from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

@loglevel
export default class AiPlayer extends Player
{
  constructor(id = null, auth)
  {
    super(id);
    this.email = 'AI@internet.com';
    this.client = new MockClient();
    this.connected = true;
    this.isAdmin = false;

    const roles = ['player'];
    this.client.token = auth.generateToken(this, roles);

    this.binder = new EmitterBinder(this.client);
    this.binder.bindInstance(this);
  }

  @on('received')
  onServerCommand({command, data})
  {
    this.log.info('Ai received command %j data %j', command, data);
  }

}
