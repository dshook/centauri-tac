import HttpService from './services/HttpService.js';
import PostgresService from './services/PostgresService.js';
import DatastoreService from './services/DatastoreService.js';
import HashService from './services/HashService.js';

import MasterComponent from './components/MasterComponent.js';
import AuthComponent from './components/AuthComponent.js';
import PortalComponent from './components/PortalComponent.js';

/**
 * Mapping of server components into various configuration and services
 */
export default {

  /**
   * View status of the servers
   */
  portal: {
    TComponent: PortalComponent,
    services: [
      HttpService,
    ],
  },

  /**
   * Central service registry
   */
  master: {
    TComponent: MasterComponent,
    services: [
      HttpService,
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
      HttpService,
      PostgresService,
      DatastoreService,
      HashService,
    ],
  },

};
