// Use bluebird over native promises
import Promise from 'bluebird';
global.Promise = Promise;

// Shim ES6 APIs
import 'babel/polyfill';

import WebPortal from './WebPortal.js';

// app is started from index.html
global.app = new WebPortal();

// expose debug for editing
global.debug = require('debug');
