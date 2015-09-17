/**
 * Configuration dealing with the component manager etc
 */
export default class ComponentsConfig
{
  constructor()
  {

    /**
     * Master URL to connect to the game network
     */
    this.masterURL = process.env.MASTER_URL ||
      'http://localhost:10123/components/master';

    /**
     * Realm to run (non-master) components on
     */
    this.realm = process.env.REALM || null;

    /**
     * How frequently clients are pinged to update latency
     */
    this.serverPingInterval = 0 | process.env.PING_INTERVAL || 2000;
  }
}
