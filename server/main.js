import CentauriTacServer from './CentauriTacServer.js';

// Pull out what components we want to run from env vars
const components = (process.env.COMPONENTS || '')
  .split(',')
  .map(x => x.trim())
  .filter(x => x);


// boot
new CentauriTacServer(components).start();

