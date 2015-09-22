import test from 'tape';
import {AggregateBinder} from './';
import {EventEmitter} from 'events';
import {on} from './';

test('basic multi binding', t => {
  t.plan(6);

  const DATA = {};
  let CHECK = null;

  class T {
    @on('e')
    foo(data, emitter) {
      t.strictEqual(data, DATA, 'param is passed');
      t.strictEqual(emitter, CHECK, 'emitter is passed as last arg');
    }
  }

  const emitter1 = new EventEmitter();
  const emitter2 = new EventEmitter();

  const binder = new AggregateBinder();
  binder.addEmitter(emitter1);
  binder.addEmitter(emitter2);

  const o = new T();
  binder.bindInstance(o);

  CHECK = emitter1;
  emitter1.emit('e', DATA); // +2

  CHECK = emitter2;
  emitter2.emit('e', DATA); // +2

  binder.removeEmitter(emitter1);
  emitter1.emit('e', DATA); // nop
  emitter2.emit('e', DATA); // +2

  binder.removeEmitter(emitter2);
  emitter2.emit('e', DATA); // nop
});

