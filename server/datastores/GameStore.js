import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';

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

  async all()
  {
    const resp = await this.sql.tquery(Game, Component, ComponentType)(`

        select * from games as g
        join components c
          on g.game_component_id = c.id
        join component_types t
          on c.component_type_id = t.id

      `, null,
      (g, c, t) => {
        g.gameComponent = c;
        g.gameComponent.type = t;
        return g;
      });

    return resp.toArray();
  }
}
