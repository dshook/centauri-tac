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
    let enemyPieces = this.pieceState.pieces
      .filter(p => p.playerId !== playerId)
      .sort((a, b) => b.attack - a.attack);

    for (const piece of piecesThatCanAttack) {
      let pathToHero = this.mapState.findMovePath(piece, opponent, null);

      //try to attack their hero first
      if(pathToHero != null && pathToHero.length > 0){
        this.log.info('Ai attacking enemy hero with %s:%s', piece.id, piece.name);
        this.pieceAttack(piece, opponent, pathToHero);
        return;
      }

      let piecesWeCanKill = [];
      //next see if there are any pieces we can kill without dying
      //or at the very least kill taking us down with it
      //TODO: see if we should avoid taunt areas
      for (const enemyPiece of enemyPieces) {

        let pathToPiece = this.mapState.findMovePath(piece, enemyPiece, null);
        if(pathToPiece == null || pathToPiece.length === 0){
          continue;
        }
        let tileDist = this.mapState.tileDistance(piece.position, enemyPiece.position);
        let kingDist = this.mapState.kingDistance(piece.position, enemyPiece.position);

        let canKillIt = piece.attack >= enemyPiece.health && (enemyPiece.statuses & Statuses.Shield) == 0;
        let canSurviveIt = (enemyPiece.statuses & Statuses.Shield) == 1
          || piece.health > enemyPiece.attack
          || (piece.range && !enemyPiece.range && tileDist > 1)
          || (piece.range && enemyPiece.range && kingDist > enemyPiece.range);

        if(canKillIt && canSurviveIt){
          this.log.info('Ai attacking with %s to kill %s without dying', piece.id, enemyPiece.id);
          this.pieceAttack(piece, enemyPiece, pathToPiece);
          return;
        }

        if(canKillIt){
          piecesWeCanKill.push({piece: enemyPiece, path: pathToPiece});
        }
      }

      //if there weren't any minions we could kill and survive we're left with trading
      if(piecesWeCanKill.length){
        this.log.info('Ai attacking with %s to trade with %s', piece.id, piecesWeCanKill[0].piece.id);
        this.pieceAttack(piece, piecesWeCanKill[0].piece, piecesWeCanKill[0].path);
        return;
      }

      //and if we made it this far we have to resort to trying to get close to the enemy hero
      let fullPathToEnemyHero = this.mapState.findPath( this.mapState.getTile(piece.position), opponentTile, 10, piece);
      if(fullPathToEnemyHero != null){
        fullPathToEnemyHero = fullPathToEnemyHero.slice(0, piece.movement - piece.moveCount);
        if(fullPathToEnemyHero.length > 0){
          this.log.info('Ai moving piece %s closer to the enemy hero', piece.id);
          this.send('move', {pieceId: piece.id, route: fullPathToEnemyHero.map(p => p.position)});
          return;
        }
      }
    }
  }

  pieceAttack(piece, enemyPiece, movePath){
    movePath.splice(-1, 1); //splice off the last move tile since it'll be the enemy
    this.send('moveattack', {
      attackingPieceId: piece.id,
      targetPieceId: enemyPiece.id,
      route: movePath.map(p => p.position)
    });
  }

}
