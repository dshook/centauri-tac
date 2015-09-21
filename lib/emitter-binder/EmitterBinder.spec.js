import test from 'tape';
import EmitterBinder from './';
import {EventEmitter} from 'events';
import {on} from './';

const DATA = {};

test('basic binding / unbinding', t => {
  t.plan(2);

  class T {
    @on('hey')
    foo(data) { t.strictEqual(data, DATA, 'param is passed'); }
  }

  const o = new T();
  const emitter = new EventEmitter();
  const binder = new EmitterBinder(emitter);

  binder.bindInstance(o);

  emitter.emit('hey', DATA); // 1
  emitter.emit('hey', DATA); // 2
  emitter.emit('xhey', DATA); // nop

  binder.unbindInstance(o);
  emitter.emit('hey', DATA); // nop
});

test('predicate action', t => {
  t.plan(1);

  class T {
    @on('hey', ({a, b}) => a === b)
    foo() { t.pass('cb fired'); }
  }

  const o = new T();
  const emitter = new EventEmitter();
  const binder = new EmitterBinder(emitter);

  binder.bindInstance(o);

  emitter.emit('hey', {a: 1, b: 2}); // nop
  emitter.emit('hey', {a: 2, b: 2}); // 1
});
