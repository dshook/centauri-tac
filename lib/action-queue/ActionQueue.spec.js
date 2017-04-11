import ActionQueue from './';
import test from 'tape';

class Nop
{
  handleAction(action, queue)
  {
    queue.complete(action);
  }
}

test('injected factory', async (t) => {
  t.plan(1);
  const factory = (T) => {
    t.strictEqual(T, Nop, 'passed in constructor');
  };

  const q = new ActionQueue(factory);
  q.addProcessor(Nop);
});

test('basic run', async (t) => {
  t.plan(1);
  const q = new ActionQueue();
  q.addProcessor(Nop);
  q.push({});
  await q.processUntilDone();
  t.pass('didnt blow up');
});


test('processor order and skipping', async (t) => {
  t.plan(4);

  const q = new ActionQueue();

  let a = 0;
  let b = 0;

  q.addProcessor(class T {
    handleAction(action, queue) {
      if (action.a) {
        a++;
        queue.complete(action);
      }
    }
  });

  q.addProcessor(class U {
    handleAction(action, queue) {
      if (action.b) {
        b++;
        queue.complete(action);
      }
    }
  });

  q.push({a: true});
  q.push({b: true});

  t.strictEqual(a, 0, 'hasnt run yet');
  t.strictEqual(b, 0, 'hasnt run yet');

  await q.processUntilDone();

  t.strictEqual(a, 1, 'correct ran');
  t.strictEqual(b, 1, 'correct ran');
});

test('completed actions since', async(t) => {
  t.plan(5);

  const q = new ActionQueue();
  q.addProcessor(Nop);

  q.push({a: true});
  q.push({b: true});
  q.push({c: true});

  await q.processUntilDone();

  const actions = [...q.iterateCompletedSince()];

  t.strictEqual(actions[0].a, true, 'in order');
  t.strictEqual(actions[1].b, true, 'in order');
  t.strictEqual(actions[2].c, true, 'in order');

  const _actions = [...q.iterateCompletedSince(actions[1].id)];
  t.strictEqual(_actions[0].c, true, 'since');
  t.strictEqual(_actions.length, 1);
});

test('pushing actions after an action', async (t) => {
  t.plan(5);

  const q = new ActionQueue();

  // will spawn 3 b actions whenever there's an a
  q.addProcessor(class SpawnBsAfterA {
    handleAction(action, queue) {
      if (action.a) {
        queue.push({b: true});
        queue.push({b: true});
        queue.push({b: true});
      }
    }
  });

  q.push({a: true});

  await q.processUntilDone();

  const actions = [...q.iterateCompletedSince()];

  t.strictEqual(actions.length, 4);
  t.strictEqual(actions[0].a, true, 'a');
  t.strictEqual(actions[1].b, true, 'b');
  t.strictEqual(actions[2].b, true, 'b');
  t.strictEqual(actions[3].b, true, 'b');
});

test('pushing actions before an action', async (t) => {
  t.plan(5);

  const q = new ActionQueue();

  // will spawn 3 b actions whenever there's an a
  q.addProcessor(class SpawnBsBeforeA {
    handleAction(action, queue) {
      if (action.a) {
        queue.pushFront({b: true});
        queue.pushFront({b: true});
        queue.pushFront({b: true});
      }
    }
  });

  q.push({a: true});

  await q.processUntilDone();

  const actions = [...q.iterateCompletedSince()];

  t.strictEqual(actions.length, 4);
  t.strictEqual(actions[0].b, true, 'b');
  t.strictEqual(actions[1].b, true, 'b');
  t.strictEqual(actions[2].b, true, 'b');
  t.strictEqual(actions[3].a, true, 'a');
});

test('pushing array of actions after an action', async (t) => {
  t.plan(5);

  const q = new ActionQueue();

  // will spawn 3 b actions whenever there's an a
  q.addProcessor(class SpawnBsAfterA {
    handleAction(action, queue) {
      if (action.a) {
        queue.push([{b: true}, {c: true}, {d: true}]);
      }
    }
  });

  q.push({a: true});

  await q.processUntilDone();

  const actions = [...q.iterateCompletedSince()];

  t.strictEqual(actions.length, 4);
  t.strictEqual(actions[0].a, true, 'a');
  t.strictEqual(actions[1].b, true, 'b');
  t.strictEqual(actions[2].c, true, 'c');
  t.strictEqual(actions[3].d, true, 'd');
});

test('pushing array of actions before an action', async (t) => {
  t.plan(5);

  const q = new ActionQueue();

  // will spawn 3 b actions whenever there's an a
  q.addProcessor(class SpawnBsBeforeA {
    handleAction(action, queue) {
      if (action.a) {
        queue.pushFront([{b: true}, {c: true}, {d: true}]);
      }
    }
  });

  q.push({a: true});

  await q.processUntilDone();

  const actions = [...q.iterateCompletedSince()];

  t.strictEqual(actions.length, 4);
  t.strictEqual(actions[0].b, true, 'b');
  t.strictEqual(actions[1].c, true, 'b');
  t.strictEqual(actions[2].d, true, 'b');
  t.strictEqual(actions[3].a, true, 'a');
});
