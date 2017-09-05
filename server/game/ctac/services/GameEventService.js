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
  constructor(container, queue, game, players, turnState)
  {
    this.queue = queue;
    this.game = game;
    this.players = players;
    this.turnState = turnState;

    this.autoTurnInterval = new IntervalTimer(
      'Auto Turn Interval',
      () => this.passTurn(),
      this.turnLength(game, turnState.currentTurn),
      1
    );
    this.autoEnergyInterval = new IntervalTimer(
      'Auto Energy Interval',
      () => this.giveEnergy(),
      this.turnLength(game, turnState.currentTurn) - game.turnEndBufferLengthMs
    );

    this.gameKickoff = new IntervalTimer(
      'Game Kickoff Interval',
      () => this.kickoff(),
      5000,
      1
    );

    this.registeredTimers = [this.autoTurnInterval, this.autoEnergyInterval];

    container.registerValue('gameEventService', this);
    this.log.info('Registered Game event service with turn length: %s End buffer: %s Increment: %s'
      , game.turnLengthMs, game.turnEndBufferLengthMs, game.turnIncrementLengthMs);
  }

  shutdown()
  {
    this.log.info('Killing Game Event Timers');
    for(let timer of this.registeredTimers){
      timer.stop();
    }
  }

  pauseAll(){
    this.log.info('Pausing Game Event Timers');
    for(let timer of this.registeredTimers){
      timer.pause();
    }
  }

  stopAll(){
    this.log.info('Stopping Game Event Timers');
    for(let timer of this.registeredTimers){
      timer.stop();
    }
  }

  resumeAll(){
    this.log.info('Resuming Game Event Timers');
    for(let timer of this.registeredTimers){
      timer.resume();
    }
  }

  //Start the game!
  kickoff(){
    this.log.info('Game kickoff!');
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
    this.autoTurnInterval.setInterval(this.turnLength(this.game, this.turnState.currentTurn));
    this.autoTurnInterval.start();
  }

  giveEnergy(){
    this.log.info('Giving out energy');
    for(let player of this.players){
      this.queue.push(new SetPlayerResource(player.id, 1));
    }

    this.queue.processUntilDone();
  }

  //setup the timer to distribute the energy over the turn
  startTurnEnergyTimer(currentTurn){
    let neededEnergy = currentTurn - 1; //one is auto given by the turn processor
    let intervalLength = (this.turnLength(this.game, currentTurn) - this.game.turnEndBufferLengthMs) / currentTurn;
    this.log.info('Setting up energy timer for turn %s. Interval %s', currentTurn, intervalLength);
    if(neededEnergy <= 0) return;

    this.autoEnergyInterval.stop();
    this.autoEnergyInterval.setMaxFires(neededEnergy);
    this.autoEnergyInterval.setInterval(intervalLength);
    this.autoEnergyInterval.start();
  }

  turnLength(game, currentTurn){
    return game.turnLengthMs + (currentTurn * game.turnIncrementLengthMs);
  }
}

