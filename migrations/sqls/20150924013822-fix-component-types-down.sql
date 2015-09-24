update component_types
  set enable_rest = false
  where name = 'game' or name = 'gamelist';
