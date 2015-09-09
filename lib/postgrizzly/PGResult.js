import moment from 'moment';
import fromSQL from './fromSQLSymbol.js';
import _ from 'lodash';

/**
 * Iterable thing that holds our results
 */
export default class PGResult
{
  constructor(results, types = null, mapping = x => x)
  {
    this._results = results;

    // Ensure that types is always an array or null
    this.types = types && !Array.isArray(types) ? [types] : types;

    // the way the final models are combined (if needed)
    this.mapping = mapping;
  }

  /**
   * Dump n chump
   */
  toArray()
  {
    return [...this];
  }

  /**
   * First item or null of none
   */
  firstOrNull(): ?Object
  {
    for (const m of this) {
      return m;
    }

    return null;
  }

  /**
   * The results are iterable, with all processing happening "lazily" as we
   * iterate
   */
  * [Symbol.iterator]()
  {
    for (const row of this._results.rows) {

      // break fields to separate POJOs based on table
      const models = this._processModels(row);

      // Map each model into a class instance (if provided)
      if (this.types) {
        this._mapModels(models);
      }

      // yield a single model if we only have one, otherwise projected
      // according to our injected rule, otherwise just as the array
      if (models.length === 1) {
        yield models[0];
      }
      else if (this.mapping) {
        yield this.mapping(...models);
      }
      else {
        yield models;
      }
    }
  }

  /**
   * Map a set of models according to our type information
   * @private
   */
  _mapModels(models): Array<Object>
  {
    for (const [index, model] of models.entries()) {

      // if we've run out of classes to map to
      if (index >= this.types.length) {
        continue;
      }

      const T = this.types[index];

      // Explicit map if one is implemented, otherwise just deep clone into a
      // new instance of the class
      if (typeof T[fromSQL] === 'function') {
        models[index] = T[fromSQL](model);
      }
      else {
        const m = new T();
        Object.assign(m, _.cloneDeep(model));
        models[index] = m;
      }
    }
  }

  /**
   * Loop over each field in a row to split out the results into proper models
   * @private
   */
  _processModels(row): Array<Object>
  {
    const models = new Map();

    for (const [index, f] of this._results.fields.entries()) {
      const tId = f.tableID;

      // Get the model we're building up for this type
      let model = models.get(tId);
      if (!model) {
        model = {};
        models.set(tId, model);
      }

      // field value
      let val = row[index];

      // timestamp with time zone
      if (f.dataTypeID === 1184) {
        val = val !== null ? moment.parseZone(val) : null;
      }

      // Add field
      model[f.name] = val;
    }

    return [...models.values()];
  }

}
