import test from 'tape';
import lang from './cardlang.js';

var parser = lang.parser;

test('basic play event', t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER)
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
    DrawCard(PLAYER)
    SetAttribute(TARGET, health, 3)
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
    SetAttribute(Random(CHARACTER), health, 3)
  }
  death{
    DrawCard(PLAYER)
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
    DrawCard(PLAYER) * 2
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
    Hit(CHARACTER, 2)
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
    DrawCard(ENEMY & CHARACTER & HERO)
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
    Hit(Random(FRIENDLY & MINION - HERO), 1)
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
    Heal(FRIENDLY & HERO, 2)
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
    Heal(FRIENDLY & HERO, 2)
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'damaged',
      args: [{
        left: 'ENEMY',
        op: '&',
        right: 'MINION'
      }],
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
    DrawCard(PLAYER) * Random(1,2,3)
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
            eNumber: true,
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
      Buff(FRIENDLY, 'Buff of the Chosen', attack(1), health(2) )
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

test('Turn timers', t => {
  t.plan(1);

  let input = `
    playSpell{
      GiveStatus(ENEMY & MINION, Paralyze)
      startTurnTimer(2, false, RemoveStatus(SAVED, Paralyze) )
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: "playSpell",
      actions: [
        {
          action: "GiveStatus",
          args: [
            {
              left: "ENEMY",
              op: "&",
              right: "MINION"
            },
            "Paralyze"
          ]
        },
        {
          action: "startTurnTimer",
          args: [
            2,
            false,
            {
              action: "RemoveStatus",
              args: [
                {
                  left: "SAVED"
                },
                "Paralyze"
              ]
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Eventual Number with Attribute selector', t => {
  t.plan(1);

  let input = `
    playMinion{
      SetAttribute(TARGET & MINION, attack, SelectAttribute(TARGET, health))
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: "playMinion",
      actions: [
        {
          action: "SetAttribute",
          args: [
            {
              left: "TARGET",
              op: "&",
              right: "MINION"
            },
            "attack",
            {
              eNumber: true,
              attributeSelector: {
                left: "TARGET"
              },
              attribute: "health"
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Selector with comparison expression', t => {
  t.plan(1);

  let input = `
    playMinion{
      Hit(TARGET & (SelectAttribute(TARGET, health) > 4) & MINION, 1)
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: "playMinion",
      actions: [
        {
          action: "Hit",
          args: [
            {
              left: {
                left: "TARGET",
                op: "&",
                right: {
                  compareExpression: true,
                  left: {
                    eNumber: true,
                    attributeSelector: {
                      left: "TARGET"
                    },
                    attribute: "health"
                  },
                  op: ">",
                  right: 4
                }
              },
              op: "&",
              right: "MINION"
            },
            1
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Event with number and text', t => {
  t.plan(1);

  let input = `
    ability(2, 'Test'){ Hit(TARGET, 1)  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'ability',
      args: [
        2,
        'Test'
      ],
      actions: [
        {
          action: 'Hit',
          args: [
            {
              left: 'TARGET',
            },
            1
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Hit area', t => {
  t.plan(1);

  let input = `
    death{ Hit(Area(SELF, Square, 1), 2) }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'death',
      actions: [
        {
          action: 'Hit',
          args: [
            {
              left: {
                area: true,
                args: [
                  {
                    left: 'SELF'
                  },
                  'Square',
                  1
                ]
              }
            },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Tag selection', t => {
  t.plan(1);

  let input = `
    death{ Hit(Tagged('test'), 2) }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      event: 'death',
      actions: [
        {
          action: 'Hit',
          args: [
            {
              left: {
                tag: 'test'
              }
            },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Count eNum selector', t => {
  t.plan(1);

  let input = `
    playSpell{DrawCard(PLAYER) * Count(ENEMY & MINION) }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "playSpell",
      "actions": [
        {
          "action": "DrawCard",
          "args": [
            {
              "left": "PLAYER"
            }
          ],
          "times": {
            "eNumber": true,
            "count": true,
            "selector": {
              "left": "ENEMY",
              "op": "&",
              "right": "MINION"
            }
          }
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Conditional action', t => {
  t.plan(1);

  let input = `
    playSpell{ Count(FRIENDLY & MINION) > 1 && DrawCard(PLAYER) Discard(OPPONENT)}
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "playSpell",
      "actions": [
        {
          "condition": {
            "compareExpression": true,
            "left": {
              "eNumber": true,
              "count": true,
              "selector": {
                "left": "FRIENDLY",
                "op": "&",
                "right": "MINION"
              }
            },
            "op": ">",
            "right": 1
          },
          "action": "DrawCard",
          "args": [
            {
              "left": "PLAYER"
            }
          ]
        },
        {
          "action": "Discard",
          "args": [
            {
              "left": "OPPONENT"
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Attach code', t => {
  t.plan(1);

  let input = `
    playSpell{
      AttachCode(TARGET, death{ Hit(ENEMY,1) } )
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "playSpell",
      "actions": [
        {
          "action": "AttachCode",
          "args": [
            {
              "left": "TARGET"
            },
            [
              {
                "event": "death",
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
            ]
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Conditional expression as arg', t => {
  t.plan(1);

  let input = `
  playMinion{
    Buff(SELF, SelectAttribute(SELF, health) > 2, cost(-2) )
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "Buff",
          "args": [
            {
              "left": "SELF"
            },
            {
              "compareExpression": true,
              "left": {
                "eNumber": true,
                "attributeSelector": {
                  "left": "SELF"
                },
                "attribute": "health"
              },
              "op": ">",
              "right": 2
            },
            {
              "attribute": "cost",
              "amount": -2
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Double Conditional expression', t => {
  t.plan(1);

  let input = `
    playMinion{
      COMBOCOUNT == 0 && Hit(ENEMY & MINION, 1)
      COMBOCOUNT > 0 && Hit(ENEMY & MINION, 2)
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "condition": {
            "compareExpression": true,
            "left": {
              "stat": true,
              "path": "COMBOCOUNT"
            },
            "op": "==",
            "right": 0
          },
          "action": "Hit",
          "args": [
            {
              "left": "ENEMY",
              "op": "&",
              "right": "MINION"
            },
            1
          ]
        },
        {
          "condition": {
            "compareExpression": true,
            "left": {
              "stat": true,
              "path": "COMBOCOUNT"
            },
            "op": ">",
            "right": 0
          },
          "action": "Hit",
          "args": [
            {
              "left": "ENEMY",
              "op": "&",
              "right": "MINION"
            },
            2
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Conditional event', t => {
  t.plan(1);

  let input = `
    ability(1, 1, 'Charge Up')[ 1 > 2 ]{ ChargeUp(PLAYER, 1)  }
    chargeChange[Charges(PLAYER) >= 3]{
      Hit(Area(Square, 1, SELF) & ENEMY, 3)
      ChargeUp(PLAYER, -3)
    }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      "event": "ability",
      "args": [
        1,
        1,
        "Charge Up"
      ],
      "condition": {
        "compareExpression": true,
        "left": 1,
        "op": ">",
        "right": 2
      },
      "actions": [
        {
          "action": "ChargeUp",
          "args": [
            {
              "left": "PLAYER"
            },
            1
          ]
        }
      ]
    },
    {
      "event": "chargeChange",
      "condition": {
        "compareExpression": true,
        "left": {
          "resource": "Charges",
          "selector": {
            "left": "PLAYER"
          }
        },
        "op": ">=",
        "right": 3
      },
      "actions": [
        {
          "action": "Hit",
          "args": [
            {
              "left": {
                "area": true,
                "args": [
                  "Square",
                  1,
                  {
                    "left": "SELF"
                  }
                ]
              },
              "op": "&",
              "right": "ENEMY"
            },
            3
          ]
        },
        {
          "action": "ChargeUp",
          "args": [
            {
              "left": "PLAYER"
            },
            -3
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);

});