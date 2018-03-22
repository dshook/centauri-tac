import _ from 'lodash';
import Player from 'models/Player';
import Statuses from '../models/Statuses.js';
import {MockClient} from 'socket-client';
import EmitterBinder from 'emitter-binder';
import loglevel from 'loglevel-decorator';
import moment from 'moment';
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
    this.possibleActions = null;

    //limit how fast to do things
    this.lastActionTime = null;

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
      case 'possibleActions':
        this.possibleActions = data;
        this.log.info('Ai got possible actions');
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

  //More like find something to do right now
  makeAPlan(){
    //throttle finding something to do to every 2 seconds or so if we keep getting qpc's
    if(this.lastActionTime == null){
      this.lastActionTime = moment();
      return;
    }
    if(moment().diff(this.lastActionTime, 'seconds') < 2){
      return;
    }
    this.lastActionTime = moment();

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

    let playerId = hero.playerId;
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
          return;
        }else{
          this.log.info('Path Not Found');
        }
      }else{
        this.log.info('No Radius Tiles');
      }
    }

    //figure out what cards to play this turn.
    let currentEnergy = this.playerResourceState.get(playerId);
    let playableCards = this.cardState.hands[playerId].filter(c => c.cost <= currentEnergy);

    if(playableCards.length){
      let cardToPlay = playableCards[0];

      if(cardToPlay.isMinion){
        this.log.info('Found Playable minions');
        //find closest tile to opponent that's playable
        let isAirdrop = (cardToPlay.statuses & Statuses.Airdrop) != 0;
        let allowableDistance = isAirdrop ? 4 : 1;
        let availablePositions = this.mapState.getKingTilesInRadius(hero.position, allowableDistance)
          .map(p => this.mapState.getTile(p))
          .filter(t => !t.unpassable && !this.pieceState.pieceAt(t.position));
        let sortedAvailable = _.sortBy(availablePositions, k => this.mapState.tileDistance(opponent.position, k.position));

        if(sortedAvailable.length){
          this.log.info('Found a spot to deploy');
          let playingPosition = sortedAvailable[0].position;
          this.send('activatecard', {
            playerId: playerId,
            cardInstanceId: cardToPlay.id,
            position: playingPosition,
            pivotPosition: null,
            targetPieceId: null,
            chooseCardTemplateId: null
          });
          return;
        }
      }
    }

    //Find out if there's a minion that can attack (highest attack first)
    let piecesThatCanAttack = this.pieceState.pieces
      .filter(p => p.playerId === playerId && p.canAttack)
      .sort((a, b) => b.attack - a.attack);
    for (const piece of piecesThatCanAttack) {
        let pathToHero = this.mapState.findMovePath(piece, opponent, null);

        if(pathToHero != null && pathToHero.length > 0){
          this.log.info('Ai attacking enemy hero with %s:%s', piece.cardTemplateId, piece.name);
          pathToHero.splice(-1, 1); //splice off the last move tile since it'll be the enemy
          this.send('moveattack', {
            attackingPieceId: piece.id,
            targetPieceId: opponent.id,
            route: pathToHero.map(p => p.position)
          });
          return;
        }
    }
  }

}
