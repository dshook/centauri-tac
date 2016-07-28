import test from 'tape';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import PieceStatusChange from '../game/ctac/actions/PieceStatusChange.js';
import SpawnPiece from '../game/ctac/actions/SpawnPiece.js';
import TransformPiece from '../game/ctac/actions/TransformPiece.js';
import PieceBuff from '../game/ctac/actions/PieceBuff.js';
import MovePiece from '../game/ctac/actions/MovePiece.js';
import AttackPiece from '../game/ctac/actions/AttackPiece.js';
import AttachCode from '../game/ctac/actions/AttachCode.js';
import Position from '../game/ctac/models/Position.js';
import Statuses from '../game/ctac/models/Statuses.js';

export default class ProcessorServiceTests
{
  constructor(queue, cardDirectory, pieceState, cardEvaluator, cardState)
  {
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
  }

  setupTest(){
    this.queue.init();
    this.pieceState.reset();
    this.cardState.initPlayer(1);
    this.cardState.initPlayer(2);
  }

  spawnCards(){
    //each player gets 2 minions, 2 spells, and then draws one of each into hand
    this.spawnCard(this.cardState, 2, 1);
    this.spawnCard(this.cardState, 40, 1);
    this.spawnCard(this.cardState, 3, 1);
    this.spawnCard(this.cardState, 50, 1);
    this.spawnCard(this.cardState, 2, 2);
    this.spawnCard(this.cardState, 40, 2);
    this.spawnCard(this.cardState, 3, 2);
    this.spawnCard(this.cardState, 50, 2);

    this.cardState.drawCard(1);
    this.cardState.drawCard(1);
    this.cardState.drawCard(2);
    this.cardState.drawCard(2);
  }

  spawnCard(cardState, cardTemplateId, playerId, addToHand){
    let cardClone = this.cardDirectory.newFromId(cardTemplateId);
    cardState.addToDeck(playerId, cardClone);

    return cardClone;
  }

  spawnPiece(pieceState, cardTemplateId, playerId, addToState = true){
    var newPiece = pieceState.newFromCard(this.cardDirectory, cardTemplateId, playerId, null);

    if(addToState){
      pieceState.add(newPiece);
    }
    return newPiece;
  }

  async start()
  {
    test('Normal piece health change', async (t) => {

      t.plan(4);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 1, 1);
      const beforeHealth = piece.health;
      const damage = -2;

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      const hpChangeAction = actions[0];
      t.equal(actions.length, 1, '1 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.equal(hpChangeAction.change, damage, 'Action change is equal to damage');
      t.equal(piece.health, beforeHealth + damage, 'Piece was damaged');

    });

    test('Shield piece health change', async (t) => {

      t.plan(7);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 28, 1);
      const beforeHealth = piece.health;
      const damage = -2;
      t.ok(piece.statuses & Statuses.Shield, 'Piece has Shield');

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];
      const hpChangeAction = actions[0];
      t.equal(actions.length, 2, '2 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.ok(actions[1] instanceof PieceStatusChange, 'Second Action is status change');
      t.equal(hpChangeAction.change, 0, 'Health change was blocked by shield');
      t.equal(beforeHealth, piece.health, 'Piece was not damaged');
      t.ok(!(piece.statuses & Statuses.Shield), 'Shield was removed');

    });

    test('Armor piece health change', async (t) => {

      t.plan(7);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 1, 1);
      const beforeHealth = piece.health;
      const damage = -2;
      const armor = 1;
      piece.armor = armor;

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      const hpChangeAction = actions[0];
      t.equal(actions.length, 1, '1 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.equal(hpChangeAction.change, damage, 'Action change is equal to damage');
      t.equal(piece.health, beforeHealth + damage + armor, 'Piece was damaged, but armor took the hit');
      t.equal(piece.armor, 0, 'No more armor');
      t.equal(hpChangeAction.newCurrentArmor, 0, 'Action armor is 0');
      t.equal(hpChangeAction.newCurrentHealth, 29, 'Action hp is 29');
    });

    test('Armor piece health change with armor remaining', async (t) => {

      t.plan(7);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 1, 1);
      const beforeHealth = piece.health;
      const damage = -2;
      const armor = 3;
      piece.armor = armor;

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      const hpChangeAction = actions[0];
      t.equal(actions.length, 1, '1 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.equal(hpChangeAction.change, damage, 'Action change is equal to damage');
      t.equal(piece.health, beforeHealth, 'Piece was not damaged');
      t.equal(piece.armor, armor + damage, 'Armor remaining');
      t.equal(hpChangeAction.newCurrentArmor, armor + damage, 'Action armor also remaining');
      t.equal(hpChangeAction.newCurrentHealth, beforeHealth, 'Action hp unchanged');
    });

    test('Silence removes statuses and clears events', async (t) => {
      t.plan(3);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 77, 1);
      t.ok(piece.statuses & Statuses.CantAttack, 'Piece has Status');

      this.queue.push(new PieceStatusChange(piece.id, Statuses.Silence));

      await this.queue.processUntilDone();

      t.ok(!(piece.statuses & Statuses.CantAttack), 'Status was removed');
      t.equal(piece.events, null, 'Events were removed');

    });

    test('Buff and remove buff', async (t) => {
      t.plan(5);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 2, 1);
      t.equal(piece.health, 2, 'Piece is unbuffed with hp 2');

      let buffName = 'test buff';
      let buff = new PieceBuff(piece.id, buffName);
      buff.health = 2;
      this.queue.push(buff);

      await this.queue.processUntilDone();

      t.equal(piece.health, 4, 'Piece now has 4 hp');
      t.equal(piece.buffs.length, 1, 'Piece has buff in array');

      this.queue.push(new PieceBuff(piece.id, buffName, true));

      await this.queue.processUntilDone();

      t.equal(piece.health, 2, 'Piece now back down to 2 hp');
      t.equal(piece.buffs.length, 0, 'Piece has no more buffs');
    });

    //move an unbuffed unit through an aura and make sure it was buffed by the aura _before_ it attacks
    test('Aura activate on move', async (t) => {
      t.plan(4);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 39, 1);
      piece.position = new Position(0, 0, 1);
      piece.bornOn = 1; //fake the waiting for attack
      t.equal(piece.attack, 1, 'Piece is unbuffed with 1 attack');

      var buffPiece = this.spawnPiece(this.pieceState, 58, 1);
      buffPiece.position = new Position(2, 0, 0);

      //make sure the aura gets applied
      this.cardEvaluator.evaluatePieceEvent('playMinion', buffPiece, {position: piece.position});

      var enemyPiece = this.spawnPiece(this.pieceState, 1, 2);
      enemyPiece.position = new Position(2, 0, 1);

      t.equal(enemyPiece.health, 30, 'Enemy hero has 30 hp to start');


      this.queue.push(new MovePiece(piece.id, new Position(1, 0, 1)));
      this.queue.push(new AttackPiece(piece.id, enemyPiece.id));

      await this.queue.processUntilDone();

      t.equal(piece.attack, 2, 'Piece got the buff');
      t.equal(enemyPiece.health, 28, 'Enemy got hit for 2 damage, not 1');
    });

    test('Transform action from card', async (t) => {
      t.plan(9);
      this.setupTest();

      this.queue.push(new SpawnPiece(1, null, 28, new Position(0,0,0)));
      await this.queue.processUntilDone();

      var piece = this.pieceState.pieceAt(0,0);
      t.ok(piece, 'Spawned a piece');
      t.ok(piece.statuses & Statuses.Shield, 'Piece has Shield');

      this.queue.push(new TransformPiece(piece.id, 59, false));

      await this.queue.processUntilDone();

      t.ok(!(piece.statuses & Statuses.Shield), 'Shield was removed');
      t.equal(piece.cardTemplateId, 59, 'Piece got new template Id');
      t.equal(piece.attack, 3, 'Piece got transformed & buffed attack');
      t.equal(piece.health, 2, 'Piece got health');
      t.equal(piece.movement, 2, 'Piece got movement');
      t.ok(piece.tags.includes('XK'), 'Piece got new tags');
      t.ok(piece.aura, 'Piece copied aura');

    });

    test('Transform action from piece', async (t) => {
      t.plan(4);
      this.setupTest();

      this.queue.push(new SpawnPiece(1, null, 8, new Position(0,0,0)));
      this.queue.push(new SpawnPiece(2, null, 38, new Position(1,0,1)));
      await this.queue.processUntilDone();

      var piece = this.pieceState.pieceAt(0,0);
      t.ok(piece, 'Spawned a piece');

      this.queue.push(new TransformPiece(piece.id, 0, 2));

      await this.queue.processUntilDone();

      t.equal(piece.attack, 1, 'Piece got transformed attack');
      t.equal(piece.cardTemplateId, 38, 'Piece got new template Id');

      t.ok(this.cardEvaluator.startTurnTimers.find(t => t.piece && t.piece.id === piece.id),
        'Start turn timer was copied for transformed piece');
    });

    test('Attach code', async (t) => {
      this.setupTest();

      //spawn pieces with incompatable events, existing events, and no events
      var pieceWithIncompatableDamagedEvent = this.spawnPiece(this.pieceState, 12, 1);
      var pieceWithDamagedEvent = this.spawnPiece(this.pieceState, 11, 1);
      var pieceNothing = this.spawnPiece(this.pieceState, 8, 1);

      let newCode = [
        {
          "event": "damaged",
          "actions": [
            {
              "action": "Hit",
              "args": [
                {
                  "left": "ENEMY"
                },
                1
              ]
            }
          ]
        }
      ];

      let attach0 = new AttachCode(pieceWithIncompatableDamagedEvent.id, newCode );
      let attach1 = new AttachCode(pieceWithDamagedEvent.id, newCode );
      let attach2 = new AttachCode(pieceNothing.id, newCode );

      this.queue.push(attach0);
      this.queue.push(attach1);
      this.queue.push(attach2);

      await this.queue.processUntilDone();

      t.plan(3);
      t.deepEqual(pieceNothing.events, newCode
        , 'Piece with nothing gained new code');

      t.equal(pieceWithDamagedEvent.events[0].actions.length, 2
        , 'Piece with damaged event got new action');

      t.deepEqual(pieceWithIncompatableDamagedEvent.events[1], newCode[0]
        , 'Piece with incompatable event got added as another event')
    });

    //spawn a piece with a card aura and make sure it takes affect
    test('Card Aura', async (t) => {
      t.plan(6);
      this.setupTest();

      this.spawnCards();
      let playerSpell = this.cardState.hands[1][1];
      t.equal(playerSpell.cost, 1, 'Found spell with normal cost');

      let piece = this.spawnPiece(this.pieceState, 78, 1);

      //make sure the aura gets applied
      this.cardEvaluator.evaluatePieceEvent('playMinion', piece, {position: piece.position});

      await this.queue.processUntilDone();

      t.equal(playerSpell.cost, 0, 'Spell got buffed');
      t.equal(playerSpell.buffs.length, 1, 'Buffs were added');

      //now kill and make sure it's removed
      let pieceId = piece.id;
      this.queue.push(new PieceHealthChange(pieceId, -2));

      await this.queue.processUntilDone();

      t.ok(!this.pieceState.piece(pieceId), 'Piece was killed');
      t.equal(playerSpell.cost, 1, 'Spell has normal cost again');
      t.equal(playerSpell.buffs.length, 0, 'Buffs were removed');
    });
  }
}
