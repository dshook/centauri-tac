Cardlang Possibilities

Event triggers
	play
	death
	unit takes damage
	turn end
	card drawn
	spell played

Selectors
	Need set operators for Union, intersection, and compliment
	target
	self
	random enemy
	all minions

	tags (battlecry, damaged, paralyzed, etc)
	zones (play, deck, hand, secret, graveyard)
	friendly/enemy
	cardtype (minion, weapons, spell, secret, hero power)

Areas
    cross
    square
    line
    diagonal
    cone


Randomness
	ranom number in list
	random minion, possibly from class or by cost

Conditions - check for AND and OR
	Unit is dead (self?)
	holding tribal card
	attributes with selector (get enemy hero hp)


Actions
	Shuffle into deck
	Hit for damage
	spawn minion
	add buff
	destroy target minion
	copy minion (into hand or play)
	give status effect (silence, shield)
    aura effects
    card cost modification