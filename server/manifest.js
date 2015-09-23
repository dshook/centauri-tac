import HttpService from './services/HttpService.js';
import PostgresService from './services/PostgresService.js';
import DatastoreService from './services/DatastoreService.js';
import HashService from './services/HashService.js';
import AuthTokenService from './services/AuthTokenService.js';
import HttpTransportService from './services/HttpTransportService.js';
import NetClientService from './services/NetClientService.js';
import MessengerService from './services/MessengerService.js';
import GameService from './services/GameService.js';
import GamelistService from './services/GamelistService.js';

import MasterComponent from './components/MasterComponent.js';
import AuthComponent from './components/AuthComponent.js';
import PortalComponent from './components/PortalComponent.js';
import GamelistComponent from './components/GamelistComponent.js';
import GameComponent from './components/GameComponent.js';
import DispatchComponent from './components/DispatchComponent.js';
import MatchmakerComponent from './components/MatchmakerComponent.js';

// common things for all backend components
const common = [
  HttpService,
  HttpTransportService,
  AuthTokenService,
  NetClientService,
  MessengerService,
];

/**
 * Mapping of server components into various configuration and services
 */
export default {

  matchmaker: {
    TComponent: MatchmakerComponent,
    services: [
      ...common,
    ],
  },

  /**
   * Runs game instances
   */
  dispatch: {
    TComponent: DispatchComponent,
    services: [
      ...common,
    ],
  },

  /**
   * Runs game instances
   */
  game: {
    TComponent: GameComponent,
    services: [
      ...common,
      GameService,
    ],
  },

  /**
   * Manages game servers
   */
  gamelist: {
    TComponent: GamelistComponent,
    services: [
      ...common,
      PostgresService,
      DatastoreService,
      GamelistService,
    ],
  },

  /**
   * View status of the servers
   */
  portal: {
    TComponent: PortalComponent,
    services: [
      ...common,
    ],
  },

  /**
   * Central service registry
   */
  master: {
    TComponent: MasterComponent,
    services: [
      ...common,
      PostgresService,
      DatastoreService,
    ],
  },

  /**
   * User login / session verification
   */
  auth: {
    TComponent: AuthComponent,
    services: [
      ...common,
      PostgresService,
      DatastoreService,
      HashService,
    ],
  },

};
