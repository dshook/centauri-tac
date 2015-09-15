alter table components 
  drop column last_ping,
  add column is_active boolean default false;
