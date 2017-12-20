import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import Kickoff from '../actions/Kickoff.js';
import IntervalTimer from 'interval-timer';

/**
 * Stuff that happens on a regular basis for the game
 */
@loglevel
export default class GameEventService
{
  constructor(container, queue, game, players, turnState, playerResourceState)
  {
    this.queue = queue;
    this.game = game;
    this.players = players;
    this.turnState = turnState;
    this.playerResourceState = playerResourceState;

    this.autoTurnInterval = new IntervalTimer(
      'Auto Turn Interval Game ' + this.game.id,
      () => this.passTurn(),
      this.turnLength(game, turnState.currentTurn) + game.turnEndBufferLengthMs,
      1
    );
    this.autoEnergyInterval = new IntervalTimer(
      'Auto Energy Interval Game ' + this.game.id,
      () => this.giveEnergy(),
      this.turnLength(game, turnState.currentTurn)
    );

    this.gameKickoff = new IntervalTimer(
      'Game Kickoff Interval',
      () => this.kickoff(),
      2000,
      1
    );

    this.registeredTimers = [this.autoTurnInterval, this.autoEnergyInterval];

    container.registerValue('gameEventService', this);
    this.log.info('Registered Game event service with turn length: %s End buffer: %s Increment: %s'
      , game.turnLengthMs, game.turnEndBufferLengthMs, game.turnIncrementLengthMs);
  }

  shutdown()
  {
    this.log.info('Killing Game Event Timers for Game ' + this.game.id);
    for(let timer of this.registeredTimers){
      timer.stop();
    }
  }

  pauseAll(){
    this.log.info('Pausing Game Event Timers for Game ' + this.game.id);
    for(let timer of this.registeredTimers){
      timer.pause();
    }
  }

  stopAll(){
    this.log.info('Stopping Game Event Timers for Game ' + this.game.id);
    for(let timer of this.registeredTimers){
      timer.stop();
    }
  }

  resumeAll(){
    this.log.info('Resuming Game Event Timers for Game ' + this.game.id);
    for(let timer of this.registeredTimers){
      timer.resume();
    }
  }

  //Start the game!
  kickoff(){
    this.log.info('Game kickoff!');
    this.game.allowCommands = true;
    this.autoTurnInterval.start();

    this.queue.push(new Kickoff('Go!'));
    this.queue.push(new PassTurn());
    this.queue.processUntilDone();
  }

  passTurn(){
    this.log.info('Auto Passing turn for %s', this.turnState.currentTurn);
    const action = new PassTurn();
    this.queue.push(action);
    this.queue.processUntilDone();

    //now reset the turn interval for the next turn
    this.autoTurnInterval.stop();
    this.autoTurnInterval.changeInterval(this.turnLength(this.game, this.turnState.currentTurn) + this.game.turnEndBufferLengthMs);
    this.autoTurnInterval.start();
  }

  giveEnergy(){
    this.log.info('Giving out energy Game ' + this.game.id);
    for(let player of this.players){
      let needed = this.playerResourceState.getNeeded(player.id);
      this.log.info('Player %s needs %s energy', player.id, needed);
      if(needed > 0){
        this.playerResourceState.changeNeeded(player.id, -1);
        this.queue.push(new SetPlayerResource(player.id, 1));
      }
    }

    this.queue.processUntilDone();
  }

  //setup the timer to distribute the energy over the turn
  startTurnEnergyTimer(currentTurn){
    let maxNeededEnergy = currentTurn - 1; //one energy is auto given by the turn processor, this is the max we'll have to give out per turn
    if(maxNeededEnergy <= 0) return;

    let intervalLength = this.turnLength(this.game, maxNeededEnergy) / currentTurn;
    this.log.info('Setting up energy timer for turn %s. Interval %s Game %s', currentTurn, intervalLength, this.game.id);

    this.autoEnergyInterval.stop();
    this.autoEnergyInterval.setMaxFires(maxNeededEnergy);
    this.autoEnergyInterval.changeInterval(intervalLength);
    this.autoEnergyInterval.start();
  }

  turnLength(game, currentTurn){
    return game.turnLengthMs + (Math.min(currentTurn, 10) * game.turnIncrementLengthMs);
  }
}

