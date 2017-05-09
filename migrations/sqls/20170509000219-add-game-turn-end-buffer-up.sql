alter table games
  add column turn_end_buffer_ms int not null default 5000;
