// local lib hack
// https://gist.github.com/branneman/8048520
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

// compiler hook
require('babel/register');

// use bluebird instead of native promise
global.Promise = require('bluebird');

// require all tests
require('../lib/emitter-binder/EmitterBinder.spec.js');
require('../lib/emitter-binder/AggregateBinder.spec.js');
require('../lib/action-queue/ActionQueue.spec.js');
