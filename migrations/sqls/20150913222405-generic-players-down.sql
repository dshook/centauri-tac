delete from games where host_player_id IN (select id from players where email = 'player1@gmail.com' or email = 'player2@gmail.com');
delete from players where
  email = 'player1@gmail.com'
  or email = 'player2@gmail.com';
