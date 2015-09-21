import {on} from 'emitter-binder';

export default {

  /**
   * Recv a command from a connected client thats been bound by a binder
   * injected with some useful SHIT
   */
  command: (name) => {
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

};
