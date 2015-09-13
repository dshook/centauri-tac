create table component_types (
  id serial primary key,
  name character varying not null
);

insert into component_types (name) values
  ('master'),
  ('auth'),
  ('portal'),
  ('gamelist'),
  ('game');

create table components (
  id serial primary key,
  component_type_id integer not null,
  url character varying not null,
  registered timestamp with time zone not null default now(),
  last_ping timestamp with time zone not null default now(),
  version varchar not null default '0.0.0',
  realm varchar,

  foreign key (component_type_id) references component_types(id)
);
