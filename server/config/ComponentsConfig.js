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

    //Game stuff here for now since the gamelist needs them in the process of creating a game
    //length of the first turn
    this.turnLengthMs = process.env.TURN_LENGTH_MS || 40000;
    //time after the end of the normal turn length when you will have all the energy for that turn
    this.turnEndBufferLengthMs = process.env.TURN_END_BUFFER_LENGTH_MS || 5000;
    //amount of time each turn gets longer so turn number * increment = extra time at turn
    this.turnIncrementLengthMs = process.env.TURN_INCREMENT_LENGTH_MS || 5000;
  }
}
