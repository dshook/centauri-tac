// local overrides for env variables
require('dotenv').load();

// local lib hack
// https://gist.github.com/branneman/8048520
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

// compiler hook
require('babel-register')({
  plugins: [
    "transform-es2015-modules-commonjs",
    "babel-plugin-transform-object-rest-spread",
    "babel-plugin-transform-decorators-legacy"
  ]
});

// use bluebird instead of native promise
global.Promise = require('bluebird');

console.log('Preprossesing complete, loading main');
// boot
require('./main.js');

