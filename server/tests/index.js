process.on('unhandledRejection', (reason, p) => {
  console.log('Unhandled Rejection at: Promise', p, 'reason:', reason);
});

require('./MapState.spec.js');
require('./Selector.spec.js');
require('./CardEvaluator.spec.js');
require('./Processors.spec.js');
