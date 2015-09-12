import HttpService from './services/HttpService.js';
import PostgresService from './services/PostgresService.js';
import DatastoreService from './services/DatastoreService.js';
import HashService from './services/HashService.js';
import AuthTokenService from './services/AuthTokenService.js';
import HttpTransportService from './services/HttpTransportService.js';
import NetClientService from './services/NetClientService.js';

import MasterComponent from './components/MasterComponent.js';
import AuthComponent from './components/AuthComponent.js';
import PortalComponent from './components/PortalComponent.js';
import GamelistComponent from './components/GamelistComponent.js';
import GameComponent from './components/GameComponent.js';

// common things for all backend components
const common = [
  HttpService,
  HttpTransportService,
  AuthTokenService,
  NetClientService,
];

/**
 * Mapping of server components into various configuration and services
 */
export default {

  /**
   * Runs game instances
   */
  game: {
    TComponent: GameComponent,
    services: [
      ...common,
    ],
  },

  /**
   * Manages game servers
   */
  gamelist: {
    TComponent: GamelistComponent,
    services: [
      ...common,
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
