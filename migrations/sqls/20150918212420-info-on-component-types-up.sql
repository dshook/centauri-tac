alter table component_types
  add column show_in_client boolean not null default false,
  add column enable_http boolean not null default false,
  add column enable_rest boolean not null default false,
  add column enable_ws boolean not null default false;

update component_types set
  show_in_client = true,
  enable_rest = true,
  enable_ws = true
where name = 'master';

update component_types set
  show_in_client = true,
  enable_rest = true,
  enable_ws = true
where name = 'auth';

update component_types set
  enable_http = true
where name = 'portal';

update component_types set
  show_in_client = true,
  enable_ws = true
where name = 'gamelist';

update component_types set
  enable_ws = true
where name = 'game';

update component_types set
  enable_ws = true
where name = 'dispatch';


