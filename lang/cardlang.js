/* parser generated by jison 0.4.17 */
/*
  Returns a Parser object of the following structure:

  Parser: {
    yy: {}
  }

  Parser.prototype: {
    yy: {},
    trace: function(),
    symbols_: {associative list: name ==> number},
    terminals_: {associative list: number ==> name},
    productions_: [...],
    performAction: function anonymous(yytext, yyleng, yylineno, yy, yystate, $$, _$),
    table: [...],
    defaultActions: {...},
    parseError: function(str, hash),
    parse: function(input),

    lexer: {
        EOF: 1,
        parseError: function(str, hash),
        setInput: function(input),
        input: function(),
        unput: function(str),
        more: function(),
        less: function(n),
        pastInput: function(),
        upcomingInput: function(),
        showPosition: function(),
        test_match: function(regex_match_array, rule_index),
        next: function(),
        lex: function(),
        begin: function(condition),
        popState: function(),
        _currentRules: function(),
        topState: function(),
        pushState: function(condition),

        options: {
            ranges: boolean           (optional: true ==> token location info will include a .range[] member)
            flex: boolean             (optional: true ==> flex-like lexing behaviour where the rules are tested exhaustively to find the longest match)
            backtrack_lexer: boolean  (optional: true ==> lexer regexes are tested in order and for each matching regex the action code is invoked; the lexer terminates the scan when a token is returned by the action code)
        },

        performAction: function(yy, yy_, $avoiding_name_collisions, YY_START),
        rules: [...],
        conditions: {associative list: name ==> set},
    }
  }


  token location info (@$, _$, etc.): {
    first_line: n,
    last_line: n,
    first_column: n,
    last_column: n,
    range: [start_number, end_number]       (where the numbers are indexes into the input string, regular zero-based)
  }


  the parseError function receives a 'hash' object with these members for lexer and parser errors: {
    text:        (matched text)
    token:       (the produced terminal token, if any)
    line:        (yylineno)
  }
  while parser (grammar) errors will also provide these members, i.e. parser errors deliver a superset of attributes: {
    loc:         (yylloc)
    expected:    (string describing the set of expected tokens)
    recoverable: (boolean: TRUE when the parser has a error recovery rule available for this particular error)
  }
*/
var cardlang = (function(){
var o=function(k,v,o,l){for(o=o||{},l=k.length;l--;o[k[l]]=v);return o},$V0=[1,4],$V1=[5,7],$V2=[1,11],$V3=[1,17],$V4=[1,15],$V5=[1,16],$V6=[10,15],$V7=[13,19],$V8=[1,23],$V9=[1,24],$Va=[1,25],$Vb=[13,19,32,33,34],$Vc=[2,24],$Vd=[1,31],$Ve=[1,32],$Vf=[1,33],$Vg=[1,37],$Vh=[1,38],$Vi=[1,44],$Vj=[1,45],$Vk=[1,46],$Vl=[11,29],$Vm=[1,50],$Vn=[10,13,15,19,36,37,38,39,40],$Vo=[28,43,44],$Vp=[10,13,15,19];
var parser = {trace: function trace() { },
yy: {},
symbols_: {"error":2,"events":3,"c":4,"EOF":5,"pEvent":6,"event":7,"{":8,"actionlist":9,"}":10,"(":11,"possibleRandSelector":12,")":13,"actionargs":14,"action":15,"arguments":16,"*":17,"eNumber":18,",":19,"argument_item":20,"attribute":21,"status":22,"buffAttribute":23,"pText":24,"pBool":25,"selector":26,"targetExpr":27,"random":28,"target":29,"comparisonExpression":30,"operator":31,"&":32,"|":33,"-":34,"compareOperator":35,"<":36,">":37,">=":38,"<=":39,"==":40,"pNumber":41,"numberList":42,"selectAttr":43,"number":44,"text":45,"bool":46,"$accept":0,"$end":1},
terminals_: {2:"error",5:"EOF",7:"event",8:"{",10:"}",11:"(",13:")",15:"action",17:"*",19:",",21:"attribute",22:"status",28:"random",29:"target",32:"&",33:"|",34:"-",36:"<",37:">",38:">=",39:"<=",40:"==",43:"selectAttr",44:"number",45:"text",46:"bool"},
productions_: [0,[3,2],[4,2],[4,1],[6,4],[6,7],[9,2],[9,1],[14,4],[14,6],[16,3],[16,1],[20,1],[20,1],[20,1],[20,1],[20,1],[20,1],[20,1],[20,1],[12,1],[12,1],[12,4],[12,4],[27,1],[27,3],[26,3],[26,3],[31,1],[31,1],[31,1],[35,1],[35,1],[35,1],[35,1],[35,1],[30,3],[18,1],[18,4],[18,6],[42,3],[42,1],[41,1],[23,4],[24,1],[25,1]],
performAction: function anonymous(yytext, yyleng, yylineno, yy, yystate /* action[1] */, $$ /* vstack */, _$ /* lstack */) {
/* this == yyval */

var $0 = $$.length - 1;
switch (yystate) {
case 1:
return $$[$0-1];
break;
case 2: case 6:
 this.$ = $$[$0-1]; this.$.push($$[$0]); 
break;
case 3: case 7: case 11: case 41:
 this.$ = [$$[$0]]; 
break;
case 4:
 this.$ = { event: $$[$0-3], actions: $$[$0-1] } 
break;
case 5:
 this.$ = { event: $$[$0-6], selector: $$[$0-4], actions: $$[$0-1] } 
break;
case 8:
 this.$ =
    { action: $$[$0-3], args: $$[$0-1] }
  
break;
case 9:
 this.$ =
    { action: $$[$0-5], args: $$[$0-3], times: $$[$0] }
  
break;
case 10: case 40:
 this.$ = $$[$0-2]; this.$.push($$[$0]); 
break;
case 12: case 13: case 14: case 15: case 16: case 17: case 18: case 19: case 24: case 37:
this.$ = $$[$0];
break;
case 21:
 this.$ = { left: $$[$0]}; 
break;
case 22:
 this.$ = { random: true, selector: { left: $$[$0-1]} }; 
break;
case 23:
 this.$ = { random: true, selector: $$[$0-1] }; 
break;
case 25:
this.$ = $$[$0-1];
break;
case 26: case 27:
 this.$ = { left: $$[$0-2], op: $$[$0-1], right: $$[$0] }; 
break;
case 36:
 this.$ = { compareExpression: true, left: $$[$0-2], op: $$[$0-1], right: $$[$0] }; 
break;
case 38:
 this.$ = { eNumber: true, randList: $$[$0-1] }; 
break;
case 39:
 this.$ = { eNumber: true, attributeSelector: $$[$0-3], attribute: $$[$0-1] }; 
break;
case 42:
this.$ = parseInt($$[$0]);
break;
case 43:
 this.$ = { attribute: $$[$0-3], amount: $$[$0-1] }; 
break;
case 44:
this.$ = $$[$0].substring(1, $$[$0].length-1);;
break;
case 45:
this.$ = $$[$0] == 'true';
break;
}
},
table: [{3:1,4:2,6:3,7:$V0},{1:[3]},{5:[1,5],6:6,7:$V0},o($V1,[2,3]),{8:[1,7],11:[1,8]},{1:[2,1]},o($V1,[2,2]),{9:9,14:10,15:$V2},{11:$V3,12:12,26:13,27:14,28:$V4,29:$V5},{10:[1,18],14:19,15:$V2},o($V6,[2,7]),{11:[1,20]},{13:[1,21]},o($V7,[2,20],{31:22,32:$V8,33:$V9,34:$Va}),o($V7,[2,21],{31:26,32:$V8,33:$V9,34:$Va}),{11:[1,27]},o($Vb,$Vc),{18:29,28:$Vd,30:28,41:30,43:$Ve,44:$Vf},o($V1,[2,4]),o($V6,[2,6]),{11:$V3,12:36,14:43,15:$V2,16:34,18:40,20:35,21:$Vg,22:$Vh,23:39,24:41,25:42,26:13,27:14,28:$Vi,29:$V5,41:30,43:$Ve,44:$Vf,45:$Vj,46:$Vk},{8:[1,47]},{11:$V3,27:48,29:$V5},o($Vl,[2,28]),o($Vl,[2,29]),o($Vl,[2,30]),{11:$V3,27:49,29:$V5},{11:$V3,26:51,27:52,29:$Vm},{13:[1,53]},{35:54,36:[1,55],37:[1,56],38:[1,57],39:[1,58],40:[1,59]},o($Vn,[2,37]),{11:[1,60]},{11:[1,61]},o($Vn,[2,42]),{13:[1,62],19:[1,63]},o($V7,[2,11]),o($V7,[2,12]),o($V7,[2,13],{11:[1,64]}),o($V7,[2,14]),o($V7,[2,15]),o($V7,[2,16]),o($V7,[2,17]),o($V7,[2,18]),o($V7,[2,19]),{11:[1,65]},o($V7,[2,44]),o($V7,[2,45]),{9:66,14:10,15:$V2},o($Vb,[2,26]),o($Vb,[2,27]),o([32,33,34],$Vc,{13:[1,67]}),{13:[1,68],31:22,32:$V8,33:$V9,34:$Va},{31:26,32:$V8,33:$V9,34:$Va},o($Vb,[2,25]),{18:69,28:$Vd,41:30,43:$Ve,44:$Vf},o($Vo,[2,31]),o($Vo,[2,32]),o($Vo,[2,33]),o($Vo,[2,34]),o($Vo,[2,35]),{41:71,42:70,44:$Vf},{11:$V3,12:72,26:13,27:14,28:$V4,29:$V5},o($Vp,[2,8],{17:[1,73]}),{11:$V3,12:36,14:43,15:$V2,18:40,20:74,21:$Vg,22:$Vh,23:39,24:41,25:42,26:13,27:14,28:$Vi,29:$V5,41:30,43:$Ve,44:$Vf,45:$Vj,46:$Vk},{18:75,28:$Vd,41:30,43:$Ve,44:$Vf},{11:$V3,26:51,27:52,29:$Vm,41:71,42:70,44:$Vf},{10:[1,76],14:19,15:$V2},o($V7,[2,22]),o($V7,[2,23]),{13:[2,36]},{13:[1,77],19:[1,78]},o($V7,[2,41]),{19:[1,79]},{18:80,28:$Vd,41:30,43:$Ve,44:$Vf},o($V7,[2,10]),{13:[1,81]},o($V1,[2,5]),o($Vn,[2,38]),{41:82,44:$Vf},{21:[1,83]},o($Vp,[2,9]),o($V7,[2,43]),o($V7,[2,40]),{13:[1,84]},o($Vn,[2,39])],
defaultActions: {5:[2,1],69:[2,36]},
parseError: function parseError(str, hash) {
    if (hash.recoverable) {
        this.trace(str);
    } else {
        function _parseError (msg, hash) {
            this.message = msg;
            this.hash = hash;
        }
        _parseError.prototype = Error;

        throw new _parseError(str, hash);
    }
},
parse: function parse(input) {
    var self = this, stack = [0], tstack = [], vstack = [null], lstack = [], table = this.table, yytext = '', yylineno = 0, yyleng = 0, recovering = 0, TERROR = 2, EOF = 1;
    var args = lstack.slice.call(arguments, 1);
    var lexer = Object.create(this.lexer);
    var sharedState = { yy: {} };
    for (var k in this.yy) {
        if (Object.prototype.hasOwnProperty.call(this.yy, k)) {
            sharedState.yy[k] = this.yy[k];
        }
    }
    lexer.setInput(input, sharedState.yy);
    sharedState.yy.lexer = lexer;
    sharedState.yy.parser = this;
    if (typeof lexer.yylloc == 'undefined') {
        lexer.yylloc = {};
    }
    var yyloc = lexer.yylloc;
    lstack.push(yyloc);
    var ranges = lexer.options && lexer.options.ranges;
    if (typeof sharedState.yy.parseError === 'function') {
        this.parseError = sharedState.yy.parseError;
    } else {
        this.parseError = Object.getPrototypeOf(this).parseError;
    }
    function popStack(n) {
        stack.length = stack.length - 2 * n;
        vstack.length = vstack.length - n;
        lstack.length = lstack.length - n;
    }
    _token_stack:
        var lex = function () {
            var token;
            token = lexer.lex() || EOF;
            if (typeof token !== 'number') {
                token = self.symbols_[token] || token;
            }
            return token;
        };
    var symbol, preErrorSymbol, state, action, a, r, yyval = {}, p, len, newState, expected;
    while (true) {
        state = stack[stack.length - 1];
        if (this.defaultActions[state]) {
            action = this.defaultActions[state];
        } else {
            if (symbol === null || typeof symbol == 'undefined') {
                symbol = lex();
            }
            action = table[state] && table[state][symbol];
        }
                    if (typeof action === 'undefined' || !action.length || !action[0]) {
                var errStr = '';
                expected = [];
                for (p in table[state]) {
                    if (this.terminals_[p] && p > TERROR) {
                        expected.push('\'' + this.terminals_[p] + '\'');
                    }
                }
                if (lexer.showPosition) {
                    errStr = 'Parse error on line ' + (yylineno + 1) + ':\n' + lexer.showPosition() + '\nExpecting ' + expected.join(', ') + ', got \'' + (this.terminals_[symbol] || symbol) + '\'';
                } else {
                    errStr = 'Parse error on line ' + (yylineno + 1) + ': Unexpected ' + (symbol == EOF ? 'end of input' : '\'' + (this.terminals_[symbol] || symbol) + '\'');
                }
                this.parseError(errStr, {
                    text: lexer.match,
                    token: this.terminals_[symbol] || symbol,
                    line: lexer.yylineno,
                    loc: yyloc,
                    expected: expected
                });
            }
        if (action[0] instanceof Array && action.length > 1) {
            throw new Error('Parse Error: multiple actions possible at state: ' + state + ', token: ' + symbol);
        }
        switch (action[0]) {
        case 1:
            stack.push(symbol);
            vstack.push(lexer.yytext);
            lstack.push(lexer.yylloc);
            stack.push(action[1]);
            symbol = null;
            if (!preErrorSymbol) {
                yyleng = lexer.yyleng;
                yytext = lexer.yytext;
                yylineno = lexer.yylineno;
                yyloc = lexer.yylloc;
                if (recovering > 0) {
                    recovering--;
                }
            } else {
                symbol = preErrorSymbol;
                preErrorSymbol = null;
            }
            break;
        case 2:
            len = this.productions_[action[1]][1];
            yyval.$ = vstack[vstack.length - len];
            yyval._$ = {
                first_line: lstack[lstack.length - (len || 1)].first_line,
                last_line: lstack[lstack.length - 1].last_line,
                first_column: lstack[lstack.length - (len || 1)].first_column,
                last_column: lstack[lstack.length - 1].last_column
            };
            if (ranges) {
                yyval._$.range = [
                    lstack[lstack.length - (len || 1)].range[0],
                    lstack[lstack.length - 1].range[1]
                ];
            }
            r = this.performAction.apply(yyval, [
                yytext,
                yyleng,
                yylineno,
                sharedState.yy,
                action[1],
                vstack,
                lstack
            ].concat(args));
            if (typeof r !== 'undefined') {
                return r;
            }
            if (len) {
                stack = stack.slice(0, -1 * len * 2);
                vstack = vstack.slice(0, -1 * len);
                lstack = lstack.slice(0, -1 * len);
            }
            stack.push(this.productions_[action[1]][0]);
            vstack.push(yyval.$);
            lstack.push(yyval._$);
            newState = table[stack[stack.length - 2]][stack[stack.length - 1]];
            stack.push(newState);
            break;
        case 3:
            return true;
        }
    }
    return true;
}};
/* generated by jison-lex 0.3.4 */
var lexer = (function(){
var lexer = ({

EOF:1,

parseError:function parseError(str, hash) {
        if (this.yy.parser) {
            this.yy.parser.parseError(str, hash);
        } else {
            throw new Error(str);
        }
    },

// resets the lexer, sets new input
setInput:function (input, yy) {
        this.yy = yy || this.yy || {};
        this._input = input;
        this._more = this._backtrack = this.done = false;
        this.yylineno = this.yyleng = 0;
        this.yytext = this.matched = this.match = '';
        this.conditionStack = ['INITIAL'];
        this.yylloc = {
            first_line: 1,
            first_column: 0,
            last_line: 1,
            last_column: 0
        };
        if (this.options.ranges) {
            this.yylloc.range = [0,0];
        }
        this.offset = 0;
        return this;
    },

// consumes and returns one char from the input
input:function () {
        var ch = this._input[0];
        this.yytext += ch;
        this.yyleng++;
        this.offset++;
        this.match += ch;
        this.matched += ch;
        var lines = ch.match(/(?:\r\n?|\n).*/g);
        if (lines) {
            this.yylineno++;
            this.yylloc.last_line++;
        } else {
            this.yylloc.last_column++;
        }
        if (this.options.ranges) {
            this.yylloc.range[1]++;
        }

        this._input = this._input.slice(1);
        return ch;
    },

// unshifts one char (or a string) into the input
unput:function (ch) {
        var len = ch.length;
        var lines = ch.split(/(?:\r\n?|\n)/g);

        this._input = ch + this._input;
        this.yytext = this.yytext.substr(0, this.yytext.length - len);
        //this.yyleng -= len;
        this.offset -= len;
        var oldLines = this.match.split(/(?:\r\n?|\n)/g);
        this.match = this.match.substr(0, this.match.length - 1);
        this.matched = this.matched.substr(0, this.matched.length - 1);

        if (lines.length - 1) {
            this.yylineno -= lines.length - 1;
        }
        var r = this.yylloc.range;

        this.yylloc = {
            first_line: this.yylloc.first_line,
            last_line: this.yylineno + 1,
            first_column: this.yylloc.first_column,
            last_column: lines ?
                (lines.length === oldLines.length ? this.yylloc.first_column : 0)
                 + oldLines[oldLines.length - lines.length].length - lines[0].length :
              this.yylloc.first_column - len
        };

        if (this.options.ranges) {
            this.yylloc.range = [r[0], r[0] + this.yyleng - len];
        }
        this.yyleng = this.yytext.length;
        return this;
    },

// When called from action, caches matched text and appends it on next action
more:function () {
        this._more = true;
        return this;
    },

// When called from action, signals the lexer that this rule fails to match the input, so the next matching rule (regex) should be tested instead.
reject:function () {
        if (this.options.backtrack_lexer) {
            this._backtrack = true;
        } else {
            return this.parseError('Lexical error on line ' + (this.yylineno + 1) + '. You can only invoke reject() in the lexer when the lexer is of the backtracking persuasion (options.backtrack_lexer = true).\n' + this.showPosition(), {
                text: "",
                token: null,
                line: this.yylineno
            });

        }
        return this;
    },

// retain first n characters of the match
less:function (n) {
        this.unput(this.match.slice(n));
    },

// displays already matched input, i.e. for error messages
pastInput:function () {
        var past = this.matched.substr(0, this.matched.length - this.match.length);
        return (past.length > 20 ? '...':'') + past.substr(-20).replace(/\n/g, "");
    },

// displays upcoming input, i.e. for error messages
upcomingInput:function () {
        var next = this.match;
        if (next.length < 20) {
            next += this._input.substr(0, 20-next.length);
        }
        return (next.substr(0,20) + (next.length > 20 ? '...' : '')).replace(/\n/g, "");
    },

// displays the character position where the lexing error occurred, i.e. for error messages
showPosition:function () {
        var pre = this.pastInput();
        var c = new Array(pre.length + 1).join("-");
        return pre + this.upcomingInput() + "\n" + c + "^";
    },

// test the lexed token: return FALSE when not a match, otherwise return token
test_match:function (match, indexed_rule) {
        var token,
            lines,
            backup;

        if (this.options.backtrack_lexer) {
            // save context
            backup = {
                yylineno: this.yylineno,
                yylloc: {
                    first_line: this.yylloc.first_line,
                    last_line: this.last_line,
                    first_column: this.yylloc.first_column,
                    last_column: this.yylloc.last_column
                },
                yytext: this.yytext,
                match: this.match,
                matches: this.matches,
                matched: this.matched,
                yyleng: this.yyleng,
                offset: this.offset,
                _more: this._more,
                _input: this._input,
                yy: this.yy,
                conditionStack: this.conditionStack.slice(0),
                done: this.done
            };
            if (this.options.ranges) {
                backup.yylloc.range = this.yylloc.range.slice(0);
            }
        }

        lines = match[0].match(/(?:\r\n?|\n).*/g);
        if (lines) {
            this.yylineno += lines.length;
        }
        this.yylloc = {
            first_line: this.yylloc.last_line,
            last_line: this.yylineno + 1,
            first_column: this.yylloc.last_column,
            last_column: lines ?
                         lines[lines.length - 1].length - lines[lines.length - 1].match(/\r?\n?/)[0].length :
                         this.yylloc.last_column + match[0].length
        };
        this.yytext += match[0];
        this.match += match[0];
        this.matches = match;
        this.yyleng = this.yytext.length;
        if (this.options.ranges) {
            this.yylloc.range = [this.offset, this.offset += this.yyleng];
        }
        this._more = false;
        this._backtrack = false;
        this._input = this._input.slice(match[0].length);
        this.matched += match[0];
        token = this.performAction.call(this, this.yy, this, indexed_rule, this.conditionStack[this.conditionStack.length - 1]);
        if (this.done && this._input) {
            this.done = false;
        }
        if (token) {
            return token;
        } else if (this._backtrack) {
            // recover context
            for (var k in backup) {
                this[k] = backup[k];
            }
            return false; // rule action called reject() implying the next rule should be tested instead.
        }
        return false;
    },

// return next match in input
next:function () {
        if (this.done) {
            return this.EOF;
        }
        if (!this._input) {
            this.done = true;
        }

        var token,
            match,
            tempMatch,
            index;
        if (!this._more) {
            this.yytext = '';
            this.match = '';
        }
        var rules = this._currentRules();
        for (var i = 0; i < rules.length; i++) {
            tempMatch = this._input.match(this.rules[rules[i]]);
            if (tempMatch && (!match || tempMatch[0].length > match[0].length)) {
                match = tempMatch;
                index = i;
                if (this.options.backtrack_lexer) {
                    token = this.test_match(tempMatch, rules[i]);
                    if (token !== false) {
                        return token;
                    } else if (this._backtrack) {
                        match = false;
                        continue; // rule action called reject() implying a rule MISmatch.
                    } else {
                        // else: this is a lexer rule which consumes input without producing a token (e.g. whitespace)
                        return false;
                    }
                } else if (!this.options.flex) {
                    break;
                }
            }
        }
        if (match) {
            token = this.test_match(match, rules[index]);
            if (token !== false) {
                return token;
            }
            // else: this is a lexer rule which consumes input without producing a token (e.g. whitespace)
            return false;
        }
        if (this._input === "") {
            return this.EOF;
        } else {
            return this.parseError('Lexical error on line ' + (this.yylineno + 1) + '. Unrecognized text.\n' + this.showPosition(), {
                text: "",
                token: null,
                line: this.yylineno
            });
        }
    },

// return next match that has a token
lex:function lex() {
        var r = this.next();
        if (r) {
            return r;
        } else {
            return this.lex();
        }
    },

// activates a new lexer condition state (pushes the new lexer condition state onto the condition stack)
begin:function begin(condition) {
        this.conditionStack.push(condition);
    },

// pop the previously active lexer condition state off the condition stack
popState:function popState() {
        var n = this.conditionStack.length - 1;
        if (n > 0) {
            return this.conditionStack.pop();
        } else {
            return this.conditionStack[0];
        }
    },

// produce the lexer rule set which is active for the currently active lexer condition state
_currentRules:function _currentRules() {
        if (this.conditionStack.length && this.conditionStack[this.conditionStack.length - 1]) {
            return this.conditions[this.conditionStack[this.conditionStack.length - 1]].rules;
        } else {
            return this.conditions["INITIAL"].rules;
        }
    },

// return the currently active lexer condition state; when an index argument is provided it produces the N-th previous condition state, if available
topState:function topState(n) {
        n = this.conditionStack.length - 1 - Math.abs(n || 0);
        if (n >= 0) {
            return this.conditionStack[n];
        } else {
            return "INITIAL";
        }
    },

// alias for begin(condition)
pushState:function pushState(condition) {
        this.begin(condition);
    },

// return the number of states currently on the stack
stateStackSize:function stateStackSize() {
        return this.conditionStack.length;
    },
options: {},
performAction: function anonymous(yy,yy_,$avoiding_name_collisions,YY_START) {
var YYSTATE=YY_START;
switch($avoiding_name_collisions) {
case 0:/* skip whitespace */
break;
case 1:return 7
break;
case 2:return 7
break;
case 3:return 29
break;
case 4:return 29
break;
case 5:return 29
break;
case 6:return 29
break;
case 7:return 29
break;
case 8:return 28
break;
case 9:return 43
break;
case 10:return 15
break;
case 11:return 15
break;
case 12:return 21
break;
case 13:return 22
break;
case 14:return 46
break;
case 15:return 44
break;
case 16:return 45
break;
case 17:return 11
break;
case 18:return 13
break;
case 19:return 19
break;
case 20:return 8
break;
case 21:return 10
break;
case 22:return 17
break;
case 23:return '='
break;
case 24:return 33
break;
case 25:return 32
break;
case 26:return 34
break;
case 27:return 36
break;
case 28:return 39
break;
case 29:return 37
break;
case 30:return 38
break;
case 31:return 40
break;
case 32:return 5
break;
case 33:return 'INVALID'
break;
}
},
rules: [/^(?:\s+)/,/^(?:(playMinion|death|damaged|attacks))/,/^(?:(cardDrawn|turnEnd|turnStart|playSpell))/,/^(?:(PLAYER|OPPONENT))/,/^(?:(TARGET|SELF|ACTIVATOR))/,/^(?:(SAVED))/,/^(?:(ENEMY|CHARACTER|MINION|FRIENDLY|HERO|DAMAGED|BASIC))/,/^(?:(SILENCE|SHIELD|PARALYZE|TAUNT|CLOAK|TECHRESIST|ROOTED))/,/^(?:(Random))/,/^(?:(SelectAttribute))/,/^(?:(DrawCard|SetAttribute|Hit|Heal|Buff|RemoveBuff|Spawn|GiveStatus|RemoveStatus|Charm|Destroy))/,/^(?:(endTurnTimer|startTurnTimer))/,/^(?:(health|attack|movement))/,/^(?:(Silence|Shield|Paralyze|Taunt|Cloak|TechResist|Root))/,/^(?:(true|false))/,/^(?:(-?[0-9]))/,/^(?:('(.*?)'))/,/^(?:\()/,/^(?:\))/,/^(?:,)/,/^(?:\{)/,/^(?:\})/,/^(?:\*)/,/^(?:=)/,/^(?:\|)/,/^(?:&)/,/^(?:-)/,/^(?:<)/,/^(?:<=)/,/^(?:>)/,/^(?:>=)/,/^(?:==)/,/^(?:$)/,/^(?:.)/],
conditions: {"INITIAL":{"rules":[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33],"inclusive":true}}
});
return lexer;
})();
parser.lexer = lexer;
function Parser () {
  this.yy = {};
}
Parser.prototype = parser;parser.Parser = Parser;
return new Parser;
})();


if (typeof require !== 'undefined' && typeof exports !== 'undefined') {
exports.parser = cardlang;
exports.Parser = cardlang.Parser;
exports.parse = function () { return cardlang.parse.apply(cardlang, arguments); };
exports.main = function commonjsMain(args) {
    if (!args[1]) {
        console.log('Usage: '+args[0]+' FILE');
        process.exit(1);
    }
    var source = require('fs').readFileSync(require('path').normalize(args[1]), "utf8");
    return exports.parser.parse(source);
};
if (typeof module !== 'undefined' && require.main === module) {
  exports.main(process.argv.slice(1));
}
}