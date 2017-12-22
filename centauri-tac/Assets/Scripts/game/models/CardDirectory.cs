using System.Collections.Generic;
using System.Linq;
using System;
using strange.extensions.signal.impl;

namespace ctac
{
    [Singleton]
    public class CardDirectory
    {
        public List<CardModel> directory = new List<CardModel>();
        Signal<List<CardModel>> directoryLoaded = new Signal<List<CardModel>>();

        Signal finishedLoading = null; //real nasty but hey... should only call load once
        public void LoadCards(IJsonNetworkService network, Signal finishedLoadingSignal)
        {
            directory.Clear();
            var directoryUrl = "components/game/rest/cards/directory";

            directoryLoaded.AddListener(CardsLoaded);
            finishedLoading = finishedLoadingSignal;
            network.GetJson(directoryUrl, directoryLoaded);
        }

        public void CardsLoaded(List<CardModel> cards)
        {
            foreach (var card in cards)
            {
                AddCard(card);
            }
            finishedLoading.Dispatch();
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
