alter table components
  add column
    master_component_id integer default null,
  add constraint
    mcidfk foreign key (master_component_id) references components(id);
