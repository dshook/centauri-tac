/**
 * Configuration dealing with the component manager etc
 */
export default class GameConfig
{
  constructor()
  {
    /**
     * How frequently clients are pinged to update latency
     */
    this.dev = process.env.DEV || false;
  }
}
