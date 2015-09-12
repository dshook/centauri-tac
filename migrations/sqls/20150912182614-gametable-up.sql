create table games (
  id serial primary key,
  name varchar not null,
  game_component_id integer not null,
  host_player_id integer not null,
  current_player_count integer default 0,
  max_player_count integer default 0,
  registered timestamp with time zone not null default now(),
  last_ping timestamp with time zone not null default now(),

  foreign key (game_component_id) references components(id),
  foreign key (host_player_id) references players(id)
);
