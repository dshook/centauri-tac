using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    [Singleton]
    public class CardDirectory
    {
        public List<CardModel> directory = new List<CardModel>();

        public CardModel Card(int cardTemplateId)
        {
            return directory.FirstOrDefault(x => x.cardTemplateId == cardTemplateId);
        }

        public CardModel NewFromTemplate(int cardId, int cardTemplateId, int playerId)
        {
            var cardTemplate = Card(cardTemplateId);

            return new CardModel()
                {
                    id = cardId,
                    cardTemplateId = cardTemplateId,
                    playerId = playerId,
                    name = cardTemplate.name,
                    description = cardTemplate.description,
                    cost = cardTemplate.cost,
                    baseCost = cardTemplate.cost,
                    attack = cardTemplate.attack,
                    health = cardTemplate.health,
                    movement = cardTemplate.movement,
                    range = cardTemplate.range,
                    tags = cardTemplate.tags,
                    playable = false,
                    buffs = new List<CardBuffModel>(),
                    statuses = cardTemplate.statuses,
                    rarity = cardTemplate.rarity
                };
        }
    }
}
