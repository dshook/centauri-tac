import {EventEmitter} from 'events';
import EmitterBinder from './EmitterBinder.js';

/**
 * An emitter binder that can dynamically have multiple emitter sources wired
 * to multiple decorated listener instances
 */
export default class AggregateBinder
{
  constructor()
  {
    // internal bus and binder combo that all emitters are routed to
    this._emitter = new EventEmitter();
    this._binder = new EmitterBinder(this._emitter);

    // map of emitter -> old emit function
    this._emitters = new WeakMap();
  }

  /**
   * Bind a listening instance onto the internal bus binder
   */
  bindInstance(instance)
  {
    return this._binder.bindInstance(instance);
  }

  /**
   * Remove from bus binder
   */
  unbindInstance(instance)
  {
    return this._binder.unbindInstance(instance);
  }

  /**
   * Have an emitter broadcast events to bound listeners
   */
  addEmitter(emitter)
  {
    if (this._emitters.has(emitter)) {
      throw new Error('cannot add the same emitter twice');
    }

    const old = emitter.emit;
    const patched = (...args) => {

      // call original
      const r = old.apply(emitter, args);

      // broadcast on our bus
      args.push(emitter);
      this._emitter.emit(...args);

      return r;
    };

    emitter.emit = patched;
    this._emitters.set(emitter, old);
  }

  /**
   * Have an emitter stop broadcasting events to the bound listeners
   */
  removeEmitter(emitter)
  {
    if (!this._emitters.has(emitter)) {
      throw new Error('emitter not in list');
    }

    // restore original function and remove from our list
    emitter.emit = this._emitters.get(emitter);
    this._emitters.delete(emitter);
  }
}
