create table player_decks (
  id serial primary key,
  player_id integer not null,
  name varchar not null,
  race integer not null,
  type smallint default 0,
  is_valid bool default false,

  foreign key (player_id) references players(id)
);

create index idx_player_decks on player_decks(player_id);

create table deck_cards (
  deck_id integer not null,
  card_template_id integer not null,
  quantity smallint default 0,

  foreign key (deck_id) references player_decks(id)
);

create index idx_deck_cards on deck_cards(deck_id);