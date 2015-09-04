// local lib hack
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

// TODO: change this based on debug/release mode
require('loglevel').setLevel(0);

// compiler hook
require('babel/register');

// boot
var GameServer = require('./game/GameServer.js');
new GameServer();

