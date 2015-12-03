import loglevel from 'loglevel-decorator';
import DrawCard from '../actions/DrawCard.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
export default class CardEvaluator{
  constructor(queue, turnState, selector){
    this.queue = queue;
    this.turnState = turnState;
    this.selector = selector;
  }

  evaluateAction(event, card){
    if(!card.events || !card.events[event]) return;
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
            let playerSelector = this.selector.selectPlayer(action.args[0]);
            this.queue.push(new DrawCard(playerSelector));
            break;
          case 'SetAttribute':
            break;
        }
      }
    };
  }
}