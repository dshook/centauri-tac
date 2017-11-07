import HttpService from './services/HttpService.js';
import PostgresService from './services/PostgresService.js';
import DatastoreService from './services/DatastoreService.js';
import HashService from './services/HashService.js';
import AuthTokenService from './services/AuthTokenService.js';
import HttpTransportService from './services/HttpTransportService.js';
import GameService from './services/GameService.js';
import LobbyService from './services/LobbyService.js';
import EventService from './services/EventService.js';

import AuthComponent from './components/AuthComponent.js';
import LogComponent from './components/LogComponent.js';
import SiteComponent from './components/SiteComponent.js';
import GameComponent from './components/GameComponent.js';
import LobbyComponent from './components/LobbyComponent.js';

// common things for all backend components
const common = [
  EventService,
  HttpService,
  HttpTransportService,
  AuthTokenService,
];

/**
 * Mapping of server components into various configuration and services
 */
export default {

  lobby: {
    TComponent: LobbyComponent,
    services: [
      ...common,
      LobbyService
    ],
  },

  /**
   * Runs game instances
   */
  game: {
    TComponent: GameComponent,
    services: [
      ...common,
      PostgresService,
      DatastoreService,
      GameService,
    ],
  },

  // /**
  //  * Manages game servers
  //  */
  // gamelist: {
  //   TComponent: GamelistComponent,
  //   services: [
  //     ...common,
  //     PostgresService,
  //     DatastoreService,
  //     GamelistService,
  //   ],
  // },

  //Client log view
  log: {
    TComponent: LogComponent,
    services: [
      ...common,
    ],
  },

  site: {
    TComponent: SiteComponent,
    services: [
      ...common,
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
