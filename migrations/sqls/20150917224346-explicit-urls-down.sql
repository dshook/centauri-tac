alter table components
  drop column http_url,
  drop column rest_url,
  drop column ws_url,
  add column url varchar;
