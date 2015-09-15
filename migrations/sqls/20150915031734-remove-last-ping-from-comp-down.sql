alter table components 
  drop column is_active,
  add column last_ping timestamp with time zone not null default now();
