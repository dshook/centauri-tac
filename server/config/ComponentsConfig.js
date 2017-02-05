/**
 * Configuration dealing with the component manager etc
 */
export default class ComponentsConfig
{
  constructor()
  {
    /**
     * How frequently clients are pinged to update latency
     */
    this.serverPingInterval = 0 | process.env.PING_INTERVAL || 2000;
  }
}
