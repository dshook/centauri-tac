import test from 'tape';
import Application from 'billy';
import ActionQueue from 'action-queue';
import Player from 'models/Player';

import GameDataService from '../game/ctac/services/GameDataService.js';
import ProcessorsService from '../game/ctac/services/ProcessorsService.js';
import CardService from '../game/ctac/services/CardService.js';
import MapService from '../game/ctac/services/MapService.js';
import ProcessorServiceTests from './ProcessorServiceTests.spec.js';


//wire up a new app with real dependencies plus the tests as a service (TAAS)
var app = new Application();

const queue = new ActionQueue(T => app.make(T));
app.registerInstance('queue', queue);

let players = [];
players.push(Player.fromTest(1));
players.push(Player.fromTest(2));
app.registerInstance('players', players);
app.service(MapService);
app.service(GameDataService);
app.service(CardService);
app.service(ProcessorsService);

app.service(ProcessorServiceTests);

app.start();
