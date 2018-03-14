import _ from 'lodash';
import Player from 'models/Player';
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

    this.opponent = null;
    this.game = null;
    this.turnState = null;
    this.playerResourceState = null;
    this.pieceState = null;
    this.mapState = null;
    this.cardState = null;
  }

  @on('received')
  onServerCommand({command, data})
  {
    this.log.info('Ai received command %j', command);
    switch(command){
      case 'game:current':
        this.currentGame = data;
        if(data && data.id){
          this.send('join', data.id);
        }
        break;
      case 'gameInstance':
        this.game = data;
        if(!data){ return; }
        //some convieniences
        this.turnState = this.game.container.resolve('turnState');
        this.playerResourceState = this.game.container.resolve('playerResourceState');
        this.pieceState = this.game.container.resolve('pieceState');
        this.mapState = this.game.container.resolve('mapState');
        this.cardState = this.game.container.resolve('cardState');
        break;
      case 'player:join':
        if(data && data.id && data.id != this.id){
          this.opponent = data;
          this.log.info('Ai got an opponent %j', data);
        }
        break;
      case 'qpc':
        this.makeAPlan();
        break;
    }
  }

  send(method, data){
    let message = method + " " + JSON.stringify(data);
    this.client.sendToServer({data: message});
  }

  makeAPlan(){
    if(!this.opponent) return;

    this.log.info('Ai Planning');
    var hero = this.pieceState.hero(this.id);
    var opponent = this.pieceState.hero(this.opponent.id);

    if(!opponent){
      this.log.info('AI Won!');
      return;
    }
    if(!hero){
      this.log.info('AI Lost :(');
      return;
    }

    let heroTile = this.mapState.getTile(hero.position);
    let opponentTile = this.mapState.getTile(opponent.position);

    let heroMovementLeft = hero.movement - hero.moveCount;
    if(heroMovementLeft > 0 && this.mapState.tileDistance(hero.position, opponent.position) !== 2){
      //try to find the closest tile 2 distance away from the enemy hero
      let radiusTiles = this.mapState.getTilesInRadius(opponent.position, 2)
        .map(p => this.mapState.getTile(p))
        .filter(p => !p.unpassable && this.mapState.tileDistance(p.position, opponent.position) === 2);

      if(radiusTiles.length){
        var dest = _.sortBy(radiusTiles, k => this.mapState.tileDistance(hero.position, k.position));

        let path = this.mapState.findMovePath(hero, null, dest[0]);

        if(path === null || !path.length){
          //hero is out of range of our movement this turn so do a full path find and grab the first bit of the path
          path = this.mapState.findPath(heroTile, opponentTile, 25, hero);
          if(path != null){
            path = path.slice(0, heroMovementLeft);
          }
        }

        if(path != null && path.length > 0){
          this.log.info('Ai Moving Hero');
          this.send('move', {pieceId: hero.id, route: path.map(p => p.position)});
        }else{
          this.log.info('Path Not Found');
        }
      }else{
        this.log.info('No Radius Tiles');
      }
    }
  }

}
