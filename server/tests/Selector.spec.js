import test from 'tape';
import Selector from '../game/ctac/cardlang/Selector.js';
import Statuses from '../game/ctac/models/Statuses.js';
import GamePiece from '../game/ctac/models/GamePiece.js';
import Card from '../game/ctac/models/Card.js';
import PieceState from '../game/ctac/models/PieceState.js';
import CardState from '../game/ctac/models/CardState.js';
import Position from '../game/ctac/models/Position.js';
import Player from 'models/Player';
import MapState from '../game/ctac/models/MapState.js';
import CardDirectory from '../game/ctac/models/CardDirectory.js';
import cubeland from '../../maps/cubeland.json';

//manually init the dependencies for the selector
var cardDirectory = new CardDirectory({cardSets: ['test']});

var mapState = new MapState();
mapState.add(cubeland, true);

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

//dummy activating and self pieces
var selfPiece = new GamePiece();

//various setups for game state
var pieceStateMix = new PieceState();
spawnPiece(pieceStateMix, 1, 1);
spawnPiece(pieceStateMix, 2, 1);
spawnPiece(pieceStateMix, 3, 1);
spawnPiece(pieceStateMix, 1, 2);
spawnPiece(pieceStateMix, 2, 2);
spawnPiece(pieceStateMix, 3, 2);

var heroesOnly = new PieceState();
spawnPiece(heroesOnly, 1, 1);
spawnPiece(heroesOnly, 1, 2);

var noPieces = new PieceState();

var statusPieces = new PieceState();
spawnPiece(statusPieces, 1, 1);
spawnPiece(statusPieces, 2, 1);
spawnPiece(statusPieces, 28, 1);
spawnPiece(statusPieces, 1, 2);
spawnPiece(statusPieces, 2, 2);
spawnPiece(statusPieces, 28, 2);

//each player gets 2 minions, 2 spells, and then draws one of each into hand
var cardStateMix = new CardState();
cardStateMix.initPlayer(1);
cardStateMix.initPlayer(2);
spawnCard(cardStateMix, 2, 1);
spawnCard(cardStateMix, 40, 1);
spawnCard(cardStateMix, 3, 1);
spawnCard(cardStateMix, 50, 1);
spawnCard(cardStateMix, 2, 2);
spawnCard(cardStateMix, 40, 2);
spawnCard(cardStateMix, 3, 2);
spawnCard(cardStateMix, 50, 2);

cardStateMix.drawCard(1);
cardStateMix.drawCard(1);
cardStateMix.drawCard(2);
cardStateMix.drawCard(2);

var noCards = new CardState();
noCards.initPlayer(1);
noCards.initPlayer(2);

test('Select Player', t => {
  t.plan(2);
  let selector = new Selector(players, heroesOnly, mapState);

  t.equal(selector.selectPlayer(1, {left: 'PLAYER'}), 1, 'Player is equal to itself');
  t.equal(selector.selectPlayer(1, {left: 'OPPONENT'}), 2, 'Select opponent');

});

//Not worrying about the "randomness" of the result since that's provided through lodash
//which should be tested enough for us.  Testing that the selector returns the right class
//of whatever should be taken care of by the multi piece selector tests
test('Select a piece', t => {
  let selectorTemplate = {
    random: true,
    selector:{
      left : '' //replaced
    }
  };
  let selectors = [
    'FRIENDLY',
    'ENEMY',
    'MINION',
    'HERO'
  ];
  t.plan(selectors.length * 2);
  let selector = new Selector(players, pieceStateMix, mapState);

  for(let selectString of selectors){
    selectorTemplate.selector.left = selectString;
    let selection = selector.selectPieces(selectorTemplate, {selfPiece, controllingPlayerId: 1});
    t.equal(selection.length, 1, 'Got a single piece back');
    t.ok(selection[0] instanceof GamePiece, 'Selection got a gamepiece back for ' + selectString);
  }
});

test('Random none selected', t => {
  t.plan(2);

  let selectorTemplate = {
    random: true,
    selector:{
      left : 'MINION'
    }
  };
  let selector = new Selector(players, heroesOnly, mapState);
  let selection = selector.selectPieces(selectorTemplate, {selfPiece, controllingPlayerId: 1});
  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 0, 'Got nothing back');
});

test('Characters', t => {
  t.plan(3);
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces({left: 'CHARACTER' }, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all the pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
});

test('Friendly Characters Intersection', t => {
  t.plan(6);
  let select =
    {
      left: 'FRIENDLY',
      op: '&',
      right: 'CHARACTER'
    };
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got only friendly Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 1, 'First piece is for the right player');

  let emptySelector = new Selector(players, noPieces, mapState);
  let emptySelection = emptySelector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Enemy Characters by Difference', t => {
  t.plan(9);
  let select =
    {
      left: 'CHARACTER',
      op: '-',
      right: 'FRIENDLY'
    };
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got only enemy Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'First piece is for the enemy player');

  let emptySelector = new Selector(players, noPieces, mapState);
  let emptySelection = emptySelector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');

  let heroSelector = new Selector(players, heroesOnly, mapState);
  let heroesSelection = heroSelector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  t.equal(heroesSelection.length, 1, 'Selected 1 hero');
  t.equal(heroesSelection[0].tags[0], 'Hero', 'Selected piece is a hero');
  t.equal(heroesSelection[0].playerId, 2, 'Selected enemy hero');
});

test('Enemy Minions', t => {
  t.plan(7);
  let select =
    {
      left: 'ENEMY',
      op: '&',
      right: 'MINION'
    };
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, 'Got only enemy Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'First piece is for the right player');
  t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

  let emptySelector = new Selector(players, noPieces, mapState);
  let emptySelection = emptySelector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Peace treaty union', t => {
  t.plan(5);
  let select =
    {
      left: 'ENEMY',
      op: '|',
      right: 'FRIENDLY'
    };
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');

  let emptySelector = new Selector(players, noPieces, mapState);
  let emptySelection = emptySelector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Nested condition', t => {
  t.plan(4);
  let select =
    {
      left: {
        left: 'FRIENDLY',
        op: '&',
        right: 'CHARACTER'
      },
      op: '-',
      right: 'HERO'
    };
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, 'Got back friendly characters who arent a hero');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

});

test('Self Selector', t => {
  t.plan(5);
  let select =
    {
      left: 'SELF',
    };

  let selfPiece = pieceStateMix.pieces[pieceStateMix.pieces.length - 1];
  let triggerPiece = pieceStateMix.pieces[pieceStateMix.pieces.length - 2];

  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, activatingPiece: triggerPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back just self');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0], selfPiece, 'Selected piece is self piece');
  t.notEqual(selection[0], triggerPiece, 'Selected piece is not triggering piece');
});


test('Possible Targets', t => {
  t.plan(7);
  let nonTargetSelect = {
    left: 'FRIENDLY',
    op: '&',
    right: 'CHARACTER'
  };

  let selector = new Selector(players, pieceStateMix, mapState);
  let nonTargetSelection = selector.selectPossibleTargets(nonTargetSelect, {controllingPlayerId: 1});

  t.ok(Array.isArray(nonTargetSelection), 'Got back an Array');
  t.equal(nonTargetSelection.length, 0, 'Nothing to target here');

  let randomNonTarget = {
    random: true,
    selector:{
      left : 'MINION'
    }
  };

  let randoTargetSelection = selector.selectPossibleTargets(randomNonTarget, {controllingPlayerId: 1});

  t.ok(Array.isArray(randoTargetSelection), 'Got back an Array');
  t.equal(randoTargetSelection.length, 0, 'Nothing to target here');

  let targetSelector = {
    left: 'FRIENDLY',
    op: '&',
    right: 'TARGET'
  };

  let targetSelect = selector.selectPossibleTargets(targetSelector, {controllingPlayerId: 1});

  t.ok(Array.isArray(targetSelect), 'Got back an Array');
  t.equal(targetSelect.length, 3, 'Got back 3 targets');
  t.equal(targetSelect[0].playerId, 1, 'Got back friendly target');
});

test('Status Selector', t => {
  t.plan(5);
  let select =
    {
      left: 'SHIELD',
      op: '&',
      right: 'FRIENDLY'
    };

  let selector = new Selector(players, statusPieces, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back one shielded');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 1, 'Friendly minion');
  t.equal(selection[0].statuses, Statuses.Shield, 'Selected piece has shield');
});

test('Selector with comparison expression', t => {
  t.plan(5);
  let select =
    {
      left: {
        left: "ENEMY",
        op: "&",
        right: {
          compareExpression: true,
          left: {
            eNumber: true,
            attributeSelector: {
              left: "ENEMY",
              op: "&",
              right: "MINION"
            },
            attribute: "attack"
          },
          op: "<",
          right: 3
        }
      },
      op: "&",
      right: "MINION"
    };

  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back only one piece');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'Enemy minion');
  t.ok(selection[0].attack < 3, 'Attack is less than selector specified');
});

test('Raw Comparison expression', t => {
  t.plan(3);
  let selector = new Selector(players, pieceStateMix, mapState);
  let compareNumbers =
    {
      compareExpression: true,
      left: 3,
      op: "<",
      right: 3
    };

  let compareNumberResult = selector.compareExpression(
    compareNumbers, pieceStateMix, {selfPiece, controllingPlayerId: 1}
  );
  t.equal(compareNumberResult.length, 0, 'Compare returned nothing');

  let compareTwoAttribute =
    {
      compareExpression: true,
      left: {
        eNumber: true,
        attributeSelector: {
          left: "ENEMY",
          op: "&",
          right: "MINION"
        },
        attribute: "attack"
      },
      op: "<",
      right: {
        eNumber: true,
        attributeSelector: {
          left: "ENEMY",
          op: "&",
          right: "MINION"
        },
        attribute: "health"
      }
    };
  t.equal(
    selector.compareExpression(compareTwoAttribute, pieceStateMix, {selfPiece, controllingPlayerId: 1} ).length
    , 0
    , 'Compare on two attribute selectors with more than one piece returns empty'
  );

  let compareTwoAttributeSingle =
    {
      compareExpression: true,
      left: {
        eNumber: true,
        attributeSelector: {
          left: "ENEMY",
          op: "&",
          right: "HERO"
        },
        attribute: "attack"
      },
      op: "<",
      right: {
        eNumber: true,
        attributeSelector: {
          left: "ENEMY",
          op: "&",
          right: "HERO"
        },
        attribute: "health"
      }
    };

  t.equal(
    selector.compareExpression(compareTwoAttributeSingle, pieceStateMix, {selfPiece, controllingPlayerId: 1} )
    , pieceStateMix
    , 'Compare on two single pieces returns the pieces back'
  );
});

var pieceStatePositions = new PieceState();
spawnPiece(pieceStatePositions, 1, 1, new Position(0, 0, 0));
spawnPiece(pieceStatePositions, 1, 2, new Position(0, 0, 1));
spawnPiece(pieceStatePositions, 1, 2, new Position(0, 0, 2));
spawnPiece(pieceStatePositions, 1, 2, new Position(0, 0, 3));
spawnPiece(pieceStatePositions, 1, 2, new Position(1, 0, 0));
spawnPiece(pieceStatePositions, 1, 2, new Position(1, 0, 1));
spawnPiece(pieceStatePositions, 1, 2, new Position(1, 0, 2));
spawnPiece(pieceStatePositions, 1, 2, new Position(1, 0, 3));
spawnPiece(pieceStatePositions, 2, 2, new Position(2, 0, 0));
spawnPiece(pieceStatePositions, 2, 2, new Position(2, 0, 1));
spawnPiece(pieceStatePositions, 2, 2, new Position(2, 0, 2));
spawnPiece(pieceStatePositions, 2, 2, new Position(2, 0, 3));
spawnPiece(pieceStatePositions, 2, 2, new Position(3, 0, 0));
spawnPiece(pieceStatePositions, 2, 2, new Position(3, 0, 1));
spawnPiece(pieceStatePositions, 2, 2, new Position(3, 0, 2));
spawnPiece(pieceStatePositions, 2, 1, new Position(3, 0, 3));

test('Area Selector', t => {
  t.plan(4);
  let select =
    {
      left: {
        area: true,
        args: [
          'Square',
          1,
          {
            left: 'SELF'
          }
        ]
      }
    };

  var selfPiecePosition = new GamePiece();
  selfPiecePosition.position = new Position(1, 0, 1);

  let selector = new Selector(players, pieceStatePositions, mapState);
  let selection = selector.selectPieces(select, {selfPiece: selfPiecePosition, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 9, 'Got back the nine tiles');
  t.ok(selection.some(p => p.position.tileEquals(new Position(0, 0, 0))), 'Someone is at (0, 0, 0)');
  t.ok(selection.some(p => p.position.tileEquals(new Position(2, 0, 2))), 'Someone is at (2, 0, 2)');
});

test('Line Selector with cursor', t => {
  t.plan(6);
  let select =
    {
      left: {
        area: true,
        args: [
          'Line',
          2,
          {
            left: 'CURSOR'
          },
          {
            left: 'CURSOR'
          },
          false
        ]
      }
    };

  var selfPiecePosition = new GamePiece();
  selfPiecePosition.position = new Position(1, 0, 1);

  let selector = new Selector(players, pieceStatePositions, mapState);
  let selection = selector.selectPieces(select, {
    controllingPlayerId: 1,
    selfPiece: selfPiecePosition,
    position: new Position(1, 0, 0),
    pivotPosition: new Position(1, 0, 1)
  });

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got back three tiles');
  t.ok(selection.some(p => p.position.tileEquals(new Position(1, 0, 0))), 'Someone is at (0, 0, 0)');
  t.ok(selection.some(p => p.position.tileEquals(new Position(1, 0, 1))), 'Someone is at (1, 0, 1)');
  t.ok(selection.some(p => p.position.tileEquals(new Position(1, 0, 2))), 'Someone is at (1, 0, 2)');

  t.throws(() =>
    selector.selectPieces(select, {
      controllingPlayerId: 1,
      selfPiece: selfPiecePosition,
      position: new Position(1, 0, 0),
      pivotPosition: new Position(0, 0, 2)
    })
  , 'Threw exception for bad pivot position');
});

test('Diagonal Selector with one cursor', t => {
  t.plan(6);
  let select =
    {
      left: {
        area: true,
        args: [
          'Diagonal',
          2,
          {
            left: 'SELF'
          },
          {
            left: 'CURSOR'
          },
          false
        ]
      }
    };

  var selfPiecePosition = new GamePiece();
  selfPiecePosition.position = new Position(1, 0, 1);

  let selector = new Selector(players, pieceStatePositions, mapState);
  let selection = selector.selectPieces(select, {
    controllingPlayerId: 1,
    selfPiece: selfPiecePosition,
    position: new Position(1, 0, 0),
    pivotPosition: new Position(2, 0, 2)
  });

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got back three tiles');
  t.ok(selection.some(p => p.position.tileEquals(new Position(1, 0, 1))), 'Someone is at (1, 0, 1)');
  t.ok(selection.some(p => p.position.tileEquals(new Position(2, 0, 2))), 'Someone is at (2, 0, 2)');
  t.ok(selection.some(p => p.position.tileEquals(new Position(3, 0, 3))), 'Someone is at (3, 0, 3)');

  t.throws(() =>
    selector.selectPieces(select, {
      controllingPlayerId: 1,
      selfPiece: selfPiecePosition,
      position: new Position(1, 0, 0),
      pivotPosition: new Position(0, 0, 1)
    })
  , 'Threw exception for bad pivot position');
});

test('Tag selection', t => {
  let select =
    {
      left: {
        tag: 'Minion'
      }
    };
  let selectMinion = {
    left : 'MINION'
  };
  t.plan(3);
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});
  let minionSelection = selector.selectPieces(selectMinion, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, minionSelection.length, 'Got back same number for minions');
  t.equal(selection[0], minionSelection[0], 'First element of selections are the same');
});

test('Template Id selection', t => {
  let select =
    {
      left: {
        templateId: 2
      }
    };
  t.plan(3);
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, '2 Id 2 pieces');
  t.equal(selection[0].cardTemplateId, 2, 'Got the right Id back');
});

test('Piece Id selection', t => {
  let select =
    {
      left: {
        pieceIds: [2, 3]
      }
    };
  t.plan(4);
  let selector = new Selector(players, pieceStateMix, mapState);
  let selection = selector.selectPieces(select, {selfPiece, controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, '2 Id 2 pieces');
  t.equal(selection[0].id, 2, 'Got the right first Id back');
  t.equal(selection[1].id, 3, 'Got the right second Id back');
});

test('Eventual numbers', t => {
  t.plan(3);
  let randomList = {
    eNumber: true,
    randList: [1, 2, 3]
  };

  let selector = new Selector(players, pieceStateMix, mapState);

  let randNumber = selector.eventualNumber(randomList, {selfPiece, controllingPlayerId: 1});
  t.ok(randomList.randList.includes(randNumber), 'Got back a random number in array ' + randNumber);

  let attributeSelector = {
    eNumber: true,
    attributeSelector: {
      left: "ENEMY",
      op: "&",
      right: "HERO"
    },
    attribute: "health"
  };
  let heroHp = selector.eventualNumber(attributeSelector, {selfPiece, controllingPlayerId: 1});
  t.equal(heroHp, 30, 'Got back the hero hp');

  let countSelector = {
    eNumber: true,
    count: true,
    selector: {
      left: "ENEMY",
      op: "&",
      right: "MINION"
    }
  };
  let enemyCount = selector.eventualNumber(countSelector, {selfPiece, controllingPlayerId: 1});
  t.equal(enemyCount, 2, 'Got right number of enemies');

});

test('Card Basic Selections', t => {
  t.plan(6);
  let select =
    {
      left: 'FRIENDLY'
    };
  let selector = new Selector(players, pieceStateMix, mapState, cardStateMix);
  let selection = selector.selectCards(select, {controllingPlayerId: 1});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 4, 'Got only friendly Cards');
  t.ok(selection[0] instanceof Card, 'First element is a card');
  t.equal(selection[0].playerId, 1, 'First card is for the right player');

  let emptySelector = new Selector(players, pieceStateMix, mapState, noCards);
  let emptySelection = emptySelector.selectCards(select, {controllingPlayerId: 1});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Adv Card Selections', t => {
  t.plan(12);
  let selector = new Selector(players, pieceStateMix, mapState, cardStateMix);
  let fMinions = selector.selectCards({
      left: 'DECK',
      op: '&',
      right: 'MINION'
    }, {controllingPlayerId: 1});

  t.equal(fMinions.length, 2, 'Got only friendly Minions');
  t.equal(fMinions[0].playerId, 1, 'First card is for the right player');
  t.equal(fMinions[1].playerId, 2, 'Second card is for the right player');
  t.ok(fMinions[0].tags.includes('Minion'), 'First card is a minion');

  let eMinions = selector.selectCards({
      left: 'ENEMY',
      op: '-',
      right: 'SPELL'
    }, {controllingPlayerId: 1});
  t.equal(eMinions.length, 2, 'Got only enemy Minions');
  t.equal(eMinions[0].playerId, 2, 'First card is for the right player');
  t.ok(eMinions[0].tags.includes('Minion'), 'First card is a minion');

  let tagged = selector.selectCards({
      left: {
        tag: 'Minion'
      }
    }, {controllingPlayerId: 1});
  t.equal(tagged.length, 4, 'Got tagged minions in hand');
  t.ok(tagged[0].tags.includes('Minion'), 'First tagged is minion');

  let unions = selector.selectCards({
      left: 'DECK',
      op: '|',
      right: 'HAND'
    }, {controllingPlayerId: 1});
  t.equal(unions.length, 8, 'Got union of all cards');

  let fHandUnion = selector.selectCards({
      left: 'FRIENDLY',
      op: '|',
      right: 'HAND'
    }, {controllingPlayerId: 1});
  t.equal(fHandUnion.length, 6, 'Should get all friendly cards + 2 in other hand');

  let exclusive = selector.selectCards({
      left: 'DECK',
      op: '&',
      right: 'HAND'
    }, {controllingPlayerId: 1});
  t.equal(exclusive.length, 0, 'Got nothin for both hand and deck');
});

function spawnPiece(pieceState, cardTemplateId, playerId, position){
  var newPiece = pieceState.newFromCard(cardDirectory, cardTemplateId, playerId, null);
  newPiece.position = position;

  pieceState.add(newPiece);
  return newPiece;
}

function spawnCard(cardState, cardTemplateId, playerId, addToHand){
  let cardClone = cardDirectory.newFromId(cardTemplateId);
  cardState.addToDeck(playerId, cardClone);

  return cardClone;
}
