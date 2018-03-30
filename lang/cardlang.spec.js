import test from 'tape';
import parse from './index.js';

var opts = {objectPrintDepth: 8};

test('basic play event', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER)
  }`;

  let d = parse(input);

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

test('Two actions on event', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER)
    SetAttribute(TARGET, health, 3)
  }
  `;

  let d = parse(input);

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

test('Two Events', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    SetAttribute(Random(CHARACTER), health, 3)
  }
  death{
    DrawCard(PLAYER)
  }
  `;

  let d = parse(input);

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

test('Repeating action', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER) * 2
  }
  `;

  let d = parse(input);

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

test('Hit action', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    Hit(CHARACTER, 2)
  }
  `;

  let d = parse(input);

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

test('Grouping selectors', opts, t => {

  let input = `
  playMinion{
    DrawCard(ENEMY & (MINION - HERO) - FRIENDLY)
  }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "DrawCard",
          "args": [
            {
              "left": {
                "left": "ENEMY",
                "op": "&",
                "right": {
                  "left": "MINION",
                  "op": "-",
                  "right": "HERO"
                }
              },
              "op": "-",
              "right": "FRIENDLY"
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);

  let input2 = `
  playMinion{
    DrawCard(ENEMY & (MINION - (HERO - FRIENDLY)))
  }
  `
  let d2 = parse(input2);

  let expectedPlay2 = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "DrawCard",
          "args": [
            {
              "left": "ENEMY",
              "op": "&",
              "right": {
                "left": "MINION",
                "op": "-",
                "right": {
                  "left": "HERO",
                  "op": "-",
                  "right": "FRIENDLY"
                }
              }
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d2, expectedPlay2);
  t.end();
});

test('Selector input', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(ENEMY & CHARACTER & HERO)
  }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      event: 'playMinion',
      actions: [
        {
          action: 'DrawCard',
          args: [
            {
              left: {
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

test('Selector input with random', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    Hit(Random(FRIENDLY & MINION - HERO), 1)
  }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "Hit",
          "args": [
            {
              "random": true,
              "selector": {
                "left": {
                  "left": "FRIENDLY",
                  "op": "&",
                  "right": "MINION"
                },
                "op": "-",
                "right": "HERO"
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

test('Heal action on damaged', opts, t => {
  t.plan(1);

  let input = `
  damaged{
    Heal(FRIENDLY & HERO, 2)
  }
  `;

  let d = parse(input);

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

test('Event Selector', opts, t => {
  t.plan(1);

  let input = `
  damaged(ENEMY & MINION){
    Heal(FRIENDLY & HERO, 2)
  }
  `;

  let d = parse(input);

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

test('Random number list', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    DrawCard(PLAYER) * Random(1,2,3)
  }`;

  let d = parse(input);

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
            eValue: true,
            randList: [1, 2, 3]
          }
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});

test('Buff', opts, t => {
  t.plan(1);

  let input = `
    attacks{
      Buff(FRIENDLY, 'Buff of the Chosen', attack(1), health(2) )
    }
  `;

  let d = parse(input);

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

test('Turn timers', opts, t => {
  t.plan(1);

  let input = `
    playSpell{
      GiveStatus(ENEMY & MINION, Petrify)
      startTurnTimer(2, false, RemoveStatus(SAVED, Petrify) )
    }
  `;

  let d = parse(input);

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
            "Petrify"
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
                "Petrify"
              ]
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Eventual Number with Attribute selector', opts, t => {
  t.plan(1);

  let input = `
    playMinion{
      SetAttribute(TARGET & MINION, attack, SelectAttribute(TARGET, health))
    }
  `;

  let d = parse(input);

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
              eValue: true,
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

test('Selector with comparison expression', opts, t => {
  t.plan(1);

  let input = `
    playMinion{
      Hit(TARGET & (SelectAttribute(TARGET, health) > 4) & MINION, 1)
    }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "Hit",
          "args": [
            {
              "left": {
                "left": "TARGET",
                "op": "&",
                "right": {
                  "compareExpression": true,
                  "left": {
                    "eValue": true,
                    "attributeSelector": {
                      "left": "TARGET"
                    },
                    "attribute": "health"
                  },
                  "op": ">",
                  "right": 4
                }
              },
              "op": "&",
              "right": "MINION"
            },
            1
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expectedPlay);
});

test('Event with number and text', opts, t => {
  t.plan(1);

  let input = `
    ability(2, 'Test'){ Hit(TARGET, 1)  }
  `;

  let d = parse(input);

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

test('Hit area', opts, t => {
  t.plan(1);

  let input = `
    death{ Hit(Area(SELF, Square, 1), 2) }
  `;

  let d = parse(input);

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

test('Tag selection', opts, t => {
  t.plan(1);

  let input = `
    death{ Hit(Tagged('test'), 2) }
  `;

  let d = parse(input);

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

test('Count eNum selector', opts, t => {
  t.plan(1);

  let input = `
    playSpell{DrawCard(PLAYER) * Count(ENEMY & MINION) }
  `;

  let d = parse(input);

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
            "eValue": true,
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

test('Conditional action', opts, t => {
  t.plan(1);

  let input = `
    playSpell{ Count(FRIENDLY & MINION) > 1 && DrawCard(PLAYER) Discard(OPPONENT)}
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "playSpell",
      "actions": [
        {
          "condition": {
            "compareExpression": true,
            "left": {
              "eValue": true,
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

test('Attach code', opts, t => {
  t.plan(1);

  let input = `
    playSpell{
      AttachCode(TARGET, death{ Hit(ENEMY,1) } )
    }
  `;

  let d = parse(input);

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

test('Conditional expression as arg', opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    Buff(SELF, SelectAttribute(SELF, health) > 2, cost(-2) )
  }
  `;

  let d = parse(input);

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
                "eValue": true,
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

test('Double Conditional expression', opts, t => {
  t.plan(1);

  let input = `
    playMinion{
      COMBOCOUNT == 0 && Hit(ENEMY & MINION, 1)
      COMBOCOUNT > 0 && Hit(ENEMY & MINION, 2)
    }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "playMinion",
      "actions": [
        {
          "condition": {
            "compareExpression": true,
            "left": {
              "eValue": true,
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
              "eValue": true,
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

test('Conditional event', opts, t => {
  t.plan(1);

  let input = `
    ability(1, 1, 'Charge Up')[ 1 > 2 ]{ ChargeUp(PLAYER, 1)  }
    chargeChange[Charges(PLAYER) >= 3]{
      Hit(Area(Square, 1, SELF) & ENEMY, 3)
      ChargeUp(PLAYER, -3)
    }
  `;

  let d = parse(input);

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
          "eValue": true,
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

test('Adv Select Card Template Id', opts, t => {
  t.plan(1);

  let input = `
    death{
      Spawn(
        SelectCardTemplateId(Random(DIRECTORY & MINION & (SelectAttribute(DIRECTORY & MINION, attack) > 1)))
      , 1)
    }
  `;

  let d = parse(input);

  let expectedPlay = [
    {
      "event": "death",
      "actions": [
        {
          "action": "Spawn",
          "args": [
            {
              "eValue": true,
              "selectCardTemplateId": true,
              "cardSelector": {
                "random": true,
                "selector": {
                  "left": {
                    "left": "DIRECTORY",
                    "op": "&",
                    "right": "MINION"
                  },
                  "op": "&",
                  "right": {
                    "compareExpression": true,
                    "left": {
                      "eValue": true,
                      "attributeSelector": {
                        "left": "DIRECTORY",
                        "op": "&",
                        "right": "MINION"
                      },
                      "attribute": "attack"
                    },
                    "op": ">",
                    "right": 1
                  }
                }
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

test('eNumber Expressions', {objectPrintDepth: 10}, opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    Hit(
       MINION,
      (1 + 2) * 3
     )
   }
  `;

  let d = parse(input);

  let expected = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "Hit",
          "args": [
            {
              "left": "MINION"
            },
            {
              "eNumber": true,
              "op": "*",
              "left": {
                "eNumber": true,
                "op": "+",
                "left": 1,
                "right": 2
              },
              "right": 3
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});

test('Negate complicated eNumber expressions', {objectPrintDepth: 10}, opts, t => {
  t.plan(1);

  let input = `
  playMinion{
    Aura(
      SELF,
      SELF,
      'Trapped',
      attack(-Count(Area(Square, 1, SELF) - SELF)),
      health(-Count(Area(Square, 1, SELF) - SELF))
    )
  }
  `;

  let d = parse(input);

  let expected = [
    {
      "event": "playMinion",
      "actions": [
        {
          "action": "Aura",
          "args": [
            {
              "left": "SELF"
            },
            {
              "left": "SELF"
            },
            "Trapped",
            {
              "attribute": "attack",
              "amount": {
                "eNumber": true,
                "op": "negate",
                "left": {
                  "eValue": true,
                  "count": true,
                  "selector": {
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
                    "op": "-",
                    "right": "SELF"
                  }
                }
              }
            },
            {
              "attribute": "health",
              "amount": {
                "eNumber": true,
                "op": "negate",
                "left": {
                  "eValue": true,
                  "count": true,
                  "selector": {
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
                    "op": "-",
                    "right": "SELF"
                  }
                }
              }
            }
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});