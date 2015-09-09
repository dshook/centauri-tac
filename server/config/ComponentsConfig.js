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
      'http://localhost:10123/components/master/rest';
  }
}
