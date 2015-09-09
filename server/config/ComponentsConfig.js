/**
 * Configuration dealing with the component manager etc
 */
export default class ComponentsConfig
{
  constructor()
  {
    // Pull out what components we want to run from env vars
    const components = (process.env.COMPONENTS || '')
      .split(',')
      .map(x => x.trim())
      .filter(x => x);

    /**
     * String names of all components we want to run
     */
    this.components = components;
  }
}
