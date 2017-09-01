import loglevel from 'loglevel-decorator';

/**
 * A facade exposed to the game services that can be used to deal with stuff
 * with the host
 */
@loglevel
export default class HostManager
{
  constructor(container, binder, game, emitter)
  {
    this.container = container;
    this.binder = binder;
    this.game = game;
    this.emitter = emitter;

    //binder.addEmitter(this);

    this._controllers = new Map();
  }

  /**
   * Allow a running game to change its current state
   */
  async setGameState(stateId)
  {
    this.log.info('setting game state to %s', stateId);

    this.game.stateId = stateId;

    const gameId = this.game.id;
    await this.emitter.emit('game:updatestate', {gameId, stateId});
  }

  /**
   * Toggle allow join
   */
  async setAllowJoin(allowJoin = true)
  {
    this.log.info('setting game allow join to %s', allowJoin);

    this.game.allowJoin = allowJoin;

    const gameId = this.game.id;
    await this.emitter.emit('update:allowJoin', {gameId, allowJoin});
  }

  /**
   * Game won/lost
   */
  async completeGame(winningPlayerId)
  {
    this.log.info('Player %s won!', winningPlayerId);

    const gameId = this.game.id;
    await this.emitter.emit('game:completed', {gameId, winningPlayerId});
  }

  /**
   * Bootup, inject, and bind to the client emitters
   */
  async addController(T)
  {
    if (this._controllers.has(T)) {
      throw new Error('Already have controller instance for ' + T.name);
    }

    this.log.info('creating game controller %s', T.name);

    const controller = this.container.new(T);
    this.binder.bindInstance(controller);
    this._controllers.set(T, controller);

    if (typeof controller.start === 'function') {
      await controller.start();
    }

    this.log.info('started game controller %s', T.name);
  }

  /**
   * Unbind and kill
   */
  async removeController(T)
  {
    const controller = this._controllers.get(T);

    if (!controller) {
      throw new Error('controller %s was never built', T.name);
    }

    this.binder.unbindInstance(controller);
    this.log.info('removed controller %s', T.name);
  }
}
