// local overrides for env variables
require('dotenv').load();

// local lib hack
// https://gist.github.com/branneman/8048520
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

// compiler hook
require('babel-polyfill');
require('babel-register')({
  plugins: ["babel-plugin-transform-decorators-legacy"],
  presets: [
    "es2015-node",
    "babel-preset-stage-2",
  ]
});

// use bluebird instead of native promise
global.Promise = require('bluebird');

// boot
require('./main.js');

