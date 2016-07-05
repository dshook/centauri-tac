#Cardlang Possibilities

##Event triggers
* ~~play minion~~
* ~~death~~
* ~~unit takes damage~~
* ~~unit attacks~~
* ~~turn end~~
* ~~turn start~~
* ~~card drawn~~
* ~~spell played~~
* ~~unit ability (may require n turns to charge)~~

##Selectors
* ~~Need set operators for Union (|), intersection (&), and compliment (-)~~
* ~~target~~
* ~~activator~~
* ~~self~~
* ~~random enemy~~
* ~~all minions~~
* ~~event selectors~~
* ~~friendly/enemy~~
* ~~damaged~~
* ~~damaged~~
* ~~specific card by id~~

* ~~tags (battlecry, etc)~~
* ~~statuses~~
* zones (play, deck, hand, secret, graveyard)
* cardtype (~~minion~~, ~~hero~~, weapons, ~~spell~~, secret)

##Areas
* ~~cross~~
* ~~square~~
* ~~line~~
* ~~diagonal~~

##Randomness
* ~~random number in list~~
* ~~random minion, possibly from class or by cost~~
* random tile within area

##Conditions - check for AND, OR, Equality
* Unit is dead (self?)
* holding card with selector
* ~~attributes with selector (get enemy hero hp)~~
* board state (number of minions on board)
* has minion with selector on the board


##Actions
* choose effects
* copy minion (into hand or play)
* Add code to targets (give x minions this deathrattle)
* move minion into play from deck/unsummon minion from play
* card cost modification
* Shuffle into deck
* ~~transform a minion into another minion~~
* ~~aura effects~~
* ~~discard a card~~
* ~~add buff~~
* ~~spawn minion~~
* ~~give/remove status effect (silence, shield, paralyze, taunt, cloak, tech resist, rooted)~~
* ~~take control of minion~~
* ~~destroy minion~~
* ~~swap health and attack~~
* ~~destroy/give energy~~
* ~~effect that lasts till end of turn~~
* ~~Hit for damage~~
* ~~draw card~~
* ~~Heal target~~
* move/push piece
    - ~~jump to space in radius~~
    - hook piece which pulls to closest available tile in line
    - ~~teleport to arbitrary tile in range~~
    - ~~swap positions with minion~~

##Counters
* mana spent for card
* minions died 
* Aggregation functions, sum/count all Attribute of selected pieces

