create table game_states (
  id serial primary key,
  name varchar not null
);

insert into game_states (name) values
  ('ready'),
  ('staging'),
  ('started'),
  ('done');

alter table games
  add column game_state_id int,
  add constraint games_game_state_id_fkey foreign key
    (game_state_id) references game_states (id);

