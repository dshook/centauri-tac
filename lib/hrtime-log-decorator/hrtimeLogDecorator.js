import debug from 'debug';

/**
 * Use the debug log library to output timing information for async functions
 *
 * @example
 * class Foo
 * {
 *   @hrtime('took %d ms to do this')
 *   async task()
 *   {
 *     return await someLongTask();
 *   }
 * }
 */
export default function hrtimeLogDecorator(msg)
{
  return (name, target, desc) => {

    // original function
    const method = desc.value;

    let logger;

    desc.value = async function wrapped(...args) {
      const start = process.hrtime();
      const ret = await method.apply(this, args);
      const [s, ns] = process.hrtime(start);
      const duration = (s + ns / 1e9) * 1000;

      if (!logger) {
        logger = debug(this.constructor.name);
      }

      logger(msg, duration);
      return ret;
    };
  };
}

