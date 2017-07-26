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

        internal void removeCard(DeckModel deck)
        {
            decksList.Remove(deck.deckListView);
            Destroy(deck.deckListView.gameObject);
            UpdateList();
        }

        //sort and reposition list by cost
        void UpdateList()
        {
            decksList = decksList.OrderBy(d => d.deck.id).ToList();
            const float deckListHeight = 34;
            const float padding = 5f;
            var contentHeight = deckListHeight * decksList.Count + padding;
            contentHeight = Mathf.Max(contentHeight, scrollRectTransform.sizeDelta.y);
            holderRectTransform.sizeDelta = new Vector2(holderRectTransform.sizeDelta.x, contentHeight);

            for(int i = 0; i < decksList.Count; i++)
            {
                decksList[i].gameObject.transform.localPosition = new Vector3(81f, -15 - (deckListHeight * i));
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
            deck.id = decksList.Count == 0 ? 0 : decksList.Max(d => d.deck.id) + 1;

            return deckListView;
        }


    }
}

