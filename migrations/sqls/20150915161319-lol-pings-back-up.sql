alter table components
  add column last_ping timestamp with time zone not null default now();
