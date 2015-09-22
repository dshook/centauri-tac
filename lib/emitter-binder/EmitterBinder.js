import metaDecorator from 'meta-decorator';
import {getMeta} from 'meta-decorator';

const META_PROP = 'emitter-binder:on';
export const on = metaDecorator(META_PROP);

/**
 * Bind the methods of a class instance to EventEmitter with tagging via
 * decorators
 */
export default class EmitterBinder
{
  constructor(emitter)
  {
    this._bindings = new WeakMap();
    this._emitter = emitter;
  }

  /**
   * Connect an instance to this emitter
   */
  bindInstance(instance)
  {
    if (this._bindings.has(instance)) {
      throw new Error('cannot bind instance twice');
    }

    const metas = getMeta(instance).filter(x => x.property === META_PROP);

    const bindings = [];

    for (const b of metas) {
      const fName = b.name;
      const [eventName, predicate = null, callMap = null] = b.args;

      const cb = (...args) => {

        // if we dont provide a predicate, or we have one that passes (also
        // calling it in the context of the instance)
        if (!predicate || predicate.call(instance, ...args)) {

          if (!callMap) {
            instance[fName](...args);
          }
          else {
            callMap.apply(this, [instance, fName, ...args]);
          }

        }

      };

      this._emitter.on(eventName, cb);
      bindings.push({eventName, cb});
    }

    this._bindings.set(instance, bindings);

    return bindings;
  }

  /**
   * Remove an instance from this emitter
   */
  unbindInstance(instance)
  {
    const bindings = this._bindings.get(instance);

    if (!bindings) {
      throw new Error('instance was never bound');
    }

    for (const {eventName, cb} of bindings) {
      this._emitter.removeListener(eventName, cb);
    }

    this._bindings.delete(instance);

    return bindings;
  }
}
