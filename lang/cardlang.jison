/*
 * Simple card lang
 */

%lex
%%

\s+                         /* skip whitespace */

//root events
(play|death)                    return 'event'

// targets
(PLAYER|TARGET)    return 'target'

// actions
(DrawCard|SetAttribute)    return 'action'

//attributes
(current_health)    return 'attribute'

//numbers
[0-9]    return 'number'

//syntax
'('    return '('
')'    return ')'
','    return ','
';'    return ';'
'{'    return '{'
'}'    return '}'
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
  : pEvent c
    { $$ = $c; $$.unshift($pEvent); }
  | pEvent
    { $$ = [$pEvent]; }
  ;

pEvent
  : event'{'actionlist'}'
    {{ $$ = 
      { event: $1, actions: $3 }
    }}
  ;

actionlist
  : actionargs actionlist
     { $$ = $actionlist; $$.unshift($actionargs); }
  | actionargs
     { $$ = [$actionargs]; }
  ;

actionargs
  : action'('arguments')'';' 
  {{ $$ = 
    { action: $1, args: $3 }
  }}
  ;

arguments
  : argument_item ',' arguments 
     { $$ = $arguments; $$.unshift($argument_item); }
  | argument_item 
     { $$ = [$argument_item]; }
  ;

argument_item
  : target -> $1
  | attribute -> $1
  | number -> $1
  ;