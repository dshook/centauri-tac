process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

require('babel/register');
var GameServer = require('./game/GameServer.js');
new GameServer();

