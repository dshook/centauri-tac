/*
 * Card lang
 */

%lex
%%

\s+ /* skip whitespace */

//piece events
(playMinion|death|damaged|attacks)
  return 'event'

//player events
(cardDrawn|turnEnd|turnStart|playSpell)
  return 'event'

// player targets
(PLAYER|OPPONENT)
  return 'target'

// neutral piece targets
(TARGET|SELF|ACTIVATOR)
  return 'target'

// Targets for timers
(SAVED)
  return 'target'

// selectors
(ENEMY|CHARACTER|MINION|FRIENDLY|HERO|DAMAGED|BASIC)
  return 'target'

//status selectors
(SILENCE|SHIELD|PARALYZE|TAUNT|CLOAK|TECHRESIST|ROOTED)
  return 'target'

(Random)
  return 'random'

(SelectAttribute)
  return 'selectAttr'

// actions
(DrawCard|SetAttribute|Hit|Heal|Buff|RemoveBuff|Spawn|GiveStatus|RemoveStatus|Charm|Destroy)
  return 'action'

//timer actions
(endTurnTimer|startTurnTimer)
  return 'action'

//attributes
(health|attack|movement)
  return 'attribute'

//statuses
(Silence|Shield|Paralyze|Taunt|Cloak|TechResist|Root)
  return 'status'

//bool
(true|false)
  return 'bool'

//numbers
(\-?[0-9])
  return 'number'

//text
(\'(.*?)\')
  return 'text'

//syntax
'('    return '('
')'    return ')'
','    return ','
'{'    return '{'
'}'    return '}'
'*'    return '*'
'='    return '='
'|'    return '|'
'&'    return '&'
'-'    return '-'
'<'    return '<'
'<='   return '<='
'>'    return '>'
'>='   return '>='
'=='   return '=='


<<EOF>>               return 'EOF'
.                     return 'INVALID'

/lex

%ebnf

/* operator associations and precedence */

%left '|'
%left '&'
%left '-'

%start events

%% /* language grammar */

events
  : c EOF {return $1;}
;

c
  : c pEvent
     { $$ = $c; $$.push($pEvent); }
  | pEvent
     { $$ = [$pEvent]; }
  ;

/* events */
pEvent
  : event'{'actionlist'}'
   { $$ = { event: $1, actions: $3 } }
  | event'('possibleRandSelector')''{'actionlist'}'
   { $$ = { event: $1, selector: $3, actions: $6 } }
;

/* actionlist is all the actions for each event */
actionlist
  : actionlist actionargs
     { $$ = $actionlist; $$.push($actionargs); }
  | actionargs
     { $$ = [$actionargs]; }
  ;

/* actionargs is the basic syntax for all the actions with function like arguments */
actionargs
  : action'('arguments')'
  {{ $$ =
    { action: $1, args: $3 }
  }}
  | action '('arguments')' '*' eNumber
  {{ $$ =
    { action: $1, args: $3, times: $6 }
  }}
  ;

arguments
  : arguments ',' argument_item
     { $$ = $arguments; $$.push($argument_item); }
  | argument_item
     { $$ = [$argument_item]; }
  ;

argument_item
  : possibleRandSelector -> $1
  | attribute -> $1
  | status -> $1
  | buffAttribute -> $1
  | eNumber -> $1
  | pText -> $1
  | pBool -> $1
  | actionargs -> $1
  ;

possibleRandSelector
  : selector
  | targetExpr
     { $$ = { left: $1}; }
  | random'('target')'
     { $$ = { random: true, selector: { left: $3} }; }
  | random'('selector')'
     { $$ = { random: true, selector: $3 }; }
;

targetExpr
  : target -> $1
  | '('comparisonExpression')' -> $2
;

selector
  : selector operator targetExpr
     { $$ = { left: $1, op: $2, right: $3 }; }
  | targetExpr operator targetExpr
     { $$ = { left: $1, op: $2, right: $3 }; }
  ;

operator
  : '&'
  | '|'
  | '-'
  ;

compareOperator
  : '<'
  | '>'
  | '>='
  | '<='
  | '=='
  ;

comparisonExpression
  : eNumber compareOperator eNumber
     { $$ = { compareExpression: true, left: $1, op: $2, right: $3 }; }
  ;

//eventually a number, could be random
eNumber
  : pNumber -> $1
  | random'('numberList')'
     { $$ = { eNumber: true, randList: $3 }; }
  | selectAttr'('possibleRandSelector','attribute')'
     { $$ = { eNumber: true, attributeSelector: $3, attribute: $5 }; }
  ;

numberList
  : numberList ',' pNumber
     { $$ = $numberList; $$.push($pNumber); }
  | pNumber
     { $$ = [$pNumber]; }
  ;

pNumber
  : number -> parseInt($1)
  ;

buffAttribute
  : attribute'('eNumber')'
    { $$ = { attribute: $attribute, amount: $3 }; }
  ;

pText
  : text -> $1.substring(1, $1.length-1);
  ;

pBool
  : bool -> $1 == 'true'
;