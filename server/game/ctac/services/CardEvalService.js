import loglevel from 'loglevel-decorator';
import Selector from '../cardlang/Selector.js';
import CardEvaluator from '../cardlang/CardEvaluator.js';

@loglevel
export default class CardEvalService
{
  constructor(app, queue, turnState, players)
  {
    let selector = new Selector(turnState, players);
    app.registerInstance('selector', selector);
    app.registerInstance('cardEvaluator', new CardEvaluator(queue, turnState, selector));
  }
}
