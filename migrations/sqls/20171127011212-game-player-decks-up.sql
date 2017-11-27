alter table game_players
  add column player_deck_id int,
  add constraint game_players_deck_id_fkey foreign key
    (player_deck_id) references player_decks (id);

alter table player_decks
  add column is_deleted bool default false;
