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
var o=function(k,v,o,l){for(o=o||{},l=k.length;l--;o[k[l]]=v);return o},$V0=[1,4],$V1=[5,7,13,20],$V2=[1,11],$V3=[1,15],$V4=[1,16],$V5=[1,17],$V6=[1,18],$V7=[1,19],$V8=[1,20],$V9=[1,43],$Va=[1,24],$Vb=[1,25],$Vc=[1,27],$Vd=[1,36],$Ve=[1,39],$Vf=[1,44],$Vg=[1,45],$Vh=[1,46],$Vi=[1,37],$Vj=[1,38],$Vk=[10,15,31,51,52,53,54,55],$Vl=[1,50],$Vm=[1,52],$Vn=[1,53],$Vo=[1,54],$Vp=[1,55],$Vq=[1,56],$Vr=[10,13,15,19,20,31,41,42,43,44,45,51,52,53,54,55],$Vs=[1,62],$Vt=[13,20],$Vu=[1,65],$Vv=[1,66],$Vw=[1,67],$Vx=[13,20,37,38,39],$Vy=[2,29],$Vz=[31,51,52,53,54,55],$VA=[1,80],$VB=[11,32,46,47,48],$VC=[1,88],$VD=[10,13,15,20,31,51,52,53,54,55];
var parser = {trace: function trace() { },
yy: {},
symbols_: {"error":2,"events":3,"eventList":4,"EOF":5,"pEvent":6,"event":7,"{":8,"actionlist":9,"}":10,"(":11,"arguments":12,")":13,"actionargs":14,"action":15,"*":16,"eNumber":17,"comparisonExpression":18,"&&":19,",":20,"argument_item":21,"possibleRandSelector":22,"attribute":23,"status":24,"buffAttribute":25,"area":26,"pText":27,"pBool":28,"selector":29,"targetExpr":30,"random":31,"target":32,"areaExpression":33,"tagExpression":34,"idExpression":35,"operator":36,"&":37,"|":38,"-":39,"compareOperator":40,"<":41,">":42,">=":43,"<=":44,"==":45,"Area":46,"Tagged":47,"Id":48,"pNumber":49,"numberList":50,"selectAttr":51,"count":52,"cardCount":53,"stat":54,"number":55,"text":56,"bool":57,"$accept":0,"$end":1},
terminals_: {2:"error",5:"EOF",7:"event",8:"{",10:"}",11:"(",13:")",15:"action",16:"*",19:"&&",20:",",23:"attribute",24:"status",26:"area",31:"random",32:"target",37:"&",38:"|",39:"-",41:"<",42:">",43:">=",44:"<=",45:"==",46:"Area",47:"Tagged",48:"Id",51:"selectAttr",52:"count",53:"cardCount",54:"stat",55:"number",56:"text",57:"bool"},
productions_: [0,[3,2],[4,2],[4,1],[6,4],[6,7],[9,2],[9,1],[14,4],[14,6],[14,6],[14,8],[12,3],[12,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[21,1],[22,1],[22,1],[22,4],[22,4],[30,1],[30,1],[30,1],[30,1],[30,3],[29,3],[29,3],[36,1],[36,1],[36,1],[40,1],[40,1],[40,1],[40,1],[40,1],[18,3],[33,4],[34,4],[35,4],[17,1],[17,4],[17,6],[17,4],[17,4],[17,1],[50,3],[50,1],[49,1],[25,4],[27,1],[28,1]],
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
case 3: case 7: case 13: case 55:
 this.$ = [$$[$0]]; 
break;
case 4:
 this.$ = { event: $$[$0-3], actions: $$[$0-1] } 
break;
case 5:
 this.$ = { event: $$[$0-6], args: $$[$0-4], actions: $$[$0-1] } 
break;
case 8:
 this.$ =
    { action: $$[$0-3], args: $$[$0-1] }
  
break;
case 9:
 this.$ =
    { action: $$[$0-5], args: $$[$0-3], times: $$[$0] }
  
break;
case 10:
 this.$ =
    { condition: $$[$0-5], action: $$[$0-3], args: $$[$0-1] }
  
break;
case 11:
 this.$ =
    { condition: $$[$0-7], action: $$[$0-5], args: $$[$0-3], times: $$[$0] }
  
break;
case 12: case 54:
 this.$ = $$[$0-2]; this.$.push($$[$0]); 
break;
case 14: case 15: case 16: case 17: case 18: case 19: case 20: case 21: case 22: case 23: case 24: case 29: case 30: case 31: case 32: case 48:
this.$ = $$[$0];
break;
case 26:
 this.$ = { left: $$[$0]}; 
break;
case 27:
 this.$ = { random: true, selector: { left: $$[$0-1]} }; 
break;
case 28:
 this.$ = { random: true, selector: $$[$0-1] }; 
break;
case 33:
this.$ = $$[$0-1];
break;
case 34: case 35:
 this.$ = { left: $$[$0-2], op: $$[$0-1], right: $$[$0] }; 
break;
case 44:
 this.$ = { compareExpression: true, left: $$[$0-2], op: $$[$0-1], right: $$[$0] }; 
break;
case 45:
 this.$ =
      { area: true, args: $$[$0-1] }
    
break;
case 46:
 this.$ =
      { tag: $$[$0-1] }
    
break;
case 47:
 this.$ =
      { id: $$[$0-1] }
    
break;
case 49:
 this.$ = { eNumber: true, randList: $$[$0-1] }; 
break;
case 50:
 this.$ = { eNumber: true, attributeSelector: $$[$0-3], attribute: $$[$0-1] }; 
break;
case 51:
 this.$ = { eNumber: true, count: true, selector: $$[$0-1] }; 
break;
case 52:
 this.$ = { eNumber: true, cardCount: true, selector: $$[$0-1] }; 
break;
case 53:
 this.$ = { stat: true, path: $$[$0] }; 
break;
case 56:
this.$ = parseInt($$[$0]);
break;
case 57:
 this.$ = { attribute: $$[$0-3], amount: $$[$0-1] }; 
break;
case 58:
this.$ = $$[$0].substring(1, $$[$0].length-1);;
break;
case 59:
this.$ = $$[$0] == 'true';
break;
}
},
table: [{3:1,4:2,6:3,7:$V0},{1:[3]},{5:[1,5],6:6,7:$V0},o($V1,[2,3]),{8:[1,7],11:[1,8]},{1:[2,1]},o($V1,[2,2]),{9:9,14:10,15:$V2,17:13,18:12,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},{4:33,6:3,7:$V0,11:$V9,12:21,14:32,15:$V2,17:28,18:31,21:22,22:23,23:$Va,24:$Vb,25:26,26:$Vc,27:29,28:30,29:34,30:35,31:$Vd,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8,56:$Vi,57:$Vj},{10:[1,47],14:48,15:$V2,17:13,18:12,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},o($Vk,[2,7]),{11:[1,49]},{19:$Vl},{40:51,41:$Vm,42:$Vn,43:$Vo,44:$Vp,45:$Vq},o($Vr,[2,48]),{11:[1,57]},{11:[1,58]},{11:[1,59]},{11:[1,60]},o($Vr,[2,53]),o($Vr,[2,56]),{13:[1,61],20:$Vs},o($Vt,[2,13]),o($Vt,[2,14]),o($Vt,[2,15],{11:[1,63]}),o($Vt,[2,16]),o($Vt,[2,17]),o($Vt,[2,18]),o($Vt,[2,19],{40:51,41:$Vm,42:$Vn,43:$Vo,44:$Vp,45:$Vq}),o($Vt,[2,20]),o($Vt,[2,21]),o($Vt,[2,22],{19:$Vl}),o($Vt,[2,23]),o($Vt,[2,24],{6:6,7:$V0}),o($Vt,[2,25],{36:64,37:$Vu,38:$Vv,39:$Vw}),o($Vt,[2,26],{36:68,37:$Vu,38:$Vv,39:$Vw}),{11:[1,69]},o($Vt,[2,58]),o($Vt,[2,59]),o($Vx,$Vy),o($Vx,[2,30]),o($Vx,[2,31]),o($Vx,[2,32]),{17:13,18:70,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},{11:[1,71]},{11:[1,72]},{11:[1,73]},o($V1,[2,4]),o($Vk,[2,6]),{4:33,6:3,7:$V0,11:$V9,12:74,14:32,15:$V2,17:28,18:31,21:22,22:23,23:$Va,24:$Vb,25:26,26:$Vc,27:29,28:30,29:34,30:35,31:$Vd,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8,56:$Vi,57:$Vj},{15:[1,75]},{17:76,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},o($Vz,[2,39]),o($Vz,[2,40]),o($Vz,[2,41]),o($Vz,[2,42]),o($Vz,[2,43]),{49:78,50:77,55:$V8},{11:$V9,22:79,29:34,30:35,31:$VA,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},{11:$V9,22:81,29:34,30:35,31:$VA,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},{11:$V9,22:82,29:34,30:35,31:$VA,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},{8:[1,83]},{4:33,6:3,7:$V0,11:$V9,14:32,15:$V2,17:28,18:31,21:84,22:23,23:$Va,24:$Vb,25:26,26:$Vc,27:29,28:30,29:34,30:35,31:$Vd,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8,56:$Vi,57:$Vj},{17:85,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},{11:$V9,30:86,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},o($VB,[2,36]),o($VB,[2,37]),o($VB,[2,38]),{11:$V9,30:87,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},{11:$V9,29:89,30:90,32:$VC,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:78,50:77,55:$V8},{13:[1,91]},{4:33,6:3,7:$V0,11:$V9,12:92,14:32,15:$V2,17:28,18:31,21:22,22:23,23:$Va,24:$Vb,25:26,26:$Vc,27:29,28:30,29:34,30:35,31:$Vd,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8,56:$Vi,57:$Vj},{27:93,56:$Vi},{17:94,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},{13:[1,95],20:$Vs},{11:[1,96]},o([13,19,20],[2,44]),{13:[1,97],20:[1,98]},o($Vt,[2,55]),{20:[1,99]},{11:[1,100]},{13:[1,101]},{13:[1,102]},{9:103,14:10,15:$V2,17:13,18:12,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},o($Vt,[2,12]),{13:[1,104]},o($Vx,[2,34]),o($Vx,[2,35]),o([37,38,39],$Vy,{13:[1,105]}),{13:[1,106],36:64,37:$Vu,38:$Vv,39:$Vw},{36:68,37:$Vu,38:$Vv,39:$Vw},o($Vx,[2,33]),{13:[1,107],20:$Vs},{13:[1,108]},{13:[1,109]},o($VD,[2,8],{16:[1,110]}),{4:33,6:3,7:$V0,11:$V9,12:111,14:32,15:$V2,17:28,18:31,21:22,22:23,23:$Va,24:$Vb,25:26,26:$Vc,27:29,28:30,29:34,30:35,31:$Vd,32:$Ve,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8,56:$Vi,57:$Vj},o($Vr,[2,49]),{49:112,55:$V8},{23:[1,113]},{11:$V9,29:89,30:90,32:$VC,33:40,34:41,35:42,46:$Vf,47:$Vg,48:$Vh},o($Vr,[2,51]),o($Vr,[2,52]),{10:[1,114],14:48,15:$V2,17:13,18:12,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},o($Vt,[2,57]),o($Vt,[2,27]),o($Vt,[2,28]),o($Vx,[2,45]),o($Vx,[2,46]),o($Vx,[2,47]),{17:115,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},{13:[1,116],20:$Vs},o($Vt,[2,54]),{13:[1,117]},o($V1,[2,5]),o($VD,[2,9]),o($VD,[2,10],{16:[1,118]}),o($Vr,[2,50]),{17:119,31:$V3,49:14,51:$V4,52:$V5,53:$V6,54:$V7,55:$V8},o($VD,[2,11])],
defaultActions: {5:[2,1]},
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
case 3:return 32
break;
case 4:return 32
break;
case 5:return 32
break;
case 6:return 32
break;
case 7:return 32
break;
case 8:return 32
break;
case 9:return 31
break;
case 10:return 51
break;
case 11:return 52
break;
case 12:return 53
break;
case 13:return 54
break;
case 14:return 15
break;
case 15:return 15
break;
case 16:return 15
break;
case 17:return 23
break;
case 18:return 24
break;
case 19:return 26
break;
case 20:return 46
break;
case 21:return 47
break;
case 22:return 48
break;
case 23:return 57
break;
case 24:return 55
break;
case 25:return 56
break;
case 26:return 11
break;
case 27:return 13
break;
case 28:return 20
break;
case 29:return 8
break;
case 30:return 10
break;
case 31:return 16
break;
case 32:return 45
break;
case 33:return '='
break;
case 34:return 38
break;
case 35:return 19
break;
case 36:return 37
break;
case 37:return 39
break;
case 38:return 41
break;
case 39:return 44
break;
case 40:return 42
break;
case 41:return 43
break;
case 42:return 5
break;
case 43:return 'INVALID'
break;
}
},
rules: [/^(?:\s+)/,/^(?:(playMinion|death|damaged|attacks|ability|healed))/,/^(?:(cardDrawn|turnEnd|turnStart|playSpell|cardDiscarded))/,/^(?:(PLAYER|OPPONENT))/,/^(?:(TARGET|SELF|ACTIVATOR))/,/^(?:(CURSOR))/,/^(?:(SAVED))/,/^(?:(ENEMY|CHARACTER|MINION|FRIENDLY|HERO|DAMAGED|BASIC|SPELL|DECK|HAND))/,/^(?:(SILENCE|SHIELD|PARALYZE|TAUNT|CLOAK|TECHRESIST|ROOTED|CANTATTACK|DYADSTRIKE))/,/^(?:(Random))/,/^(?:(SelectAttribute))/,/^(?:(Count))/,/^(?:(CardCount))/,/^(?:(COMBOCOUNT))/,/^(?:(SetAttribute|Hit|Heal|Buff|RemoveBuff|Spawn|GiveStatus|RemoveStatus|Charm|Destroy|Aura|Move|Transform|GiveArmor|AttachCode|CardAura|Unsummon|Choose))/,/^(?:(DrawCard|Discard|ChangeEnergy|GiveCard|ShuffleToDeck))/,/^(?:(endTurnTimer|startTurnTimer))/,/^(?:(health|attack|movement|range|cardTemplateId|armor|cost|baseHealth|baseAttack|baseMovement|baseRange|spellDamage))/,/^(?:(Silence|Shield|Paralyze|Taunt|Cloak|TechResist|Root|CantAttack|DyadStrike))/,/^(?:(Cross|Square|Line|Diagonal|Row))/,/^(?:(Area))/,/^(?:(Tagged))/,/^(?:(Id))/,/^(?:(true|false))/,/^(?:(-?[0-9]+))/,/^(?:('(.*?)'))/,/^(?:\()/,/^(?:\))/,/^(?:,)/,/^(?:\{)/,/^(?:\})/,/^(?:\*)/,/^(?:==)/,/^(?:=)/,/^(?:\|)/,/^(?:&&)/,/^(?:&)/,/^(?:-)/,/^(?:<)/,/^(?:<=)/,/^(?:>)/,/^(?:>=)/,/^(?:$)/,/^(?:.)/],
conditions: {"INITIAL":{"rules":[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43],"inclusive":true}}
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