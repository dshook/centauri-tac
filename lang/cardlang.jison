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

// player targets
(PLAYER|OPPONENT)
  return 'target'

// neutral piece targets
(TARGET|SELF|RANDOM_CHARACTER)
  return 'target'

// enemy piece targets
(RANDOM_ENEMY_CHARACTER|RANDOM_ENEMY_MINION)
  return 'target'

// friendly piece targets
(RANDOM_FRIENDLY_CHARACTER|RANDOM_FRIENDLY_MINION)
  return 'target'

// actions
(DrawCard|SetAttribute|Hit)
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

<<EOF>>               return 'EOF'
.                     return 'INVALID'

/lex

%ebnf

%start events

%% /* language grammar */

events
  : c EOF {return $1;}
;

c
  : pPlay? pDeath?
    {{ $$ = {
         play: $1,
         death: $2
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
  : target -> $1
  | attribute -> $1
  | pNumber -> $1
  ;

pNumber
  : number -> parseInt($1)
  ;