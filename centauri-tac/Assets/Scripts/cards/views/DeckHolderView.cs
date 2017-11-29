using UnityEngine.UI;
using System.Collections.Generic;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class DeckHolderView : View
    {
        public GameObject contentHolder;
        RectTransform holderRectTransform;
        RectTransform scrollRectTransform;

        IResourceLoaderService loader;

        List<DeckListView> decksList = new List<DeckListView>();
        const int fakeDeckId = 0;

        internal void init(IResourceLoaderService l)
        {
            loader = l;
            holderRectTransform = contentHolder.GetComponent<RectTransform>();

            var scrollRect = GetComponentInChildren<ScrollRect>();
            scrollRectTransform = scrollRect.gameObject.GetComponent<RectTransform>();

            contentHolder.transform.DestroyChildren(true);
            UpdateList();
        }

        void Update()
        {
        }

        internal void addDeck(DeckModel deck)
        {
            decksList.Add(CreateDeckList(deck, contentHolder.transform));
            UpdateList();
        }

        internal void removeDeck(DeckModel deck)
        {
            var deckView = decksList.FirstOrDefault(d => d.deck.id == deck.id);
            decksList.Remove(deckView);
            Destroy(deckView.gameObject);
            UpdateList();
        }

        internal void clearDecks()
        {
            for(int i = 0; i < decksList.Count; i++)
            {
                Destroy(decksList[i].gameObject);
            }
            decksList.Clear();
            UpdateList();
        }

        internal void selectDeck(DeckModel deck)
        {
            for(int i = 0; i < decksList.Count; i++)
            {
                decksList[i].isSelected = deck == null ? false : deck.id == decksList[i].deck.id;
            }
        }

        //once deck is saved on the server and has a real ID find the corresponding deck and update
        internal void deckSaved(DeckModel deck)
        {
            var toUpdate = decksList.FirstOrDefault(d => d.deck.id == fakeDeckId);
            if (toUpdate != null) {
                toUpdate.deck.id = deck.id;
            }
            UpdateList();
        }

        //sort list by id and display
        void UpdateList()
        {
            decksList = decksList.OrderBy(d => d.deck.id).ToList();
            const float deckListHeight = 36;
            const float margin = 5f;
            var contentHeight = deckListHeight * decksList.Count + margin;
            contentHeight = Mathf.Max(contentHeight, scrollRectTransform.sizeDelta.y);
            holderRectTransform.sizeDelta = new Vector2(holderRectTransform.sizeDelta.x, contentHeight);

            for(int i = 0; i < decksList.Count; i++)
            {
                decksList[i].gameObject.transform.localPosition = new Vector3(81f, -20 - (deckListHeight * i));
            }
        }

        public DeckListView CreateDeckList(DeckModel deck, Transform parent)
        {
            var deckPrefab = loader.Load<GameObject>("DeckList");

            var newDeck = GameObject.Instantiate(
                deckPrefab,
                Constants.cardSpawnPosition,
                Quaternion.identity
            ) as GameObject;
            if (parent != null)
            {
                newDeck.transform.SetParent(parent, false);
            }
            newDeck.name = deck.name;

            var deckListView = newDeck.AddComponent<DeckListView>();
            deckListView.deck = deck;
            deck.deckListView = deckListView;

            //TODO: id from server prolly
            deck.id = deck.id != 0 ? deck.id : fakeDeckId;

            return deckListView;
        }


    }
}

