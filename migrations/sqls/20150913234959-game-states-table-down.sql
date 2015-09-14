alter table games
  drop constraint games_game_state_id_fkey,
  drop column game_state_id;

drop table game_states;
