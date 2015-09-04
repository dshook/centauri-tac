create table players (
  id serial primary key,
  email character varying not null,
  password character varying not null,
  registered timestamp with time zone not null default now()
);
