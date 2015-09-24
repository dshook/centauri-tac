alter table games
  add column locked boolean not null default false,
  add column allow_join boolean not null default true;
