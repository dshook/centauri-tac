export default class ComponentType
{
  constructor()
  {
    this.id = null;
    this.name = null;
  }

  /**
   * From data
   */
  static fromJSON(data)
  {
    return data ? Object.assign(new ComponentType(), data) : null;
  }
}
