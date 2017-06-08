import test from 'tape';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import PieceStatusChange from '../game/ctac/actions/PieceStatusChange.js';
import SpawnPiece from '../game/ctac/actions/SpawnPiece.js';
import TransformPiece from '../game/ctac/actions/TransformPiece.js';
import PieceBuff from '../game/ctac/actions/PieceBuff.js';
import MovePiece from '../game/ctac/actions/MovePiece.js';
import AttackPiece from '../game/ctac/actions/AttackPiece.js';
import PlaySpell from '../game/ctac/actions/PlaySpell.js';
import AttachCode from '../game/ctac/actions/AttachCode.js';
import ActivateCard from '../game/ctac/actions/ActivateCard.js';
import PassTurn from '../game/ctac/actions/PassTurn.js';
import Position from '../game/ctac/models/Position.js';
import Direction from '../game/ctac/models/Direction.js';
import Statuses from '../game/ctac/models/Statuses.js';

export default class ProcessorServiceTests
{
  constructor(queue, cardDirectory, pieceState, cardEvaluator, cardState, turnState, mapState)
  {
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
    this.turnState = turnState;
    this.mapState = mapState;
  }

  setupTest(){
    this.queue.init();
    this.pieceState.reset();
    this.cardState.initPlayer(1);
    this.cardState.initPlayer(2);
    this.turnState.passTurn();
    this.cardEvaluator.startTurnTimers = [];
    this.cardEvaluator.endTurnTimers = [];
    this.mapState.setMap('cubeland');
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

  spawnCard(cardState, cardTemplateId, playerId){
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
      t.plan(5);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 1, 1);
      const beforeHealth = piece.health;
      const damage = -2;

      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: damage}));

      await this.queue.processUntilDone();

      let generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      const hpChangeAction = actions[0];
      t.equal(actions.length, 1, '1 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.equal(hpChangeAction.change, damage, 'Action change is equal to damage');
      t.equal(piece.health, beforeHealth + damage, 'Piece was damaged');

      const heal = 4;
      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: heal}));

      await this.queue.processUntilDone();

      t.equal(piece.health, piece.maxBuffedHealth, 'Piece returned to undamaged state');
    });

    test('Shield piece health change', async (t) => {

      t.plan(7);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 28, 1);
      const beforeHealth = piece.health;
      const damage = -2;
      t.ok(piece.statuses & Statuses.Shield, 'Piece has Shield');

      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: damage}));

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

      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: damage}));

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

      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: damage}));

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

      this.queue.push(new PieceStatusChange({pieceId: piece.id, add: Statuses.Silence}));

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
      let buff = new PieceBuff({pieceId: piece.id, name: buffName});
      buff.health = 2;
      this.queue.push(buff);

      await this.queue.processUntilDone();

      t.equal(piece.health, 4, 'Piece now has 4 hp');
      t.equal(piece.buffs.length, 1, 'Piece has buff in array');

      this.queue.push(new PieceBuff({pieceId: piece.id, name: buffName, removed: true}));

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


      this.queue.push(new MovePiece({pieceId: piece.id, to: new Position(1, 0, 1)}));
      this.queue.push(new AttackPiece(piece.id, enemyPiece.id));

      await this.queue.processUntilDone();

      t.equal(piece.attack, 2, 'Piece got the buff');
      t.equal(enemyPiece.health, 28, 'Enemy got hit for 2 damage, not 1');
    });

    test('Transform action from card', async (t) => {
      t.plan(9);
      this.setupTest();

      this.queue.push(new SpawnPiece({playerId: 1, cardTemplateId: 28, position: new Position(0,0,0)}));
      await this.queue.processUntilDone();

      var piece = this.pieceState.pieceAt(0,0);
      t.ok(piece, 'Spawned a piece');
      t.ok(piece.statuses & Statuses.Shield, 'Piece has Shield');

      this.queue.push(new TransformPiece({pieceId: piece.id, cardTemplateId: 59}));

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

      this.queue.push(new SpawnPiece({playerId: 1, cardTemplateId: 8, position: new Position(0,0,0)}));
      this.queue.push(new SpawnPiece({playerId: 2, cardTemplateId: 38, position: new Position(1,0,1)}));
      await this.queue.processUntilDone();

      var piece = this.pieceState.pieceAt(0,0);
      t.ok(piece, 'Spawned a piece');

      this.queue.push(new TransformPiece({pieceId: piece.id, transformPieceId: 2}));

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

      let attach0 = new AttachCode({pieceId: pieceWithIncompatableDamagedEvent.id, eventList: newCode });
      let attach1 = new AttachCode({pieceId: pieceWithDamagedEvent.id, eventList: newCode });
      let attach2 = new AttachCode({pieceId: pieceNothing.id, eventList: newCode });

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
      this.queue.push(new PieceHealthChange({pieceId, change: -2}));

      await this.queue.processUntilDone();

      t.ok(!this.pieceState.piece(pieceId), 'Piece was killed');
      t.equal(playerSpell.cost, 1, 'Spell has normal cost again');
      t.equal(playerSpell.buffs.length, 0, 'Buffs were removed');
    });

    //spawn a piece with choices and make sure it takes effect
    test('Choose Card', async (t) => {
      t.plan(4);
      this.setupTest();

      //spawn a hero for the activate card
      var hero = this.spawnPiece(this.pieceState, 1, 1);
      hero.position = new Position(0, 0, 1);

      //draw two choose card to hand
      this.spawnCard(this.cardState, 81, 1);
      this.spawnCard(this.cardState, 81, 1);
      let firstChooseCard  = this.cardState.drawCard(1);
      let secondChooseCard = this.cardState.drawCard(1);

      //now activate it
      this.queue.push(new ActivateCard(1, firstChooseCard.id, new Position(0,0,0), null, null, 82));

      await this.queue.processUntilDone();

      let piece = this.pieceState.piece(2);
      t.equal(piece.cardTemplateId, 81, 'Found the choose piece');
      t.equal(piece.buffs.length, 1, 'Piece Got a buff');

      //now use the other choice, with the first piece as the target
      this.queue.push(new ActivateCard(1, secondChooseCard.id, new Position(1,0,1), piece.id, null, 83));

      await this.queue.processUntilDone();

      let secondChoosePiece = this.pieceState.piece(3);
      t.equal(secondChoosePiece.cardTemplateId, 81, 'Found the second choose piece');
      t.equal(piece.buffs.length, 2, 'First Piece Got another (de)buff');
    });

    //spawn a piece with a conditional buff and make sure it turns off and on right
    test('Conditional Buff', async (t) => {
      t.plan(8);
      this.setupTest();

      this.spawnCards();

      let piece = this.spawnPiece(this.pieceState, 85, 1);

      //make sure the buff gets applied
      this.cardEvaluator.evaluatePieceEvent('playMinion', piece);

      await this.queue.processUntilDone();

      t.equal(piece.attack, 1, 'Still normal stats');
      t.equal(piece.buffs.length, 1, 'Got buff though');
      t.equal(piece.buffs[0].enabled, false, 'Buff is not enabled');

      //now for the damage to activate the buff
      this.queue.push(new PieceHealthChange({pieceId: piece.id, change: -1}));

      await this.queue.processUntilDone();

      t.equal(piece.attack, 3, 'Attack was buffed');
      t.equal(piece.health, 2, 'Health Damage');
      t.equal(piece.maxBuffedHealth, 3, 'Max Buffed health is still 3');
      t.equal(piece.buffs.length, 1, 'Buff is still there');
      t.equal(piece.buffs[0].enabled, true, 'Buff is enabled');
    });

    //walk around and make sure you face the right direction
    test('Directions', async (t) => {
      t.plan(4);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 7, 1);
      piece.position = new Position(0, 0, 0);
      piece.bornOn = -100; //fake the waiting for attack
      piece.attackCount = 0;

      let enemy = this.spawnPiece(this.pieceState, 7, 2);
      enemy.position = new Position(1, 0, 1);

      //this.cardEvaluator.evaluatePieceEvent('playMinion', piece, {position: piece.position});

      t.equal(piece.direction, Direction.South, 'Start facing south');

      this.queue.push(new MovePiece({pieceId: piece.id, to: new Position(1, 0, 0)}));
      await this.queue.processUntilDone();
      t.equal(piece.direction, Direction.East, 'Move to the east');

      this.queue.push(new MovePiece({pieceId: piece.id, to: new Position(2, 0, 0)}));
      this.queue.push(new MovePiece({pieceId: piece.id, to: new Position(2, 0, 1)}));
      await this.queue.processUntilDone();
      t.equal(piece.direction, Direction.North, 'Now up to North');

      this.queue.push(new AttackPiece(piece.id, enemy.id));
      await this.queue.processUntilDone();
      t.equal(piece.direction, Direction.West, 'Attack west');

    });

    test('Repeating damage not overkilling', async (t) => {
      t.plan(3);
      this.setupTest();

      var piece = this.spawnPiece(this.pieceState, 28, 1);

      this.spawnCard(this.cardState, 87, 2);
      var card = this.cardState.drawCard(2);

      this.queue.push(new PlaySpell(2, card.id, card.cardTemplateId, null, null, null, null ));

      await this.queue.processUntilDone();

      t.equal(piece.health, 0, 'Piece died');

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      let hpChangeActions = actions.filter(a => a instanceof PieceHealthChange);
      t.equal(hpChangeActions.length, 3, 'There were only 3 hp change actions instead of the full 4');
      t.equal(actions.filter(a => a instanceof PieceStatusChange).length, 1, 'Shield was also removed');
    });

    test('Activating piece on timer events', async (t) => {
      t.plan(7);
      this.setupTest();

      this.spawnCards();

      let piece1 = this.spawnPiece(this.pieceState, 38, 1);
      let piece2 = this.spawnPiece(this.pieceState, 38, 1);

      //make sure the timers get set up
      this.cardEvaluator.evaluatePieceEvent('playMinion', piece1);
      this.cardEvaluator.evaluatePieceEvent('playMinion', piece2);

      await this.queue.processUntilDone();
      const preGeneratedActions = this.queue.iterateCompletedSince();
      let preActions = [...preGeneratedActions];
      let preBuffActions = preActions.filter(a => a instanceof PieceBuff);
      t.equal(preBuffActions.length, 0, 'No buff actions have been added yet');

      t.equal(this.cardEvaluator.startTurnTimers.length, 2, 'Timers were added');

      this.queue.push(new PassTurn());
      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      let buffActions = actions.filter(a => a instanceof PieceBuff);

      t.equal(buffActions.length, 2, 'A buff action for each of the pieces');
      t.equal(buffActions[0].pieceId, piece1.id, 'First buff action is for first piece');
      t.equal(buffActions[0].activatingPieceId, piece1.id, 'First buff activating piece');
      t.equal(buffActions[1].pieceId, piece2.id, 'Second buff is for second piece');
      t.equal(buffActions[1].activatingPieceId, piece2.id, 'Second buff activating piece');
    });

    test('End turn timer resolution order 1/2', async (t) => {
      t.plan(1);
      this.setupTest();

      //spawn a hero for the activate card
      var hero = this.spawnPiece(this.pieceState, 1, 1);
      hero.position = new Position(0, 0, 1);
      var enemyHero = this.spawnPiece(this.pieceState, 1, 2);
      enemyHero.position = new Position(1, 0, 1);

      //draw two choose card to hand
      this.spawnCard(this.cardState, 97, 1);
      this.spawnCard(this.cardState, 96, 1);
      let reggoh  = this.cardState.drawCard(1);
      let noddeg = this.cardState.drawCard(1);

      //destroyer first
      this.queue.push(new ActivateCard(1, noddeg.id, new Position(0,0,0), null, null, null));

      await this.queue.processUntilDone();

      //spawner second
      this.queue.push(new ActivateCard(1, reggoh.id, new Position(1,0,2), null, null, null));

      await this.queue.processUntilDone();

      this.queue.push(new PassTurn());
      await this.queue.processUntilDone();

      let spawnedPiece = this.pieceState.pieces.find(p => p.cardTemplateId === 8);
      t.ok(spawnedPiece, 'Spawned Piece is Still Alive');
    });

    test('End turn timer resolution order 2/2', async (t) => {
      t.plan(1);
      this.setupTest();

      //spawn a hero for the activate card
      var hero = this.spawnPiece(this.pieceState, 1, 1);
      hero.position = new Position(0, 0, 1);
      var enemyHero = this.spawnPiece(this.pieceState, 1, 2);
      enemyHero.position = new Position(1, 0, 1);

      //draw two choose card to hand
      this.spawnCard(this.cardState, 97, 1);
      this.spawnCard(this.cardState, 96, 1);
      let reggoh  = this.cardState.drawCard(1);
      let noddeg = this.cardState.drawCard(1);

      //spawner first
      this.queue.push(new ActivateCard(1, reggoh.id, new Position(1,0,2), null, null, null));

      await this.queue.processUntilDone();

      //destroyer second
      this.queue.push(new ActivateCard(1, noddeg.id, new Position(0,0,0), null, null, null));

      await this.queue.processUntilDone();

      this.queue.push(new PassTurn());
      await this.queue.processUntilDone();

      let spawnedPiece = this.pieceState.pieces.find(p => p.cardTemplateId === 8);
      t.ok(!spawnedPiece, 'Spawned Piece was destroyed');
    });

    test('Saved pieces selection', async (t) => {
      t.plan(7);
      this.setupTest();

      var hero = this.spawnPiece(this.pieceState, 1, 1);
      hero.position = new Position(0, 0, 1);
      var enemyHero = this.spawnPiece(this.pieceState, 1, 2);
      enemyHero.position = new Position(1, 0, 1);

      //2 zombies for each player
      this.spawnPiece(this.pieceState, 2, 1);
      this.spawnPiece(this.pieceState, 2, 1);
      this.spawnPiece(this.pieceState, 2, 2);
      this.spawnPiece(this.pieceState, 2, 2);

      //draw two choose card to hand
      this.spawnCard(this.cardState, 31, 1);
      let card = this.cardState.drawCard(1);

      //paralyze
      this.queue.push(new ActivateCard(1, card.id, null, null, null, null));

      await this.queue.processUntilDone();

      let paralyzed = this.pieceState.withStatus(Statuses.Paralyze);
      t.equal(paralyzed.length, 2, 'Two pieces paralyzed');
      t.equal(paralyzed[0].playerId, 2, 'Enemy Piece is paralyzed');
      t.equal(this.cardEvaluator.startTurnTimers.length, 1, '1 start turn timer saved');

      this.queue.push(new PassTurn());
      await this.queue.processUntilDone();

      let secondPar = this.pieceState.withStatus(Statuses.Paralyze);
      t.equal(secondPar.length, 2, 'Two pieces still paralyzed');
      t.equal(this.cardEvaluator.startTurnTimers.length, 1, 'Still 1 timer saved');

      this.queue.push(new PassTurn());
      await this.queue.processUntilDone();

      let thirdPar = this.pieceState.withStatus(Statuses.Paralyze);
      t.equal(thirdPar.length, 0, 'Paralysis wore off');
      t.equal(this.cardEvaluator.startTurnTimers.length, 0, 'No timers left');
    });

    //spawn a pieces in an area
    test('Area pieces', async (t) => {
      t.plan(4);
      this.setupTest();

      //spawn a hero for the activate card
      var hero = this.spawnPiece(this.pieceState, 1, 1);
      hero.position = new Position(0, 0, 1);

      //draw two choose card to hand
      this.spawnCard(this.cardState, 103, 1);
      let cardDrawn  = this.cardState.drawCard(1);

      //now activate it
      this.queue.push(new ActivateCard(1, cardDrawn.id, new Position(1,0,1), null, new Position(2,0,1), null));

      await this.queue.processUntilDone();

      let walls = this.pieceState.fromTemplateId(104);
      t.equal(walls.length, 3, '3 Walls were spawned');
      t.ok(walls[0].position.tileEquals(new Position(1,0,1)), 'First wall in right position');
      t.ok(walls[1].position.tileEquals(new Position(2,0,1)), 'Second wall in right position');
      t.ok(walls[2].position.tileEquals(new Position(3,0,1)), 'Third wall in right position');

    });
  }
}
