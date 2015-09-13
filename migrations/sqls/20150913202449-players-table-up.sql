create table players (
  id serial primary key,
  email character varying not null,
  password character varying not null,
  registered timestamp with time zone not null default now(),
  last_login timestamp with time zone not null default now(),
  is_admin boolean not null default false
);
