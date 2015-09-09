create table component_types (
  id serial primary key,
  name character varying not null
);

create table components (
  id serial primary key,
  component_type_id integer not null,
  url character varying not null,

  foreign key (component_type_id) references component_types(id)
);
