delete from components where component_type_id = (select id from component_types where name = 'matchmaker' limit 1);
delete from component_types where name = 'matchmaker';
