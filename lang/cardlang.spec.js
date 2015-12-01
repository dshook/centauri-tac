import test from 'tape';
import lang from './cardlang.js';

test('basic play event', t => {
  t.plan(1);

  let parser = lang.parser;
  let input = `
  play{ 
    DrawCard(PLAYER); 
  }`;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      action: 'DrawCard',
      args: [
        'PLAYER'
      ]
    }
  ];

  t.deepEqual(d.play, expectedPlay);
});

test('Two actions on event', t => {
  t.plan(1);

  let parser = lang.parser;
  let input = `
  play{ 
    DrawCard(PLAYER); 
    SetAttribute(TARGET, current_health, 3);
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      action: 'DrawCard',
      args: [
        'PLAYER'
      ]
    },
    {
      action: 'SetAttribute',
      args: [
        'TARGET',
        'current_health',
        '3'
      ]
    }
  ];

  t.deepEqual(d.play, expectedPlay);
});

test('Two Events', t => {
  t.plan(1);

  let parser = lang.parser;
  let input = `
  play{ 
    SetAttribute(TARGET, current_health, 3);
  }
  death{
    DrawCard(PLAYER, 3);
  }
  `;

  let d = parser.parse(input);

  let expected = {
    play: [
      {
        action: 'SetAttribute',
        args: [
          'TARGET',
          'current_health',
          '3'
        ]
      }
    ],
    death: [
      {
        action: 'DrawCard',
        args: [
          'PLAYER',
          '3'
        ]
      }
    ]
  };

  t.deepEqual(d, expected);
});

test('Repeating action', t => {
  t.plan(1);

  let parser = lang.parser;
  let input = `
  play{ 
    DrawCard(PLAYER) * 2; 
  }
  `;

  let d = parser.parse(input);

  let expectedPlay = [
    {
      action: 'DrawCard',
      args: [
        'PLAYER'
      ],
      times: '2'
    }
  ];

  t.deepEqual(d.play, expectedPlay);
});
