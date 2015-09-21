// local overrides for env variables
require('dotenv').load();

// local lib hack
// https://gist.github.com/branneman/8048520
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

// TODO: change this based on debug/release mode
require('loglevel').setLevel(0);

// compiler hook
require('babel/register');

// use bluebird instead of native promise
global.Promise = require('bluebird');

// require all tests
require('../lib/emitter-binder/EmitterBinder.spec.js');
