import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import IntervalTimer from 'interval-timer';

/**
 * Stuff that happens on a regular basis for the game
 */
@loglevel
export default class GameEventService
{
  constructor(app, queue, game, players)
  {
    this.queue = queue;
    this.game = game;
    this.players = players;
    this.autoTurnInterval = new IntervalTimer('Auto Turn Interval', () => this.passTurn(), game.turnLengthMs);
    this.autoEnergyInterval = new IntervalTimer('Auto Energy Interval', () => this.giveEnergy(), game.turnLengthMs);

    this.registeredTimers = [this.autoTurnInterval, this.autoEnergyInterval];

    //app.registerInstance('autoTurnInterval', this.autoTurnInterval);
    app.registerInstance('gameEventService', this);
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

  resumeAll(){
    this.log.info('Resuming Game Event Timers');
    for(let timer of this.registeredTimers){
      timer.resume();
    }
  }

  passTurn(){
    this.log.info('Auto Passing turn');
    const action = new PassTurn();
    this.queue.push(action);
    this.queue.processUntilDone();
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
    let intervalLength = this.game.turnLengthMs / currentTurn;
    this.log.info('Setting up energy timer for turn %s. Interval %s', currentTurn, intervalLength);
    if(neededEnergy <= 0) return;

    this.autoEnergyInterval.stop();
    this.autoEnergyInterval.setMaxFires(neededEnergy);
    this.autoEnergyInterval.setInterval(intervalLength);
    this.autoEnergyInterval.start();
  }
}

