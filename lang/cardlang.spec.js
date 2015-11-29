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
