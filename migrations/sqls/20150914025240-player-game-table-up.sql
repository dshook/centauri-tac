create table game_players (
  game_id int not null,
  player_id int not null,

  foreign key (game_id) references games (id),
  foreign key (player_id) references players (id)
)
