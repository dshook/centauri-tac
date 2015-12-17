/*
 * Simple card lang
 */

%lex
%%

\s+ /* skip whitespace */

//root events
(play) 
  return 'play'
(death)
  return 'death'
(damaged)
  return 'damaged'

// player targets
(PLAYER|OPPONENT)
  return 'target'

// neutral piece targets
(TARGET|SELF)
  return 'target'

// selectors
(ENEMY|CHARACTER|MINION|FRIENDLY|HERO)
  return 'target'

(Random)
  return 'random'

// actions
(DrawCard|SetAttribute|Hit|Heal)
  return 'action'

//attributes
(health|attack|movement)
  return 'attribute'

//numbers
[0-9]
  return 'number'

//syntax
'('    return '('
')'    return ')'
','    return ','
';'    return ';'
'{'    return '{'
'}'    return '}'
'*'    return '*'
'='    return '='
'|'    return '|'
'&'    return '&'
'-'    return '-'

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
  : pPlay? pDeath? pDamaged?
    {{ $$ = {
         play: $1,
         death: $2,
         damaged: $3
      }; 
    }}
  ;

/* events */
pPlay
  : play'{'actionlist'}' -> $3
;

pDeath
  : death'{'actionlist'}' -> $3
;

pDamaged
  : damaged'{'actionlist'}' -> $3
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
  : action'('arguments')'';' 
  {{ $$ = 
    { action: $1, args: $3 }
  }}
  | action '('arguments')' '*' pNumber';'
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
  | pNumber -> $1
  ;

possibleRandSelector
  : selector
  | target
     { $$ = { left: $1}; }
  | random'('target')'
     { $$ = { random: true, selector: { left: $3} }; }
  | random'('selector')'
     { $$ = { random: true, selector: $3 }; }
;

selector
  : target operator target 
     { $$ = { left: $1, op: $2, right: $3 }; }
  | selector operator target
     { $$ = { left: $1, op: $2, right: $3 }; }
  ;

operator
  : '&'
  | '|'
  | '-'
  ;

pNumber
  : number -> parseInt($1)
  ;