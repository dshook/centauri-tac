import {EventEmitter} from 'events';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

let _nextId = 0;

/**
 * Large queue that can have actions pushed onto it and inserted in at various
 * points
 */
@loglevel
export default class ActionQueue extends EventEmitter
{
  constructor(factory = T => new T())
  {
    super();

    // builder for our processors
    this._factory = factory;

    // systems that handle each type of action
    this._processors = [];

    // handler functions to get called before the queue complete signal is fired
    this._postCompleteProcessors = [];

    this.init();
  }

  init(){
    // number of proc runs
    this._ticks = 0;

    // all actions in the queue
    this._actions = [];

    // handled
    this._completed = [];

    // cancelled actions that need broadcasting
    this._cancelled = [];

    // action, processor obj tuple of actions that have been partially
    // processed
    this._processing = [];

    // how many completed actions we've broadcast
    this._broadcastedActions = 0;

    // how many cancelled actions we've broadcast
    this._broadcastedCancelledActions = 0;

    // queue was touched
    this._dirty = false;
  }

  /**
   * Add an action to the queue
   */
  push(action)
  {
    if(Array.isArray(action)){
      for(let curAction of action){
        curAction.id = _nextId++;
        this._actions.push(curAction);
      }

      this._dirty = true;
      return action.id;
    }else{
      action.id = _nextId++;
      this._actions.push(action);
      this._dirty = true;

      return action.id;
    }
  }

  /**
   * Add an action to the FRONT of the the queue
   */
  pushFront(action)
  {
    if(Array.isArray(action)){
      //loop backwards over array so the order remains correct
      for(let i = action.length - 1; i >= 0; i--){
        let curAction = action[i];
        curAction.id = _nextId++;
        this._actions.unshift(curAction);
      }
      this._dirty = true;
      return;
    }else{
      action.id = _nextId++;
      this._actions.unshift(action);
      this._dirty = true;

      return action.id;
    }
  }

  //Cancel an action, and emit an event if needed
  cancel(action, emit = false)
  {
    return this.complete(action, true, emit);
  }

  /**
   * Move an action from the queue into the completed bin and mark the queue as
   * dirty
   */
  complete(action, cancel = false, emit = true)
  {
    const verb = cancel ? 'cancel' : 'complete';

    if (this._actions[0] !== action) {
      throw new Error(`can only ${verb} action at start of queue`);
    }

    // move to completed stack
    this._actions.shift();

    if (!cancel && emit) {
      this._completed.push(action);
    }else if(cancel && emit){
      this._cancelled.push(action);
    }

    // remove all refs of processing
    _.remove(this._processing, x => x.action === action);

    this.log.info('%s %s: %s, %s still processing',
        verb, action.id, action.constructor.name, this._processing.length);

    this._dirty = true;
  }

  /**
   * Yields IDs since a specific id
   */
  * iterateCompletedSince(actionId = null)
  {
    let seen = actionId === null;

    for (const action of this._completed) {
      if (seen) {
        yield action;
        continue;
      }

      seen = action.id === actionId;
    }
  }

  /**
   * Get items without popping
   */
  peek(depth)
  {
    return this._actions.slice(0, depth);
  }

  /**
   * Add a processor in the pipeline
   */
  addProcessor(T)
  {
    const processor = this._factory(T);
    this._processors.push(processor);
  }

  addPostCompleteProcessor(T)
  {
    const processor = this._factory(T);
    this._postCompleteProcessors.push(processor);
    //also add to regular processors so actions can also trigger
    this._processors.push(processor);
  }

  /**
   * Process until the queue is empty
   */
  async processUntilDone()
  {
    this.emit('qps', this._ticks);

    // loop until we're done. since we're doing async its actually chaining
    // method calls so we're not at risk of locking up the stack frame. similar
    // to recursion via setImmediate just via promises
    let done;
    while (!done) {
      done = await this.process();
    }

    //now that we're done with the first step, see if there are any post complete
    //handlers that need to fire off and potentially add more actions to process
    for(let proc of this._postCompleteProcessors){
      await proc.handleAction(null, this);

      done = false;
      while (!done) {
        done = await this.process();
      }
    }

    this.emit('qpc', this._ticks);
  }

  /**
   * Do a single iteration of the queue processor. Returns true if we're done
   */
  async process()
  {
    this._dirty = false;

    if (!this._actions.length) {
      return true;
    }

    const action = this._actions[0];

    this.log.info('action proc %s: %s', action.id, action.constructor.name);

    for (const proc of this._processors) {

      // determine if this proc already happend for this action
      const alreadyProcessed = this._processing
        .find(x => x.action === action && x.proc === proc);

      if (alreadyProcessed) {
        continue;
      }

      this._processing.push({proc, action});

      await proc.handleAction(action, this);

      // queue has changed, can't go any further
      if (this._dirty) {
        break;
      }
    }

    // if nothing's happened, implicity complete the action
    if (!this._dirty) {
      this.complete(action);
    }

    this._ticks++;

    this._emitCompleted();
    this._emitCancelled();

    // done?
    return this._actions.length === 0;
  }

  /**
   * Emit all completed actions this tick
   */
  _emitCompleted()
  {
    while (this._broadcastedActions < this._completed.length) {
      this.emit('actionComplete', this._completed[this._broadcastedActions++]);
    }
  }

  /**
   * Same for cancelled
   */
  _emitCancelled()
  {
    while (this._broadcastedCancelledActions < this._cancelled.length) {
      this.emit('actionCancelled', this._cancelled[this._broadcastedCancelledActions++]);
    }
  }
}

