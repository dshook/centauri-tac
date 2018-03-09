import Player from 'models/player';
import {MockClient} from 'socket-client';
import EmitterBinder from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

@loglevel
export default class AiPlayer extends Player
{
  constructor(id = null, auth, sockHarness)
  {
    super(id);
    this.email = 'AI@internet.com';
    this.client = new MockClient();
    this.connected = true;
    this.isAdmin = false;

    const roles = ['player'];
    this.client.token = auth.generateToken(this, roles);

    //Bind server events that are directed to this client to be picked up by the @on handler
    this.binder = new EmitterBinder(this.client);
    this.binder.bindInstance(this);

    this.currentGame = null;
  }

  @on('received')
  onServerCommand({command, data})
  {
    this.log.info('Ai received command %j', command);
    switch(command){
      case 'game:current':
        this.currentGame = data;
        this.client.sendToServer({data: 'join ' + data.id});
        break;
    }
  }

}
