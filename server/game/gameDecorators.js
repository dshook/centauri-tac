import {on} from 'emitter-binder';

// simple event binding with a player re-wire
const basicDecorator = (eventName) => on(
    eventName,
    null,
    (instance, fname, player) => instance[fname](player)
  );

export default {

  /**
   * Recv a command from a connected client thats been bound by a binder
   * injected with some useful SHIT
   */
  playerCommand: (name) => {
    return on(
        // command event
        'command',

        // matches our name
        ({command}) => command === name,

        // Change the way we call the handler
        function caller(instance, fName, {params}) {
          if (!this.player) {
            throw new Error('player isnt injected into binder!');
          }

          instance[fName](this.player, params);
        }
      );
  },

  /**
   * Player has been added to the game
   */
  playerJoined: () => basicDecorator('playerJoined'),

  /**
   * Player is leaving the game
   */
  playerParting: () => basicDecorator('playerParting'),

  /**
   * New client connection for a player
   */
  playerConnected: () => basicDecorator('playerConnected'),

  /**
   * Client connection for a player has been disonnected
   */
  playerDisconnected: () => basicDecorator('playerDisconnected'),

};
