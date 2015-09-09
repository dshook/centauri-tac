alter table components
  add column registered timestamp with time zone
    not null
    default now();

alter table components
  add column last_ping timestamp with time zone;
