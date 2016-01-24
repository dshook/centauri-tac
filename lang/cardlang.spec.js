import test from 'tape';
import lang from './cardlang.js';

var parser = lang.parser;

test('basic play event', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER);
  }`;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            {
              left: 'PLAYER'
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Two actions on event', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER);
    SetAttribute(TARGET, health, 3);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            {
              left: 'PLAYER'
            }
          ]
        },
        {
          action: 'SetAttribute',
          args: [
            { left: 'TARGET' },
            'health',
            3
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Two Events', t => {
  t.plan(1);

  let input = `
  playMinion{
    SetAttribute(Random(CHARACTER), health, 3);
  }
  death{
    DrawCard(PLAYER);
  }
  `;

  let d = parser.parse(input);

  let expected = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'SetAttribute',
          args: [
            {
              random: true,
              selector: {
                left: 'CHARACTER'
              }
            },
            'health',
            3
          ]
        }
      ]
    },
    {
      event: 'death',
      actions: [
        {
          action: 'DrawCard',
          args: [
            { left: 'PLAYER' }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});

test('Repeating action', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER) * 2;
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            { left: 'PLAYER' }
          ],
          times: 2
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Hit action', t => {
  t.plan(1);

  let input = `
  playMinion{
    Hit(CHARACTER, 2);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'Hit',
          args: [
            {left :'CHARACTER' },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Selector input', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(ENEMY & CHARACTER & HERO);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            {
              left :
                {
                  left: 'ENEMY',
                  op: '&',
                  right: 'CHARACTER'
                },
              op: '&',
              right: 'HERO'
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Selector input with random', t => {
  t.plan(1);

  let input = `
  playMinion{
    Hit(Random(FRIENDLY & MINION - HERO), 1);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'Hit',
          args: [
            {
              random: true,
              selector:{
                left :
                  {
                    left: 'FRIENDLY',
                    op: '&',
                    right: 'MINION'
                  },
                op: '-',
                right: 'HERO'
              }
            },
            1
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Heal action on damaged', t => {
  t.plan(1);

  let input = `
  damaged{
    Heal(FRIENDLY & HERO, 2);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'damaged',
      actions: [
        {
          action: 'Heal',
          args: [
            {
              left: 'FRIENDLY',
              op: '&',
              right: 'HERO'
            },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Event Selector', t => {
  t.plan(1);

  let input = `
  damaged(ENEMY & MINION){
    Heal(FRIENDLY & HERO, 2);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'damaged',
      selector: {
        left: 'ENEMY',
        op: '&',
        right: 'MINION'
      },
      actions: [
        {
          action: 'Heal',
          args: [
            {
              left: 'FRIENDLY',
              op: '&',
              right: 'HERO'
            },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Random number list', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER) * Random(1,2,3);
  }`;

  let d = parser.parse(input);

  let expected = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            {
              left: 'PLAYER'
            }
          ],
          times: {
            randList: [1, 2, 3]
          }
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});

test('Buff', t => {
  t.plan(1);

  let input = `
    attacks{
      Buff(FRIENDLY, 'Buff of the Chosen', attack(1), health(2) );
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'attacks',
      actions: [
        {
          action: 'Buff',
          args: [
            {
              left: 'FRIENDLY'
            },
            'Buff of the Chosen',
            {
              attribute: 'attack',
              amount: 1
            },
            {
              attribute: 'health',
              amount: 2
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});
