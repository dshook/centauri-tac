/*
 * Simple card lang
 */

%lex
%%

\s+                         /* skip whitespace */

//root events
(play)                    return 'play'
(death)                   return 'death'

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
  : playaction? ';'? deathaction?
      {{ $$ =
        {
          play: $1,
          deathaction: $3
        };
      }}
  ;



playaction
  : play '=' actionlist -> 'Play'
  ;

deathaction
  : death '=' actionlist -> 'Death'
  ;

actionlist
  : actionlist ';' actionargs
  | actionlist ';'
  | actionargs
  ;

actionargs
  : action'('arguments')'
  ;

arguments
  : arguments',' argument_item
  | argument_item
  ;

argument_item
  : target
  | attribute
  | number
  ;
