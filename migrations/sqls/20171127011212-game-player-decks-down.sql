alter table game_players
  drop constraint game_players_deck_id_fkey,
  drop column player_deck_id;

alter table player_decks
  drop column is_deleted;
