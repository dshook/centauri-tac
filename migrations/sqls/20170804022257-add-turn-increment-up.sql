alter table games
  add column turn_increment_ms int not null default 5000;
