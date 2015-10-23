using strange.extensions.command.impl;
using strange.extensions.context.api;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ctac
{
    public class PlayerConnectCommand : Command
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        [Inject]
        public PlayersModel players { get; set; }

        [Inject]
        public CardsModel cards { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public GameJoinConnectModel playerConnected { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        public override void Execute()
        {
            var player = players.players.FirstOrDefault(x => x.id == playerConnected.id);
            if (player != null)
            {
                //this must be a local player so just grab them
                gamePlayers.AddOrUpdate(player);
            }
            else
            {
                //remote players need a new player obj created
                gamePlayers.AddOrUpdate(new PlayerModel()
                {
                    clientId = Guid.NewGuid(),
                    isLocal = false,
                    id = playerConnected.id,
                    email = playerConnected.email,
                    registered = playerConnected.registered
                });
            }

            //give the player some cards if they don't have some
            if (!cards.Cards.Any(c => c.playerId == playerConnected.id))
            {
                var cardPrefab = Resources.Load("Card") as GameObject;
                var cardParent = contextView.transform.FindChild("cardCanvas");
                cards.Cards.Add(
                    new CardModel()
                    {
                        id = 1,
                        playerId = playerConnected.id,
                        name = "Dude",
                        description = "I do stuff, sometimes",
                        attack = 1,
                        health = 2
                    }
                );
                cards.Cards.Add(
                    new CardModel() {
                        id = 2,
                        playerId = playerConnected.id,
                        name = "Dudette",
                        description = "I always do something",
                        attack = 7,
                        health = 7
                    }
                );

                foreach (var card in cards.Cards)
                {
                    if (card.gameObject != null) continue;
                    var newCard = GameObject.Instantiate(
                        cardPrefab,
                        Vector3.zero,
                        Quaternion.identity
                    ) as GameObject;
                    newCard.transform.SetParent(cardParent.transform);
                    newCard.transform.localPosition = Vector3.zero;
                    newCard.transform.localScale = Vector3.one;

                    card.gameObject = newCard;
                    var pieceView = newCard.AddComponent<CardView>();
                    pieceView.card = card;
                }
            }

            debug.Log("Player Joined " + playerConnected.email + " Total Players " + gamePlayers.players.Count, socketKey);

        }
    }
}

