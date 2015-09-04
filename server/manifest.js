import HttpService from './services/HttpService.js';
import PostgresService from './services/PostgresService.js';
import DatastoreService from './services/DatastoreService.js';
import RESTService from './services/RESTService.js';
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
    services: [
      HttpService,
      PortalComponent,
    ],
  },

  /**
   * Central service registry
   */
  master: {
    services: [
      HttpService,
      PostgresService,
      DatastoreService,
      RESTService,
      MasterComponent,
    ],
  },

  /**
   * User login / session verification
   */
  auth: {
    services: [
      HttpService,
      PostgresService,
      DatastoreService,
      RESTService,
      HashService,
      AuthComponent,
    ],
  },

};
