alter table games
  add column last_ping timestamp with time zone not null default now();
