import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';
import Player from 'models/Player';

/**
 * Data layer for games
 */
@loglevel
export default class GameStore
{
  constructor(sql)
  {
    this.sql = sql;
  }

  /**
   * Every damn game (or by realm)
   */
  async all(realm = null)
  {
    let sql = `

      select * from games as g
      join components c
        on g.game_component_id = c.id
      join component_types t
        on c.component_type_id = t.id
      join players p
        on g.host_player_id = p.id

    `;

    const params = {};

    // TODO: active check here
    if (realm) {
      sql += `
        where c.realm = @realm
      `;

      params.realm = realm;
    }

    const models = [Game, Component, ComponentType, Player];

    const resp = await this.sql.tquery(...models)(sql, params,
      (g, c, t, p) => {
        g.gameComponent = c;
        g.gameComponent.type = t;
        g.hostPlayer = p;
        return g;
      });

    return resp.toArray();
  }

  /**
   * Get active games from a particular realm
   */
  async activeFromRealm(realm)
  {
    return await this.all(realm);
  }


  /**
   * Create a new game entry from a model, updates the id in the model
   */
  async register(game: Game): Promise<Game>
  {
    const resp = await this.sql.query(Game)(`

      insert into games
        (name, game_component_id, host_player_id)
      values
        (@name, @componentId, @hostPlayerId)
      returning id

      `,
      game);

    const {id} = resp.firstOrNull();

    game.id = id;

    return game;
  }
}
