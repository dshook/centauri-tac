create table game_component_info (
  id serial primary key,
  component_id int not null,
  max_slot_count int not null default 0,
  current_slot_count int not null default 0,
  registered timestamp with time zone not null default now(),
  last_ping timestamp with time zone not null default now(),

  foreign key (component_id) references components (id)
)
