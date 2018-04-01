import _ from 'lodash';
import loglevel from 'loglevel-decorator';
import uniqueId from 'unique-id';
import EvalError from './EvalError.js';
import Statuses from '../models/Statuses.js';
import PieceAction from '../actions/PieceAction.js';
import DrawCard from '../actions/DrawCard.js';
import DiscardCard from '../actions/DiscardCard.js';
import GiveCard from '../actions/GiveCard.js';
import ShuffleToDeck from '../actions/ShuffleToDeck.js';
import Message from '../actions/Message.js';
import ActionTimer from '../actions/ActionTimer.js';
import CharmPiece from '../actions/CharmPiece.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceDestroyed from '../actions/PieceDestroyed.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import PieceAttributeChange from '../actions/PieceAttributeChange.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import PieceBuff from '../actions/PieceBuff.js';
import PieceAura from '../actions/PieceAura.js';
import CardAura from '../actions/CardAura.js';
import MovePiece from '../actions/MovePiece.js';
import TransformPiece from '../actions/TransformPiece.js';
import ChargeUp from '../actions/ChargeUp.js';
import PieceArmorChange from '../actions/PieceArmorChange.js';
import AttachCode from '../actions/AttachCode.js';
import UnsummonPiece from '../actions/UnsummonPiece.js';
import Choose from '../actions/Choose.js';
import TilesCleared from '../actions/TilesCleared.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
export default class CardEvaluator{
  constructor(queue, selector, pieceState, mapState){
    this.queue = queue;
    this.selector = selector;
    this.pieceState = pieceState;
    this.mapState = mapState;
    this.startTurnTimers = [];
    this.endTurnTimers = [];

    this.eventDefaultSelectors = {
      playMinion: {left: 'SELF'},
      death: {left: 'SELF'},
      damaged: {left: 'SELF'},
      healed: {left: 'SELF'},
      attacks: {left: 'SELF'},
      ability: {left: 'SELF'},
      cardDrawn: {left: 'PLAYER'},
      playSpell: {left: 'PLAYER'},
      chargeChange: {left: 'PLAYER'}
    };
    this.targetableActions = [
      'Hit',
      'Heal',
      'SetAttribute',
      'Buff',
      'GiveStatus',
      'RemoveStatus',
      'Charm',
      'Destroy',
      'Move',
      'Transform',
      'GiveCard',
      'ShuffleToDeck',
      'Spawn',
      'Unsummon',
      'startTurnTimer',
      'endTurnTimer'
    ];
    this.targetableEvents = ['playMinion', 'playSpell'];
  }

  //evaluate an event that directly relates to a piece, i.e. the piece dies
  evaluatePieceEvent(event, activatingPiece, pieceEventParams){
    this.log.info('Eval piece event %s activating piece: %j', event, activatingPiece);
    let evalActions = [];

    let {targetPieceId, position, pivotPosition, chooseCardTemplateId} = pieceEventParams || {};

    if(event === 'death'){
      this.cleanupTimers(activatingPiece);
    }

    //in the pieces case, if this is a spawn piece event the evaluator has the chance to return false
    //and scrub the spawn of the piece, so the activating piece isn't in the piece state yet.
    //However, include it in the loop so its events will be evaluated
    let pieces = new Set([...this.pieceState.pieces, activatingPiece]);

    //first look through all the pieces on the board to see if any have actions on this event
    for(let piece of pieces){
      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        let pieceSelectorParams = {selfPiece: piece, activatingPiece, controllingPlayerId: piece.playerId};

        if(cardEvent.condition){
          let compareResult = this.selector.compareExpression(cardEvent.condition, this.pieceState, pieceSelectorParams, this.selector.selectPieces);
          if(compareResult.length === 0){
            continue;
          }
        }

        //see if the selector matches up for this piece
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find all pieces that match the selector given the context of the piece that the event is for
        let piecesSelected = this.selector.selectPieces(eventSelector, pieceSelectorParams);

        let selectorMatched = piecesSelected.indexOf(activatingPiece) > -1;

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(selectorMatched){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: activatingPiece.playerId,
              position,
              pivotPosition,
              targetPieceId,
              action: cardEventAction,
              chooseCardTemplateId
            });
          }
        }
      }
    }

    return this.processActions(evalActions, activatingPiece, activatingPiece.playerId);

  }

  //evaluate an event that doesn't correspond to a piece directly
  evaluatePlayerEvent(event, playerId){
    this.log.info('Eval player event %s player: %s', event, playerId);

    let evalActions = [];

    //look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        if(cardEvent.condition){
          let pieceSelectorParams = {selfPiece: piece, activatingPiece: piece, controllingPlayerId: piece.playerId};
          let compareResult = this.selector.compareExpression(cardEvent.condition, this.pieceState, pieceSelectorParams, this.selector.selectPieces);
          if(compareResult.length === 0){
            continue;
          }
        }

        //see if the selector matches up for this piece
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(playerSelected === playerId){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: piece.playerId,
              activatingPiece: piece,
              action: cardEventAction
            });
          }
        }
      }
    }

    return this.processActions(evalActions, null, playerId);
  }

  //when a spell is played
  evaluateSpellEvent(event,
    {spellCard, playerId, targetPieceId, position, pivotPosition, chooseCardTemplateId, activatingPiece, selfPiece}
  ){
    this.log.info('Eval spell event %s with spell %s player: %s', event, spellCard.name, playerId);

    let evalActions = [];

    //always add the actions from this spell card
    let spellActions = spellCard.events.filter(e => e.event === event);
    if(spellActions.length > 0){
      for(let cardEventAction of spellActions[0].actions){
        evalActions.push({
          piece: selfPiece || null,
          card: spellCard,
          playerId,
          position,
          pivotPosition,
          targetPieceId,
          action: cardEventAction,
          chooseCardTemplateId
        });
      }
    }

    //then look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        if(cardEvent.condition){
          let pieceSelectorParams = {selfPiece: piece, activatingPiece: piece, controllingPlayerId: piece.playerId};
          let compareResult = this.selector.compareExpression(cardEvent.condition, this.pieceState, pieceSelectorParams, this.selector.selectPieces);
          if(compareResult.length === 0){
            continue;
          }
        }

        //see if the selector matches up for this card
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(playerSelected === playerId){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: piece.playerId,
              action: cardEventAction,
              position,
              pivotPosition,
              targetPieceId,
              chooseCardTemplateId
            });
          }
        }
      }
    }

    return this.processActions(evalActions, activatingPiece || null, playerId);
  }

  //Process all actions that have been selected in the evaluation phase into actual queue actions
  // evalActions -> array of actions to be eval'd, with playerId's of the controlling player (current turn player)
  // activatingPiece -> optional piece that will be used for SELF selections
  // activatingPlayerId -> id of the player that fired all this off (generally the current turn player)
  processActions(evalActions, activatingPiece, activatingPlayerId){
    //actions to be added to the real queue
    let queue = [];
    let queueIndex = 0;

    try{
      for(let pieceAction of evalActions){
        let action = pieceAction.action;
        let piece = pieceAction.piece;
        let card = pieceAction.card;
        let pieceSelectorParams = {
          selfPiece: piece,
          controllingPlayerId: pieceAction.playerId,
          activatingPiece: activatingPiece || pieceAction.activatingPiece || null,
          targetPieceId: pieceAction.targetPieceId,
          savedPieces: pieceAction.savedPieces,
          position: pieceAction.position,
          pivotPosition: pieceAction.pivotPosition,
          isSpell: !pieceAction.piece,
          isTimer: pieceAction.isTimer || false
        };

        //check to see if we should even do this action
        if(action.condition){
          let compareResult = this.selector.compareExpression(action.condition, this.pieceState, pieceSelectorParams, this.selector.selectPieces)
          if(compareResult.length === 0){
            continue;
          }
        }

        let times = 1;
        if(action.times){
          times = this.selector.eventualNumber(action.times, pieceSelectorParams);
        }
        let totalSpellDamage = this.pieceState.totalSpellDamage(pieceSelectorParams.controllingPlayerId);
        //for multi cast spells use spell damage as additional times instead of additional damage
        if(pieceSelectorParams.isSpell && times > 1 && totalSpellDamage > 0){
          times += totalSpellDamage;
        }

        let actionTriggerer = piece ? `piece ${piece.name}` : `spell ${pieceAction.card.name}`;
        this.log.info('Evaluating action %s for %s %s %s'
          , action.action, actionTriggerer, times, times > 1 ? 'times' : 'time');

        //despite not using the selected pieces for each action we need to run them to catch any exceptions now
        //for things like needing a target without one provided
        for (var time = 0; time < times; time++) {
          switch(action.action){
            //Charm(pieceSelector)
            case 'Charm':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, CharmPiece));
              break;
            }
            //Destroy(pieceSelector)
            case 'Destroy':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceDestroyed));
              break;
            }
            //Discard(playerSelector)
            //Only chooses a random card for now
            case 'Discard':
            {
              let playerToDiscard = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new DiscardCard(playerToDiscard));
              break;
            }
            //DrawCard(playerSelector)
            case 'DrawCard':
            {
              let playerToDraw = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new DrawCard(playerToDraw));
              break;
            }
            //ChangeEnergy(playerSelector, amount, permanent, full)
            //Permanent meaning for every turn going forward, non permanent is just this turn
            //Full meaning whether or not to fill the adjusted amount or if they come empty
            case 'ChangeEnergy':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new SetPlayerResource(
                playerSelector,
                this.selector.eventualNumber(action.args[1], pieceSelectorParams),
                action.args[2],
                action.args[3]
              ));
              break;
            }
            //ChargeUp(playerSelector, amount)
            //Change a players charge by amount
            case 'ChargeUp':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new ChargeUp(
                playerSelector,
                this.selector.eventualNumber(action.args[1], pieceSelectorParams)
              ));
              break;
            }
            //Hit(pieceSelector, damageAmount)
            case 'Hit':
            {
              let spellDamageBonus = 0;
              let pieceSelector = action.args[0];
              let hitDamage = action.args[1];
              //apply spell damage only for one time cast spells
              if(pieceSelectorParams.isSpell && times === 1){
                spellDamageBonus = totalSpellDamage;
              }

              this.selector.selectPieces(pieceSelector, pieceSelectorParams);
              queue.push(new PieceAction(pieceSelector, pieceSelectorParams, PieceHealthChange, {changeENumber: hitDamage, pieceSelectorParams, isHit: true, spellDamageBonus}));

              //Find any tiles that might be made passable by an AOE
              let clearedTiles = this.selector.selectClearableTiles(pieceSelector, pieceSelectorParams);
              if(clearedTiles.length){
                queue.push(new TilesCleared(clearedTiles.map(t => t.position)));
              }
              break;
            }
            //Heal(pieceSelector, healAmount)
            case 'Heal':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let spellDamageBonus = 0;
              if(pieceSelectorParams.isSpell && times === 1){
                spellDamageBonus = totalSpellDamage;
              }
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceHealthChange, {changeENumber: action.args[1], pieceSelectorParams, isHit: false, spellDamageBonus}));
              break;
            }
            //SetAttribute(pieceSelector, attribute, value)
            case 'SetAttribute':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let attributeParams = {};
              //set up the appropriate attribute change from args, i.e. attack = 1
              attributeParams[action.args[1]] = this.selector.eventualNumber(action.args[2], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceAttributeChange, attributeParams));

              break;
            }
            //Buff(pieceSelector, buffName, [optional condition expression], attribute(amount), ...moreAttributes)
            //Note that the attributes can also be Statuses with a true or false to add (or remove) the status
            case 'Buff':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);

              let buffName = action.args[1];

              let buffParams = {}
              let attributeIndex = 2;
              let condition = null;
              buffParams.addStatus = buffParams.removeStatus = 0;
              if(action.args[2] && action.args[2].compareExpression){
                attributeIndex = 3;
                condition = action.args[2];
              }
              let buffAttributes = action.args.slice(attributeIndex);
              for(let buffAttribute of buffAttributes){
                if(buffAttribute.attribute){
                  buffParams[buffAttribute.attribute] = this.selector.eventualNumber(buffAttribute.amount, pieceSelectorParams);
                }else if(buffAttribute.status){
                  if(buffAttribute.adding){
                    buffParams.addStatus |= Statuses[buffAttribute.status];
                  }else{
                    buffParams.removeStatus |= Statuses[buffAttribute.status];
                  }
                }else{
                  this.log.error('Unrecognized buff attribute %j', buffAttribute);
                }
              }
              buffParams.buffId = uniqueId();
              buffParams.condition = condition;
              buffParams.name = buffName;
              buffParams.removed = false;
              buffParams.buffAttributes = buffAttributes;

              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceBuff, buffParams));

              break;
            }
            //Aura(auraPieceSelector, pieceSelector, auraName, attribute(amount), ...moreAttributes)
            //to clarify further: first piece selector is who to attach the aura to,
            //second one is who is affected by the aura
            case 'Aura':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let auraParams = {
                pieceSelector: action.args[1],
                name: action.args[2],
              };
              let auraAttributes = action.args.slice(3);
              for(let auraAttribute of auraAttributes){
                //specifically don't use eventual number here because it will be evaluated in the aura update
                auraParams[auraAttribute.attribute] = auraAttribute.amount;
              }

              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceAura, auraParams));
              break;
            }
            //CardAura(auraPieceSelector, cardSelector, auraName, attribute(amount), ...moreAttributes)
            //to clarify further: first piece selector is who to attach the aura to,
            //second one is which cards are affected by the aura.
            case 'CardAura':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);

              let auraParams = {
                cardSelector: action.args[1],
                name: action.args[2],
              };
              let auraAttributes = action.args.slice(3);
              for(let auraAttribute of auraAttributes){
                //specifically don't use eventual number here because it will be evaluated in the aura update
                auraParams[auraAttribute.attribute] = auraAttribute.amount;
              }
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, CardAura, auraParams));
              break;
            }
            //RemoveBuff(pieceSelector, buffName)
            case 'RemoveBuff':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let buffParams = {name: action.args[1], removed: true};

              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceBuff, buffParams));
              break;
            }
            //Spawn(pieceId, kingsRadiusToSpawnIn)
            //Spawn a unit based on where the activating piece is located.
            //So kingsRadius 0 = right where the piece was (after it died presumably).
            //If it's 1, pick a random position from any of the surrounding tiles
            //This requires an activating piece at the moment to act as the center position
            //ALTERNATE:
            //Spawn(pieceId, areaSelector)
            //This means to spawn a piece in each of the tiles returned by the area selector
            case 'Spawn':
            {
              let cardTemplateId = this.selector.eventualNumber(action.args[0], pieceSelectorParams);
              let placeArg = action.args[1];
              if(placeArg.left && placeArg.left.area){
                //if it's an area, eval the tiles now rather than at exec time since they shouldn't be changing
                //exec time will determine if a piece actually spawns or not in the case of an occupied tile
                let areaToSpawn = this.selector.selectArea(placeArg.left, pieceSelectorParams);
                if(areaToSpawn && areaToSpawn.areaTiles && areaToSpawn.areaTiles.length > 0){
                  for(let tile of areaToSpawn.areaTiles ){
                    queue.push(
                      new SpawnPiece({
                        playerId: pieceAction.playerId,
                        cardTemplateId,
                        position: tile,
                      })
                    );
                  }
                }
              }else if(piece){
                queue.push(
                  new SpawnPiece({
                    playerId: piece.playerId,
                    cardTemplateId,
                    position: piece.position,
                    spawnKingRadius: placeArg
                  })
                );
              }

              break;
            }

            //GiveStatus(pieceSelector, Status)
            case 'GiveStatus':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceStatusChange, {add: Statuses[action.args[1]]}));
              break;
            }
            //RemoveStatus(pieceSelector, Status)
            case 'RemoveStatus':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceStatusChange, {remove: Statuses[action.args[1]]}));
              break;
            }
            //startTurnTimer(initialTurnDelay, repeatingInterval, ...Actions)
            //turns is a counter that gets decremented each start of the turn and fires when 0
            //so a turns value of 1 will activate on the start of the following (probably enemy) turn
            //repeating interval, if truth, determines what the turns counter is reset to every time
            //the timer activates
            case 'startTurnTimer':
            {
              let timerActions = action.args.slice(2);

              queue.push(new ActionTimer({
                isStartTimer: true,
                pieceSelectorParams,
                piece,
                card,
                playerId: pieceAction.playerId,
                interval: action.args[1],
                timer: action.args[0],
                timerActions
              }));

              break;
            }
            //endTurnTimer(initialTurnDelay, repeatingInterval, ...Actions)
            //see startTurnTimer above ^
            case 'endTurnTimer':
            {
              let timerActions = action.args.slice(2);

              queue.push(new ActionTimer({
                isStartTimer: false,
                pieceSelectorParams,
                piece,
                card,
                playerId: pieceAction.playerId,
                interval: action.args[1],
                timer: action.args[0],
                timerActions
              }));
              break;
            }
            //Move(pieceSelector, tile in areaSelector, isTeleport, ignoreCollisionCheck)
            case 'Move':
            {
              let lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              if(lastSelected.length > 1){
                this.log.warn('Move selected more than one piece to move %j', lastSelected);
                break;
              }
              let moveTo = this.selector.selectArea(action.args[1], pieceSelectorParams);

              if(!moveTo || !moveTo.resolvedPosition){
                this.log.warn("Move didn't resolve to a position %j", moveTo);
                break;
              }

              //preemptively check for colliding piece so the action can be scrubbed early
              let ignoreCollisionCheck = action.args[3] || false;
              if(!ignoreCollisionCheck && this.pieceState.pieceAt(moveTo.resolvedPosition)){
                throw new EvalError("You can't move on top of another piece!");
              }

              let movingPiece = lastSelected[0];
              let destinationTile = this.mapState.getTile(moveTo.resolvedPosition)

              //other checks for scrubbing early. Really need to dedupe w/move piece processor
              if(!destinationTile || destinationTile.unpassable){
                this.log.warn('Cannot move piece %s to unpassable tile %s'
                  , movingPiece.id, destinationTile ? destinationTile.position : 'Missing dest: ' + moveTo.resolvedPosition);
                throw new EvalError("That tile doesn't look safe!");
                return;
              }

              if(lastSelected && lastSelected.length === 1){
                queue.push(new MovePiece({
                  pieceId: movingPiece.id,
                  to: moveTo.resolvedPosition,
                  isJump: true,
                  isTeleport: action.args[2],
                  ignoreCollisionCheck
                }));
              }
              break;
            }
            //Transform(pieceSelector, cardTemplateId, targetPieceSelector)
            //Transforms the piece selector pieces into either the cardTemplateId if it's a valid minion ID,
            //otherwise, use the targetPieceSelector to find the first minion to copy props from
            case 'Transform':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let cardTemplateId = action.args[1];
              let targetPieceSelector = action.args[2];

              //TODO: should be moved to processor to select
              let transformPieceId = null;
              if(targetPieceSelector){
                let transformPieces = this.selector.selectPieces(targetPieceSelector, pieceSelectorParams);
                if(transformPieces && transformPieces.length > 0){
                  transformPieceId = transformPieces[0].id;
                }
              }

              queue.push(new PieceAction(action.args[0], pieceSelectorParams, TransformPiece, {cardTemplateId, transformPieceId}));
              break;
            }
            //GiveCard(PlayerSelector, cardTemplateId)
            case 'GiveCard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              let cardId = this.selector.eventualNumber(action.args[1], pieceSelectorParams);
              queue.push(new GiveCard(playerSelector, cardId));
              break;
            }
            //ShuffleToDeck(PlayerSelector, cardTemplateId)
            case 'ShuffleToDeck':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              let cardId = this.selector.eventualNumber(action.args[1], pieceSelectorParams);
              queue.push(new ShuffleToDeck(playerSelector, cardId));
              break;
            }
            //GiveArmor(pieceSelector, amount)
            case 'GiveArmor':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              let change = this.selector.eventualNumber(action.args[1], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, PieceArmorChange, {change}));
              break;
            }
            //AttachCode(pieceSelector, eventList)
            //Second arg is like a top level event list node that will get merged into the selected pieces events
            case 'AttachCode':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, AttachCode, {eventList: action.args[1]}));
              break;
            }
            //Unsummon(pieceSelector)
            case 'Unsummon':
            {
              this.selector.selectPieces(action.args[0], pieceSelectorParams);
              queue.push(new PieceAction(action.args[0], pieceSelectorParams, UnsummonPiece));
              break;
            }
            //Choose(cardTemplateId1, cardTemplateId2)
            case 'Choose':
            {
              queue.push(new Choose(
                action.args[0],
                action.args[1],
                pieceAction.chooseCardTemplateId,
                {
                  playerId: pieceAction.playerId,
                  activatingPiece: pieceSelectorParams.activatingPiece,
                  selfPiece: pieceSelectorParams.selfPiece,
                  targetPieceId: pieceSelectorParams.targetPieceId,
                  position: pieceSelectorParams.position,
                  pivotPosition: pieceSelectorParams.pivotPosition
                }
              ));
              break;
            }
          }
        }
        //blast through all the items we need to add to the queue and associate the activating piece with them
        //mainly for the client.  Done each loop so that the activating piece can be included in the action we're on
        //the queue index is persisted across the piece action loop so we only go over each item once with the appropriate
        //pieceSelectorParams
        for(queueIndex; queueIndex < queue.length; queueIndex++){
          let action = queue[queueIndex];
          action.activatingPieceId = pieceSelectorParams.activatingPiece ? pieceSelectorParams.activatingPiece.id : null;
        }
      }
    }catch(e){
      if(e instanceof EvalError){
        this.log.info('EvalError player %s : %s', activatingPlayerId, e.message);
        if(activatingPlayerId){
          this.queue.push(new Message(e.message, activatingPlayerId));
        }
        return false;
      }
      throw e;
    }

    for(let action of queue){
      this.queue.push(action);
    }
    return true;
  }

  //When a piece is transformed there's some state maintenance to do
  updateTransformedPiece(piece, copiedPiece){
    //First, cleanup any timers associated with the old piece
    this.cleanupTimers(piece);

    //if we're copying an active piece, duplicate any existing timers but maintain the new piece Id
    if(copiedPiece){
      let copiedEndTurnTimers = this.endTurnTimers.filter(t => t.piece && t.piece.id === copiedPiece.id);
      let copiedStartTurnTimers = this.startTurnTimers.filter(t => t.piece && t.piece.id === copiedPiece.id);

      for(let copiedEndTurnTimer of copiedEndTurnTimers){
        let clonedTimer = _.cloneDeep(copiedEndTurnTimer);
        clonedTimer.piece = piece;
        clonedTimer.playerId = piece.playerId; //Not sure if this is really needed or might cause an issue
        this.endTurnTimers.push(clonedTimer);
      }

      for(let copiedStartTurnTimer of copiedStartTurnTimers){
        let clonedTimer = _.cloneDeep(copiedStartTurnTimer);
        clonedTimer.piece = piece;
        clonedTimer.playerId = piece.playerId; //see above
        this.startTurnTimers.push(clonedTimer);
      }
    }
  }

  evaluateTurnEvent(isStart){
    let evalActions = this.updateTimers(isStart);
    this.log.info('Eval %s %s turn events', evalActions.length, isStart ? 'start' : 'end');

    return this.processActions(evalActions, null, null);
  }

  //Tick down all the timers in the array. When the metaphorical bomb ticks down remove it from
  //the saved timers and return its actions to be evaluated
  //have to use a bool instead of passing the array in so we can reassign it to remove items :(
  updateTimers(isStartTimers){
    let timerArray = isStartTimers ? this.startTurnTimers : this.endTurnTimers;
    if(timerArray.length === 0) return [];

    for(let timer of timerArray){
      timer.timer--;
    }
    let activatedTimers = timerArray.filter(t => t.timer === 0);

    //reactivate those that are repeating
    for(let activated of activatedTimers){
      if(activated.interval){
        activated.timer = activated.interval;
      }
    }

    //refilter for expired
    let expiredTimers = timerArray.filter(t => t.timer === 0);

    if(isStartTimers){
      this.startTurnTimers = _.without(timerArray, ...expiredTimers);
    }else{
      this.endTurnTimers = _.without(timerArray, ...expiredTimers);
    }

    let activatedEvents = [];

    for(let activatedTimer of activatedTimers){
      activatedEvents.push({
        piece: activatedTimer.piece,
        activatingPiece: activatedTimer.piece,
        card: activatedTimer.card,
        savedPieces: activatedTimer.saved,
        playerId: activatedTimer.playerId,
        action: activatedTimer.timerAction,
        isTimer: true
      });
    }

    return activatedEvents;
  }

  //Whenever a piece dies that had a repeating turn timer on it we need to remove it
  cleanupTimers(killedPiece){
    let abandonedFilter = (t) => (t.interval && t.piece && t.piece.id === killedPiece.id);

    let abandonedStartTimers = this.startTurnTimers.filter(abandonedFilter);
    if(abandonedStartTimers.length > 0){
      this.log.info('Cleaning up %s start timers', abandonedStartTimers.length);
    }
    this.startTurnTimers = _.without(this.startTurnTimers, ...abandonedStartTimers);

    let abandonedEndTimers = this.endTurnTimers.filter(abandonedFilter);
    if(abandonedEndTimers.length > 0){
      this.log.info('Cleaning up %s end timers', abandonedEndTimers.length);
    }
    this.endTurnTimers = _.without(this.endTurnTimers, ...abandonedEndTimers);
  }

  //look through the cards for any cards needing a TARGET
  //on one of the targetableEvents
  //and then find what the possible targets are
  //   [
  //     {cardId: 2, event: 'x', targetPieceIds: [4,5,6]}
  //   ]
  findPossibleTargets(cards, playerId){
    let targets = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let targetPieceIds = this.findActionTargets(event.actions, playerId, card.isSpell);
        if(targetPieceIds){
          targets.push({
            cardId: card.id,
            event: event.event,
            targetPieceIds
          });
        }
      }
    }

    return targets;
  }

  findActionTargets(eventActions, playerId, isSpell){

    //try to find TARGETS in any of the actions
    for(let cardEventAction of eventActions){
      if(this.targetableActions.indexOf(cardEventAction.action) === -1) continue;

      for(let arg of cardEventAction.args){

        //check for recursive step if arg is another action (for a turn timer for instance)
        if(arg.action){
          let recursivePieceIds = this.findActionTargets([arg], playerId, isSpell);
          if(recursivePieceIds){
            return recursivePieceIds;
          }
        }

        let selector = arg;

        if(arg.attributeSelector){
          selector = arg.attributeSelector;
        }

        //Selectors should always have a left
        if(!selector.left) continue;

        if(!this.selector.doesSelectorUse(selector, 'TARGET')) continue;

        let targetPieceIds = this.selector.selectPossibleTargets(
          selector, {controllingPlayerId: playerId, isSpell}
        ).map(p => p.id);

        //only allow max of 1 targetable action per event
        return targetPieceIds;
      }
    }

    return null;
  }

  //look through the pieces for any piece with an ability
  //then find the name, charge time, and what the possible targets are
  //   [
  //     {pieceId: 2, ability: 'x', abilityCost: 0, abilityChargeTime: 0, abilityCooldown: 0, targetPieceIds: [4,5,6]}
  //   ]
  //Needs to be documented in a better place but the args to the ability event are:
  // ability(cost, chargeTime, name, playerSelector?)
  // OR
  // ability(pieceSelector)
  // to trigger on pieces using abilities
  findPossibleAbilities(pieces, playerId){
    let targets = [];

    for(let piece of pieces){
      if(!piece.events) continue;

      let ability = piece.events.find(e => e.event === 'ability');

      if(!ability) continue;

      if(!ability.args.length || ability.args[0].left ){
        //If the ability event on the piece is to react to other pieces using abilities don't consider it as a piece ability
        continue;
      }

      //Check to see if the piece ability is for this player, either they have the optional arg to select which player or just the default of player
      if(ability.args[3]){
        let playerSelected = this.selector.selectPlayer(piece.playerId, ability.args[3]);
        if(playerSelected !== playerId){ continue; }
      } else if(piece.playerId != playerId){
        continue;
      }

      let targetPieceIds = this.findActionTargets(ability.actions, playerId, true) || [];
      targets.push({
        pieceId: piece.id,
        abilityCost: ability.args[0],
        abilityChargeTime: ability.args[1],
        abilityCooldown: Math.max(0, ability.args[1] - piece.abilityCharge),
        ability: ability.args[2],
        targetPieceIds
      });
    }

    return targets;
  }

  //look through cards for any areas when played
  //   [
  //     {cardId: 2, event: 'x', area: [Square|Cross...], size: 3, center?: Position}
  //   ]
  findPossibleAreas(cards, playerId){
    let areas = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let areaSelector = null;
        let alsoNeedsTarget = false;
        let moveRestricted = false; //Does the area represent somewhere where a piece needs to move to?
        //try to find areas in any of the actions
        for(let cardEventAction of event.actions){
          if(!this.targetableActions.includes(cardEventAction.action)) continue;

          //hacky way to see if the action needs a main target for its selection
          alsoNeedsTarget = this.selector.doesSelectorUse(cardEventAction.args[0], 'TARGET');

          //For now only a move in area command uses an area that is move restricted
          moveRestricted = cardEventAction.action === 'Move';

          for(let arg of cardEventAction.args){
            //Area selectors will always have a left
            if(!arg.left) continue;

            areaSelector = this.selector.findSelector(arg, s => s && s.area);

            if(areaSelector){
              let areaDescrip = this.selector.selectArea(
                areaSelector,
                {isSpell: card.isSpell, controllingPlayerId: playerId}
              );
              areas.push({
                cardId: card.id,
                event: event.event,
                areaType: areaDescrip.areaType,
                size: areaDescrip.size,
                isCursor: areaDescrip.isCursor,
                isDoubleCursor: areaDescrip.isDoubleCursor || alsoNeedsTarget,
                bothDirections: areaDescrip.bothDirections,
                selfCentered: areaDescrip.selfCentered,
                stationaryArea: areaDescrip.stationaryArea,
                centerPosition: areaDescrip.centerPosition,
                pivotPosition: areaDescrip.pivotPosition,
                areaTiles: areaDescrip.areaTiles,
                moveRestricted
              });
            }
          }
        }

      }
    }

    return areas;
  }

  //look through pieces for non synthesis events that have areas to show the player
  //Very similar to cards above
  findPieceAreas(pieces, playerId){
    let areas = [];

    for(let piece of pieces){
      if(!piece.events) continue;

      for(let event of piece.events){
        if(this.targetableEvents.includes(event)){ continue; } //Skip events for things that happened when the piece was played

        let areaSelector = null;
        let moveRestricted = false; //Does the area represent somewhere where a piece needs to move to?
        //try to find areas in any of the actions
        for(let cardEventAction of event.actions){
          //Don't think this needs to be restricted to any particular actions
          //if(!this.targetableActions.includes(cardEventAction.action)) continue;

          //For now only a move in area command uses an area that is move restricted
          moveRestricted = cardEventAction.action === 'Move';

          for(let arg of cardEventAction.args){
            //Area selectors will always have a left
            if(!arg.left) continue;

            areaSelector = this.selector.findSelector(arg, s => s && s.area);

            if(areaSelector){
              let areaDescrip = this.selector.selectArea(
                areaSelector,
                {isSpell: false, controllingPlayerId: playerId}
              );
              areas.push({
                pieceId: piece.id,
                event: event.event,
                areaType: areaDescrip.areaType,
                size: areaDescrip.size,
                isCursor: areaDescrip.isCursor,
                isDoubleCursor: areaDescrip.isDoubleCursor,
                bothDirections: areaDescrip.bothDirections,
                selfCentered: areaDescrip.selfCentered,
                stationaryArea: areaDescrip.stationaryArea,
                centerPosition: areaDescrip.centerPosition,
                pivotPosition: areaDescrip.pivotPosition,
                areaTiles: areaDescrip.areaTiles,
                moveRestricted
              });
            }
          }
        }

      }
    }

    return areas;
  }

  //Find cards that have a choice, and tell the client what the choices are, as well as possible targets
  //for each of the choices
  //TODO: in the future add possibility to do different areas
  //   [
  //     {cardId: 2, choices: [{ cardTemplateId: 1, targets: <return from findPossibleTargets>  } ] }
  //   ]
  findChooseCards(cards, playerId, cardDirectory){
    let choices = [];

    for(let card of cards){
      if(!card.events) continue;

      //targetable events also applies to choices
      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let choice = event.actions.find(a => a.action === 'Choose');
        if(!choice) continue;

        let chooseCardIds = [choice.args[0], choice.args[1]];
        let cardChoices = [];

        //since the directory cards won't have id's we have to find the targets individually
        //to associate them properly
        for(let chooseCardId of chooseCardIds){
          let directoryCard = cardDirectory.directory[chooseCardId];

          //   [
          //     {cardId: 2, event: 'x', targetPieceIds: [4,5,6]}
          //   ]
          let targets = this.findPossibleTargets([directoryCard], playerId);
          cardChoices.push({
            cardTemplateId: chooseCardId,
            targets: targets.length > 0 ? targets[0] : null
          });
        }

        choices.push({
          cardId: card.id,
          choices: cardChoices
        });
      }
    }

    return choices;
  }

  //Get a list of pieces with events/death events to send to the client
  findEventedPieces(){
    let eventedPieces = [];

    for(let piece of this.pieceState.pieces){
      if(!piece.events) continue;
      if(piece.isHero) continue;

      let deathEvent = piece.events.find(e => e.event === 'death');
      if(deathEvent != null){
        eventedPieces.push({pieceId: piece.id, event: 'd'});
      }

      //don't highlight pieces with the default playMinion action
      //ones with other args should be shown though
      let ignoredEvents = ['playMinion', 'death'];
      let otherEvent = piece.events.find(e => !ignoredEvents.includes(e.event) || e.args);
      if(otherEvent != null){
        eventedPieces.push({pieceId: piece.id, event: 'e'});
      }

      //also show repeating timers
      let repeatingStartTimer = this.startTurnTimers.find(t => t.piece && t.piece.id === piece.id && t.interval);
      let repeatingEndTimer = this.endTurnTimers.find(t => t.piece && t.piece.id === piece.id && t.interval);
      if(repeatingStartTimer || repeatingEndTimer){
        eventedPieces.push({pieceId: piece.id, event: 't'});
      }
    }

    return eventedPieces;
  }

  //look through cards for any that have a condition, and if it's met
  //If a card has multiple conditions, only send if the last one is met so combo cards will highlight properly
  //   [
  //     {cardId: 2}
  //   ]
  findMetConditionCards(cards, playerId){
    let conditionals = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let event of card.events){

        //try to find conditionals in any of the actions
        let conditions = event.actions.filter(a => a.condition);
        if(conditions.length == 0) continue;

        //only consider the last condition to send to the client (Combo)
        let cardEventAction = conditions[conditions.length - 1];

        let pieceSelectorParams = {
          controllingPlayerId: card.playerId,
          isSpell: card.isSpell,
          isTimer: false
        };

        let compareResult = this.selector.compareExpression(cardEventAction.condition, this.pieceState, pieceSelectorParams, this.selector.selectPieces);
        if(compareResult.length === 0){
          continue;
        }

        conditionals.push({cardId: card.id});
      }
    }

    return conditionals;
  }

  //Gets the event selector for the card event.  By convention this is the 0th arg that's a selector
  //but the 0th arg doesn't have to be a selector
  eventSelector(cardEvent){
    if(!cardEvent.args) return null;

    const firstArg = cardEvent.args[0];
    if(firstArg.left){
      return firstArg;
    }
    return null;
  }

  //Responsible for tracking piece damage taken across multiple "times" loops of the evaluation process
  //This is to fix a multi time hit spell overkilling a minion and wasting damage
  //Returns a bool indicating if the piece already died from previous damage taken
  checkPieceDeath(pieceDamages, pieceId){
    if(!pieceDamages[pieceId]){
      pieceDamages[pieceId] = {
        shieldRemoved: false,
        totalDamageTaken: 0
      };
    }
    let pieceStatus = pieceDamages[pieceId];
    let piece = this.pieceState.piece(pieceId);
    if(!piece) return false;

    return pieceStatus.totalDamageTaken >= piece.armor + piece.health;
  }

  //Complimentary method to above, just add to the total damage taken for pieces that did receive damage
  addPieceDamageTaken(pieceDamages, pieceId, damage){
    let pieceStatus = pieceDamages[pieceId];
    let piece = this.pieceState.piece(pieceId);
    if(!piece || !pieceStatus) return false;

    if(((piece.statuses & Statuses.Shield) == Statuses.Shield) && !pieceStatus.shieldRemoved){
      pieceStatus.shieldRemoved = true;
      return false;
    }

    //subtract the damage (since it's negative) to make a more sensible totalDamageTaken number
    pieceStatus.totalDamageTaken -= damage;

  }
}
