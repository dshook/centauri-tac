alter table components
  drop column url,
  add column http_url varchar,
  add column rest_url varchar,
  add column ws_url varchar;
