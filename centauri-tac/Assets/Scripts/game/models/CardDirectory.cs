using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace ctac
{
    [Singleton]
    public class CardDirectory
    {
        public List<CardModel> directory = new List<CardModel>();

        public void LoadCards()
        {
            directory.Clear();
            //fetch all cards from disk
            foreach (string file in Directory.GetFiles("../cards", "*.json", SearchOption.AllDirectories))
            {
                string cardText = File.ReadAllText(file);
                var cardTemplate = JsonConvert.DeserializeObject<CardModel>(cardText);
                AddCard(cardTemplate);
            }
        }

        public void AddCard(CardModel card)
        {
            card.baseCost = card.cost;

            //auto add ranged/melee tag for minions
            if (card.isMinion)
            {
                if (card.range != null && card.range > 0)
                {
                    card.tags.Add("Ranged");
                }
                else
                {
                    card.tags.Add("Melee");
                }
            }

            directory.Add(card);
        }

        public CardModel Card(int cardTemplateId)
        {
            return directory.FirstOrDefault(x => x.cardTemplateId == cardTemplateId);
        }

        public CardModel NewFromTemplate(int cardId, int cardTemplateId, int playerId)
        {
            var cardTemplate = Card(cardTemplateId);
            if(cardTemplate == null){
                throw new Exception("Card template Id " + cardTemplateId + " not found in directory");
            }

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
                    rarity = cardTemplate.rarity,
                    race = cardTemplate.race
                };
        }
    }
}
