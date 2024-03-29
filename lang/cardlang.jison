/*
 * Card lang
 */

%lex
%%

\s+ /* skip whitespace */

//Events are things to be reacted to, like an action that happens when a piece is damaged
//piece events
(playMinion|death|damaged|attacks|ability|healed)
  return 'event'

//player events
(cardDrawn|turnEnd|turnStart|playSpell|cardDiscarded|chargeChange)
  return 'event'

//Targets are who the actions affect
// player targets
(PLAYER|OPPONENT)
  return 'target'

// neutral piece targets
(TARGET|SELF|ACTIVATOR|SELECTED)
  return 'target'

// targets for area selection
(CURSOR)
  return 'target'

// Targets for timers
(SAVED)
  return 'target'

// selectors
(ENEMY|CHARACTER|MINION|FRIENDLY|HERO|DAMAGED|BASIC|MELEE|RANGED)
  return 'target'

//Card specific selectors
(SPELL|DECK|HAND|DIRECTORY)
  return 'target'

//status selectors
(SILENCE|SHIELD|PETRIFY|TAUNT|CLOAK|ELUSIVE|ROOTED|CANTATTACK|DYADSTRIKE|FLYING|AIRDROP|CLEAVE|PIERCING)
  return 'target'

(Random)
  return 'random'
(PieceIds)
  return 'pieceIds'

//eventual number stuff
(SelectAttribute)
  return 'selectAttr'
(Count)
  return 'count'
(CardCount)
  return 'cardCount'
(SelectCardTemplateId)
  return 'selectCardTemplateId'

//Player resource eNumbers
(Charges|Resources)
  return 'resource'

//stats that change throughout the game
(COMBOCOUNT)
  return 'stat'

// actions
(SetAttribute|Hit|Heal|Buff|RemoveBuff|Spawn|GiveStatus|RemoveStatus|Charm|Destroy|Aura|Move|Transform|GiveArmor|AttachCode|CardAura|Unsummon|Choose)
  return 'action'

// actions about players
(DrawCard|Discard|ChangeEnergy|GiveCard|ShuffleToDeck|ChargeUp)
  return 'action'

//timer actions
(endTurnTimer|startTurnTimer)
  return 'action'

//attributes
(health|attack|movement|range|cardTemplateId|armor|cost|baseHealth|maxBuffedHealth|baseAttack|baseMovement|baseRange|spellDamage)
  return 'attribute'

//statuses
(Silence|Shield|Petrify|Taunt|Cloak|Elusive|Root|CantAttack|DyadStrike|Flying)
  return 'status'

//areas
(Cross|Square|Line|Diagonal|Row|Diamond|Star)
  return 'area'

(North|East|South|West)
  return 'areaDirection'

(Area)
  return 'Area'
(Tagged)
  return 'Tagged'
(Id)
  return 'Id'

//bool
(true|false)
  return 'bool'

//numbers
(\-?[0-9]+)
  return 'number'

//text
(\'(.*?)\')
  return 'text'

//syntax
'('    return '('
')'    return ')'
'['    return '['
']'    return ']'
','    return ','
'{'    return '{'
'}'    return '}'
'=='   return '=='
'='    return '='
'|'    return '|'
'&&'   return '&&'
'&'    return '&'
'-'    return '-'
'+'    return '+'
'*'    return '*'
'/'    return '/'
'<='   return '<='
'>='   return '>='
'>'    return '>'
'<'    return '<'


<<EOF>>               return 'EOF'
.                     return 'INVALID'

/lex

%ebnf

/* operator associations and precedence */

%left '|'
%left '&'
%left '-'
%left '&&'

%left '+' '-'
%left '*' '/'
%left UMINUS

%start events

%% /* language grammar */

events
  : eventList EOF {return $1;}
;

eventList
  : eventList pEvent
     { $$ = $eventList; $$.push($pEvent); }
  | pEvent
     { $$ = [$pEvent]; }
  ;

/* events */
pEvent
  : event'{'actionlist'}'
   { $$ = { event: $1, actions: $3 } }
  | event'('arguments')''{'actionlist'}'
   { $$ = { event: $1, args: $3, actions: $6 } }
  | event'['comparisonExpression']''{'actionlist'}'
   { $$ = { event: $1, condition: $3, actions: $6 } }
  | event'('arguments')''['comparisonExpression']''{'actionlist'}'
   { $$ = { event: $1, args: $3, condition: $6, actions: $9 } }
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
  | comparisonExpression '&&' action '('arguments')'
  {{ $$ =
    { condition: $1, action: $3, args: $5 }
  }}
  | comparisonExpression '&&' action '('arguments')' '*' eNumber
  {{ $$ =
    { condition: $1, action: $3, args: $5, times: $8 }
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
  | area -> $1
  | eNumber -> $1
  | pText -> $1
  | pBool -> $1
  | comparisonExpression -> $1
  | actionargs -> $1
  | eventList -> $1
  | pAreaDirection -> $1
  ;

possibleRandSelector
  : selector
  | random'('selector')'
     { $$ = { random: true, selector: $3 }; }
;

selector
  : targetExpr
     { $$ = { left: $1}; }
  | selector operator '(' selector ')'
     { $$ = {left: $1, op: $2, right: $4}; }
  | '(' selector ')' operator '(' selector ')'
     { $$ = {left: $2, op: $4, right: $6}; }
  | '(' selector ')' operator targetExpr
     { $$ = {left: $2, op: $4, right: $5}; }
  | selector operator targetExpr
     { $$ = { left: $1, op: $2, right: $3 }; }
  ;

targetExpr
  : target -> $1
  | areaExpression -> $1
  | tagExpression -> $1
  | templateIdExpression -> $1
  | pieceIdsExpression -> $1
  | '('comparisonExpression')' -> $2
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

areaExpression
  : Area'('arguments')'
    {{ $$ =
      { area: true, args: $3 }
    }}
  ;

tagExpression
  : Tagged'('pText')'
    {{ $$ =
      { tag: $3 }
    }}
  ;

//piece by card template Id
templateIdExpression
  : Id'('eNumber')'
    {{ $$ =
      { templateId: $3 }
    }}
  ;

//by piece instance Id
pieceIdsExpression
  : pieceIds'('numberList')'
    {{ $$ =
      { pieceIds: $3 }
    }}
  ;

//eventually a number, could be random
eNumber
    : eValue
        {$$ = $1 }
    | '(' eNumber')'
        {$$ = $2 }
    | eNumber '+' eNumber
        {$$ = {eNumber: true, op: '+', left: $1, right: $3}; }
    | eNumber '-' eNumber
        {$$ = {eNumber: true, op: '-', left: $1, right: $3}; }
    | eNumber '*' eNumber
        {$$ = {eNumber: true, op: '*', left: $1, right: $3}; }
    | eNumber '/' eNumber
        {$$ = {eNumber: true, op: '/', left: $1, right: $3}; }
    | '-' eNumber %prec UMINUS
        {$$ = {eNumber: true, op: 'negate', left: $2 }; }
    ;

//The things that actually will resolve to an integer value
eValue
  : pNumber -> $1
  | random'('numberList')'
     { $$ = { eValue: true, randList: $3 }; }
  | selectAttr'('possibleRandSelector','attribute')'
     { $$ = { eValue: true, attributeSelector: $3, attribute: $5 }; }
  | count'('possibleRandSelector')'
     { $$ = { eValue: true, count: true, selector: $3 }; }
  | cardCount'('possibleRandSelector')'
     { $$ = { eValue: true, cardCount: true, selector: $3 }; }
  | stat
     { $$ = { eValue: true, stat: true, path: $1 }; }
  | resource'('possibleRandSelector')'
     { $$ = { eValue: true, resource: $1, selector: $3 }; }
  | selectCardTemplateId'('possibleRandSelector')'
     { $$ = { eValue: true, selectCardTemplateId: true, cardSelector: $3 }; }
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
  | status'('pBool')'
    { $$ = { status: $status, adding: $3 }; }
  ;

pText
  : text -> $1.substring(1, $1.length-1);
  ;

pBool
  : bool -> $1 == 'true'
;

pAreaDirection
  : areaDirection
     { $$ = {areaDirection: $areaDirection}; }
;
