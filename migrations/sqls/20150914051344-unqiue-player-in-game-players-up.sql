alter table game_players
  add constraint game_player_key unique (game_id, player_id);

