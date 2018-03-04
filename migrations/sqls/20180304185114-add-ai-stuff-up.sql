insert into players
  (id, email, password, is_admin)
values
  (-1, 'AI@internet.com', '$2a$10$1zx0gRSgduvF1/.Uwz6M1OhvrejqyflepAb9vna917G59WPAP2pay', false);


INSERT INTO player_decks (id, player_id, name, race, type, is_valid, is_deleted)
VALUES (-1, -1, 'AI Deck', 2, 0, '1', '0');


INSERT INTO deck_cards (deck_id, card_template_id, quantity)
VALUES
    (-1, 1713, 1),
    (-1, 1635, 1),
    (-1, 1718, 1),
    (-1, 1693, 2),
    (-1, 1631, 1),
    (-1, 1629, 1),
    (-1, 1685, 1),
    (-1, 1682, 1),
    (-1, 1624, 1),
    (-1, 1677, 2),
    (-1, 1674, 2),
    (-1, 1617, 2),
    (-1, 1662, 2),
    (-1, 1660, 2),
    (-1, 1612, 2),
    (-1, 1651, 2),
    (-1, 1656, 2),
    (-1, 1646, 2),
    (-1, 1601, 2);
