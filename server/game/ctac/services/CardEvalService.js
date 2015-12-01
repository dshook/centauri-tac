import loglevel from 'loglevel-decorator';
import DrawCard from '../actions/DrawCard.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
class CardEvaluator{
  constructor(queue, turnState){
    this.queue = queue;
    this.turnState = turnState;
  }

  evaluateAction(event, card){
    let evalActions = card.events[event];

    for (let i = 0; i < evalActions.length; i++) {
      let action = evalActions[i];
      let times = 1;
      if(action.times){
        //eventually eval number objects
        times = Number.parseInt(action.times);
      }

      this.log.info('Evaluating action %s for card %s %s %s'
        , action.action, card.name, times, times > 1 ? 'times' : 'time');

      for (var t = 0; t < times; t++) {
        switch(action.action){
          case 'DrawCard':
            this.queue.push(new DrawCard(this.turnState.currentPlayerId));
            break;
          case 'SetAttribute':
            break;
        }
      }
    };
  }
}

@loglevel
export default class CardEvalService
{
  constructor(app, queue, turnState)
  {
    app.registerInstance('cardEvaluator', new CardEvaluator(queue, turnState));
  }
}
