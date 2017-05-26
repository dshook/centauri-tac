import Application from 'billy';
import ActionQueue from 'action-queue';
import Player from 'models/Player';
import Game from 'models/Game';

import GameDataService from '../game/ctac/services/GameDataService.js';
import ProcessorsService from '../game/ctac/services/ProcessorsService.js';
import CardService from '../game/ctac/services/CardService.js';
import MapService from '../game/ctac/services/MapService.js';
import GameEventService from '../game/ctac/services/GameEventService.js';
import ProcessorServiceTests from './ProcessorServiceTests.spec.js';


//wire up a new app with real dependencies plus the tests as a service (TAAS)
var app = new Application();

const queue = new ActionQueue(T => app.container.new(T));
app.container.registerValue('queue', queue);

var game = new Game();
game.turnLengthMs = 10;
app.container.registerValue('game', game);

let players = [];
players.push(Player.fromTest(1));
players.push(Player.fromTest(2));
app.container.registerValue('players', players);
app.container.registerValue('gameConfig', {cardSets: ['test']});
app.service(MapService);
app.service(GameDataService);
app.service(CardService);
app.service(GameEventService);
app.service(ProcessorsService);

app.service(ProcessorServiceTests);

app.start();

