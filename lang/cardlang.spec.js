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

  let expected = [
    {
      event: 'play',
      actions: [
        {
          action: 'DrawCard',
          args: [
            'PLAYER'
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
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

  let expected = [
    {
      event: 'play',
      actions: [
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
      ]
    }
  ];

  t.deepEqual(d, expected);
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

  let expected = [
    {
      event: 'play',
      actions: [
        {
          action: 'SetAttribute',
          args: [
            'TARGET',
            'current_health',
            '3'
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
            'PLAYER',
            '3'
          ]
        }
      ]
    }
  ];

  t.deepEqual(d, expected);
});
