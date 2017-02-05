create table games (
  id serial primary key,
  name varchar not null,
  host_player_id integer not null,
  max_player_count integer default 0,
  registered timestamp with time zone not null default now(),

  foreign key (host_player_id) references players(id)
);
