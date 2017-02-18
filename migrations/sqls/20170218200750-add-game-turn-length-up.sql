/* Replace with your SQL commands */
alter table games
  add column turn_length_ms int not null default 30000;
